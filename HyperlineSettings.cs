using Monocle;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Celeste.Mod.Hyperline
{
    [SettingName("modoptions_hyperline_title")]
    public class HyperlineSettings : EverestModuleBinarySettings
    {
        public const int MIN_HAIR_LENGTH = 1;
        public const int MAX_HAIR_LENGTH = 1000;
        public const int MIN_HAIR_SPEED = -40;
        public const int MAX_HAIR_SPEED = 40;

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
        public readonly byte[] version = new byte[] { 0, 2, 4 }; //MAJOR,MINOR,SUB

        public HyperlineSettings()
        {
            ResetSettings();
        }

        public void ResetSettings()
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

        [SettingIgnore]
        public bool DoFeatherColor { get; set; } = true;

        [SettingIgnore]
        public int HairLengthSoftCap { get; set; } = 100;

        [SettingIgnore]
        public bool DoDashFlash { get; set; } = true;

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

        public void ReadV1_17(BinaryReader reader)
        {
            byte[] header = reader.ReadBytes(4);
            if (header.SequenceEqual(oldHeader) || header.SequenceEqual(newHeader))
            {
                byte[] version = new byte[] { 0, 1, 7 };
                if (header.SequenceEqual(newHeader))
                    version = reader.ReadBytes(3);
                Logger.Log(LogLevel.Debug, "Hyperline", "Hyperline loading settings file v" + version[0] + "." + version[1] + "." + version[2]);
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
                    if (!Hyperline.Instance.hairTypes.Has(hairTypeList[i]))
                    {
                        Logger.Log(LogLevel.Warn, "Hyperline", "Settings file contained invalid hair type " + hairTypeList[i]);
                        hairTypeList[i] = SolidHair.hash;
                    }

                    hairLengthList[i] = reader.ReadInt32();
                    if (hairLengthList[i] < MIN_HAIR_LENGTH || hairLengthList[i] > MAX_HAIR_LENGTH)
                    {
                        Logger.Log(LogLevel.Warn, "Hyperline", "Settings file contained invalid hair length " + hairLengthList[i]);
                        hairLengthList[i] = 4;
                    }

                    hairSpeedList[i] = reader.ReadInt32();
                    if (hairSpeedList[i] < MIN_HAIR_SPEED || hairSpeedList[i] > MAX_HAIR_SPEED)
                    {
                        Logger.Log(LogLevel.Warn, "Hyperline", "Settings file contained invalid hair speed " + hairSpeedList[i]);
                        hairSpeedList[i] = 0;
                    }

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
        }

        public override void Read(BinaryReader reader)
        {
            try
            {
                byte[] header = reader.ReadBytes(4);
                reader.BaseStream.Seek(-4, SeekOrigin.Current);
                if (header.SequenceEqual(oldHeader) || header.SequenceEqual(newHeader))
                    ReadV1_17(reader);
                else
                {
                    MemoryStream currentReader = new MemoryStream(reader.ReadBytes((int)reader.BaseStream.Length));
                    XDocument document = XDocument.Load(currentReader);
                    XElement root = document.Element("root");

                    if (root != null)
                    {
                        XElement enabledElement = root.Element("enabled");
                        if (enabledElement != null)
                            Enabled = (bool)enabledElement;
                        XElement doDashFlashElement = root.Element("doDashFlash");
                        if (doDashFlashElement != null)
                            DoDashFlash = (bool)doDashFlashElement;

                        XElement allowMapColors = root.Element("allowMapHairColor");
                        if (allowMapColors != null)
                            AllowMapHairColors = (bool)allowMapColors;
                        XElement doMaddyCrown = root.Element("doMaddyCrown");
                        if (doMaddyCrown != null)
                            DoMaddyCrown = (bool)doMaddyCrown;

                        XElement hyperlineSoftCap = root.Element("hairLengthSoftCap");
                        if (hyperlineSoftCap != null)
                            HairLengthSoftCap = (int)hyperlineSoftCap;
                        if (HairLengthSoftCap > MAX_HAIR_LENGTH)
                            HairLengthSoftCap = MAX_HAIR_LENGTH;

                        XElement doFeatherColor = root.Element("doFeatherColor");
                        if (doFeatherColor != null)
                            DoFeatherColor = (bool)doFeatherColor;

                        XElement dashesElement = root.Element("dashes");
                        if (dashesElement != null)
                        {
                            foreach (XElement dashCountElement in dashesElement.Elements("dash"))
                            {
                                XAttribute dashAttr = dashCountElement.Attribute("count");
                                if (dashAttr != null)
                                {
                                    int dash = (int)dashAttr;
                                    if (dash < Hyperline.MAX_DASH_COUNT)
                                    {
                                        XElement hairLengthElement = dashCountElement.Element("hairLength");
                                        if (hairLengthElement != null)
                                        {
                                            hairLengthList[dash] = (int)hairLengthElement;
                                            if (hairLengthList[dash] > HairLengthSoftCap || hairLengthList[dash] < MIN_HAIR_LENGTH)
                                                hairLengthList[dash] = 4;
                                        }
                                        else
                                            Logger.Log(LogLevel.Warn, "Hyperline", "Hyperline settings XML missing dash hair length element.");

                                        XElement hairSpeedElement = dashCountElement.Element("hairSpeed");
                                        if (hairSpeedElement != null)
                                        {
                                            hairSpeedList[dash] = (int)hairSpeedElement;
                                            if (hairSpeedList[dash] > MAX_HAIR_SPEED || hairSpeedList[dash] < MIN_HAIR_SPEED)
                                                hairSpeedList[dash] = 0;
                                        }
                                        else
                                            Logger.Log(LogLevel.Warn, "Hyperline", "Hyperline settings XML missing dash hair speed element.");

                                        XElement hairBangsElement = dashCountElement.Element("bangsTexture");
                                        if (hairBangsElement != null)
                                            hairBangsSource[dash] = (string)hairBangsElement;
                                        XElement hairTextureElement = dashCountElement.Element("hairTexture");
                                        if (hairTextureElement != null)
                                            hairTextureSource[dash] = (string)hairTextureElement;

                                        string chosenType = SolidHair.id;
                                        XElement hairTypeElement = dashCountElement.Element("type");
                                        if (hairTypeElement != null)
                                            chosenType = (string)hairTypeElement;
                                        else
                                            Logger.Log(LogLevel.Warn, "Hyperline", "Hyperline settings XML missing dash hair type element.");
                                        if (Hyperline.Instance.hairTypes.Has(Hashing.FNV1Hash(chosenType)))
                                            hairTypeList[dash] = Hashing.FNV1Hash(chosenType);

                                        XElement tp = dashCountElement.Element("types");
                                        if (tp != null)
                                        {
                                            foreach (XElement currentType in tp.Elements())
                                            {
                                                uint type = Hashing.FNV1Hash(currentType.Name.LocalName);
                                                try
                                                {
                                                    IHairType hair = Hyperline.Instance.hairTypes.CreateNewHairType(type);
                                                    if (hair != null)
                                                    {
                                                        hair.Read(currentType);
                                                        hairList[dash][type] = hair;
                                                    }
                                                    else
                                                        Logger.Log(LogLevel.Warn, "Hyperline", "Hyperline contained invalid hair type " + currentType.Name);
                                                }
                                                catch (Exception exception)
                                                {
                                                    Logger.Log(LogLevel.Warn, "Hyperline", "Exception occured while loading hair type " + currentType.Name.LocalName + " dash count " + dash + "\n" + exception);
                                                }
                                            }
                                        }
                                        else
                                            Logger.Log(LogLevel.Warn, "Hyperline", "XML file missing types for dash count " + dash);
                                    }
                                }
                            }
                        }
                        else
                            Logger.Log(LogLevel.Warn, "Hyperline", "Hyperline settings XML missing dashs element.");
                    }
                    else
                        Logger.Log(LogLevel.Warn, "Hyperline", "Hyperline settings XML missing root element.");
                }
            }
            catch (Exception exception)
            {
                Logger.Log(LogLevel.Error, "Hyperline", "Error while loading save file...\n" + exception.ToString());
            }
        }

        public override void Write(BinaryWriter writer)
        {
            MemoryStream currentWriter = new MemoryStream();
            XDocument document = new XDocument();
            XElement root = new XElement("root");
            root.Add(new XElement("enabled", Enabled), new XElement("allowMapHairColor", AllowMapHairColors),
                         new XElement("doMaddyCrown", DoMaddyCrown), new XElement("doFeatherColor", DoFeatherColor), 
                         new XElement("hairLengthSoftCap", HairLengthSoftCap), new XElement("doDashFlash", DoDashFlash));

            XElement dashesElement = new XElement("dashes");
            for (int i = 0; i < Hyperline.MAX_DASH_COUNT; i++)
            {
                XElement dashCountElement = new XElement("dash", new XAttribute("count", i));
                dashCountElement.Add(new XElement("hairLength", hairLengthList[i]), new XElement("hairSpeed", hairSpeedList[i]),
                                     new XElement("bangsTexture", hairBangsSource[i]), new XElement("hairTexture", hairTextureSource[i]));
                dashCountElement.Add(new XElement("type", Hyperline.Instance.hairTypes.GetType(hairTypeList[i]).GetId()));

                XElement typesElement = new XElement("types");
                foreach (KeyValuePair<uint, IHairType> tp in hairList[i])
                {
                    XElement tpElement = new XElement(tp.Value.GetId());
                    tp.Value.Write(tpElement);
                    typesElement.Add(tpElement);
                }

                dashCountElement.Add(typesElement);
                dashesElement.Add(dashCountElement);
            }
            root.Add(dashesElement);
            document.Add(root);
            document.Save(currentWriter);

            byte[] bytes = currentWriter.ToArray();
            writer.Write(bytes, 0, bytes.Length);
        }
    }
}
