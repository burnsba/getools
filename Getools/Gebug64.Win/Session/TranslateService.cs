using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Win.Session
{
    /// <summary>
    /// Service to facilitate type conversion.
    /// </summary>
    public class TranslateService
    {
        private List<TranslatorLookup> _lookups = new List<TranslatorLookup>();

        /// <summary>
        /// Initializes a new instance of the <see cref="TranslateService"/> class.
        /// This uses reflection to register all types in the current assembly
        /// that implement <see cref="ISessionTranslator{,}"/>.
        /// </summary>
        public TranslateService()
        {
            var types = System.Reflection.Assembly.GetExecutingAssembly().GetTypes();
            var sessionTranslatorTypes = types
                .Where(x =>
                    !x.IsAbstract
                    && !x.IsInterface
                    && x.BaseType != null
                    && x.GetInterfaces().Count() > 0
                    && x.GetInterfaces().Any(y => y.Name == "ISessionTranslator`2"))
                .ToList();

            foreach (var translatorType in sessionTranslatorTypes)
            {
                var interfaceType = translatorType.GetInterfaces().FirstOrDefault(y => y.Name == "ISessionTranslator`2");
                var genericArgs = interfaceType.GetGenericArguments();
                var runtimeType = genericArgs[0];
                var containerType = genericArgs[1];

                var translator = Activator.CreateInstance(translatorType, new object[] { this });

                var tl = new TranslatorLookup()
                {
                    TranslatorType = translatorType,
                    RuntimeType = runtimeType,
                    ContainerType = containerType,
                    Translator = translator,
                };

                _lookups.Add(tl);
            }
        }

        /// <summary>
        /// Covnerts from one type to another.
        /// </summary>
        /// <typeparam name="TSource">Source type.</typeparam>
        /// <typeparam name="TDest">Destination type.</typeparam>
        /// <param name="source">Source values to convert.</param>
        /// <returns>Converted object.</returns>
        public TDest Translate<TSource, TDest>(TSource source)
        {
            var sourceType = typeof(TSource);
            var destType = typeof(TDest);

            var translateFromUntyped = _lookups.FirstOrDefault(x => x.RuntimeType == sourceType && x.ContainerType == destType);
            if (!object.ReferenceEquals(null, translateFromUntyped))
            {
                var translateFrom = (ISessionTranslator<TSource, TDest>)translateFromUntyped.Translator;
                return translateFrom.ConvertFrom(source);
            }

            var translateBackUntyped = _lookups.FirstOrDefault(x => x.RuntimeType == destType && x.ContainerType == sourceType);
            if (!object.ReferenceEquals(null, translateBackUntyped))
            {
                var translateBack = (ISessionTranslator<TDest, TSource>)translateBackUntyped.Translator;
                return translateBack.ConvertBack(source);
            }

            var sourceTypeName = typeof(TSource).Name;
            var destTypeName = typeof(TDest).Name;
            throw new NotSupportedException($"Could not find translator for type conversion. \"{sourceTypeName}\" => \"{destTypeName}\"");
        }

        public void ReflectionTranslate(object source, object dest)
        {
            if (object.ReferenceEquals(null, source))
            {
                throw new NullReferenceException(nameof(source));
            }

            if (object.ReferenceEquals(null, dest))
            {
                throw new NullReferenceException(nameof(dest));
            }

            var sourceProperties = source.GetType().GetProperties();
            var destType = dest.GetType();

            foreach (var property in sourceProperties)
            {
                if (!property.CanRead)
                {
                    continue;
                }

                PropertyInfo destProperty = destType.GetProperty(property.Name);

                if (destProperty == null)
                {
                    continue;
                }

                if (!destProperty.CanWrite)
                {
                    continue;
                }

                if (destProperty.GetSetMethod(true) != null && destProperty.GetSetMethod(true)!.IsPrivate)
                {
                    continue;
                }

                if ((destProperty.GetSetMethod().Attributes & MethodAttributes.Static) != 0)
                {
                    continue;
                }

                if (!destProperty.PropertyType.IsAssignableFrom(property.PropertyType))
                {
                    continue;
                }

                // Passed all tests, lets set the value
                destProperty.SetValue(dest, property.GetValue(source, null), null);
            }
        }

        private class TranslatorLookup
        {
            public Type TranslatorType { get; set; }

            public Type RuntimeType { get; set; }

            public Type ContainerType { get; set; }

            public object Translator { get; set; }
        }
    }
}
