using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Celeste.Mod.Hyperline
{
    class DefaultHair : IHairType
    {
        public DefaultHair()
        {
        }

        public DefaultHair(DefaultHair rvalue)
        {
        }

        public override IHairType Clone()
        {
            return new DefaultHair(this);
        }

        public override string GetHairName()
        {
            return "MODOPTIONS_HYPERLINE_DEFAULT";
        }

        public override Color GetColor(Color colorOrig, float phase)
        {
            Logger.Log("Hyperline", "Default color gotten " + colorOrig.R + " " + colorOrig.G + " " + colorOrig.B);
            return colorOrig;
        }

        public override void Read(BinaryReader reader, byte[] version)
        {
        }

        public override void Write(BinaryWriter writer)
        {
        }

        public override List<TextMenu.Item> CreateMenu(TextMenu menu, bool inGame)
        {
            List<TextMenu.Item> colorMenus = new List<TextMenu.Item>();
            return colorMenus;
        }

        public override IHairType CreateNew()
        {
            return new DefaultHair();
        }

        public override IHairType CreateNew(int i)
        {
            return new DefaultHair();
        }

        public override IHairType CreateNew(string str)
        {
            return new DefaultHair();
        }

        public override string GetId()
        {
            return id;
        }

        public override uint GetHash()
        {
            return hash;
        }

        public override void Read(XElement element)
        {
        }

        public override void Write(XElement element)
        {
        }

        public override void AfterUpdate(On.Celeste.PlayerHair.orig_AfterUpdate orig, PlayerHair self)
        {
            orig(self);
        }

        public override void PlayerUpdate(Color lastColor, Player player)
        {
            player.OverrideHairColor = null;
            Hyperline.Instance.maddyCrownSprite = null;
        }

        public static string id = "Hyperline_DefaultHair";
        public static uint hash = Hashing.FNV1Hash(id);
    }
}
