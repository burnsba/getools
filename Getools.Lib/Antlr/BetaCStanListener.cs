using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Antlr4.Runtime.Misc;
using Getools.Lib;
using Getools.Lib.Antlr.Gen;
using Getools.Lib.Error;
using Getools.Lib.Game;
using Getools.Lib.Game.Asset.Stan;

namespace Getools.Lib.Antlr
{
    /// <summary>
    /// Parser listener. Methods are called as parsed tree is walked.
    /// </summary>
    public class BetaCStanListener : CBaseListener
    {
        /*
         * This class is a simple state machine to parse the beat .c file definition for stan file (beta/debug).
         *
         * Order of operations for a declaration:
         *     EnterStorageClassSpecifier (optional. example: extern)
         *     EnterTypeSpecifier (example: u8, float)
         *     EnterDeclarator (example: tile[4])
         *     EnterDeclaration (example: {1,1,1,3} )
         *     EnterAssignmentExpression (example: 1)
         *
         * When a new declaraction is entered (EnterDeclaration), the current parse state is reset.
         */

        private ParseState _parseState = ParseState.Unset;
        private int _currentFieldIndex = -1;
        private int _ignoreAssignmentCount = 0;

        private StandTile _workingTile = null;
        private StandTilePoint _workingPoint = null;

        private StandFile _workingResult = null;

        private bool _footerDone = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="BetaCStanListener"/> class.
        /// </summary>
        public BetaCStanListener()
        {
        }

        private enum ParseState
        {
            // used on extern declarations
            IgnoreDeclaration = -2,

            Unset = -1,

            DefaultUnknown = 0,

            /// <summary>
            /// Currently parsing header object.
            /// </summary>
            Header = 1,

            /// <summary>
            /// Currently parsing a tile.
            /// </summary>
            Tile = 2,

            /// <summary>
            /// Currently parsing a point.
            /// </summary>
            Point = 3,

            /// <summary>
            /// Currently parsing footer.
            /// </summary>
            Footer = 4,

            /// <summary>
            /// Currently parsing beta footer list of strings.
            /// </summary>
            BetaFooter = 6,
        }

        /// <summary>
        /// Gets the result after parsing a stan C file.
        /// </summary>
        public StandFile Result { get; private set; }

        /// <summary>
        /// Entry point to begin parsing.
        /// </summary>
        /// <param name="context">Context.</param>
        public override void EnterCompilationUnit([NotNull] CParser.CompilationUnitContext context)
        {
            Result = null;
            _workingResult = new StandFile(TypeFormat.Beta);
        }

        /// <summary>
        /// Finalize current parsing.
        /// </summary>
        /// <param name="context">Context.</param>
        public override void ExitCompilationUnit([NotNull] CParser.CompilationUnitContext context)
        {
            _workingResult.Tiles = _workingResult.Tiles.OrderBy(x => x.OrderIndex).ToList();

            string msg;

            if (object.ReferenceEquals(null, _workingResult.Header))
            {
                msg = $"No header section found. Expected a variable with type {Config.Stan.HeaderCTypeName}";
                throw new BadFileFormatException(msg);
            }

            if (object.ReferenceEquals(null, _workingResult.Footer))
            {
                msg = $"No footer section found. Expected a variable with type {Config.Stan.FooterCTypeName}";
                throw new BadFileFormatException(msg);
            }

            if (object.ReferenceEquals(null, _workingResult.Tiles) || !_workingResult.Tiles.Any())
            {
                msg = $"No tiles were found. Expected variable declarations with type {Config.Stan.TileBetaCTypeName}";
                throw new BadFileFormatException(msg);
            }

            ////// should missing points list at the end throw?

            Result = _workingResult;
        }

        /// <summary>
        /// Resets parse state to start parsing a declaration.
        /// Unsets <see cref="_currentFieldIndex"/>.
        /// </summary>
        /// <param name="context">Context.</param>
        public override void EnterDeclaration([NotNull] CParser.DeclarationContext context)
        {
            //////Console.WriteLine($"{System.Reflection.MethodBase.GetCurrentMethod().Name}: {context.GetText()}");

            // the last assignment in the tile should change the parse state to a point, so
            // if a declaration is enounctered while the parse state is a tile
            // then something went wrong.
            if (_parseState == ParseState.Tile)
            {
                throw new BadFileFormatException("Error parsing tile, missing fields before entering point declaration.");
            }

            _parseState = ParseState.Unset;
            _currentFieldIndex = -1;
        }

