using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Gebug64.Unfloader.Manage;
using Gebug64.Unfloader.Protocol.Gebug.Dto;
using Gebug64.Unfloader.Protocol.Gebug.Message;
using Gebug64.Win.Mvvm;
using Getools.Lib.Game;
using Getools.Lib.Game.Enums;
using Microsoft.Extensions.Logging;

namespace Gebug64.Win.ViewModels.Game
{
    /// <summary>
    /// Character or guard object.
    /// </summary>
    public class Chr : GameObject, IMapSelectedObjectViewModel
    {
        private int _chrNum = -1;
        private int _chrSlotIndex = -1;
        private GuardActType _actionType = GuardActType.ActInvalidData;
        private Single _damage = 0.0f;
        private Single _maxDamage = 0.0f;
        private Single _intolerance = 0.0f;
        private UInt32 _anim = 0;
        private UInt32 _chrFlags = 0;
        private string _chrFlagsText = string.Empty;
        private byte _propFlags = 0;
        private string _propFlagsText = string.Empty;
        private byte _chrFlags2 = 0;
        private string _chrFlags2Text = string.Empty;
        private UInt16 _chrHidden = 0;
        private string _chrHiddenText = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="Chr"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="connectionServiceProviderResolver">Connection service provider.</param>
        public Chr(ILogger logger, IConnectionServiceProviderResolver connectionServiceProviderResolver)
            : base(logger, connectionServiceProviderResolver)
        {
            GhostHpCommand = new CommandHandler(GhostHpCommandHandler, () => CanGhostHpCommand);
            MaxHpCommand = new CommandHandler(MaxHpCommandHandler, () => CanMaxHpCommand);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Chr"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="connectionServiceProviderResolver">Connection service provider.</param>
        /// <param name="msgGuard">Mesg source to copy.</param>
        public Chr(ILogger logger, IConnectionServiceProviderResolver connectionServiceProviderResolver, RmonGuardPosition msgGuard)
            : this(logger, connectionServiceProviderResolver)
        {
            _chrNum = msgGuard.Chrnum;
            PropDefType = PropDef.Guard;
            _chrSlotIndex = msgGuard.ChrSlotIndex;
            _actionType = msgGuard.ActionType;
            PropPos = msgGuard.PropPos.Clone();
            TargetPos = msgGuard.TargetPos.Clone();
            Subroty = msgGuard.Subroty;
            _damage = msgGuard.Damage;
            _maxDamage = msgGuard.MaxDamage;
            _intolerance = msgGuard.Intolerance;
            _anim = msgGuard.Anim;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Chr"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="connectionServiceProviderResolver">Connection service provider.</param>
        /// <param name="chr">Cource to copy.</param>
        public Chr(ILogger logger, IConnectionServiceProviderResolver connectionServiceProviderResolver, Chr chr)
            : this(logger, connectionServiceProviderResolver)
        {
            _chrNum = chr._chrNum;
            _chrSlotIndex = chr._chrSlotIndex;
            _actionType = chr._actionType;
            PropPos = chr.PropPos.Clone();
            TargetPos = chr.TargetPos.Clone();
            Subroty = chr.Subroty;
            _damage = chr._damage;
            _maxDamage = chr._maxDamage;
            _intolerance = chr._intolerance;
            _anim = chr._anim;
        }

        /// <summary>
        /// In-game chrnum id.
        /// </summary>
        public int ChrNum
        {
            get => _chrNum;

            set
            {
                if (_chrNum != value)
                {
                    _chrNum = value;
                    OnPropertyChanged(nameof(ChrNum));
                    OnPropertyChanged(nameof(ChrNumText));
                }
            }
        }

        /// <summary>
        /// Format <see cref="ChrNum"/> as <see cref="string"/>.
        /// </summary>
        public string ChrNumText => ChrNum.ToString();

        /// <summary>
        /// Index source from g_ChrSlots.
        /// </summary>
        public int ChrSlotIndex
        {
            get => _chrSlotIndex;

            set
            {
                if (_chrSlotIndex != value)
                {
                    _chrSlotIndex = value;
                    OnPropertyChanged(nameof(ChrSlotIndex));
                    OnPropertyChanged(nameof(ChrSlotIndexText));
                }
            }
        }

        /// <summary>
        /// Format <see cref="ChrSlotIndex"/> as <see cref="string"/>.
        /// </summary>
        public string ChrSlotIndexText => ChrSlotIndex.ToString();

        /// <summary>
        /// Guard current action.
        /// </summary>
        public GuardActType ActionType
        {
            get => _actionType;

            set
            {
                if (_actionType != value)
                {
                    _actionType = value;
                    OnPropertyChanged(nameof(ActionType));
                    OnPropertyChanged(nameof(ActionTypeText));
                }
            }
        }

        /// <summary>
        /// Format <see cref="ActionType"/> as <see cref="string"/>.
        /// </summary>
        public string ActionTypeText => Getools.Lib.Formatters.EnumFormat.GuardActTypeFriendlyName(ActionType);

        /// <summary>
        /// Prop->pos property.
        /// </summary>
        public Coord3dd PropPos { get; set; } = Coord3dd.Zero.Clone();

        /// <summary>
        /// If <see cref="ActionType"/> is <see cref="GuardActType.ActGoPos"/>, this is guard->act_gopos->targetpos.
        /// If <see cref="ActionType"/> is <see cref="GuardActType.ActPatrol"/>, this is guard->act_patrol->waydata->pos.
        /// Otherwise this is zero.
        /// </summary>
        public Coord3dd TargetPos { get; set; } = Coord3dd.Zero.Clone();

        /// <summary>
        /// getsubroty(chr->model) result.
        /// </summary>
        public Single Subroty { get; set; } = 0;

        /// <summary>
        /// Canonical "damage", this is how much damage the chr has taken.
        /// </summary>
        public Single Damage
        {
            get => _damage;

            set
            {
                if (_damage != value)
                {
                    _damage = value;
                    OnPropertyChanged(nameof(Damage));
                    OnPropertyChanged(nameof(DamageText));
                    OnPropertyChanged(nameof(BodyArmorRemain));
                    OnPropertyChanged(nameof(BodyArmorRemainText));
                    OnPropertyChanged(nameof(HpRemain));
                    OnPropertyChanged(nameof(HpRemainText));
                }
            }
        }

        /// <summary>
        /// Format <see cref="Damage"/> as <see cref="string"/>.
        /// </summary>
        public string DamageText => Damage.ToString(Gebug64.Win.Config.Constants.DefaultDecimalFormat);

        /// <summary>
        /// Canonical "maxdamage", this is how much damage the chr can take before dying.
        /// </summary>
        public Single MaxDamage
        {
            get => _maxDamage;

            set
            {
                if (_maxDamage != value)
                {
                    _maxDamage = value;
                    OnPropertyChanged(nameof(MaxDamage));
                    OnPropertyChanged(nameof(MaxDamageText));
                }
            }
        }

        /// <summary>
        /// Format <see cref="MaxDamage"/> as <see cref="string"/>.
        /// </summary>
        public string MaxDamageText => MaxDamage.ToString(Gebug64.Win.Config.Constants.DefaultDecimalFormat);

        /// <summary>
        /// Canonical "shotbondsum", current intolerance.
        /// </summary>
        public Single Intolerance
        {
            get => _intolerance;

            set
            {
                if (_intolerance != value)
                {
                    _intolerance = value;
                    OnPropertyChanged(nameof(Intolerance));
                    OnPropertyChanged(nameof(IntoleranceText));
                }
            }
        }

        /// <summary>
        /// Format <see cref="Intolerance"/> as <see cref="string"/>.
        /// </summary>
        public string IntoleranceText => Intolerance.ToString(Gebug64.Win.Config.Constants.DefaultDecimalFormat);

        /// <summary>
        /// Remaining body armor.
        /// </summary>
        public double BodyArmorRemain => (Damage < 0) ? (double)(-1 * Damage) : 0;

        /// <summary>
        /// Format <see cref="BodyArmorRemain"/> as <see cref="string"/>.
        /// </summary>
        public string BodyArmorRemainText => BodyArmorRemain.ToString(Gebug64.Win.Config.Constants.DefaultDecimalFormat);

        /// <summary>
        /// Remaining hit points. Ignores bordy armor.
        /// </summary>
        public double HpRemain
        {
            get
            {
                if (Damage < 0)
                {
                    // If there is body armor ignore that.
                    return (double)MaxDamage;
                }
                else if (Damage < MaxDamage)
                {
                    // Need to make sure when Damage=0 that the full HP is returned.
                    return (double)(MaxDamage - Damage);
                }

                return 0;
            }
        }

        /// <summary>
        /// Format <see cref="HpRemain"/> as <see cref="string"/>.
        /// </summary>
        public string HpRemainText => HpRemain.ToString(Gebug64.Win.Config.Constants.DefaultDecimalFormat);

        /// <summary>
        /// Gets rotation of model in degrees.
        /// </summary>
        public double ModelRotationDegrees => Subroty * 180.0 / Math.PI;

        /// <summary>
        /// Remove all body armor and all but 0.01 HP from guard.
        /// </summary>
        public ICommand GhostHpCommand { get; set; }

        /// <summary>
        /// Gets a value indicating whether <see cref="GhostHpCommand"/> can execute.
        /// </summary>
        public bool CanGhostHpCommand
        {
            get
            {
                IConnectionServiceProvider? connectionServiceProvider = _connectionServiceProviderResolver.GetDeviceManager();
                if (object.ReferenceEquals(null, connectionServiceProvider))
                {
                    return false;
                }

                return !connectionServiceProvider.IsShutdown;
            }
        }

        /// <summary>
        /// Set guard HP to zero. This removes any current body armor.
        /// </summary>
        public ICommand MaxHpCommand { get; set; }

        /// <summary>
        /// Gets a value indicating whether <see cref="MaxHpCommand"/> can execute.
        /// </summary>
        public bool CanMaxHpCommand
        {
            get
            {
                IConnectionServiceProvider? connectionServiceProvider = _connectionServiceProviderResolver.GetDeviceManager();
                if (object.ReferenceEquals(null, connectionServiceProvider))
                {
                    return false;
                }

                return !connectionServiceProvider.IsShutdown;
            }
        }

        /// <summary>
        /// Untranslated animation (e.g., idle starts at 0x1c).
        /// </summary>
        public UInt32 Anim
        {
            get => _anim;

            set
            {
                if (_anim != value)
                {
                    _anim = value;
                    OnPropertyChanged(nameof(Anim));
                    OnPropertyChanged(nameof(AnimText));
                }
            }
        }

        /// <summary>
        /// Gets the name of the current <see cref="Anim"/>.
        /// </summary>
        public string AnimText => Getools.Lib.Game.Animation.AnimationTableData.AddressToAnim((int)_anim).Name;

        /// <summary>
        /// Bitmask of current character flags. <see cref="Getools.Lib.Game.Flags.ChrFlags"/>.
        /// </summary>
        public UInt32 ChrFlags
        {
            get => _chrFlags;

            set
            {
                if (_chrFlags != value)
                {
                    _chrFlags = value;
                    _chrFlagsText = string.Join(" | ", Getools.Lib.Formatters.FlagFormat.ResolveChrFlagFriendlyName(value));
                    OnPropertyChanged(nameof(ChrFlags));
                    OnPropertyChanged(nameof(ChrFlagsText));
                }
            }
        }

        /// <summary>
        /// Gets the friendly name of all flags set in <see cref="ChrFlags"/>.
        /// </summary>
        public string ChrFlagsText => _chrFlagsText;

        /// <summary>
        /// Bitmask of current prop flags. <see cref="Getools.Lib.Game.Flags.PropFlag"/>.
        /// </summary>
        public byte PropFlags
        {
            get => _propFlags;

            set
            {
                if (_propFlags != value)
                {
                    _propFlags = value;
                    _propFlagsText = string.Join(" | ", Getools.Lib.Formatters.FlagFormat.ResolvePropFlagFriendlyName(value));
                    OnPropertyChanged(nameof(PropFlags));
                    OnPropertyChanged(nameof(PropFlagsText));
                }
            }
        }

        /// <summary>
        /// Gets the friendly name of all flags set in <see cref="PropFlags"/>.
        /// </summary>
        public string PropFlagsText => _propFlagsText;

        /// <summary>
        /// Bitmask of current character flags2.
        /// </summary>
        public byte ChrFlags2
        {
            get => _chrFlags2;

            set
            {
                if (_chrFlags2 != value)
                {
                    _chrFlags2 = value;
                    OnPropertyChanged(nameof(ChrFlags2));
                    OnPropertyChanged(nameof(ChrFlags2Text));
                }
            }
        }

        /// <summary>
        /// String of <see cref="ChrFlags2"/>.
        /// </summary>
        public string ChrFlags2Text => "0x" + ChrFlags2.ToString("X2");

        /// <summary>
        /// Bitmask of current chr->hidden flags. <see cref="Getools.Lib.Game.Flags.ChrHidden"/>.
        /// </summary>
        public UInt16 ChrHidden
        {
            get => _chrHidden;

            set
            {
                if (_chrHidden != value)
                {
                    _chrHidden = value;
                    _chrHiddenText = string.Join(" | ", Getools.Lib.Formatters.FlagFormat.ResolveChrHiddenFriendlyName(value));
                    OnPropertyChanged(nameof(ChrHidden));
                    OnPropertyChanged(nameof(ChrHiddenText));
                }
            }
        }

        /// <summary>
        /// Gets the friendly name of all flags set in <see cref="ChrHidden"/>.
        /// </summary>
        public string ChrHiddenText => _chrHiddenText;

        /// <inheritdoc />
        public override int PreferredId
        {
            get
            {
                if (LayerInstanceId > -1)
                {
                    return LayerInstanceId;
                }

                return ChrSlotIndex;
            }
        }

        /// <summary>
        /// Copies non-id values.
        /// </summary>
        /// <param name="chr">Object to copy from.</param>
        public void UpdateFrom(Chr chr)
        {
            ChrSlotIndex = chr.ChrSlotIndex;
            ActionType = chr.ActionType;
            PropPos = chr.PropPos.Clone();
            TargetPos = chr.TargetPos.Clone();
            Subroty = chr.Subroty;
            Damage = chr.Damage;
            MaxDamage = chr.MaxDamage;
            Intolerance = chr.Intolerance;
            Anim = chr.Anim;
            ChrFlags = chr.ChrFlags;
            PropFlags = chr.PropFlags;
            ChrFlags2 = chr.ChrFlags2;
            ChrHidden = chr.ChrHidden;
        }

        /// <summary>
        /// Copies non-id values.
        /// </summary>
        /// <param name="chr">Object to copy from.</param>
        public void UpdateFrom(RmonGuardPosition chr)
        {
            ChrSlotIndex = chr.ChrSlotIndex;
            ActionType = chr.ActionType;
            PropPos = chr.PropPos.Clone();
            TargetPos = chr.TargetPos.Clone();
            Subroty = chr.Subroty;
            Damage = chr.Damage;
            MaxDamage = chr.MaxDamage;
            Intolerance = chr.Intolerance;
            Anim = chr.Anim;
            ChrFlags = chr.ChrFlags;
            PropFlags = chr.PropFlags;
            ChrFlags2 = chr.ChrFlags2;
            ChrHidden = chr.ChrHidden;
        }

        private void GhostHpCommandHandler()
        {
            IConnectionServiceProvider? connectionServiceProvider = _connectionServiceProviderResolver.GetDeviceManager();

            if (object.ReferenceEquals(null, connectionServiceProvider))
            {
                return;
            }

            var msg = new GebugChrGhostHpMessage()
            {
                ChrNum = (UInt16)_chrNum,
                ChrSlotIndex = (byte)_chrSlotIndex,
            };

            _logger.Log(LogLevel.Information, "Send: " + msg.ToString());

            connectionServiceProvider.SendMessage(msg);
        }

        private void MaxHpCommandHandler()
        {
            IConnectionServiceProvider? connectionServiceProvider = _connectionServiceProviderResolver.GetDeviceManager();

            if (object.ReferenceEquals(null, connectionServiceProvider))
            {
                return;
            }

            var msg = new GebugChrMaxHpMessage()
            {
                ChrNum = (UInt16)_chrNum,
                ChrSlotIndex = (byte)_chrSlotIndex,
            };

            _logger.Log(LogLevel.Information, "Send: " + msg.ToString());

            connectionServiceProvider.SendMessage(msg);
        }
    }
}
