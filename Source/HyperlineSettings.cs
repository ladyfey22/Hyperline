namespace Celeste.Mod.Hyperline
{
    using Monocle;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml.Linq;

    [SettingName("modoptions_hyperline_title")]
    public class HyperlineSettings : EverestModuleBinarySettings
    {
        public const int MinHairLength = 1;
        public const int MaxHairLength = 1000;
        public const int MinHairSpeed = -40;
        public const int MaxHairSpeed = 40;
        public const int MinHairPhase = 0;
        public const int MaxHairPhase = 100;

        public byte[] Version { get; private set; } = [0, 3, 4];  //MAJOR,MINOR,SUB

        public class DashSettings : ICloneable
        {
            public uint HairType { get; set; } = DefaultHair.Hash;
            public int HairSpeed { get; set; }
            public int HairLength { get; set; }
            public int HairPhase { get; set; }
            public List<MTexture> HairTextures { get; set; }
            public string HairTextureSource { get; set; } = string.Empty;
            public string HairBangsSource { get; set; } = string.Empty;

            public int Dash { get; set; }
            public List<MTexture> HairBangs { get; set; }
            public Dictionary<uint, IHairType> HairList { get; set; } = Hyperline.HairTypes.CopyHairDict();

            public object Clone() => MemberwiseClone();

            public DashSettings(int dashCount)
            {
                Dash = dashCount;

                HairLength = dashCount < 2 ? 4 : 5;
            }

            public void Read(XElement dashCountElement)
            {
                XElement hairLengthElement = dashCountElement.Element("hairLength");
                if (hairLengthElement != null)
                {
                    HairLength = (int)hairLengthElement;
                    if (HairLength > Hyperline.Settings.HairLengthSoftCap || HairLength < MinHairLength)
                    {
                        HairLength = 4;
                    }
                }
                else
                {
                    Logger.Log(LogLevel.Warn, "Hyperline", "Hyperline settings XML missing dash hair length element.");
                }

                HairSpeed = Math.Clamp((int?)dashCountElement.Element("hairSpeed") ?? 0, MinHairSpeed, MaxHairSpeed);
                HairPhase = Math.Clamp((int?)dashCountElement.Element("hairPhase") ?? 0, 0, 100);
                HairBangsSource = (string)dashCountElement.Element("bangsTexture") ?? "";
                HairTextureSource = (string)dashCountElement.Element("hairTexture") ?? "";

                string chosenType = (string)dashCountElement.Element("type") ?? SolidHair.Id;
                if (Hyperline.HairTypes.Has(Hashing.FNV1Hash(chosenType)))
                {
                    HairType = Hashing.FNV1Hash(chosenType);
                }
                else
                {
                    Logger.Log(LogLevel.Error, "Hyperline", "Hyperline settings contained a chosen hair type that didn't exist: " + chosenType);
                }

                XElement tp = dashCountElement.Element("types");
                if (tp != null)
                {
                    foreach (XElement currentType in tp.Elements())
                    {
                        uint type = Hashing.FNV1Hash(currentType.Name.LocalName);
                        try
                        {
                            IHairType hair = Hyperline.HairTypes.CreateNewHairType(type);
                            if (hair != null)
                            {
                                hair.Read(currentType);
                                HairList[type] = hair;
                            }
                            else
                            {
                                Logger.Log(LogLevel.Warn, "Hyperline", "Hyperline contained invalid hair type " + currentType.Name);
                            }
                        }
                        catch (Exception exception)
                        {
                            Logger.Log(LogLevel.Warn, "Hyperline", "Exception occured while loading hair type " + currentType.Name.LocalName + " dash count " + Dash + "\n" + exception);
                        }
                    }
                }
                else
                {
                    Logger.Log(LogLevel.Warn, "Hyperline", "XML file missing types for dash count " + Dash);
                }
            }

            public void Write(XElement dashCountElement)
            {
                dashCountElement.Add(new XElement("hairLength", HairLength), new XElement("hairSpeed", HairSpeed), new XElement("hairPhase", HairPhase),
                                     new XElement("bangsTexture", HairBangsSource), new XElement("hairTexture", HairTextureSource));
                dashCountElement.Add(new XElement("type", Hyperline.HairTypes.GetType(HairType).GetId()));

                XElement typesElement = new("types");
                foreach (KeyValuePair<uint, IHairType> tp in HairList)
                {
                    XElement tpElement = new(tp.Value.GetId());
                    tp.Value.Write(tpElement);
                    typesElement.Add(tpElement);
                }

                dashCountElement.Add(typesElement);
            }
        }

        public List<DashSettings> DashList { get; set; }


        public HyperlineSettings()
        {
            ResetSettings();
        }

        public void ResetSettings()
        {
            DashList = new((int)Hyperline.MaxDashCount);

            for (int i = 0; i < Hyperline.MaxDashCount; i++)
            {
                DashList.Add(new(i));
            }
        }

        public void SetCustomTexture(int i, string s)
        {
            DashList[i].HairTextureSource = s;
            LoadCustomTexture(i);
        }

        public void LoadCustomTexture(int i)
        {
            if (!string.IsNullOrEmpty(DashList[i].HairTextureSource))
            {
                if (!HasAtlasSubtexture("hyperline/" + DashList[i].HairTextureSource))
                {
                    DashList[i].HairTextures = null;
                    Logger.Log(LogLevel.Warn, "Hyperline", "Invalid texture inputted in custom hair texture " + DashList[i].HairTextureSource);
                    DashList[i].HairTextureSource = string.Empty;
                }
                else
                {
                    DashList[i].HairTextures = GFX.Game.GetAtlasSubtextures("hyperline/" + DashList[i].HairTextureSource);
                }
            }
            else
            {
                DashList[i].HairTextures = null;
            }
        }

        public static bool HasAtlasSubtexture(string str)
        {
            int length = str.Length;
            while (str.Length < length + 6)
            {
                if (GFX.Game.Has(str))
                {
                    return true;
                }

                str += "0";
            }
            return false;
        }

        public void LoadCustomBangs(int i)
        {
            if (!string.IsNullOrEmpty(DashList[i].HairBangsSource))
            {
                if (!HasAtlasSubtexture("hyperline/" + DashList[i].HairBangsSource))
                {
                    Logger.Log(LogLevel.Warn, "Hyperline", "Invalid texture inputted in custom bang texture " + DashList[i].HairBangsSource);
                    DashList[i].HairBangs = null;
                    DashList[i].HairBangsSource = string.Empty;
                }
                else
                {
                    DashList[i].HairBangs = GFX.Game.GetAtlasSubtextures("hyperline/" + DashList[i].HairBangsSource);
                }
            }
            else
            {
                DashList[i].HairBangs = null;
            }
        }

        public static List<DashSettings> CloneSettings(List<DashSettings> d)
        {
            List<DashSettings> returnV = new((int)Hyperline.MaxDashCount);
            for (int i = 0; i < Hyperline.MaxDashCount; i++)
            {
                returnV.Add((DashSettings)d[i].Clone());
            }
            return returnV;
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
            for (int i = 0; i < Hyperline.MaxDashCount; i++)
            {
                LoadCustomTexture(i);
                LoadCustomBangs(i);
            }
        }

        public override void Read(BinaryReader reader)
        {
            try
            {
                MemoryStream currentReader = new(reader.ReadBytes((int)reader.BaseStream.Length));
                XDocument document = XDocument.Load(currentReader);
                XElement root = document.Element("root");

                if (root != null)
                {
                    Enabled = (bool?)root.Element("enabled") ?? Enabled;
                    DoDashFlash = (bool?)root.Element("doDashFlash") ?? DoDashFlash;
                    AllowMapHairColors = (bool?)root.Element("allowMapHairColor") ?? AllowMapHairColors;
                    DoMaddyCrown = (bool?)root.Element("doMaddyCrown") ?? DoMaddyCrown;
                    HairLengthSoftCap = Math.Clamp((int?)root.Element("hairLengthSoftCap") ?? HairLengthSoftCap, 1, MaxHairLength);
                    DoFeatherColor = (bool?)root.Element("doFeatherColor") ?? DoFeatherColor;

                    XElement dashesElement = root.Element("dashes");
                    if (dashesElement != null)
                    {
                        foreach (XElement dashCountElement in dashesElement.Elements("dash"))
                        {
                            XAttribute dashAttr = dashCountElement.Attribute("count");
                            if (dashAttr == null)
                            {
                                continue;
                            }

                            int dash = (int)dashAttr;
                            if (dash < Hyperline.MaxDashCount)
                            {
                                DashList[dash].Read(dashCountElement);
                            }
                        }
                    }
                    else
                    {
                        Logger.Log(LogLevel.Warn, "Hyperline", "Hyperline settings XML missing dashes element.");
                    }
                }
                else
                {
                    Logger.Log(LogLevel.Warn, "Hyperline", "Hyperline settings XML missing root element.");
                }
            }
            catch (Exception exception)
            {
                Logger.Log(LogLevel.Error, "Hyperline", "Error while loading save file...\n" + exception);
            }
        }

        public override void Write(BinaryWriter writer)
        {
            MemoryStream currentWriter = new();
            XDocument document = new();
            XElement root = new("root");
            root.Add(new XElement("version", $"{Version[0]}.{Version[1]}.{Version[2]}"), new XElement("enabled", Enabled), new XElement("allowMapHairColor", AllowMapHairColors),
                         new XElement("doMaddyCrown", DoMaddyCrown), new XElement("doFeatherColor", DoFeatherColor),
                         new XElement("hairLengthSoftCap", HairLengthSoftCap), new XElement("doDashFlash", DoDashFlash));

            XElement dashesElement = new("dashes");
            for (int i = 0; i < Hyperline.MaxDashCount; i++)
            {
                XElement dashCountElement = new("dash", new XAttribute("count", i));
                DashList[i].Write(dashCountElement);
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
