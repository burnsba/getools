using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Asset.Intro
{
    /// <summary>
    /// Base class for setup intro definitions.
    /// </summary>
    public abstract class IntroBase : GameObjectHeaderBase, IIntro
    {
        /// <summary>
        /// C file, type name. Should match known struct type.
        /// </summary>
        public const string CTypeName = "s32";

        /// <summary>
        /// Initializes a new instance of the <see cref="IntroBase"/> class.
        /// </summary>
        public IntroBase()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IntroBase"/> class.
        /// </summary>
        /// <param name="type">Type of intro definition.</param>
        public IntroBase(IntroType type)
        {
            Type = type;
        }

        /// <summary>
        /// Gets or sets the intro type.
        /// This is a wrapper for <see cref="IGameObjectHeader.TypeRaw"/>.
        /// </summary>
        public IntroType Type
        {
            get
            {
                return (IntroType)TypeRaw;
            }

            set
            {
                TypeRaw = (byte)value;
            }
        }
    }
}
