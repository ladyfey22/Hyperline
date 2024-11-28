namespace Celeste.Mod.Hyperline.HairTypes
{
    using Microsoft.Xna.Framework;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml.Linq;
    using Lib.Utility;
    using Lib;
    using Lib.UI;

    public class SolidHair : BaseHairType
    {
        private HSVColor color;

        public SolidHair()
        {
            color = new();
        }

        public SolidHair(HSVColor color)
        {
            this.color = color;
        }

        public SolidHair(SolidHair rvalue)
        {
            color = rvalue.color.Clone();
        }

        public override IHairType Clone() => new SolidHair(this);

        public override string GetHairName() => "MODOPTIONS_HYPERLINE_SOLID";

        public override Color GetColor(Color colorOrig, float phase) => color.ToColor();

        public override List<TextMenu.Item> CreateMenu(TextMenu menu, bool inGame)
        {
            List<TextMenu.Item> colorMenus =
            [
                new ColorSubmenu(menu, "Color ", color, inGame).Change(v => color = new(v))
            ];
            return colorMenus;
        }
        public override IHairType CreateNew() => new SolidHair();

        public override IHairType CreateNew(int i) => new SolidHair(DefaultColors[i % DefaultColors.Length]);

        public override IHairType CreateNew(string str)
        {
            SolidHair returnV = new();
            string[] tokens = str.Split(',');
            if (tokens.Length >= 1)
            {
                returnV.color = new(tokens[0]);
            }

            return returnV;
        }

        public override string GetId() => Id;
        public override uint GetHash() => Hash;
        public override void Read(BinaryReader reader, byte[] version) { }
        public override void Write(BinaryWriter writer) { }

        public override void Read(XElement element) => ReadColorElement(element, "color", ref color);

        public override void Write(XElement element) => element.Add(new XElement("color", color.ToHSVString()));

        public const string Id = "Hyperline_SolidHair";
        public static readonly uint Hash = Hashing.FNV1Hash(Id);
        private static readonly HSVColor[] DefaultColors = [new("44B7FF"), new("AC3232"), new("FF6DEF")];
    }
}