        /// <summary>
        /// Unsets the current parse state.
        /// If this is the close of a tile or point, also adds current item to the appropriate result collection.
        /// </summary>
        /// <param name="context">Context.</param>
        public override void ExitDeclaration([NotNull] CParser.DeclarationContext context)
        {
            //////Console.WriteLine($"{System.Reflection.MethodBase.GetCurrentMethod().Name}: {context.GetText()}");

            // If the tile has any points, then the parse state will be changed to Point,
            // so we need to check for either state here when exiting the declaration.
            if (_parseState == ParseState.Tile || _parseState == ParseState.Point)
            {
                if (!object.ReferenceEquals(null, _workingTile))
                {
                    if (!_workingTile.Points.Any())
                    {
                        throw new BadFileFormatException("Error, found tile without any points.");
                    }

                    _workingResult.Tiles.Add(_workingTile);
                }

                _workingTile = null;
            }
            else if (_parseState == ParseState.Footer)
            {
                _footerDone = true;
            }

            _parseState = ParseState.Unset;
        }

        /// <summary>
        /// Called once for each single value in an array or struct declaration.
        /// Sets the next property depending on current parse state.
        /// Header values are only set once, from the first header found.
        /// Footer values are only set once, from the first footer found.
        /// Will add <see cref="_workingPoint"/> to <see cref="_workingPointsList"/> once all values are set.
        /// </summary>
        /// <param name="context">Context.</param>
        public override void EnterAssignmentExpression([NotNull] CParser.AssignmentExpressionContext context)
        {
            //////Console.WriteLine($"{System.Reflection.MethodBase.GetCurrentMethod().Name}: {context.GetText()}");

            if (_ignoreAssignmentCount > 0)
            {
                _ignoreAssignmentCount--;
                return;
            }

            var text = context.GetText();

            // String values include open and closing quotes which are no longer needed.
            if (text.StartsWith("\"") && text.EndsWith("\""))
            {
                text = text.Substring(1, text.Length - 2);
            }

            int? val = null;
            int i;

            // assume value will be an int and try to parse it
            if (ParseHelpers.TryParseInt(text, out i))
            {
                val = i;
            }

            if (_parseState == ParseState.Header)
            {
                // header
                if (object.ReferenceEquals(null, _workingResult.Header))
                {
                    _workingResult.Header = new StandFileHeader();
                }

                if (_currentFieldIndex == -1 || _currentFieldIndex == 0)
                {
                    _workingResult.Header.Unknown1 = val;
                    _currentFieldIndex = 1;
                }
                else if (_currentFieldIndex == 1)
                {
                    _workingResult.Header.FirstTileOffset = val.Value;
                    _currentFieldIndex++;
                }
                else if (_currentFieldIndex >= 2)
                {
                    if (val == null)
                    {
                        // convert NULL pointer to bytes
                        _workingResult.Header.UnknownHeaderData.Add(0);
                        _workingResult.Header.UnknownHeaderData.Add(0);
                        _workingResult.Header.UnknownHeaderData.Add(0);
                        _workingResult.Header.UnknownHeaderData.Add(0);
                    }
                    else
                    {
                        _workingResult.Header.UnknownHeaderData.Add((byte)val.Value);
                        _currentFieldIndex++;
                    }
                }
            }
            else if (_parseState == ParseState.Tile)
            {
                // tile
                if (object.ReferenceEquals(null, _workingTile))
                {
                    _workingTile = new StandTile();
                }

                if (!val.HasValue)
                {
                    throw new BadFileFormatException("Tile does not allow nulls");
                }

                switch (_currentFieldIndex)
                {
                    case -1:
                    case 0:
                        _workingTile.InternalName = val.Value;
                        _currentFieldIndex = 1;
                        break;

                    case 1:
                        _workingTile.Room = (byte)val.Value;
                        _currentFieldIndex++;
                        break;

                    case 2:
                        _workingTile.Flags = (byte)val.Value;
                        _currentFieldIndex++;
                        break;

                    case 3:
                        _workingTile.R = (byte)val.Value;
                        _currentFieldIndex++;
                        break;

                    case 4:
                        _workingTile.G = (byte)val.Value;
                        _currentFieldIndex++;
                        break;

                    case 5:
                        _workingTile.B = (byte)val.Value;
                        _currentFieldIndex++;
                        break;

                    case 6:
                        _workingTile.PointNameOffset = (short)val.Value;
                        _currentFieldIndex++;
                        break;

                    case 7:
                        _workingTile.PointCount = (byte)val.Value;
                        _currentFieldIndex++;
                        break;

                    case 8:
                        _workingTile.FirstPoint = (byte)val.Value;
                        _currentFieldIndex++;
                        break;

                    case 9:
                        _workingTile.SecondPoint = (byte)val.Value;
                        _currentFieldIndex++;
                        break;

                    case 10:
                        _workingTile.ThirdPoint = (byte)val.Value;

                        // after the tile properties is the array of points related to the tile.
                        // advance state to start parsing points if there are any.
                        if (_workingTile.PointCount > 0)
                        {
                            _parseState = ParseState.Point;
                            _currentFieldIndex = 0;
                        }
                        else
                        {
                            _currentFieldIndex++;
                        }

                        break;
                }
            }
            else if (_parseState == ParseState.Point)
            {
                if (object.ReferenceEquals(null, _workingPoint))
                {
                    _workingPoint = new StandTilePoint();
                }

                Single? floatVal = null;
                Single s;
                if (Single.TryParse(text, out s))
                {
                    floatVal = s;
                }

                switch (_currentFieldIndex)
                {
                    case -1:
                    case 0:
                        _workingPoint = new StandTilePoint();

                        if (!floatVal.HasValue)
                        {
                            throw new BadFileFormatException("Point does not allow nulls");
                        }

                        _workingPoint.FloatX = floatVal.Value;
                        _currentFieldIndex = 1;
                        break;

                    case 1:

                        if (!floatVal.HasValue)
                        {
                            throw new BadFileFormatException("Point does not allow nulls");
                        }

                        _workingPoint.FloatY = floatVal.Value;
                        _currentFieldIndex++;
                        break;

                    case 2:

                        if (!floatVal.HasValue)
                        {
                            throw new BadFileFormatException("Point does not allow nulls");
                        }

                        _workingPoint.FloatZ = floatVal.Value;
                        _currentFieldIndex++;
                        break;

                    case 3:
                        _workingPoint.Link = (int)val.Value;

                        // This declaration is an inline array listing, so need to reset the working point
                        // for the next value in the array.
                        _currentFieldIndex = 0;
                        _workingTile.Points.Add(_workingPoint);
                        break;
                }
            }
            else if (_parseState == ParseState.Footer)
            {
                // footer
                if (object.ReferenceEquals(null, _workingResult.Footer))
                {
                    _workingResult.Footer = new StandFileFooter();
                }

                switch (_currentFieldIndex)
                {
                    case -1:
                    case 0:
                        _workingResult.Footer.Unknown1 = val;
                        _currentFieldIndex = 1;
                        break;

                    case 1:
                        _workingResult.Footer.Unknown2 = val;
                        _currentFieldIndex++;
                        break;

                    case 2:
                        _workingResult.Footer.C = text;
                        _currentFieldIndex++;
                        break;

                    case 3:
                        _workingResult.Footer.Unknown3 = val;
                        _currentFieldIndex++;
                        break;

                    case 4:
                        _workingResult.Footer.Unknown4 = val;
                        _currentFieldIndex++;
                        break;

                    case 5:
                        _workingResult.Footer.Unknown5 = val;
                        _currentFieldIndex++;
                        break;

                    case 6:
                        _workingResult.Footer.Unknown5 = val;
                        _currentFieldIndex++;
                        break;
                }
            }
            else if (_parseState == ParseState.BetaFooter)
            {
                if (object.ReferenceEquals(null, _workingResult.BetaFooter))
                {
                    _workingResult.BetaFooter = new BetaFooter();
                }

                // Should only be long array of strings, so no switch statement or anything.
                // Note that the list may contain empty strings. However, the first empty string
                // is actually a fake value added to get the .c file to building into a matching
                // binary. This is inserted automatically on .c output generation, so ignore
                // that here.
                if (_currentFieldIndex < 1 && text == string.Empty)
                {
                    _currentFieldIndex = 1;
                }
                else
                {
                    _workingResult.BetaFooter.BetaPointList.Add(text);
                    _currentFieldIndex++;
                }
            }
        }

