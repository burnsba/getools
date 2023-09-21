using Gebug64.Test.Framework;
using Gebug64.Test.Tests;
using Gebug64.Unfloader.Protocol.Flashcart;
using Gebug64.Unfloader.SerialPort;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Reflection;

namespace Gebug64.Test
{
    internal class Program
    {
        public static IServiceProvider ServiceProvider { get; set; }

        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder();
            //.SetBasePath(Directory.GetCurrentDirectory())
            //.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
            var configuration = builder.Build();

            var serviceCollection = new ServiceCollection();

            ConfigureServices(serviceCollection, configuration);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            ServiceProvider = serviceProvider;

            ///

            //var results = RunTests();
            var results = TestSingle(nameof(BasicCommunication), nameof(BasicCommunication.EverdriveTestCommandResponse));

            Console.WriteLine("Test failures: ");

            if (results.TestFailures.Any())
            {
                foreach (var fail in results.TestFailures)
                {
                    Console.WriteLine($"{fail.ClassName} : {fail.MethodName}");
                }
            }
            else
            {
                Console.WriteLine("... none");
            }

            Console.WriteLine($"Total test cases: {results.TotalTestMethods}");
            Console.WriteLine($"Fail: {results.FailCount}");
            Console.WriteLine($"Pass: {results.PassCount}");
        }

        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            var typeGetter = new SerialPortFactoryTypeGetter() { Type = typeof(VirtualSerialPort) };
            services.AddSingleton<SerialPortFactoryTypeGetter>(typeGetter);

            services.AddTransient<SerialPortFactory>();

            services.AddTransient<MockConsoleHost>();
        }

        private static TestResults TestSingle(string className, string methodName)
        {
            return RunTests(x => x.Name == className, x => x.Name == methodName);
        }

        private static TestResults RunTests(
            Func<Type, bool>? classFilter = null,
            Func<MethodInfo, bool>? methodFilter = null)
        {
            var result = new TestResults();

            var testClasses = GetTypesWithTestAttribute(Assembly.GetExecutingAssembly());
            if (classFilter != null)
            {
                testClasses = testClasses.Where(classFilter);
            }

            foreach (var testClass in testClasses)
            {
                result.TotalTestClasses++;

                var testMethods = GetMethodsWithFactAttribute(testClass);
                if (methodFilter != null)
                {
                    testMethods = testMethods.Where(methodFilter);
                }

                if (testMethods.Any())
                {
                    var instance = ActivatorUtilities.CreateInstance(ServiceProvider, testClass);

                    foreach (var method in testMethods)
                    {
                        result.TotalTestMethods++;

                        try
                        {
                            method.Invoke(instance, null);
                            result.PassCount++;
                        }
                        catch (Exception ex)
                        {
                            result.FailCount++;

                            result.TestFailures.Add(new TestCaseInfo(testClass.Name, method.Name, ex));
                        }
                    }
                }
            }

            return result;
        }

        private static IEnumerable<Type> GetTypesWithTestAttribute(Assembly assembly)
        {
            foreach (Type type in assembly.GetTypes())
            {
                if (type.GetCustomAttributes(typeof(TestAttribute), true).Length > 0)
                {
                    yield return type;
                }
            }
        }

        private static IEnumerable<MethodInfo> GetMethodsWithFactAttribute(Type type)
        {
            foreach (MethodInfo mi in type.GetMethods())
            {
                if (mi.GetCustomAttributes(typeof(FactAttribute), false).Length > 0)
                {
                    yield return mi;
                }
            }
        }
    }
}