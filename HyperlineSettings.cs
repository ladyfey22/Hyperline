using Monocle;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Celeste.Mod.Hyperline
{
    [SettingName("modoptions_hyperline_title")]
    public class HyperlineSettings : EverestModuleBinarySettings
    {
        public const int MIN_HAIR_LENGTH = 1;
        public const int MAX_HAIR_LENGTH = 100;

        public Dictionary<uint, IHairType>[] hairList;

        public uint[] hairTypeList;
        public int[] hairSpeedList;
        public int[] hairLengthList;

        public string[] hairTextureSource;
        public List<MTexture>[] hairTextures;

        public string[] hairBangsSource;
        public List<MTexture>[] hairBangs;

        public readonly byte[] oldHeader = new byte[] { 0xDE, 0xAD, 0xBE, 0xEF };
        public readonly byte[] newHeader = new byte[] { 0xBE, 0xEF, 0xDE, 0xAD };
        public readonly byte[] version = new byte[] { 0, 1, 17 }; //MAJOR,MINOR,SUB

        public HyperlineSettings()
        {
            hairTypeList = new uint[Hyperline.MAX_DASH_COUNT];
            hairLengthList = new int[Hyperline.MAX_DASH_COUNT];
            hairSpeedList = new int[Hyperline.MAX_DASH_COUNT];
            hairTextureSource = new string[Hyperline.MAX_DASH_COUNT];
            hairBangsSource = new string[Hyperline.MAX_DASH_COUNT];
            hairTextures = new List<MTexture>[Hyperline.MAX_DASH_COUNT];
            hairBangs = new List<MTexture>[Hyperline.MAX_DASH_COUNT];
            hairList = new Dictionary<uint, IHairType>[Hyperline.MAX_DASH_COUNT];


            for (int i = 0; i < hairLengthList.Length; i++)
            {
                hairBangs[i] = null;
                hairTextures[i] = null;
                hairLengthList[i] = 4;
                hairTypeList[i] = SolidHair.hash;
                hairSpeedList[i] = 0;
                hairTextureSource[i] = string.Empty;
                hairBangsSource[i] = string.Empty;
                hairList[i] = Hyperline.Instance.hairTypes.CopyHairDict(i);
            }
        }

        public void SetCustomTexture(int i, string s)
        {
            hairTextureSource[i] = s;
            LoadCustomTexture(i);
        }

        public void LoadCustomTexture(int i)
        {
            if (!string.IsNullOrEmpty(hairTextureSource[i]))
            {
                if (!HasAtlasSubtexture("hyperline/" + hairTextureSource[i]))
                {
                    hairTextures[i] = null;
                    Logger.Log(LogLevel.Warn, "Hyperline", "Invalid texture inputted in custom hair texture " + hairTextureSource[i]);
                    hairTextureSource[i] = string.Empty;
                }
                else
                    hairTextures[i] = GFX.Game.GetAtlasSubtextures("hyperline/" + hairTextureSource[i]);
            }
            else
                hairTextures[i] = null;
        }

        public bool HasAtlasSubtexture(string str)
        {
            int length = str.Length;
            while (str.Length < length + 6)
            {
                if (GFX.Game.Has(str))
                    return true;
                str += "0";
            }
            return false;
        }

        public void LoadCustomBangs(int i)
        {
            if (!string.IsNullOrEmpty(hairBangsSource[i]))
            {
                if (!HasAtlasSubtexture("hyperline/" + hairBangsSource[i]))
                {
                    Logger.Log(LogLevel.Warn, "Hyperline", "Invalid texture inputted in custom bang texture " + hairBangsSource[i]);
                    hairBangs[i] = null;
                    hairBangsSource[i] = string.Empty;
                }
                else
                    hairBangs[i] = GFX.Game.GetAtlasSubtextures("hyperline/" + hairBangsSource[i]);
            }
            else
                hairBangs[i] = null;
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


        /* Here for backwards compatability */
        public void ReadV1_15(BinaryReader reader)
        {
            const int DEFAULT_HAIR_TYPE_COUNT = 3;
            IHairType[] HairTypeDict = new IHairType[DEFAULT_HAIR_TYPE_COUNT];
            HairTypeDict[0] = (new GradientHair());
            HairTypeDict[1] = (new PatternHair());
            HairTypeDict[2] = (new SolidHair());

            IHairType[,] HairList = new IHairType[Hyperline.MAX_DASH_COUNT, DEFAULT_HAIR_TYPE_COUNT];

            for (int i = 0; i < Hyperline.MAX_DASH_COUNT; i++)
                for (int c = 0; c < (int)HairTypeDict.Length; c++)
                    HairList[i, c] = HairTypeDict[c].CreateNew(i);

            Enabled = reader.ReadBoolean();
            AllowMapHairColors = reader.ReadBoolean();
            for (int i = 0; i < Hyperline.MAX_DASH_COUNT; i++)
            {
                hairTypeList[i] = (uint)reader.ReadByte();
                hairLengthList[i] = reader.ReadInt32();
                hairLengthList[i] = (hairLengthList[i] >= MIN_HAIR_LENGTH && hairLengthList[i] <= MAX_HAIR_LENGTH) ? hairLengthList[i] : 4;
                hairSpeedList[i] = reader.ReadInt32();
                for (int x = 0; x < HairTypeDict.Length; x++)
                {
                    HairList[i, x] = HairTypeDict[x].CreateNew();
                    HairList[i, x].Read(reader, version);
                }
            }
            DoMaddyCrown = reader.ReadBoolean();
            for (int i = 0; i < Hyperline.MAX_DASH_COUNT; i++)
                hairTextureSource[i] = reader.ReadString();
            for (int i = 0; i < Hyperline.MAX_DASH_COUNT; i++)
                hairBangsSource[i] = reader.ReadString();

            for (int i = 0; i < Hyperline.MAX_DASH_COUNT; i++)
            {
                for (int x = 0; x < HairTypeDict.Length; x++)
                {
                    uint hash = HairList[i, x].GetHash();
                    hairList[i][hash] = HairList[i, x];
                }
                hairTypeList[i] = HairTypeDict[hairTypeList[i]].GetHash();
            }
        }

        public override void Read(BinaryReader reader)
        {
            byte[] header = reader.ReadBytes(4);
            if (header.SequenceEqual(oldHeader) || header.SequenceEqual(newHeader))
            {
                byte[] version = new byte[] { 0, 1, 7 };
                if (header.SequenceEqual(newHeader))
                    version = reader.ReadBytes(3);
                if (version[0] == 0 && version[1] <= 1 && version[2] <= 15)
                {
                    ReadV1_15(reader);
                    return;
                }
                Enabled = reader.ReadBoolean();
                AllowMapHairColors = reader.ReadBoolean();
                DoMaddyCrown = reader.ReadBoolean();
                for (int i = 0; i < Hyperline.MAX_DASH_COUNT; i++)
                {
                    hairTypeList[i] = reader.ReadUInt32();
                    hairTypeList[i] = Hyperline.Instance.hairTypes.Has(hairTypeList[i]) ? hairTypeList[i] : SolidHair.hash;
                    hairLengthList[i] = reader.ReadInt32();
                    hairLengthList[i] = (hairLengthList[i] >= MIN_HAIR_LENGTH && hairLengthList[i] <= MAX_HAIR_LENGTH) ? hairLengthList[i] : 4;
                    hairSpeedList[i] = reader.ReadInt32();
                    hairBangsSource[i] = reader.ReadString();
                    hairTextureSource[i] = reader.ReadString();

                    uint hairTypeCount = reader.ReadUInt32();
                    for (uint j = 0u; j < hairTypeCount; j++)
                    {
                        uint id = reader.ReadUInt32();
                        uint byteCount = reader.ReadUInt32();

                        BinaryReader tmpReader = new BinaryReader(new MemoryStream(reader.ReadBytes((int)byteCount)));

                        if (hairList[i].ContainsKey(id))
                            hairList[i][id].Read(tmpReader, version);
                        //if we don't know what type this is... throw it out
                    }
                }
            }
            else
                Logger.Log(LogLevel.Error, "Hyperline", "Settings file was found corrupted.");
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(newHeader, 0, 4);
            writer.Write(version, 0, 3);
            writer.Write(Enabled);
            writer.Write(AllowMapHairColors);
            writer.Write(DoMaddyCrown);
            for (int i = 0; i < Hyperline.MAX_DASH_COUNT; i++)
            {
                writer.Write(hairTypeList[i]);
                writer.Write(hairLengthList[i]);
                writer.Write(hairSpeedList[i]);
                writer.Write(hairBangsSource[i]);
                writer.Write(hairTextureSource[i]);

                uint hairTypeCount = (uint)hairList[i].Count;
                writer.Write(hairTypeCount);
                foreach (KeyValuePair<uint, IHairType> hair in hairList[i])
                {
                    uint id = hair.Key;
                    writer.Write(id);

                    MemoryStream memStream = new MemoryStream();
                    BinaryWriter tmpWriter = new BinaryWriter(memStream);
                    hair.Value.Write(tmpWriter);
                    uint byteCount = (uint)memStream.Length;
                    writer.Write(byteCount);
                    writer.Write(memStream.ToArray(), 0, (int)byteCount);
                }
            }

        }
    }
}
