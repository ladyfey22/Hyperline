using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet;
using YamlDotNet.Serialization;
using System.Globalization;
using System.Reflection;
using Microsoft.Xna.Framework;
using FMOD.Studio;
using Microsoft.Xna.Framework.Input;
using Celeste.Mod.UI;
using Monocle;
using On.Celeste;
using IL.MonoMod;
using FMOD;

namespace Celeste.Mod.Hyperline
{
    [SettingName("modoptions_hyperline_title")]
    public class HyperlineSettings : EverestModuleBinarySettings
    {
        public const int MIN_HAIR_LENGTH = 1;
        public const int MAX_HAIR_LENGTH = 100;
        public const int DEFAULT_HAIR_TYPE_COUNT = 3;


        public int[] HairTypeList;
        public IHairType[,] HairList;
        public int[] HairSpeedList;
        public int[] HairLengthList;
        public IHairType[] HairTypeDict;

        public String[] HairTextureSource;
        public List<MTexture>[] HairTextures;

        public String[] HairBangsSource;
        public List<MTexture>[] HairBangs;

        readonly byte[] OldHeader = new byte[] { 0xDE, 0xAD, 0xBE, 0xEF };
        readonly byte[] NewHeader = new byte[] { 0xBE, 0xEF, 0xDE, 0xAD };
        readonly byte[] Version   = new byte[] { 0, 1, 15 }; //MAJOR,MINOR,SUB

        public HyperlineSettings()
        {
            HairList = new IHairType[Hyperline.MAX_DASH_COUNT, DEFAULT_HAIR_TYPE_COUNT];
            HairTypeList = new int[Hyperline.MAX_DASH_COUNT];
            HairLengthList = new int[Hyperline.MAX_DASH_COUNT];
            HairSpeedList = new int[Hyperline.MAX_DASH_COUNT];
            HairTextureSource = new String[Hyperline.MAX_DASH_COUNT];
            HairBangsSource = new string[Hyperline.MAX_DASH_COUNT];
            HairTextures = new List<MTexture>[Hyperline.MAX_DASH_COUNT];
            HairBangs = new List<MTexture>[Hyperline.MAX_DASH_COUNT];

            HairTypeDict = new IHairType[DEFAULT_HAIR_TYPE_COUNT];
            HairTypeDict[0] = (new GradientHair());
            HairTypeDict[1] = (new PatternHair());
            HairTypeDict[2] = (new SolidHair());

            for (int i = 0; i < HairLengthList.Length; i++)
            {
                HairBangs[i] = null;
                HairTextures[i] = null;
                HairLengthList[i] = 4;
                HairTypeList[i] = 2;
                HairSpeedList[i] = 0;
                HairTextureSource[i] = String.Empty;
                HairBangsSource[i] = String.Empty;
            }

            for (int i = 0; i < Hyperline.MAX_DASH_COUNT; i++)
                for (int c = 0; c < (int)HairTypeDict.Length; c++)
                    HairList[i, c] = HairTypeDict[c].CreateNew(i);
        }

        public void SetCustomTexture(int i, string s)
        {
            HairTextureSource[i] = s;
            LoadCustomTexture(i);
        }

        public void LoadCustomTexture(int i)
        {
            if (!String.IsNullOrEmpty(HairTextureSource[i]))
            {
                if (!HasAtlasSubtexture("hyperline/" + HairTextureSource[i]))
                {
                    HairTextures[i] = null;
                    Logger.Log(LogLevel.Warn, "Hyperline", "Invalid texture inputted in custom hair texture " + HairTextureSource[i]);
                    HairTextureSource[i] = String.Empty;
                }
                else
                    HairTextures[i] = GFX.Game.GetAtlasSubtextures("hyperline/" + HairTextureSource[i]);
            }
            else
                HairTextures[i] = null;
        }

        public bool HasAtlasSubtexture(string str)
        {
            int length = str.Length;
            while (str.Length < length + 6)
            {
                if (GFX.Game.Has(str))
                    return true;
                str +=  "0";
            }
            return false;
        }

        public void LoadCustomBangs(int i)
        {
            if (!String.IsNullOrEmpty(HairBangsSource[i]))
            {
                if (!HasAtlasSubtexture("hyperline/" + HairBangsSource[i]))
                {
                    Logger.Log(LogLevel.Warn, "Hyperline", "Invalid texture inputted in custom bang texture " + HairBangsSource[i]);
                    HairBangs[i] = null;
                    HairBangsSource[i] = String.Empty;
                }
                else
                    HairBangs[i] = GFX.Game.GetAtlasSubtextures("hyperline/" + HairBangsSource[i]);
            }
            else
                HairBangs[i] = null;
        }

        [SettingIgnore]
        public bool Enabled { get; set; } = true;
        
        [SettingIgnore]
        public bool AllowMapHairColors { get; set; } = true;

        [SettingIgnore]
        public bool DoMaddyCrown { get; set; } = true;

        public void LoadTextures()
        {
            for (int i = 0; i < Hyperline.MAX_DASH_COUNT; i++)
            {
                LoadCustomTexture(i);
                LoadCustomBangs(i);
            }
        }
        public override void Read(BinaryReader reader)
        {
            byte[] header = reader.ReadBytes(4);
            if (header.SequenceEqual(OldHeader) || header.SequenceEqual(NewHeader))
            {
                byte[] version = new byte[] { 0, 1, 7 };
                if (header.SequenceEqual(NewHeader))
                    version = reader.ReadBytes(3);
                Enabled = reader.ReadBoolean();
                AllowMapHairColors = reader.ReadBoolean();
                for (int i = 0; i < Hyperline.MAX_DASH_COUNT; i++)
                {
                    HairTypeList[i] = (int)reader.ReadByte();
                    HairLengthList[i] = reader.ReadInt32();
                    HairLengthList[i] = (HairLengthList[i] >= MIN_HAIR_LENGTH && HairLengthList[i] <= MAX_HAIR_LENGTH) ? HairLengthList[i] : 4;
                    HairSpeedList[i] = reader.ReadInt32();
                    for (int x = 0; x < HairTypeDict.Length; x++)
                    {
                        HairList[i, x] = HairTypeDict[x].CreateNew();
                        HairList[i, x].Read(reader, version);
                    }
                }
                DoMaddyCrown = reader.ReadBoolean();
                for (int i = 0; i < Hyperline.MAX_DASH_COUNT; i++)
                    HairTextureSource[i] = reader.ReadString();
                for (int i = 0; i < Hyperline.MAX_DASH_COUNT; i++)
                    HairBangsSource[i] = reader.ReadString();

            }
            else
                Logger.Log(LogLevel.Error, "Hyperline", "Settings file was found corrupted.");
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(NewHeader, 0, 4);
            writer.Write(Version, 0, 3); 
            writer.Write(Enabled);
            writer.Write(AllowMapHairColors);
            for(int i=0; i < Hyperline.MAX_DASH_COUNT; i++)
            {
                writer.Write((byte)HairTypeList[i]);
                writer.Write(HairLengthList[i]);
                writer.Write(HairSpeedList[i]);
                for(int x=0; x < HairTypeDict.Length; x++)
                    HairList[i,x].Write(writer);
            }
            writer.Write(DoMaddyCrown);
            for (int i = 0; i < Hyperline.MAX_DASH_COUNT; i++)
                writer.Write(HairTextureSource[i]);
            for (int i = 0; i < Hyperline.MAX_DASH_COUNT; i++)
                writer.Write(HairBangsSource[i]);
        }
    }
}
