using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime.Tree;
using Getools.Lib.Game.EnumModel;
using Getools.Lib.Game.Enums;

namespace Getools.Lib.Game.Asset.Setup
{
    public class SourceMap
    {
        public static FileMap GetFileMap(Enums.Version version, int levelId)
        {
            switch (levelId)
            {
                case (int)LevelId.Dam: return new FileMap { Dir = string.Empty, Filename = "UsetupdamZ" };
                case (int)LevelId.Facility: return new FileMap { Dir = string.Empty, Filename = "UsetuparkZ" };
                case (int)LevelId.Runway: return new FileMap { Dir = string.Empty, Filename = "UsetuprunZ" };
                case (int)LevelId.Surface: return new FileMap { Dir = string.Empty, Filename = "UsetupsevxZ" };
                case (int)LevelId.Bunker1: return new FileMap { Dir = string.Empty, Filename = "UsetupsevbunkerZ" };
                case (int)LevelId.Silo:
                {
                    switch (version)
                    {
                        case Enums.Version.Ntsc: return new FileMap { Dir = "u", Filename = "UsetupsiloZ" };
                        case Enums.Version.Pal: return new FileMap { Dir = "e", Filename = "UsetupsiloZ" };
                        case Enums.Version.NtscJ: return new FileMap { Dir = "j", Filename = "UsetupsiloZ" };
                    }
                }

                break;

                case (int)LevelId.Frigate:
                {
                    switch (version)
                    {
                        case Enums.Version.Ntsc: return new FileMap { Dir = "u", Filename = "UsetupdestZ" };
                        case Enums.Version.Pal: return new FileMap { Dir = "e", Filename = "UsetupdestZ" };
                        case Enums.Version.NtscJ: return new FileMap { Dir = "j", Filename = "UsetupdestZ" };
                    }
                }

                break;

                case (int)LevelId.Surface2: return new FileMap { Dir = string.Empty, Filename = "UsetupsevxbZ" };
                case (int)LevelId.Bunker2: return new FileMap { Dir = string.Empty, Filename = "UsetupsevbZ" };
                case (int)LevelId.Statue:
                {
                    switch (version)
                    {
                        case Enums.Version.Ntsc: return new FileMap { Dir = "u", Filename = "UsetupstatueZ" };
                        case Enums.Version.Pal: return new FileMap { Dir = "e", Filename = "UsetupstatueZ" };
                        case Enums.Version.NtscJ: return new FileMap { Dir = "j", Filename = "UsetupstatueZ" };
                    }
                }

                break;

                case (int)LevelId.Archives: return new FileMap { Dir = string.Empty, Filename = "UsetuparchZ" };
                case (int)LevelId.Streets: return new FileMap { Dir = string.Empty, Filename = "UsetuppeteZ" };
                case (int)LevelId.Depot: return new FileMap { Dir = string.Empty, Filename = "UsetupdepoZ" };
                case (int)LevelId.Train:
                {
                    switch (version)
                    {
                        case Enums.Version.Ntsc: return new FileMap { Dir = "u", Filename = "UsetuptraZ" };
                        case Enums.Version.Pal: return new FileMap { Dir = "e", Filename = "UsetuptraZ" };
                        case Enums.Version.NtscJ: return new FileMap { Dir = "j", Filename = "UsetuptraZ" };
                    }
                }

                break;

                case (int)LevelId.Jungle:
                {
                    switch (version)
                    {
                        case Enums.Version.Ntsc: return new FileMap { Dir = "u", Filename = "UsetupjunZ" };
                        case Enums.Version.Pal: return new FileMap { Dir = "e", Filename = "UsetupjunZ" };
                        case Enums.Version.NtscJ: return new FileMap { Dir = "j", Filename = "UsetupjunZ" };
                    }
                }

                break;

                case (int)LevelId.Control: return new FileMap { Dir = string.Empty, Filename = "UsetupcontrolZ" };
                case (int)LevelId.Caverns: return new FileMap { Dir = string.Empty, Filename = "UsetupcaveZ" };
                case (int)LevelId.Cradle:
                {
                    switch (version)
                    {
                        case Enums.Version.Ntsc: return new FileMap { Dir = "u", Filename = "UsetupcradZ" };
                        case Enums.Version.Pal: return new FileMap { Dir = "e", Filename = "UsetupcradZ" };
                        case Enums.Version.NtscJ: return new FileMap { Dir = "j", Filename = "UsetupcradZ" };
                    }
                }

                break;

                case (int)LevelId.Aztec: return new FileMap { Dir = string.Empty, Filename = "UsetupaztZ" };
                case (int)LevelId.Egypt: return new FileMap { Dir = string.Empty, Filename = "UsetupcrypZ" };
                case (int)LevelId.Cuba:
                {
                    switch (version)
                    {
                        case Enums.Version.Ntsc: return new FileMap { Dir = "u", Filename = "UsetuplenZ" };
                        case Enums.Version.Pal: return new FileMap { Dir = "e", Filename = "UsetuplenZ" };
                        case Enums.Version.NtscJ: return new FileMap { Dir = "j", Filename = "UsetuplenZ" };
                    }
                }

                break;
            }

            throw new NotImplementedException();
        }
    }
}