        /// <summary>
        /// Only used to flag extern declarations to be ignored.
        /// </summary>
        /// <param name="context">Context.</param>
        public override void EnterStorageClassSpecifier([NotNull] CParser.StorageClassSpecifierContext context)
        {
            //////Console.WriteLine($"{System.Reflection.MethodBase.GetCurrentMethod().Name}: {context.GetText()}");

            var text = context.GetText();

            if (text == "extern")
            {
                _parseState = ParseState.IgnoreDeclaration;
                _currentFieldIndex = -1;
                return;
            }
        }

        /// <summary>
        /// Type specifier determines the parse state, and how properties are set in <see cref="EnterAssignmentExpression"/>.
        /// </summary>
        /// <param name="context">Context.</param>
        public override void EnterTypeSpecifier([NotNull] CParser.TypeSpecifierContext context)
        {
            //////Console.WriteLine($"{System.Reflection.MethodBase.GetCurrentMethod().Name}: {context.GetText()}");

            var text = context.GetText();

            if (_parseState == ParseState.Unset)
            {
                if (text == Config.Stan.HeaderCTypeName)
                {
                    _parseState = ParseState.Header;
                }
                else if (text == Config.Stan.TileBetaCTypeName)
                {
                    _parseState = ParseState.Tile;
                }
                else if (text == Config.Stan.FooterCTypeName)
                {
                    _parseState = ParseState.Footer;
                }
                else if (_footerDone && text == Config.Stan.BetaFooterCTypeName)
                {
                    _parseState = ParseState.BetaFooter;
                }
            }
            else
            {
                throw new BadFileFormatException("Attempted to parse type specifier while parsing another declaration.");
            }
        }

