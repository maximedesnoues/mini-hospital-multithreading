using Raylib_cs;
using System.Numerics;
using static MiniHospital.Config.NavNode;

namespace MiniHospital.Config
{
    public static class HospitalLayout
    {
        public const int Width = 1440;
        public const int Height = 712;

        public static readonly RoomConfig[] Rooms =
        [
            new RoomConfig("Généraliste 1", RoomType.Generalist,
                new Slot(new(467, 140), new(503, 150), G1Door, CharacterAction.Sitting, Facing.Left),
                new Slot(new(415, 178), new(427, 170), G1Door, CharacterAction.Sitting, Facing.Right),
                new Rectangle(222, 22, 318, 174)),
            new RoomConfig("Généraliste 2", RoomType.Generalist,
                new Slot(new(808, 140), new(844, 150), G2Door, CharacterAction.Sitting, Facing.Left),
                new Slot(new(756, 178), new(768, 170), G2Door, CharacterAction.Sitting, Facing.Right),
                new Rectangle(563, 22, 318, 174)),
            new RoomConfig("Généraliste 3", RoomType.Generalist,
                new Slot(new(1144, 140), new(1180, 150), G3Door, CharacterAction.Sitting, Facing.Left),
                new Slot(new(1092, 178), new(1104, 170), G3Door, CharacterAction.Sitting, Facing.Right),
                new Rectangle(899, 22, 318, 174)),
            new RoomConfig("Radiologie", RoomType.Radiology,
                new Slot(new(1322, 346), new(1352, 346), RadiologyIn, CharacterAction.Standing, Facing.Left),
                new Slot(new(1195, 342), new(1224, 340), RadiologyIn, CharacterAction.Lying, Facing.Right),
                new Rectangle(1094, 196, 302, 156)),
            new RoomConfig("Pouletokwatrie", RoomType.Psychiatry,
                new Slot(new(1200, 520), new(1218, 522), PsychiatryIn, CharacterAction.Standing, Facing.Right),
                new Slot(new(1302, 496), new(1302, 490), PsychiatryIn, CharacterAction.Sitting, Facing.Left),
                new Rectangle(1095, 366, 314, 162))
        ];

        // Secrétaires assises derrière les comptoirs.
        public static readonly Slot[] ReceptionStaff =
        [
            new Slot(new(122, 256), new(200, 256), ReceptionStaff1, CharacterAction.Sitting, Facing.Right),
            new Slot(new(122, 356), new(200, 356), ReceptionStaff2, CharacterAction.Sitting, Facing.Right),
            new Slot(new(122, 456), new(200, 456), ReceptionStaff3, CharacterAction.Sitting, Facing.Right)
        ];

        // Patients debout à droite des comptoirs.
        public static readonly Slot[] ReceptionPatients =
        [
            new Slot(new(288, 302), new(278, 302), Reception1, CharacterAction.Standing, Facing.Left),
            new Slot(new(288, 402), new(278, 402), Reception2, CharacterAction.Standing, Facing.Left),
            new Slot(new(288, 502), new(278, 502), Reception3, CharacterAction.Standing, Facing.Left)
        ];

        // Quatre bancs de trois places
        public static readonly Slot[] WaitingSeats =
        [
            new Slot(new(499, 330), new(499, 312), Aisle1, CharacterAction.Sitting, Facing.Right),
            new Slot(new(548, 330), new(548, 312), Aisle1, CharacterAction.Sitting, Facing.Right),
            new Slot(new(597, 330), new(597, 312), Aisle1, CharacterAction.Sitting, Facing.Right),
            new Slot(new(823, 330), new(823, 312), Aisle2, CharacterAction.Sitting, Facing.Right),
            new Slot(new(872, 330), new(872, 312), Aisle2, CharacterAction.Sitting, Facing.Right),
            new Slot(new(921, 330), new(921, 312), Aisle2, CharacterAction.Sitting, Facing.Right),
            new Slot(new(499, 475), new(499, 457), Aisle3, CharacterAction.Sitting, Facing.Right),
            new Slot(new(548, 475), new(548, 457), Aisle3, CharacterAction.Sitting, Facing.Right),
            new Slot(new(597, 475), new(597, 457), Aisle3, CharacterAction.Sitting, Facing.Right),
            new Slot(new(823, 475), new(823, 457), Aisle4, CharacterAction.Sitting, Facing.Right),
            new Slot(new(872, 475), new(872, 457), Aisle4, CharacterAction.Sitting, Facing.Right),
            new Slot(new(921, 475), new(921, 457), Aisle4, CharacterAction.Sitting, Facing.Right)
        ];

        // Places debout (non réservées), pour avoir des sports quand plus de place sur les bancs
        public static readonly Slot[] StandingWaitingSlots =
        [
            new Slot(new(1008, 380), new(1008, 380), StandEast, CharacterAction.Standing, Facing.Left),
            new Slot(new(1008, 490), new(1008, 490), StandEast2, CharacterAction.Standing, Facing.Left),
            new Slot(new(412, 490), new(412, 490), StandWest, CharacterAction.Standing, Facing.Right)
        ];

