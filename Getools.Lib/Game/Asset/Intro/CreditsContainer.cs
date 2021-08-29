﻿using System;
using System.Collections.Generic;
using System.Text;
using Getools.Lib.Game.Enums;

namespace Getools.Lib.Game.Asset.Intro
{
    /// <summary>
    /// Container object for credits data. TO be used to convert collection into .c array.
    /// </summary>
    public class CreditsContainer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreditsContainer"/> class.
        /// </summary>
        public CreditsContainer()
        {
        }

        /// <summary>
        /// Credits entries.
        /// </summary>
        public List<IntroCreditEntry> CreditsEntries { get; set; } = new List<IntroCreditEntry>();

        /// <summary>
        /// Gets or sets the offset this entry was read from.
        /// </summary>
        public int Offset { get; set; }

        /// <summary>
        /// Gets or sets the variable name used in source file.
        /// </summary>
        public string VariableName { get; set; }

        /// <summary>
        /// Builds a string to describe the current object
        /// as a complete declaraction in c, using normal structs. Includes type, variable
        /// name and trailing semi-colon.
        /// </summary>
        /// <param name="prefix">Prefix or indentation.</param>
        /// <param name="printIndex">Whether or not to include a index of the items printed.</param>
        /// <returns>String of object.</returns>
        public string ToCDeclaration(string prefix = "", bool printIndex = false)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"{prefix}{IntroCreditEntry.CTypeName} {VariableName}[] = {{");

            if (!printIndex)
            {
                Utility.AllButLast(
                    CreditsEntries,
                    x => sb.AppendLine($"{prefix}{Config.DefaultIndent}{x.ToCInlineDeclaration()}, "),
                    x => sb.AppendLine($"{prefix}{Config.DefaultIndent}{x.ToCInlineDeclaration()}"));
            }
            else
            {
                Utility.AllButLast(
                    CreditsEntries,
                    (x, index) =>
                    {
                        sb.AppendLine($"{prefix}{Config.DefaultIndent}/* index = {index} */");
                        sb.AppendLine($"{prefix}{Config.DefaultIndent}{x.ToCInlineDeclaration()}, ");
                    },
                    (x, index) =>
                    {
                        sb.AppendLine($"{prefix}{Config.DefaultIndent}/* index = {index} */");
                        sb.AppendLine($"{prefix}{Config.DefaultIndent}{x.ToCInlineDeclaration()}");
                    });
            }

            sb.AppendLine($"{prefix}}};");

            return sb.ToString();
        }
    }
}
