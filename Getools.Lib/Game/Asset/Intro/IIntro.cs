using System;
using System.Collections.Generic;
using System.Text;
using Getools.Lib.BinPack;

namespace Getools.Lib.Game.Asset.Intro
{
    /// <summary>
    /// Interface to describe intro definitions used in setup file in the intro section.
    /// </summary>
    public interface IIntro : IGameObjectHeader, IBinData, IGetoolsLibObject
    {
        /// <summary>
        /// Gets or sets the type of intro definition.
        /// </summary>
        public IntroType Type { get; set; }
    }
}
