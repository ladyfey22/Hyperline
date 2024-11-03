namespace Celeste.Mod.Hyperline
{
    using Microsoft.Xna.Framework;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml.Linq;

    public class DefaultHair : IHairType
    {
        public DefaultHair()
        {
        }

        public DefaultHair(DefaultHair rvalue)
        {
        }

        public override IHairType Clone() => new DefaultHair(this);
        public override string GetHairName() => "MODOPTIONS_HYPERLINE_DEFAULT";
        public override Color GetColor(Color colorOrig, float phase) => colorOrig;
        public override List<TextMenu.Item> CreateMenu(TextMenu menu, bool inGame) => [];
        public override IHairType CreateNew() => new DefaultHair();
        public override IHairType CreateNew(int i) => new DefaultHair();
        public override IHairType CreateNew(string str) => new DefaultHair();
        public override string GetId() => Id;
        public override uint GetHash() => Hash;
        public override void Read(XElement element) { }
        public override void Write(XElement element) { }
        public override void Read(BinaryReader reader, byte[] version) { }
        public override void Write(BinaryWriter writer) { }


        public override void AfterUpdate(On.Celeste.PlayerHair.orig_AfterUpdate orig, PlayerHair self) => orig(self);

        public override void PlayerUpdate(Color lastColor, Player player)
        {
            player.OverrideHairColor = null;
            Hyperline.Instance.MaddyCrownSprite = null;
        }

        public const string Id = "Hyperline_DefaultHair";
        public static readonly uint Hash = Hashing.FNV1Hash(Id);
    }
}
