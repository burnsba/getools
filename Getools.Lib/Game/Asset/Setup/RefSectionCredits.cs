using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Getools.Lib.Game.Asset.Intro;
using Newtonsoft.Json;

namespace Getools.Lib.Game.Asset.Setup
{
    /// <summary>
    /// Non main section. Pseudo section, should refer to the main intro section.
    /// This is only used to print the credits data in the correct spot.
    /// </summary>
    public class RefSectionCredits : SetupDataSection
    {
        private IntroCredits _credits = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="RefSectionCredits"/> class.
        /// </summary>
        /// <param name="typeId">Section type.</param>
        [JsonConstructor]
        private RefSectionCredits(SetupSectionId typeId)
            : base(typeId)
        {
        }

        /// <summary>
        /// Creates a new credits section.
        /// </summary>
        /// <param name="credits">Parent intro section.</param>
        /// <returns>New object.</returns>
        public static RefSectionCredits NewSection(IntroCredits credits)
        {
            var section = new RefSectionCredits(SetupSectionId.CreditsData);

            section._credits = credits;

            return section;
        }

        /// <inheritdoc />
        public override void DeserializeFix(int startingIndex = 0)
        {
            // nothing to do
        }

        /// <inheritdoc />
        public override string GetDeclarationTypeName()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override void WritePrequelData(StreamWriter sw)
        {
            // nothing to do
        }

        /// <inheritdoc />
        public override void WriteSectionData(StreamWriter sw)
        {
            if (_credits.Credits != null)
            {
                sw.WriteLine(_credits.Credits.ToCDeclaration(printIndex: true));
            }
        }

        /// <summary>
        /// This is a "fake" setion only used to put the data in the right place, should refer to parent Intros section.
        /// </summary>
        /// <returns>Zero.</returns>
        public override int GetEntriesCount()
        {
            return 0;
        }

        /// <inheritdoc />
        public override int GetPrequelDataSize()
        {
            return 0;
        }
    }
}
