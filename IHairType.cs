using System;
using System.Globalization;
using System.Reflection;
using Microsoft.Xna.Framework;
using FMOD.Studio;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using Celeste.Mod.UI;
using Monocle;
using On.Celeste;
using IL.MonoMod;
using FMOD;
using System.IO;

namespace Celeste.Mod.Hyperline
{
    public abstract class IHairType
    {
        public abstract string GetHairName();
        public abstract Color GetColor(float phase);
        public abstract void Read(BinaryReader reader, byte[] version);
        public abstract void Write(BinaryWriter writer);
        public abstract IHairType CreateNew();
        public abstract IHairType CreateNew(int i);
        public abstract List<TextMenu.Item> CreateMenu(TextMenu menu, bool inGame);
    }
}