        /// <summary>
        /// Declarator contains the name and optional array length.
        /// </summary>
        /// <param name="context">Context.</param>
        public override void EnterDeclarator([NotNull] CParser.DeclaratorContext context)
        {
            //////Console.WriteLine($"{System.Reflection.MethodBase.GetCurrentMethod().Name}: {context.GetText()}");

            var text = context.GetText();

            if (_parseState == ParseState.Header)
            {
                if (object.ReferenceEquals(null, _workingResult.Header))
                {
                    _workingResult.Header = new StandFileHeader();
                }

                _workingResult.Header.Name = text;
            }
            else if (_parseState == ParseState.Tile)
            {
                if (object.ReferenceEquals(null, _workingTile))
                {
                    _workingTile = new StandTile();
                }

                _workingTile.VariableName = text;

                int i = SplitToOrderId(text, 10);
                if (i > -1)
                {
                    _workingTile.OrderIndex = i;
                }
            }
            else if (_parseState == ParseState.Footer)
            {
                if (object.ReferenceEquals(null, _workingResult.Footer))
                {
                    _workingResult.Footer = new StandFileFooter();
                }

                _workingResult.Footer.Name = text;
            }
            else if (_parseState == ParseState.BetaFooter)
            {
                if (object.ReferenceEquals(null, _workingResult.BetaFooter))
                {
                    _workingResult.BetaFooter = new BetaFooter();
                }

                // check if this is an array declaration.
                // This should be a 2d array, but we only care about the first array length.
                var openBracket = text.IndexOf('[');
                var closeBracket = text.IndexOf(']');
                int i;

                if (openBracket > -1 && closeBracket > (openBracket + 1))
                {
                    // calculate array length first.
                    string bracketText = text.Substring(openBracket + 1, closeBracket - openBracket - 1);
                    if (ParseHelpers.TryParseInt(bracketText, out i))
                    {
                        // adjust for first empty string.
                        _workingResult.BetaFooter.DeclaredLength = i - 1;

                        if (_workingResult.BetaFooter.DeclaredLength < 0)
                        {
                            _workingResult.BetaFooter.DeclaredLength = 0;
                        }
                    }

                    // Now truncate array declaration text to get just the name
                    text = text.Substring(0, openBracket);

                    // ignore this array length, and the 2nd dimension array length (should be constant anyway)
                    _ignoreAssignmentCount = 2;
                }

                _workingResult.BetaFooter.VariableName = text;
            }
        }

        /// <summary>
        /// Initializer and assignment are both called for assignments, but initializer
        /// is called once per struct instead of starting with the first value.
        /// </summary>
        /// <param name="context">Context.</param>
        public override void EnterInitializer([NotNull] CParser.InitializerContext context)
        {
            ////////Console.WriteLine($"{System.Reflection.MethodBase.GetCurrentMethod().Name}: {context.GetText()}");
        }

        /// <summary>
        /// Tiles and points in the stan .c file are named according to the order
        /// they are listed in the file, e.g., "tile_0", "tile_1", etc. This splits
        /// text on the first underscore and parses the remaining text and
        /// returns the value.
        /// </summary>
        /// <param name="s">Name of variable.</param>
        /// <param name="numberBase">Base to convert number as.</param>
        /// <returns>Parsed number, or -1 if unsuccessful.</returns>
        private int SplitToOrderId(string s, int numberBase)
        {
            int result;
            var underscore = s.IndexOf('_');
            if (underscore > 1 && (underscore + 1) < s.Length)
            {
                string text = s.Substring(underscore + 1);
                result = Convert.ToInt32(text, numberBase);
                return result;
            }

            return -1;
        }
    }
}