        public static readonly SpriteConfig[] Furniture =
        [
            Part(TextureId.Admission, 200, 300, 128),
            Part(TextureId.Admission, 200, 400, 128),
            Part(TextureId.Admission, 200, 500, 128),
            Part(TextureId.Bench, 548, 320, 160, 0, 12),
            Part(TextureId.Bench, 872, 320, 160, 0, 12),
            Part(TextureId.Bench, 548, 465, 160, 0, 12),
            Part(TextureId.Bench, 872, 465, 160, 0, 12),
            Part(TextureId.Plant, 418, 300, 60),
            Part(TextureId.Plant, 1002, 300, 60),
            Part(TextureId.Table, 420, 408, 103),
            Part(TextureId.Desk, 497, 172, 71),
            Part(TextureId.Chair, 425, 170, 32, 0, 12),
            Part(TextureId.Desk, 838, 172, 71),
            Part(TextureId.Chair, 766, 170, 32, 0, 12),
            Part(TextureId.Desk, 1174, 172, 71),
            Part(TextureId.Chair, 1102, 170, 32, 0, 12),
            Part(TextureId.Arch, 712, 664, 312)
        ];

        private static SpriteConfig Part(TextureId texture, float x, float bottom, float width, int layer = 0, float sortLift = 0)
        {
            Rectangle source = texture switch
            {
                TextureId.Admission => new Rectangle(110, 195, 1214, 745),
                TextureId.Bench => new Rectangle(163, 335, 1122, 469),
                TextureId.Chair => new Rectangle(538, 221, 453, 594),
                TextureId.Desk => new Rectangle(250, 181, 725, 608),
                TextureId.Plant => new Rectangle(126, 100, 829, 1207),
                TextureId.Planter => new Rectangle(17, 17, 1110, 606),
                TextureId.Table => new Rectangle(18, 18, 1006, 435),
                TextureId.Arch => new Rectangle(0, 0, 312, 164),
                _ => throw new ArgumentOutOfRangeException(nameof(texture))
            };
            float scale = width / source.Width;
            return new SpriteConfig(texture, source, new Vector2(x, bottom - sortLift),
                new Vector2(source.Width / 2f, source.Height - sortLift / scale), layer, scale);
        }

        public static readonly Dictionary<NavNode, Vector2> NavPoints = new()
        {
            [Outside] = new(710, 710),
            [Sidewalk] = new(710, 688),
            [Threshold] = new(710, 640),
            [Inside] = new(710, 505),
            [BusCorner] = new(330, 688),
            [BusStop] = new(215, 674),
            [SouthWest] = new(312, 505),
            [SouthEast] = new(1050, 505),
            [Reception1] = new(312, 302),
            [Reception2] = new(312, 402),
            [Reception3] = new(312, 502),
            [ReceptionStaff1] = new(122, 256),
            [ReceptionStaff2] = new(122, 356),
            [ReceptionStaff3] = new(122, 456),
            [StaffSouth] = new(122, 518),
            [WestNorth] = new(312, 235),
            [G1Hall] = new(379, 235),
            [G2Hall] = new(720, 235),
            [G3Hall] = new(1056, 235),
            [G1Door] = new(379, 188),
            [G2Door] = new(720, 188),
            [G3Door] = new(1056, 188),
            [EastRadiology] = new(1050, 300),
            [EastPsychiatry] = new(1050, 450),
            [RadiologyDoor] = new(1092, 300),
            [PsychiatryDoor] = new(1092, 450),
            [RadiologyIn] = new(1125, 337),
            [PsychiatryIn] = new(1125, 497),
            [Aisle1] = new(548, 340),
            [Aisle2] = new(872, 340),
            [Aisle3] = new(548, 485),
            [Aisle4] = new(872, 485),
            [AisleMid1] = new(715, 340),
            [AisleMid2] = new(715, 485),
            [StandEast] = new(1008, 380),
            [StandEast2] = new(1008, 490),
            [StandWest] = new(412, 490)
        };

        public static readonly (NavNode From, NavNode To)[] NavLinks =
        [
            (Outside, Sidewalk),
            (Sidewalk, Threshold),
            (Threshold, Inside),
            (Sidewalk, BusCorner),
            (BusCorner, BusStop),
            (SouthWest, Inside),
            (Inside, SouthEast),
            (SouthWest, Reception3),
            (Reception3, Reception2),
            (Reception2, Reception1),
            (Reception1, WestNorth),
            (ReceptionStaff1, ReceptionStaff2),
            (ReceptionStaff2, ReceptionStaff3),
            (ReceptionStaff3, StaffSouth),
            (StaffSouth, SouthWest),
            (WestNorth, G1Hall),
            (G1Hall, G2Hall),
            (G2Hall, G3Hall),
            (G1Hall, G1Door),
            (G2Hall, G2Door),
            (G3Hall, G3Door),
            (G3Hall, EastRadiology),
            (EastRadiology, EastPsychiatry),
            (EastPsychiatry, SouthEast),
            (EastRadiology, RadiologyDoor),
            (EastPsychiatry, PsychiatryDoor),
            (RadiologyDoor, RadiologyIn),
            (PsychiatryDoor, PsychiatryIn),
            (Aisle1, AisleMid1),
            (AisleMid1, Aisle2),
            (Aisle3, AisleMid2),
            (AisleMid2, Aisle4),
            (G2Hall, AisleMid1),
            (AisleMid1, AisleMid2),
            (AisleMid2, Inside),
            (Aisle2, StandEast),
            (Aisle4, StandEast2),
            (Aisle3, StandWest),
            (EastRadiology, StandEast),
            (Reception1, Aisle1),
            (StandWest, SouthWest),
            (StandEast, EastPsychiatry),
            (StandEast2, SouthEast)
        ];
    }
}
