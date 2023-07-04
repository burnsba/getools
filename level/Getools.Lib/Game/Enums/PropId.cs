using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Getools.Lib.Game.Enums
{
    public enum PropId
    {
        PROP_ALARM1,              /* Beta Alarm / Default Multi Weapon                                  */
        PROP_ALARM2,              /* Alarm                                                              */
        PROP_EXPLOSIONBIT,        /* White Pyramid (Explosion Bit)                                      */
        PROP_AMMO_CRATE1,         /* Ammo Crate (Brown w/ Edge Brace, 6x240 Black)                      */
        PROP_AMMO_CRATE2,         /* Ammo Crate (Brown w/ Center Brace, 12x8 Black)                     */
        PROP_AMMO_CRATE3,         /* Ammo Crate (Green w/ Center Brace, 12x8 Brown)                     */
        PROP_AMMO_CRATE4,         /* Ammo Crate (Green w/ Edge Brace, 6x8 White)                        */
        PROP_AMMO_CRATE5,         /* Ammo Crate (Green w/ Double Brace, 24x60 Black)                    */
        PROP_BIN1,                /* Rusted Trash Bin                                                   */
        PROP_BLOTTER1,            /* Desk Blotter                                                       */
        PROP_BOOK1,               /* Red Book                                                           */
        PROP_BOOKSHELF1,          /* Bookshelf                                                          */
        PROP_BRIDGE_CONSOLE1A,    /* Bridge Console w/ Monitor, Navigation + Keyboard 1A                */
        PROP_BRIDGE_CONSOLE1B,    /* Bridge Console w/ Navigation 1B                                    */
        PROP_BRIDGE_CONSOLE2A,    /* Bridge Console w/ Navigation, Monitor + Keyboard 2A                */
        PROP_BRIDGE_CONSOLE2B,    /* Bridge Console w/ Various Controls 2B                              */
        PROP_BRIDGE_CONSOLE3A,    /* Bridge Console w/ Monitor, Navigation + Keyboard 3A                */
        PROP_BRIDGE_CONSOLE3B,    /* Bridge Console w/ Monitor, Keyboard + Navigation 3B                */
        PROP_CARD_BOX1,           /* Cardboard Box, Kapto|Enb                                           */
        PROP_CARD_BOX2,           /* Cardboard Box, Red Arrow, Bahko                                    */
        PROP_CARD_BOX3,           /* Cardboard Box, Scrawled Text, Bahah                                */
        PROP_CARD_BOX4_LG,        /* Cardboard Box, Three Seams                                         */
        PROP_CARD_BOX5_LG,        /* Cardboard Box, Two Seams, Bahah                                    */
        PROP_CARD_BOX6_LG,        /* Cardboard Box, Bahko                                               */
        PROP_CCTV,                /* Surveillance Camera                                                */
        PROP_CONSOLE1,            /* Double Screen Consoles w/ Keyboards                                */
        PROP_CONSOLE2,            /* Double Screen Consoles w/ Left Keyboard                            */
        PROP_CONSOLE3,            /* Double Screen Consoles w/ Right Keyboard                           */
        PROP_CONSOLE_SEVA,        /* Console w/ Keyboard                                                */
        PROP_CONSOLE_SEVB,        /* Console w/ Monitor + Keyboard                                      */
        PROP_CONSOLE_SEVC,        /* Console w/ Switches                                                */
        PROP_CONSOLE_SEVD,        /* Console w/ Five Gauges                                             */
        PROP_CONSOLE_SEV2A,       /* Console w/ Four Faders                                             */
        PROP_CONSOLE_SEV2B,       /* Console w/ Monitor, Keyboard + Switches                            */
        PROP_CONSOLE_SEV2C,       /* Console w/ Three Gauges                                            */
        PROP_CONSOLE_SEV2D,       /* Console w/ Pressure Gauge                                          */
        PROP_CONSOLE_SEV_GEA,     /* Console w/ GoldenEye Key Slot                                      */
        PROP_CONSOLE_SEV_GEB,     /* Console w/ Faders + Pressure Gauge                                 */
        PROP_DESK1,               /* Desk w/ Kickplate                                                  */
        PROP_DESK2,               /* Desk                                                               */
        PROP_DESK_LAMP2,          /* Desk Lamp                                                          */
        PROP_DISC_READER,         /* External Hard Drive                                                */
        PROP_DISK_DRIVE1,         /* Floppy Disc Drive                                                  */
        PROP_FILING_CABINET1,     /* Filing Cabinet                                                     */
        PROP_JERRY_CAN1,          /* Jerrycan (Fuel Container)                                          */
        PROP_KEYBOARD1,           /* Computer Keyboard                                                  */
        PROP_KIT_UNITS1,          /* Kitchen Cabinets                                                   */
        PROP_LETTER_TRAY1,        /* Letter Tray                                                        */
        PROP_MAINFRAME1,          /* Mainframe, Basic                                                   */
        PROP_MAINFRAME2,          /* Mainframe, Advanced                                                */
        PROP_METAL_CHAIR1,        /* Chair (Metal)                                                      */
        PROP_METAL_CRATE1,        /* Metal Crate, 6 Top Corner                                          */
        PROP_METAL_CRATE2,        /* Metal Crate, 6 Bottom Corner                                       */
        PROP_METAL_CRATE3,        /* Metal Crate, Toxic Materials                                       */
        PROP_METAL_CRATE4,        /* Metal Crate, Double Stripe - Class D1 Hazard                       */
        PROP_MISSILE_RACK,        /* Naval Harpoon Missile in Containment Rack                          */
        PROP_MISSILE_RACK2,       /* Naval Harpoon Missiles in Containment Racks x4                     */
        PROP_OIL_DRUM1,           /* Oil Drum, Single Stripe, Ribbed                                    */
        PROP_OIL_DRUM2,           /* Oil Drum, Single Stripe, Ribbed - Class D1 Hazard                  */
        PROP_OIL_DRUM3,           /* Oil Drum, Single Stripe, Ribbed - Toxic Materials                  */
        PROP_OIL_DRUM5,           /* Oil Drum, Double Stripe - Toxic Materials                          */
        PROP_OIL_DRUM6,           /* Oil Drum - Toxic Materials                                         */
        PROP_OIL_DRUM7,           /* Oil Drum, Double Dashes - Class D1 Hazard                          */
        PROP_PADLOCK,             /* Padlock                                                            */
        PROP_PHONE1,              /* Telephone                                                          */
        PROP_RADIO_UNIT1,         /* Radio Tuner w/ 1 Knob + 2 Gauges                                   */
        PROP_RADIO_UNIT2,         /* Radio Tuner w/ 1 Knob + 5 Gauges                                   */
        PROP_RADIO_UNIT3,         /* Radio Tuner w/ 3 Knobs + 5 Gauges                                  */
        PROP_RADIO_UNIT4,         /* Radio Tuner w/ 3 Knobs + 2 Gauges                                  */
        PROP_SAT1_REFLECT,        /* GoldenEye Satellite                                                */
        PROP_SATDISH,             /* Satellite Dish (Arkangelsk)                                        */
        PROP_SATBOX,              /* Uplink Box                                                         */
        PROP_STOOL1,              /* Wooden Stool                                                       */
        PROP_SWIVEL_CHAIR1,       /* Swivel Chair                                                       */
        PROP_TORPEDO_RACK,        /* Naval Torpedo Rack x3                                              */
        PROP_TV1,                 /* Television Monitor                                                 */
        PROP_TV_HOLDER,           /* Hanging Monitor Rack                                               */
        PROP_TVSCREEN,            /* Wall Monitor Screen                                                */
        PROP_TV4SCREEN,           /* Wall Monitor Screens, 4-in-1                                       */
        PROP_WOOD_LG_CRATE1,      /* Wooden Crate w/ #4 Label, Bahah                                    */
        PROP_WOOD_LG_CRATE2,      /* Wooden Crate, Darker Shading, Kapto|Enb                            */
        PROP_WOOD_MD_CRATE3,      /* Wooden Crates x8, Bahko                                            */
        PROP_WOOD_SM_CRATE4,      /* Wooden Crate w/ #2 Label, Bahko                                    */
        PROP_WOOD_SM_CRATE5,      /* Wooden Crate w/ #4 Label, Darker Shading, Bahah                    */
        PROP_WOOD_SM_CRATE6,      /* Wooden Crate w/ UP Arrow, Kapto|Enb                                */
        PROP_WOODEN_TABLE1,       /* Wooden Table                                                       */
        PROP_SWIPE_CARD2,         /* Keycard                                                            */
        PROP_BORG_CRATE,          /* Blue and Gold Printed Circuit Cube (Borg Crate)                    */
        PROP_BOXES4X4,            /* Metal Crate Stack, 4x4                                             */
        PROP_BOXES3X4,            /* Metal Crate Stack, 3x4                                             */
        PROP_BOXES2X4,            /* Metal Crate Stack, 2x4                                             */
        PROP_SEC_PANEL,           /* Security Card Panel                                                */
        PROP_ICBM_NOSE,           /* Silo Missile (ICBM), Nose Cone Only                                */
        PROP_ICBM,                /* Silo Missile (ICBM)                                                */
        PROP_TUNING_CONSOLE1,     /* Dual Consoles on Castors                                           */
        PROP_DESK_ARECIBO1,       /* Computer Work Desk                                                 */
        PROP_LOCKER3,             /* Lockers, Single Venting                                            */
        PROP_LOCKER4,             /* Lockers, Double Venting                                            */
        PROP_ROOFGUN,             /* Ceiling Mounted Drone Gun                                          */
        PROP_DEST_ENGINE,         /* Frigate Engine                                                     */
        PROP_DEST_EXOCET,         /* Naval MK 29 Missile Launcher (Exocet)                              */
        PROP_DEST_GUN,            /* Naval 100 mm Gun Turret (TR 100)                                   */
        PROP_DEST_HARPOON,        /* Naval MK 141 Launch Canisters (Harpoon)                            */
        PROP_DEST_SEAWOLF,        /* Naval MK 26 Dual Missile Launcher (Seawolf)                        */
        PROP_WINDOW,              /* Window Glass                                                       */
        PROP_WINDOW_LIB_LG1,      /* Window Glass, Lattice Frame, 4x10 (single-sided)                   */
        PROP_WINDOW_LIB_SM1,      /* Window Glass, Lattice Frame, 4x3 (double-sided)                    */
        PROP_WINDOW_COR11,        /* Window Glass, Lattice Frame, 4x4 (single-sided)                    */
        PROP_JUNGLE3_TREE,        /* Jungle Large Tree                                                  */
        PROP_PALM,                /* Jungle Palm Tree                                                   */
        PROP_PALMTREE,            /* Jungle Palm Tree, Resprouting After Loss of Fronds                 */
        PROP_PLANT2B,             /* Jungle Plant, Low Shrub                                            */
        PROP_LABBENCH,            /* Laboratory Table w/ Sink Drains                                    */
        PROP_GASBARREL,           /* White Bin                                                          */
        PROP_GASBARRELS,          /* White Bins x4                                                      */
        PROP_BODYARMOUR,          /* Body Armor                                                         */
        PROP_BODYARMOURVEST,      /* Body Armor (Vest)                                                  */
        PROP_GASTANK,             /* Bottling Tank                                                      */
        PROP_GLASSWARE1,          /* Glass Cup                                                          */
        PROP_HATCHBOLT,           /* Metallic Securing Strip (Hatch Bolt)                               */
        PROP_BRAKEUNIT,           /* Train Brake Controller                                             */
        PROP_AK47MAG,             /* Gun Magazine (KF7 Soviet)                                          */
        PROP_M16MAG,              /* Gun Magazine (AR33 Assault Rifle)                                  */
        PROP_MP5KMAG,             /* Gun Magazine (D5K Deutsche)                                        */
        PROP_SKORPIONMAG,         /* Gun Magazine (Klobb)                                               */
        PROP_SPECTREMAG,          /* Gun Magazine (Phantom)                                             */
        PROP_UZIMAG,              /* Gun Magazine (ZMG (9mm))                                           */
        PROP_SILENCER,            /* Silencer                                                           */
        PROP_CHREXTINGUISHER,     /* Fire Extinguisher                                                  */
        PROP_BOXCARTRIDGES,       /* Box of Shells (Shotgun Cartridges)                                 */
        PROP_FNP90MAG,            /* Gun Magazine (RC-P90)                                              */
        PROP_GOLDENSHELLS,        /* Box of Shells (Golden Gun Bullets)                                 */
        PROP_MAGNUMSHELLS,        /* Box of Shells (Magnum Rounds)                                      */
        PROP_WPPKMAG,             /* Gun Magazine (PP7)                                                 */
        PROP_TT33MAG,             /* Gun Magazine (DD44 Dostovei)                                       */
        PROP_SEV_DOOR,            /* Grey Containment Door w/ Caution Stripes and Window                */
        PROP_SEV_DOOR3,           /* Grey Electronic Door w/ LEFT Arrow                                 */
        PROP_SEV_DOOR3_WIND,      /* BETA Electronic Door w/ LEFT Arrow and Fake Window                 */
        PROP_SEV_DOOR4_WIND,      /* Grey Electronic Door w/ LEFT Arrow and Window                      */
        PROP_SEV_TRISLIDE,        /* Glass Door w/ Stone Frame                                          */
        PROP_SEV_DOOR_V1,         /* Grey Electronic Door w/ UP Arrow                                   */
        PROP_STEEL_DOOR1,         /* Silver Corrugated Door w/ Caution Stripes                          */
        PROP_STEEL_DOOR2,         /* Rusty Door w/ Handle                                               */
        PROP_STEEL_DOOR3,         /* Double Cross Brace Door                                            */
        PROP_SILO_LIFT_DOOR,      /* Elevator Door                                                      */
        PROP_STEEL_DOOR2B,        /* Rusty Door w/o Handle                                              */
        PROP_DOOR_ROLLER1,        /* Blue Bay Door w/ Caution Stripes                                   */
        PROP_DOOR_ROLLER2,        /* Blue Bay Door w/ Venting and Caution Stripes                       */
        PROP_DOOR_ROLLER3,        /* Blue Bay Door w/ Venting and Caution Stripes                       */
        PROP_DOOR_ROLLER4,        /* Cargo Bay Door w/ UP Arrow and Transportation Stripes              */
        PROP_DOOR_ST_AREC1,       /* Blue Corrugated Door w/ Transportation Stripes                     */
        PROP_DOOR_ST_AREC2,       /* Blue Reversed Corrugated Door w/ Transportation Stripes            */
        PROP_DOOR_DEST1,          /* Grey Frigate Door w/ Indents and Caution Stripes                   */
        PROP_DOOR_DEST2,          /* Grey Frigate Door w/ Indents, Caution Stripes and KEEP CLEAR Label */
        PROP_GAS_PLANT_SW_DO1,    /* Grey Swinging Door w/ Blue Stripe                                  */
        PROP_GAS_PLANT_SW2_DO1,   /* Grey Swinging Door, Darker                                         */
        PROP_GAS_PLANT_SW3_DO1,   /* Grey Swinging Door, Lighter                                        */
        PROP_GAS_PLANT_SW4_DO1,   /* Light Wooden Door (Looks Like Sand)                                */
        PROP_GAS_PLANT_MET1_DO1,  /* Brown Electronic Door                                              */
        PROP_GAS_PLANT_WC_CUB1,   /* Bathroom Stall Door                                                */
        PROP_GASPLANT_CLEAR_DOOR, /* Laboratory Glass Door                                              */
        PROP_TRAIN_DOOR,          /* Dark Wooden Door                                                   */
        PROP_TRAIN_DOOR2,         /* Dark Wooden Door w/ Window                                         */
        PROP_TRAIN_DOOR3,         /* Dark Wooden Door w/ Window + Shutter                               */
        PROP_DOOR_EYELID,         /* Eyelid Door                                                        */
        PROP_DOOR_IRIS,           /* Iris Door                                                          */
        PROP_SEVDOORWOOD,         /* Cabin Door                                                         */
        PROP_SEVDOORWIND,         /* Weathered Swinging Door w/ Window                                  */
        PROP_SEVDOORNOWIND,       /* Weathered Swinging Door                                            */
        PROP_SEVDOORMETSLIDE,     /* Brown Corrugated Electronic Door                                   */
        PROP_CRYPTDOOR1A,         /* Stone Door w/ Prints (Set A)                                       */
        PROP_CRYPTDOOR1B,         /* Sand Door w/ Damage (Set A)                                        */
        PROP_CRYPTDOOR2A,         /* Stone Door w/ Prints, Darker (Set B)                               */
        PROP_CRYPTDOOR2B,         /* Sand Door w/ Damage, Darker (Set B)                                */
        PROP_CRYPTDOOR3,          /* Egyptian Moving Wall                                               */
        PROP_CRYPTDOOR4,          /* Brown Sand Door (Temple)                                           */
        PROP_VERTDOOR,            /* Blast Door (Control)                                               */
        PROP_HATCHDOOR,           /* Train Floor Hatch                                                  */
        PROP_DAMGATEDOOR,         /* Security Gate (Dam)                                                */
        PROP_DAMTUNDOOR,          /* Tunnel Flood Door (Dam)                                            */
        PROP_DAMCHAINDOOR,        /* Mesh Gate                                                          */
        PROP_SILOTOPDOOR,         /* Launch Tube Ceiling Shutter (Silo)                                 */
        PROP_DOORPRISON1,         /* Cell Door                                                          */
        PROP_DOORSTATGATE,        /* Park Gate                                                          */
        PROP_CHRKALASH,           /* KF7 Soviet                                                         */
        PROP_CHRGRENADELAUNCH,    /* Grenade Launcher                                                   */
        PROP_CHRKNIFE,            /* Hunting Knife                                                      */
        PROP_CHRLASER,            /* Moonraker Laser                                                    */
        PROP_CHRM16,              /* AR33 Assault Rifle                                                 */
        PROP_CHRMP5K,             /* D5K Deutsche                                                       */
        PROP_CHRRUGER,            /* Cougar Magnum                                                      */
        PROP_CHRWPPK,             /* PP7 Special Issue                                                  */
        PROP_CHRSHOTGUN,          /* Shotgun                                                            */
        PROP_CHRSKORPION,         /* Klobb                                                              */
        PROP_CHRSPECTRE,          /* Phantom                                                            */
        PROP_CHRUZI,              /* ZMG (9mm)                                                          */
        PROP_CHRGRENADE,          /* Hand Grenade                                                       */
        PROP_CHRFNP90,            /* RC-P90                                                             */
        PROP_CHRBRIEFCASE,        /* Briefcase                                                          */
        PROP_CHRREMOTEMINE,       /* Remote Mine                                                        */
        PROP_CHRPROXIMITYMINE,    /* Proximity Mine                                                     */
        PROP_CHRTIMEDMINE,        /* Timed Mine                                                         */
        PROP_CHRROCKET,           /* Rocket                                                             */
        PROP_CHRGRENADEROUND,     /* Grenade Round                                                      */
        PROP_CHRWPPKSIL,          /* PP7 (Silenced)                                                     */
        PROP_CHRTT33,             /* DD44 Dostovei                                                      */
        PROP_CHRMP5KSIL,          /* D5K (Silenced)                                                     */
        PROP_CHRAUTOSHOT,         /* Automatic Shotgun                                                  */
        PROP_CHRGOLDEN,           /* Golden Gun                                                         */
        PROP_CHRTHROWKNIFE,       /* Throwing Knife                                                     */
        PROP_CHRSNIPERRIFLE,      /* Sniper Rifle                                                       */
        PROP_CHRROCKETLAUNCH,     /* Rocket Launcher                                                    */
        PROP_HATFURRY,            /* Fur Hat, Blue                                                      */
        PROP_HATFURRYBROWN,       /* Fur Hat, Brown                                                     */
        PROP_HATFURRYBLACK,       /* Fur Hat, Black                                                     */
        PROP_HATTBIRD,            /* Side Cap, Light Green                                              */
        PROP_HATTBIRDBROWN,       /* Side Cap, Dark Green                                               */
        PROP_HATHELMET,           /* Combat Helmet, Green                                               */
        PROP_HATHELMETGREY,       /* Combat Helmet, Grey                                                */
        PROP_HATMOON,             /* Elite Headgear                                                     */
        PROP_HATBERET,            /* Special Forces Beret, Black                                        */
        PROP_HATBERETBLUE,        /* Special Forces Beret, Navy                                         */
        PROP_HATBERETRED,         /* Special Forces Beret, Burgundy                                     */
        PROP_HATPEAKED,           /* Officer's Peaked Visor Cap                                         */
        PROP_CHRWRISTDART,        /* Pchrwristdart (BETA)                                               */
        PROP_CHREXPLOSIVEPEN,     /* Pchrexplosivepen (BETA)                                            */
        PROP_CHRBOMBCASE,         /* Bomb Case (Briefcase Laying Down)                                  */
        PROP_CHRFLAREPISTOL,      /* Pchrflarepistol (BETA Pickup)                                      */
        PROP_CHRPITONGUN,         /* Pchrpitongun (BETA Pickup)                                         */
        PROP_CHRFINGERGUN,        /* Pchrfingergun (BETA Pickup)                                        */
        PROP_CHRSILVERWPPK,       /* Pchrsilverwppk (BETA Pickup)                                       */
        PROP_CHRGOLDWPPK,         /* Pchrgoldwppk (BETA Pickup)                                         */
        PROP_CHRDYNAMITE,         /* Pchrdynamite (BETA Pickup)                                         */
        PROP_CHRBUNGEE,           /* Pchrbungee (BETA Pickup)                                           */
        PROP_CHRDOORDECODER,      /* Door Decoder                                                       */
        PROP_CHRBOMBDEFUSER,      /* Bomb Defuser                                                       */
        PROP_CHRBUGDETECTOR,      /* Pchrbugdetector (BETA Pickup)                                      */
        PROP_CHRSAFECRACKERCASE,  /* Safe Cracker Case (Briefcase Laying Down)                          */
        PROP_CHRCAMERA,           /* Photo Camera (007)                                                 */
        PROP_CHRLOCKEXPLODER,     /* Pchrlockexploder (BETA Pickup)                                     */
        PROP_CHRDOOREXPLODER,     /* Pchrdoorexploder (BETA Pickup)                                     */
        PROP_CHRKEYANALYSERCASE,  /* Key Analyzer Case (Briefcase Laying Down)                          */
        PROP_CHRWEAPONCASE,       /* Weapon Case (Briefcase Standing Up)                                */
        PROP_CHRKEYYALE,          /* Yale Key                                                           */
        PROP_CHRKEYBOLT,          /* Bolt Key                                                           */
        PROP_CHRBUG,              /* Covert Modem / Tracker Bug                                         */
        PROP_CHRMICROCAMERA,      /* Micro Camera                                                       */
        PROP_FLOPPY,              /* Floppy Disc                                                        */
        PROP_CHRGOLDENEYEKEY,     /* GoldenEye Key                                                      */
        PROP_CHRPOLARIZEDGLASSES, /* Polarized Glasses                                                  */
        PROP_CHRCREDITCARD,       /* Pchrcreditcard (BETA Pickup)                                       */
        PROP_CHRDARKGLASSES,      /* Pchrdarkglasses (BETA Pickup)                                      */
        PROP_CHRGASKEYRING,       /* Gas Keyring                                                        */
        PROP_CHRDATATHIEF,        /* Datathief                                                          */
        PROP_SAFE,                /* Safe Body                                                          */
        PROP_BOMB,                /* Pbomb (BETA Pickup)                                                */
        PROP_CHRPLANS,            /* Plans (Briefing Folder)                                            */
        PROP_CHRSPYFILE,          /* Pchrspyfile (BETA Pickup)                                          */
        PROP_CHRBLUEPRINTS,       /* Pirate Blueprints                                                  */
        PROP_CHRCIRCUITBOARD,     /* Circuitboard                                                       */
        PROP_CHRMAP,              /* Bunker Expansion Plans                                             */
        PROP_CHRSPOOLTAPE,        /* Pchrspooltape (BETA Pickup)                                        */
        PROP_CHRAUDIOTAPE,        /* Audiotape                                                          */
        PROP_CHRMICROFILM,        /* Pchrmicrofilm (BETA Pickup)                                        */
        PROP_CHRMICROCODE,        /* Pchrmicrocode (BETA Pickup)                                        */
        PROP_CHRLECTRE,           /* Pchrlectre (BETA Pickup)                                           */
        PROP_CHRMONEY,            /* Pchrmoney (BETA Pickup)                                            */
        PROP_CHRGOLDBAR,          /* Pchrgoldbar (BETA Pickup)                                          */
        PROP_CHRHEROIN,           /* Pchrheroin (BETA Pickup)                                           */
        PROP_CHRCLIPBOARD,        /* Clipboard                                                          */
        PROP_CHRDOSSIERRED,       /* Red Dossier                                                        */
        PROP_CHRSTAFFLIST,        /* Staff List                                                         */
        PROP_CHRDATTAPE,          /* DAT                                                                */
        PROP_CHRPLASTIQUE,        /* Plastique                                                          */
        PROP_CHRBLACKBOX,         /* Black Box (Orange Flight Recorder)                                 */
        PROP_CHRVIDEOTAPE,        /* CCTV Tape (GoldenEye VHS)                                          */
        PROP_NINTENDOLOGO,        /* Nintendo Logo                                                      */
        PROP_GOLDENEYELOGO,       /* GoldenEye Logo                                                     */
        PROP_WALLETBOND,          /* Classified Folder w/ Royal Crest (Folder Menus)                    */
        PROP_MILTRUCK,            /* Supply Truck                                                       */
        PROP_JEEP,                /* Military Jeep                                                      */
        PROP_ARTIC,               /* Red Prime Mover                                                    */
        PROP_HELICOPTER,          /* Transport Helicopter w/ Natalya                                    */
        PROP_TIGER,               /* Pirate Euro Chopper                                                */
        PROP_MILCOPTER,           /* Hound Helicopter                                                   */
        PROP_HIND,                /* Soviet Camouflage Chopper                                          */
        PROP_ARTICTRAILER,        /* Black Trailer                                                      */
        PROP_MOTORBIKE,           /* Motorbike                                                          */
        PROP_TANK,                /* Tank                                                               */
        PROP_APC,                 /* Armored Personnel Carrier                                          */
        PROP_SPEEDBOAT,           /* Speedboat                                                          */
        PROP_PLANE,               /* Aeroplane                                                          */
        PROP_GUN_RUNWAY1,         /* Heavy Gun Emplacement                                              */
        PROP_SAFEDOOR,            /* Safe Door                                                          */
        PROP_KEY_HOLDER,          /* Key Rack                                                           */
        PROP_HATCHSEVX,           /* Grating (Ventshaft Hatch)                                          */
        PROP_SEVDISH,             /* Satellite Dish (Severnaya)                                         */
        PROP_ARCHSECDOOR1,        /* Archives Moving Wall (Dark)                                        */
        PROP_ARCHSECDOOR2,        /* Archives Moving Wall (Light)                                       */
        PROP_GROUNDGUN,           /* Free Standing Drone Gun                                            */
        PROP_TRAINEXTDOOR,        /* Train Exterior Door                                                */
        PROP_CARBMW,              /* White Car #1 (BMW)                                                 */
        PROP_CARESCORT,           /* White Car #2 (Escort)                                              */
        PROP_CARGOLF,             /* White Car #3 (Golf)                                                */
        PROP_CARWEIRD,            /* Red Car (Cadillac)                                                 */
        PROP_CARZIL,              /* Ourumov's Car (ZIL)                                                */
        PROP_SHUTTLE_DOOR_L,      /* Exhaust Bay Doors, Left Side                                       */
        PROP_SHUTTLE_DOOR_R,      /* Exhaust Bay Doors, Right Side                                      */
        PROP_DEPOT_GATE_ENTRY,    /* Metallic Gate w/ Red Star                                          */
        PROP_DEPOT_DOOR_STEEL,    /* Rusty Door w/ Handle (Lo-Res)                                      */
        PROP_GLASSWARE2,          /* Beaker w/ Blue Topper                                              */
        PROP_GLASSWARE3,          /* Erlenmeyer Flask                                                   */
        PROP_GLASSWARE4,          /* Set of Five Beakers                                                */
        PROP_LANDMINE,            /* Land Mine                                                          */
        PROP_PLANT1,              /* Jungle Plant, Withered and Dying                                   */
        PROP_PLANT11,             /* Jungle Plant, Turning Colour                                       */
        PROP_PLANT2,              /* Jungle Plant, Healthy and Thick                                    */
        PROP_PLANT3,              /* Jungle Plant, Tall Leaves                                          */
        PROP_JUNGLE5_TREE,        /* Jungle Tree, Moss Covered                                          */
        PROP_LEGALPAGE,           /* GoldenEye Certification Screen                                     */
        PROP_ST_PETE_ROOM_1I,     /* Roads and Buildings #1 (stretch of road)                           */
        PROP_ST_PETE_ROOM_2I,     /* Roads and Buildings #2 (stretch of road)                           */
        PROP_ST_PETE_ROOM_3T,     /* Roads and Buildings #3 (intersection)                              */
        PROP_ST_PETE_ROOM_5C,     /* Roads and Buildings #4 (street corner)                             */
        PROP_ST_PETE_ROOM_6C,     /* Roads and Buildings #5 (street corner)                             */
        PROP_DOOR_ROLLERTRAIN,    /* Roller Door                                                        */
        PROP_DOOR_WIN,            /* Glass Sliding Door (Aztec)                                         */
        PROP_DOOR_AZTEC,          /* Stone Sliding Door (Aztec)                                         */
        PROP_SHUTTLE,             /* Moonraker Shuttle                                                  */
        PROP_DOOR_AZT_DESK,       /* Boardroom Table (Aztec Exhaust Bay)                                */
        PROP_DOOR_AZT_DESK_TOP,   /* Boardroom Table Extension (Aztec Exhaust Bay)                      */
        PROP_DOOR_AZT_CHAIR,      /* Boardroom Chair (Aztec Exhaust Bay)                                */
        PROP_DOOR_MF,             /* Mainframe Door                                                     */
        PROP_FLAG,                /* Flag Tag Token                                                     */
        PROP_BARRICADE,           /* Road Barricade                                                     */
        PROP_MODEMBOX,            /* Covert Modem Connection Screen                                     */
        PROP_DOORPANEL,           /* Sliding Door Activation Switch                                     */
        PROP_DOORCONSOLE,         /* Console w/ Activation Light                                        */
        PROP_CHRTESTTUBE,         /* Glass Test Tube                                                    */
        PROP_BOLLARD,             /* Bollard                                                            */
        PROP_MAX
    }
}
