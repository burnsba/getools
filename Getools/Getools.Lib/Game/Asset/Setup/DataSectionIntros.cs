using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Getools.Lib.BinPack;
using Getools.Lib.Game.Asset.Intro;
using Newtonsoft.Json;

namespace Getools.Lib.Game.Asset.Setup
{
    /// <summary>
    /// Intros section.
    /// </summary>
    public class DataSectionIntros : SetupDataSection
    {
        private const string _defaultVariableName = "intro";

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSectionIntros"/> class.
        /// </summary>
        public DataSectionIntros()
            : base(SetupSectionId.SectionIntro, _defaultVariableName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSectionIntros"/> class.
        /// </summary>
        /// <param name="isCreditSection">Not used parameter.</param>
        private DataSectionIntros(bool isCreditSection)
            : base(SetupSectionId.CreditsData, _defaultVariableName)
        {
        }

        /// <summary>
        /// Gets or sets the path intro data.
        /// Each entry should contain any necessary "prequel" data that
        /// would be listed before this main entry.
        /// </summary>
        public List<IIntro> Intros { get; set; } = new List<IIntro>();

        /// <inheritdoc />
        [JsonIgnore]
        public override int BaseDataSize
        {
            get
            {
                return
                    GetPrequelDataSize() +
                    Intros.Sum(x => x.BaseDataSize);
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSectionIntros"/> class.
        /// </summary>
        /// <returns>New section.</returns>
        public static DataSectionIntros NewCreditsSection()
        {
            var sds = new DataSectionIntros(isCreditSection: true);
            return sds;
        }

        /// <inheritdoc />
        public override string GetDeclarationTypeName()
        {
            return $"{IntroBase.CTypeName} {VariableName}[]";
        }

        /// <inheritdoc />
        public override void WritePrequelData(StreamWriter sw)
        {
            // nothing to do
        }

        /// <inheritdoc />
        public override void WriteSectionData(StreamWriter sw)
        {
            if (TypeId == SetupSectionId.CreditsData)
            {
                if (Intros.OfType<IntroCredits>().Where(x => x.Credits != null).Any())
                {
                    foreach (var entry in Intros.OfType<IntroCredits>().Where(x => x.Credits != null))
                    {
                        if (object.ReferenceEquals(null, entry.Credits))
                        {
                            throw new NullReferenceException();
                        }

                        sw.WriteLine(entry.Credits.ToCDeclaration());
                    }
                }
            }
            else if (TypeId == SetupSectionId.SectionIntro)
            {
                sw.WriteLine($"{GetDeclarationTypeName()} = {{");

                Utility.ApplyCommaList(
                    sw.WriteLine,
                    Intros,
                    (x, index) =>
                    {
                        var s = $"{Config.DefaultIndent}/* {nameof(IIntro.Type)} = {x.Type}; index = {index} */";
                        s += Environment.NewLine;
                        s += x.ToCInlineS32Array(Config.DefaultIndent);
                        return s;
                    });

                sw.WriteLine("};");
            }
            else
            {
                throw new NotSupportedException($"Type not supported: {TypeId}");
            }
        }

        /// <inheritdoc />
        public override void DeserializeFix(int startingIndex = 0)
        {
            int index = startingIndex;

            foreach (var entry in Intros.OfType<IntroCredits>().Where(x => x.Credits != null))
            {
                if (object.ReferenceEquals(null, entry.Credits))
                {
                    throw new NullReferenceException();
                }

                if (string.IsNullOrEmpty(entry.Credits.VariableName))
                {
                    entry.Credits.VariableName = $"credits_data_{index}";
                }

                if (object.ReferenceEquals(null, entry.CreditsDataPointer))
                {
                    throw new NullReferenceException();
                }

                if (entry.CreditsDataPointer.IsNull || entry.CreditsDataPointer.PointedToOffset == 0)
                {
                    entry.CreditsDataPointer.AssignPointer(entry.Credits);
                }

                index++;
            }
        }

        /// <inheritdoc />
        public override int GetEntriesCount()
        {
            return Intros.Count;
        }

        /// <inheritdoc />
        public override int GetPrequelDataSize()
        {
            return 0;
        }

        /// <inheritdoc />
        public override void Collect(IAssembleContext context)
        {
            foreach (var entry in Intros)
            {
                context.AppendToDataSection(entry);
            }
        }

        /// <inheritdoc />
        public override void Assemble(IAssembleContext context)
        {
            // nothing to do
        }
    }
}
