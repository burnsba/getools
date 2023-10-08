using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Getools.Lib.Game.Enums
{
    /// <summary>
    /// Native enum PROP_ID.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1602:Enumeration items should be documented", Justification = "<Justification>")]
    public enum PropId
    {
        /// <summary>
        /// Beta Alarm / Default Multi Weapon.
        /// </summary>
        PROP_ALARM1,

        /// <summary>
        /// Alarm.
        /// </summary>
        PROP_ALARM2,

        /// <summary>
        /// White Pyramid (Explosion Bit).
        /// </summary>
        PROP_EXPLOSIONBIT,

        /// <summary>
        /// Ammo Crate (Brown w/ Edge Brace, 6x240 Black).
        /// </summary>
        PROP_AMMO_CRATE1,

        /// <summary>
        /// Ammo Crate (Brown w/ Center Brace, 12x8 Black).
        /// </summary>
        PROP_AMMO_CRATE2,

        /// <summary>
        /// Ammo Crate (Green w/ Center Brace, 12x8 Brown).
        /// </summary>
        PROP_AMMO_CRATE3,

        /// <summary>
        /// Ammo Crate (Green w/ Edge Brace, 6x8 White).
        /// </summary>
        PROP_AMMO_CRATE4,

        /// <summary>
        /// Ammo Crate (Green w/ Double Brace, 24x60 Black).
        /// </summary>
        PROP_AMMO_CRATE5,

        /// <summary>
        /// Rusted Trash Bin.
        /// </summary>
        PROP_BIN1,

        /// <summary>
        /// Desk Blotter.
        /// </summary>
        PROP_BLOTTER1,

        /// <summary>
        /// Red Book.
        /// </summary>
        PROP_BOOK1,

        /// <summary>
        /// Bookshelf.
        /// </summary>
        PROP_BOOKSHELF1,

        /// <summary>
        /// Bridge Console w/ Monitor, Navigation + Keyboard 1A.
        /// </summary>
        PROP_BRIDGE_CONSOLE1A,

        /// <summary>
        /// Bridge Console w/ Navigation 1B.
        /// </summary>
        PROP_BRIDGE_CONSOLE1B,

        /// <summary>
        /// Bridge Console w/ Navigation, Monitor + Keyboard 2A.
        /// </summary>
        PROP_BRIDGE_CONSOLE2A,

        /// <summary>
        /// Bridge Console w/ Various Controls 2B.
        /// </summary>
        PROP_BRIDGE_CONSOLE2B,

        /// <summary>
        /// Bridge Console w/ Monitor, Navigation + Keyboard 3A.
        /// </summary>
        PROP_BRIDGE_CONSOLE3A,

        /// <summary>
        /// Bridge Console w/ Monitor, Keyboard + Navigation 3B.
        /// </summary>
        PROP_BRIDGE_CONSOLE3B,

        /// <summary>
        /// Cardboard Box, Kapto|Enb.
        /// </summary>
        PROP_CARD_BOX1,

        /// <summary>
        /// Cardboard Box, Red Arrow, Bahko.
        /// </summary>
        PROP_CARD_BOX2,

        /// <summary>
        /// Cardboard Box, Scrawled Text, Bahah.
        /// </summary>
        PROP_CARD_BOX3,

        /// <summary>
        /// Cardboard Box, Three Seams.
        /// </summary>
        PROP_CARD_BOX4_LG,

        /// <summary>
        /// Cardboard Box, Two Seams, Bahah.
        /// </summary>
        PROP_CARD_BOX5_LG,

        /// <summary>
        /// Cardboard Box, Bahko.
        /// </summary>
        PROP_CARD_BOX6_LG,

        /// <summary>
        /// Surveillance Camera.
        /// </summary>
        PROP_CCTV,

        /// <summary>
        /// Double Screen Consoles w/ Keyboards.
        /// </summary>
        PROP_CONSOLE1,

        /// <summary>
        /// Double Screen Consoles w/ Left Keyboard.
        /// </summary>
        PROP_CONSOLE2,

        /// <summary>
        /// Double Screen Consoles w/ Right Keyboard.
        /// </summary>
        PROP_CONSOLE3,

        /// <summary>
        /// Console w/ Keyboard.
        /// </summary>
        PROP_CONSOLE_SEVA,

        /// <summary>
        /// Console w/ Monitor + Keyboard.
        /// </summary>
        PROP_CONSOLE_SEVB,

        /// <summary>
        /// Console w/ Switches.
        /// </summary>
        PROP_CONSOLE_SEVC,

        /// <summary>
        /// Console w/ Five Gauges.
        /// </summary>
        PROP_CONSOLE_SEVD,

        /// <summary>
        /// Console w/ Four Faders.
        /// </summary>
        PROP_CONSOLE_SEV2A,

        /// <summary>
        /// Console w/ Monitor, Keyboard + Switches.
        /// </summary>
        PROP_CONSOLE_SEV2B,

        /// <summary>
        /// Console w/ Three Gauges.
        /// </summary>
        PROP_CONSOLE_SEV2C,

        /// <summary>
        /// Console w/ Pressure Gauge.
        /// </summary>
        PROP_CONSOLE_SEV2D,

        /// <summary>
        /// Console w/ GoldenEye Key Slot.
        /// </summary>
        PROP_CONSOLE_SEV_GEA,

        /// <summary>
        /// Console w/ Faders + Pressure Gauge.
        /// </summary>
        PROP_CONSOLE_SEV_GEB,

        /// <summary>
        /// Desk w/ Kickplate.
        /// </summary>
        PROP_DESK1,

        /// <summary>
        /// Desk.
        /// </summary>
        PROP_DESK2,

        /// <summary>
        /// Desk Lamp.
        /// </summary>
        PROP_DESK_LAMP2,

        /// <summary>
        /// External Hard Drive.
        /// </summary>
        PROP_DISC_READER,

        /// <summary>
        /// Floppy Disc Drive.
        /// </summary>
        PROP_DISK_DRIVE1,

        /// <summary>
        /// Filing Cabinet.
        /// </summary>
        PROP_FILING_CABINET1,

        /// <summary>
        /// Jerrycan (Fuel Container).
        /// </summary>
        PROP_JERRY_CAN1,

        /// <summary>
        /// Computer Keyboard.
        /// </summary>
        PROP_KEYBOARD1,

        /// <summary>
        /// Kitchen Cabinets.
        /// </summary>
        PROP_KIT_UNITS1,

        /// <summary>
        /// Letter Tray.
        /// </summary>
        PROP_LETTER_TRAY1,

        /// <summary>
        /// Mainframe, Basic.
        /// </summary>
        PROP_MAINFRAME1,

        /// <summary>
        /// Mainframe, Advanced.
        /// </summary>
        PROP_MAINFRAME2,

        /// <summary>
        /// Chair (Metal).
        /// </summary>
        PROP_METAL_CHAIR1,

        /// <summary>
        /// Metal Crate, 6 Top Corner.
        /// </summary>
        PROP_METAL_CRATE1,

        /// <summary>
        /// Metal Crate, 6 Bottom Corner.
        /// </summary>
        PROP_METAL_CRATE2,

        /// <summary>
        /// Metal Crate, Toxic Materials.
        /// </summary>
        PROP_METAL_CRATE3,

        /// <summary>
        /// Metal Crate, Double Stripe - Class D1 Hazard.
        /// </summary>
        PROP_METAL_CRATE4,

        /// <summary>
        /// Naval Harpoon Missile in Containment Rack.
        /// </summary>
        PROP_MISSILE_RACK,

        /// <summary>
        /// Naval Harpoon Missiles in Containment Racks x4.
        /// </summary>
        PROP_MISSILE_RACK2,

        /// <summary>
        /// Oil Drum, Single Stripe, Ribbed.
        /// </summary>
        PROP_OIL_DRUM1,

        /// <summary>
        /// Oil Drum, Single Stripe, Ribbed - Class D1 Hazard.
        /// </summary>
        PROP_OIL_DRUM2,

        /// <summary>
        /// Oil Drum, Single Stripe, Ribbed - Toxic Materials.
        /// </summary>
        PROP_OIL_DRUM3,

        /// <summary>
        /// Oil Drum, Double Stripe - Toxic Materials.
        /// </summary>
        PROP_OIL_DRUM5,

        /// <summary>
        /// Oil Drum - Toxic Materials.
        /// </summary>
        PROP_OIL_DRUM6,

        /// <summary>
        /// Oil Drum, Double Dashes - Class D1 Hazard.
        /// </summary>
        PROP_OIL_DRUM7,

        /// <summary>
        /// Padlock.
        /// </summary>
        PROP_PADLOCK,

        /// <summary>
        /// Telephone.
        /// </summary>
        PROP_PHONE1,

        /// <summary>
        /// Radio Tuner w/ 1 Knob + 2 Gauges.
        /// </summary>
        PROP_RADIO_UNIT1,

        /// <summary>
        /// Radio Tuner w/ 1 Knob + 5 Gauges.
        /// </summary>
        PROP_RADIO_UNIT2,

        /// <summary>
        /// Radio Tuner w/ 3 Knobs + 5 Gauges.
        /// </summary>
        PROP_RADIO_UNIT3,

        /// <summary>
        /// Radio Tuner w/ 3 Knobs + 2 Gauges.
        /// </summary>
        PROP_RADIO_UNIT4,

        /// <summary>
        /// GoldenEye Satellite.
        /// </summary>
        PROP_SAT1_REFLECT,

        /// <summary>
        /// Satellite Dish (Arkangelsk).
        /// </summary>
        PROP_SATDISH,

        /// <summary>
        /// Uplink Box.
        /// </summary>
        PROP_SATBOX,

        /// <summary>
        /// Wooden Stool.
        /// </summary>
        PROP_STOOL1,

        /// <summary>
        /// Swivel Chair.
        /// </summary>
        PROP_SWIVEL_CHAIR1,

        /// <summary>
        /// Naval Torpedo Rack x3.
        /// </summary>
        PROP_TORPEDO_RACK,

        /// <summary>
        /// Television Monitor.
        /// </summary>
        PROP_TV1,

        /// <summary>
        /// Hanging Monitor Rack.
        /// </summary>
        PROP_TV_HOLDER,

        /// <summary>
        /// Wall Monitor Screen.
        /// </summary>
        PROP_TVSCREEN,

        /// <summary>
        /// Wall Monitor Screens, 4-in-1.
        /// </summary>
        PROP_TV4SCREEN,

        /// <summary>
        /// Wooden Crate w/ #4 Label, Bahah.
        /// </summary>
        PROP_WOOD_LG_CRATE1,

        /// <summary>
        /// Wooden Crate, Darker Shading, Kapto|Enb.
        /// </summary>
        PROP_WOOD_LG_CRATE2,

        /// <summary>
        /// Wooden Crates x8, Bahko.
        /// </summary>
        PROP_WOOD_MD_CRATE3,

        /// <summary>
        /// Wooden Crate w/ #2 Label, Bahko.
        /// </summary>
        PROP_WOOD_SM_CRATE4,

        /// <summary>
        /// Wooden Crate w/ #4 Label, Darker Shading, Bahah.
        /// </summary>
        PROP_WOOD_SM_CRATE5,

        /// <summary>
        /// Wooden Crate w/ UP Arrow, Kapto|Enb.
        /// </summary>
        PROP_WOOD_SM_CRATE6,

        /// <summary>
        /// Wooden Table.
        /// </summary>
        PROP_WOODEN_TABLE1,

        /// <summary>
        /// Keycard.
        /// </summary>
        PROP_SWIPE_CARD2,

        /// <summary>
        /// Blue and Gold Printed Circuit Cube (Borg Crate).
        /// </summary>
        PROP_BORG_CRATE,

        /// <summary>
        /// Metal Crate Stack, 4x4.
        /// </summary>
        PROP_BOXES4X4,

        /// <summary>
        /// Metal Crate Stack, 3x4.
        /// </summary>
        PROP_BOXES3X4,

        /// <summary>
        /// Metal Crate Stack, 2x4.
        /// </summary>
        PROP_BOXES2X4,

        /// <summary>
        /// Security Card Panel.
        /// </summary>
        PROP_SEC_PANEL,

        /// <summary>
        /// Silo Missile (ICBM), Nose Cone Only.
        /// </summary>
        PROP_ICBM_NOSE,

        /// <summary>
        /// Silo Missile (ICBM).
        /// </summary>
        PROP_ICBM,

        /// <summary>
        /// Dual Consoles on Castors.
        /// </summary>
        PROP_TUNING_CONSOLE1,

        /// <summary>
        /// Computer Work Desk.
        /// </summary>
        PROP_DESK_ARECIBO1,

        /// <summary>
        /// Lockers, Single Venting.
        /// </summary>
        PROP_LOCKER3,

        /// <summary>
        /// Lockers, Double Venting.
        /// </summary>
        PROP_LOCKER4,

        /// <summary>
        /// Ceiling Mounted Drone Gun.
        /// </summary>
        PROP_ROOFGUN,

        /// <summary>
        /// Frigate Engine.
        /// </summary>
        PROP_DEST_ENGINE,

        /// <summary>
        /// Naval MK 29 Missile Launcher (Exocet).
        /// </summary>
        PROP_DEST_EXOCET,

        /// <summary>
        /// Naval 100 mm Gun Turret (TR 100).
        /// </summary>
        PROP_DEST_GUN,

        /// <summary>
        /// Naval MK 141 Launch Canisters (Harpoon).
        /// </summary>
        PROP_DEST_HARPOON,

        /// <summary>
        /// Naval MK 26 Dual Missile Launcher (Seawolf).
        /// </summary>
        PROP_DEST_SEAWOLF,

        /// <summary>
        /// Window Glass.
        /// </summary>
        PROP_WINDOW,

        /// <summary>
        /// Window Glass, Lattice Frame, 4x10 (single-sided).
        /// </summary>
        PROP_WINDOW_LIB_LG1,

        /// <summary>
        /// Window Glass, Lattice Frame, 4x3 (double-sided).
        /// </summary>
        PROP_WINDOW_LIB_SM1,

        /// <summary>
        /// Window Glass, Lattice Frame, 4x4 (single-sided).
        /// </summary>
        PROP_WINDOW_COR11,

        /// <summary>
        /// Jungle Large Tree.
        /// </summary>
        PROP_JUNGLE3_TREE,

        /// <summary>
        /// Jungle Palm Tree.
        /// </summary>
        PROP_PALM,

        /// <summary>
        /// Jungle Palm Tree, Resprouting After Loss of Fronds.
        /// </summary>
        PROP_PALMTREE,

        /// <summary>
        /// Jungle Plant, Low Shrub.
        /// </summary>
        PROP_PLANT2B,

        /// <summary>
        /// Laboratory Table w/ Sink Drains.
        /// </summary>
        PROP_LABBENCH,

        /// <summary>
        /// White Bin.
        /// </summary>
        PROP_GASBARREL,

        /// <summary>
        /// White Bins x4.
        /// </summary>
        PROP_GASBARRELS,

        /// <summary>
        /// Body Armor.
        /// </summary>
        PROP_BODYARMOUR,

        /// <summary>
        /// Body Armor (Vest).
        /// </summary>
        PROP_BODYARMOURVEST,

        /// <summary>
        /// Bottling Tank.
        /// </summary>
        PROP_GASTANK,

        /// <summary>
        /// Glass Cup.
        /// </summary>
        PROP_GLASSWARE1,

        /// <summary>
        /// Metallic Securing Strip (Hatch Bolt).
        /// </summary>
        PROP_HATCHBOLT,

        /// <summary>
        /// Train Brake Controller.
        /// </summary>
        PROP_BRAKEUNIT,

        /// <summary>
        /// Gun Magazine (KF7 Soviet).
        /// </summary>
        PROP_AK47MAG,

        /// <summary>
        /// Gun Magazine (AR33 Assault Rifle).
        /// </summary>
        PROP_M16MAG,

        /// <summary>
        /// Gun Magazine (D5K Deutsche).
        /// </summary>
        PROP_MP5KMAG,

        /// <summary>
        /// Gun Magazine (Klobb).
        /// </summary>
        PROP_SKORPIONMAG,

        /// <summary>
        /// Gun Magazine (Phantom).
        /// </summary>
        PROP_SPECTREMAG,

        /// <summary>
        /// Gun Magazine (ZMG (9mm)).
        /// </summary>
        PROP_UZIMAG,

        /// <summary>
        /// Silencer.
        /// </summary>
        PROP_SILENCER,

        /// <summary>
        /// Fire Extinguisher.
        /// </summary>
        PROP_CHREXTINGUISHER,

        /// <summary>
        /// Box of Shells (Shotgun Cartridges).
        /// </summary>
        PROP_BOXCARTRIDGES,

        /// <summary>
        /// Gun Magazine (RC-P90).
        /// </summary>
        PROP_FNP90MAG,

        /// <summary>
        /// Box of Shells (Golden Gun Bullets).
        /// </summary>
        PROP_GOLDENSHELLS,

        /// <summary>
        /// Box of Shells (Magnum Rounds).
        /// </summary>
        PROP_MAGNUMSHELLS,

        /// <summary>
        /// Gun Magazine (PP7).
        /// </summary>
        PROP_WPPKMAG,

        /// <summary>
        /// Gun Magazine (DD44 Dostovei).
        /// </summary>
        PROP_TT33MAG,

        /// <summary>
        /// Grey Containment Door w/ Caution Stripes and Window.
        /// </summary>
        PROP_SEV_DOOR,

        /// <summary>
        /// Grey Electronic Door w/ LEFT Arrow.
        /// </summary>
        PROP_SEV_DOOR3,

        /// <summary>
        /// BETA Electronic Door w/ LEFT Arrow and Fake Window.
        /// </summary>
        PROP_SEV_DOOR3_WIND,

        /// <summary>
        /// Grey Electronic Door w/ LEFT Arrow and Window.
        /// </summary>
        PROP_SEV_DOOR4_WIND,

        /// <summary>
        /// Glass Door w/ Stone Frame.
        /// </summary>
        PROP_SEV_TRISLIDE,

        /// <summary>
        /// Grey Electronic Door w/ UP Arrow.
        /// </summary>
        PROP_SEV_DOOR_V1,

        /// <summary>
        /// Silver Corrugated Door w/ Caution Stripes.
        /// </summary>
        PROP_STEEL_DOOR1,

        /// <summary>
        /// Rusty Door w/ Handle.
        /// </summary>
        PROP_STEEL_DOOR2,

        /// <summary>
        /// Double Cross Brace Door.
        /// </summary>
        PROP_STEEL_DOOR3,

        /// <summary>
        /// Elevator Door.
        /// </summary>
        PROP_SILO_LIFT_DOOR,

        /// <summary>
        /// Rusty Door w/o Handle.
        /// </summary>
        PROP_STEEL_DOOR2B,

        /// <summary>
        /// Blue Bay Door w/ Caution Stripes.
        /// </summary>
        PROP_DOOR_ROLLER1,

        /// <summary>
        /// Blue Bay Door w/ Venting and Caution Stripes.
        /// </summary>
        PROP_DOOR_ROLLER2,

        /// <summary>
        /// Blue Bay Door w/ Venting and Caution Stripes.
        /// </summary>
        PROP_DOOR_ROLLER3,

        /// <summary>
        /// Cargo Bay Door w/ UP Arrow and Transportation Stripes.
        /// </summary>
        PROP_DOOR_ROLLER4,

        /// <summary>
        /// Blue Corrugated Door w/ Transportation Stripes.
        /// </summary>
        PROP_DOOR_ST_AREC1,

        /// <summary>
        /// Blue Reversed Corrugated Door w/ Transportation Stripes.
        /// </summary>
        PROP_DOOR_ST_AREC2,

        /// <summary>
        /// Grey Frigate Door w/ Indents and Caution Stripes.
        /// </summary>
        PROP_DOOR_DEST1,

        /// <summary>
        /// Grey Frigate Door w/ Indents, Caution Stripes and KEEP CLEAR Label.
        /// </summary>
        PROP_DOOR_DEST2,

        /// <summary>
        /// Grey Swinging Door w/ Blue Stripe.
        /// </summary>
        PROP_GAS_PLANT_SW_DO1,

        /// <summary>
        /// Grey Swinging Door, Darker.
        /// </summary>
        PROP_GAS_PLANT_SW2_DO1,

        /// <summary>
        /// Grey Swinging Door, Lighter.
        /// </summary>
        PROP_GAS_PLANT_SW3_DO1,

        /// <summary>
        /// Light Wooden Door (Looks Like Sand).
        /// </summary>
        PROP_GAS_PLANT_SW4_DO1,

        /// <summary>
        /// Brown Electronic Door.
        /// </summary>
        PROP_GAS_PLANT_MET1_DO1,

        /// <summary>
        /// Bathroom Stall Door.
        /// </summary>
        PROP_GAS_PLANT_WC_CUB1,

        /// <summary>
        /// Laboratory Glass Door.
        /// </summary>
        PROP_GASPLANT_CLEAR_DOOR,

        /// <summary>
        /// Dark Wooden Door.
        /// </summary>
        PROP_TRAIN_DOOR,

        /// <summary>
        /// Dark Wooden Door w/ Window.
        /// </summary>
        PROP_TRAIN_DOOR2,

        /// <summary>
        /// Dark Wooden Door w/ Window + Shutter.
        /// </summary>
        PROP_TRAIN_DOOR3,

        /// <summary>
        /// Eyelid Door.
        /// </summary>
        PROP_DOOR_EYELID,

        /// <summary>
        /// Iris Door.
        /// </summary>
        PROP_DOOR_IRIS,

        /// <summary>
        /// Cabin Door.
        /// </summary>
        PROP_SEVDOORWOOD,

        /// <summary>
        /// Weathered Swinging Door w/ Window.
        /// </summary>
        PROP_SEVDOORWIND,

        /// <summary>
        /// Weathered Swinging Door.
        /// </summary>
        PROP_SEVDOORNOWIND,

        /// <summary>
        /// Brown Corrugated Electronic Door.
        /// </summary>
        PROP_SEVDOORMETSLIDE,

        /// <summary>
        /// Stone Door w/ Prints (Set A).
        /// </summary>
        PROP_CRYPTDOOR1A,

        /// <summary>
        /// Sand Door w/ Damage (Set A).
        /// </summary>
        PROP_CRYPTDOOR1B,

        /// <summary>
        /// Stone Door w/ Prints, Darker (Set B).
        /// </summary>
        PROP_CRYPTDOOR2A,

        /// <summary>
        /// Sand Door w/ Damage, Darker (Set B).
        /// </summary>
        PROP_CRYPTDOOR2B,

        /// <summary>
        /// Egyptian Moving Wall.
        /// </summary>
        PROP_CRYPTDOOR3,

        /// <summary>
        /// Brown Sand Door (Temple).
        /// </summary>
        PROP_CRYPTDOOR4,

        /// <summary>
        /// Blast Door (Control).
        /// </summary>
        PROP_VERTDOOR,

        /// <summary>
        /// Train Floor Hatch.
        /// </summary>
        PROP_HATCHDOOR,

        /// <summary>
        /// Security Gate (Dam).
        /// </summary>
        PROP_DAMGATEDOOR,

        /// <summary>
        /// Tunnel Flood Door (Dam).
        /// </summary>
        PROP_DAMTUNDOOR,

        /// <summary>
        /// Mesh Gate.
        /// </summary>
        PROP_DAMCHAINDOOR,

        /// <summary>
        /// Launch Tube Ceiling Shutter (Silo).
        /// </summary>
        PROP_SILOTOPDOOR,

        /// <summary>
        /// Cell Door.
        /// </summary>
        PROP_DOORPRISON1,

        /// <summary>
        /// Park Gate.
        /// </summary>
        PROP_DOORSTATGATE,

        /// <summary>
        /// KF7 Soviet.
        /// </summary>
        PROP_CHRKALASH,

        /// <summary>
        /// Grenade Launcher.
        /// </summary>
        PROP_CHRGRENADELAUNCH,

        /// <summary>
        /// Hunting Knife.
        /// </summary>
        PROP_CHRKNIFE,

        /// <summary>
        /// Moonraker Laser.
        /// </summary>
        PROP_CHRLASER,

        /// <summary>
        /// AR33 Assault Rifle.
        /// </summary>
        PROP_CHRM16,

        /// <summary>
        /// D5K Deutsche.
        /// </summary>
        PROP_CHRMP5K,

        /// <summary>
        /// Cougar Magnum.
        /// </summary>
        PROP_CHRRUGER,

        /// <summary>
        /// PP7 Special Issue.
        /// </summary>
        PROP_CHRWPPK,

        /// <summary>
        /// Shotgun.
        /// </summary>
        PROP_CHRSHOTGUN,

        /// <summary>
        /// Klobb.
        /// </summary>
        PROP_CHRSKORPION,

        /// <summary>
        /// Phantom.
        /// </summary>
        PROP_CHRSPECTRE,

        /// <summary>
        /// ZMG (9mm).
        /// </summary>
        PROP_CHRUZI,

        /// <summary>
        /// Hand Grenade.
        /// </summary>
        PROP_CHRGRENADE,

        /// <summary>
        /// RC-P90.
        /// </summary>
        PROP_CHRFNP90,

        /// <summary>
        /// Briefcase.
        /// </summary>
        PROP_CHRBRIEFCASE,

        /// <summary>
        /// Remote Mine.
        /// </summary>
        PROP_CHRREMOTEMINE,

        /// <summary>
        /// Proximity Mine.
        /// </summary>
        PROP_CHRPROXIMITYMINE,

        /// <summary>
        /// Timed Mine.
        /// </summary>
        PROP_CHRTIMEDMINE,

        /// <summary>
        /// Rocket.
        /// </summary>
        PROP_CHRROCKET,

        /// <summary>
        /// Grenade Round.
        /// </summary>
        PROP_CHRGRENADEROUND,

        /// <summary>
        /// PP7 (Silenced).
        /// </summary>
        PROP_CHRWPPKSIL,

        /// <summary>
        /// DD44 Dostovei.
        /// </summary>
        PROP_CHRTT33,

        /// <summary>
        /// D5K (Silenced).
        /// </summary>
        PROP_CHRMP5KSIL,

        /// <summary>
        /// Automatic Shotgun.
        /// </summary>
        PROP_CHRAUTOSHOT,

        /// <summary>
        /// Golden Gun.
        /// </summary>
        PROP_CHRGOLDEN,

        /// <summary>
        /// Throwing Knife.
        /// </summary>
        PROP_CHRTHROWKNIFE,

        /// <summary>
        /// Sniper Rifle.
        /// </summary>
        PROP_CHRSNIPERRIFLE,

        /// <summary>
        /// Rocket Launcher.
        /// </summary>
        PROP_CHRROCKETLAUNCH,

        /// <summary>
        /// Fur Hat, Blue.
        /// </summary>
        PROP_HATFURRY,

        /// <summary>
        /// Fur Hat, Brown.
        /// </summary>
        PROP_HATFURRYBROWN,

        /// <summary>
        /// Fur Hat, Black.
        /// </summary>
        PROP_HATFURRYBLACK,

        /// <summary>
        /// Side Cap, Light Green.
        /// </summary>
        PROP_HATTBIRD,

        /// <summary>
        /// Side Cap, Dark Green.
        /// </summary>
        PROP_HATTBIRDBROWN,

        /// <summary>
        /// Combat Helmet, Green.
        /// </summary>
        PROP_HATHELMET,

        /// <summary>
        /// Combat Helmet, Grey.
        /// </summary>
        PROP_HATHELMETGREY,

        /// <summary>
        /// Elite Headgear.
        /// </summary>
        PROP_HATMOON,

        /// <summary>
        /// Special Forces Beret, Black.
        /// </summary>
        PROP_HATBERET,

        /// <summary>
        /// Special Forces Beret, Navy.
        /// </summary>
        PROP_HATBERETBLUE,

        /// <summary>
        /// Special Forces Beret, Burgundy.
        /// </summary>
        PROP_HATBERETRED,

        /// <summary>
        /// Officer's Peaked Visor Cap.
        /// </summary>
        PROP_HATPEAKED,

        /// <summary>
        /// Pchrwristdart (BETA).
        /// </summary>
        PROP_CHRWRISTDART,

        /// <summary>
        /// Pchrexplosivepen (BETA).
        /// </summary>
        PROP_CHREXPLOSIVEPEN,

        /// <summary>
        /// Bomb Case (Briefcase Laying Down).
        /// </summary>
        PROP_CHRBOMBCASE,

        /// <summary>
        /// Pchrflarepistol (BETA Pickup).
        /// </summary>
        PROP_CHRFLAREPISTOL,

        /// <summary>
        /// Pchrpitongun (BETA Pickup).
        /// </summary>
        PROP_CHRPITONGUN,

        /// <summary>
        /// Pchrfingergun (BETA Pickup).
        /// </summary>
        PROP_CHRFINGERGUN,

        /// <summary>
        /// Pchrsilverwppk (BETA Pickup).
        /// </summary>
        PROP_CHRSILVERWPPK,

        /// <summary>
        /// Pchrgoldwppk (BETA Pickup).
        /// </summary>
        PROP_CHRGOLDWPPK,

        /// <summary>
        /// Pchrdynamite (BETA Pickup).
        /// </summary>
        PROP_CHRDYNAMITE,

        /// <summary>
        /// Pchrbungee (BETA Pickup).
        /// </summary>
        PROP_CHRBUNGEE,

        /// <summary>
        /// Door Decoder.
        /// </summary>
        PROP_CHRDOORDECODER,

        /// <summary>
        /// Bomb Defuser.
        /// </summary>
        PROP_CHRBOMBDEFUSER,

        /// <summary>
        /// Pchrbugdetector (BETA Pickup).
        /// </summary>
        PROP_CHRBUGDETECTOR,

        /// <summary>
        /// Safe Cracker Case (Briefcase Laying Down).
        /// </summary>
        PROP_CHRSAFECRACKERCASE,

        /// <summary>
        /// Photo Camera (007).
        /// </summary>
        PROP_CHRCAMERA,

        /// <summary>
        /// Pchrlockexploder (BETA Pickup).
        /// </summary>
        PROP_CHRLOCKEXPLODER,

        /// <summary>
        /// Pchrdoorexploder (BETA Pickup).
        /// </summary>
        PROP_CHRDOOREXPLODER,

        /// <summary>
        /// Key Analyzer Case (Briefcase Laying Down).
        /// </summary>
        PROP_CHRKEYANALYSERCASE,

        /// <summary>
        /// Weapon Case (Briefcase Standing Up).
        /// </summary>
        PROP_CHRWEAPONCASE,

        /// <summary>
        /// Yale Key.
        /// </summary>
        PROP_CHRKEYYALE,

        /// <summary>
        /// Bolt Key.
        /// </summary>
        PROP_CHRKEYBOLT,

        /// <summary>
        /// Covert Modem / Tracker Bug.
        /// </summary>
        PROP_CHRBUG,

        /// <summary>
        /// Micro Camera.
        /// </summary>
        PROP_CHRMICROCAMERA,

        /// <summary>
        /// Floppy Disc.
        /// </summary>
        PROP_FLOPPY,

        /// <summary>
        /// GoldenEye Key.
        /// </summary>
        PROP_CHRGOLDENEYEKEY,

        /// <summary>
        /// Polarized Glasses.
        /// </summary>
        PROP_CHRPOLARIZEDGLASSES,

        /// <summary>
        /// Pchrcreditcard (BETA Pickup).
        /// </summary>
        PROP_CHRCREDITCARD,

        /// <summary>
        /// Pchrdarkglasses (BETA Pickup).
        /// </summary>
        PROP_CHRDARKGLASSES,

        /// <summary>
        /// Gas Keyring.
        /// </summary>
        PROP_CHRGASKEYRING,

        /// <summary>
        /// Datathief.
        /// </summary>
        PROP_CHRDATATHIEF,

        /// <summary>
        /// Safe Body.
        /// </summary>
        PROP_SAFE,

        /// <summary>
        /// Pbomb (BETA Pickup).
        /// </summary>
        PROP_BOMB,

        /// <summary>
        /// Plans (Briefing Folder).
        /// </summary>
        PROP_CHRPLANS,

        /// <summary>
        /// Pchrspyfile (BETA Pickup).
        /// </summary>
        PROP_CHRSPYFILE,

        /// <summary>
        /// Pirate Blueprints.
        /// </summary>
        PROP_CHRBLUEPRINTS,

        /// <summary>
        /// Circuitboard.
        /// </summary>
        PROP_CHRCIRCUITBOARD,

        /// <summary>
        /// Bunker Expansion Plans.
        /// </summary>
        PROP_CHRMAP,

        /// <summary>
        /// Pchrspooltape (BETA Pickup).
        /// </summary>
        PROP_CHRSPOOLTAPE,

        /// <summary>
        /// Audiotape.
        /// </summary>
        PROP_CHRAUDIOTAPE,

        /// <summary>
        /// Pchrmicrofilm (BETA Pickup).
        /// </summary>
        PROP_CHRMICROFILM,

        /// <summary>
        /// Pchrmicrocode (BETA Pickup).
        /// </summary>
        PROP_CHRMICROCODE,

        /// <summary>
        /// Pchrlectre (BETA Pickup).
        /// </summary>
        PROP_CHRLECTRE,

        /// <summary>
        /// Pchrmoney (BETA Pickup).
        /// </summary>
        PROP_CHRMONEY,

        /// <summary>
        /// Pchrgoldbar (BETA Pickup).
        /// </summary>
        PROP_CHRGOLDBAR,

        /// <summary>
        /// Pchrheroin (BETA Pickup).
        /// </summary>
        PROP_CHRHEROIN,

        /// <summary>
        /// Clipboard.
        /// </summary>
        PROP_CHRCLIPBOARD,

        /// <summary>
        /// Red Dossier.
        /// </summary>
        PROP_CHRDOSSIERRED,

        /// <summary>
        /// Staff List.
        /// </summary>
        PROP_CHRSTAFFLIST,

        /// <summary>
        /// DAT.
        /// </summary>
        PROP_CHRDATTAPE,

        /// <summary>
        /// Plastique.
        /// </summary>
        PROP_CHRPLASTIQUE,

        /// <summary>
        /// Black Box (Orange Flight Recorder).
        /// </summary>
        PROP_CHRBLACKBOX,

        /// <summary>
        /// CCTV Tape (GoldenEye VHS).
        /// </summary>
        PROP_CHRVIDEOTAPE,

        /// <summary>
        /// Nintendo Logo.
        /// </summary>
        PROP_NINTENDOLOGO,

        /// <summary>
        /// GoldenEye Logo.
        /// </summary>
        PROP_GOLDENEYELOGO,

        /// <summary>
        /// Classified Folder w/ Royal Crest (Folder Menus).
        /// </summary>
        PROP_WALLETBOND,

        /// <summary>
        /// Supply Truck.
        /// </summary>
        PROP_MILTRUCK,

        /// <summary>
        /// Military Jeep.
        /// </summary>
        PROP_JEEP,

        /// <summary>
        /// Red Prime Mover.
        /// </summary>
        PROP_ARTIC,

        /// <summary>
        /// Transport Helicopter w/ Natalya.
        /// </summary>
        PROP_HELICOPTER,

        /// <summary>
        /// Pirate Euro Chopper.
        /// </summary>
        PROP_TIGER,

        /// <summary>
        /// Hound Helicopter.
        /// </summary>
        PROP_MILCOPTER,

        /// <summary>
        /// Soviet Camouflage Chopper.
        /// </summary>
        PROP_HIND,

        /// <summary>
        /// Black Trailer.
        /// </summary>
        PROP_ARTICTRAILER,

        /// <summary>
        /// Motorbike.
        /// </summary>
        PROP_MOTORBIKE,

        /// <summary>
        /// Tank.
        /// </summary>
        PROP_TANK,

        /// <summary>
        /// Armored Personnel Carrier.
        /// </summary>
        PROP_APC,

        /// <summary>
        /// Speedboat.
        /// </summary>
        PROP_SPEEDBOAT,

        /// <summary>
        /// Aeroplane.
        /// </summary>
        PROP_PLANE,

        /// <summary>
        /// Heavy Gun Emplacement.
        /// </summary>
        PROP_GUN_RUNWAY1,

        /// <summary>
        /// Safe Door.
        /// </summary>
        PROP_SAFEDOOR,

        /// <summary>
        /// Key Rack.
        /// </summary>
        PROP_KEY_HOLDER,

        /// <summary>
        /// Grating (Ventshaft Hatch).
        /// </summary>
        PROP_HATCHSEVX,

        /// <summary>
        /// Satellite Dish (Severnaya).
        /// </summary>
        PROP_SEVDISH,

        /// <summary>
        /// Archives Moving Wall (Dark).
        /// </summary>
        PROP_ARCHSECDOOR1,

        /// <summary>
        /// Archives Moving Wall (Light).
        /// </summary>
        PROP_ARCHSECDOOR2,

        /// <summary>
        /// Free Standing Drone Gun.
        /// </summary>
        PROP_GROUNDGUN,

        /// <summary>
        /// Train Exterior Door.
        /// </summary>
        PROP_TRAINEXTDOOR,

        /// <summary>
        /// White Car #1 (BMW).
        /// </summary>
        PROP_CARBMW,

        /// <summary>
        /// White Car #2 (Escort).
        /// </summary>
        PROP_CARESCORT,

        /// <summary>
        /// White Car #3 (Golf).
        /// </summary>
        PROP_CARGOLF,

        /// <summary>
        /// Red Car (Cadillac).
        /// </summary>
        PROP_CARWEIRD,

        /// <summary>
        /// Ourumov's Car (ZIL).
        /// </summary>
        PROP_CARZIL,

        /// <summary>
        /// Exhaust Bay Doors, Left Side.
        /// </summary>
        PROP_SHUTTLE_DOOR_L,

        /// <summary>
        /// Exhaust Bay Doors, Right Side.
        /// </summary>
        PROP_SHUTTLE_DOOR_R,

        /// <summary>
        /// Metallic Gate w/ Red Star.
        /// </summary>
        PROP_DEPOT_GATE_ENTRY,

        /// <summary>
        /// Rusty Door w/ Handle (Lo-Res).
        /// </summary>
        PROP_DEPOT_DOOR_STEEL,

        /// <summary>
        /// Beaker w/ Blue Topper.
        /// </summary>
        PROP_GLASSWARE2,

        /// <summary>
        /// Erlenmeyer Flask.
        /// </summary>
        PROP_GLASSWARE3,

        /// <summary>
        /// Set of Five Beakers.
        /// </summary>
        PROP_GLASSWARE4,

        /// <summary>
        /// Land Mine.
        /// </summary>
        PROP_LANDMINE,

        /// <summary>
        /// Jungle Plant, Withered and Dying.
        /// </summary>
        PROP_PLANT1,

        /// <summary>
        /// Jungle Plant, Turning Colour.
        /// </summary>
        PROP_PLANT11,

        /// <summary>
        /// Jungle Plant, Healthy and Thick.
        /// </summary>
        PROP_PLANT2,

        /// <summary>
        /// Jungle Plant, Tall Leaves.
        /// </summary>
        PROP_PLANT3,

        /// <summary>
        /// Jungle Tree, Moss Covered.
        /// </summary>
        PROP_JUNGLE5_TREE,

        /// <summary>
        /// GoldenEye Certification Screen.
        /// </summary>
        PROP_LEGALPAGE,

        /// <summary>
        /// Roads and Buildings #1 (stretch of road).
        /// </summary>
        PROP_ST_PETE_ROOM_1I,

        /// <summary>
        /// Roads and Buildings #2 (stretch of road).
        /// </summary>
        PROP_ST_PETE_ROOM_2I,

        /// <summary>
        /// Roads and Buildings #3 (intersection).
        /// </summary>
        PROP_ST_PETE_ROOM_3T,

        /// <summary>
        /// Roads and Buildings #4 (street corner).
        /// </summary>
        PROP_ST_PETE_ROOM_5C,

        /// <summary>
        /// Roads and Buildings #5 (street corner).
        /// </summary>
        PROP_ST_PETE_ROOM_6C,

        /// <summary>
        /// Roller Door.
        /// </summary>
        PROP_DOOR_ROLLERTRAIN,

        /// <summary>
        /// Glass Sliding Door (Aztec).
        /// </summary>
        PROP_DOOR_WIN,

        /// <summary>
        /// Stone Sliding Door (Aztec).
        /// </summary>
        PROP_DOOR_AZTEC,

        /// <summary>
        /// Moonraker Shuttle.
        /// </summary>
        PROP_SHUTTLE,

        /// <summary>
        /// Boardroom Table (Aztec Exhaust Bay).
        /// </summary>
        PROP_DOOR_AZT_DESK,

        /// <summary>
        /// Boardroom Table Extension (Aztec Exhaust Bay).
        /// </summary>
        PROP_DOOR_AZT_DESK_TOP,

        /// <summary>
        /// Boardroom Chair (Aztec Exhaust Bay).
        /// </summary>
        PROP_DOOR_AZT_CHAIR,

        /// <summary>
        /// Mainframe Door.
        /// </summary>
        PROP_DOOR_MF,

        /// <summary>
        /// Flag Tag Token.
        /// </summary>
        PROP_FLAG,

        /// <summary>
        /// Road Barricade.
        /// </summary>
        PROP_BARRICADE,

        /// <summary>
        /// Covert Modem Connection Screen.
        /// </summary>
        PROP_MODEMBOX,

        /// <summary>
        /// Sliding Door Activation Switch.
        /// </summary>
        PROP_DOORPANEL,

        /// <summary>
        /// Console w/ Activation Light.
        /// </summary>
        PROP_DOORCONSOLE,

        /// <summary>
        /// Glass Test Tube.
        /// </summary>
        PROP_CHRTESTTUBE,

        /// <summary>
        /// Bollard.
        /// </summary>
        PROP_BOLLARD,

        PROP_MAX,
    }
}
