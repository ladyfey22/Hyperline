using Monocle;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Celeste.Mod.Hyperline
{
    public class PresetManager
    {
        public List<KeyValuePair<string, Preset>> presets;

        public class Preset
        {
            public Preset()
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

            public Dictionary<uint, IHairType>[] hairList;

            public uint[] hairTypeList;
            public int[] hairSpeedList;
            public int[] hairLengthList;

            public string[] hairTextureSource;
            public List<MTexture>[] hairTextures;

            public string[] hairBangsSource;
            public List<MTexture>[] hairBangs;

            public void Read(BinaryReader reader)
            {
                try
                {
                    MemoryStream currentReader = new MemoryStream(reader.ReadBytes((int)reader.BaseStream.Length));
                    XDocument document = XDocument.Load(currentReader);
                    XElement root = document.Element("root");

                    if (root != null)
                    {
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
                                            if (hairLengthList[dash] > HyperlineSettings.MAX_HAIR_LENGTH || hairLengthList[dash] < HyperlineSettings.MIN_HAIR_LENGTH)
                                                hairLengthList[dash] = 4;
                                        }
                                        else
                                            Logger.Log(LogLevel.Warn, "Hyperline", "Hyperline settings XML missing dash hair length element.");

                                        XElement hairSpeedElement = dashCountElement.Element("hairSpeed");
                                        if (hairSpeedElement != null)
                                        {
                                            hairSpeedList[dash] = (int)hairSpeedElement;
                                            if (hairSpeedList[dash] > HyperlineSettings.MAX_HAIR_SPEED || hairSpeedList[dash] < HyperlineSettings.MIN_HAIR_SPEED)
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
                catch (Exception exception)
                {
                    Logger.Log(LogLevel.Error, "Hyperline", "Error while loading save file...\n" + exception.ToString());
                }
            }

            public void Write(BinaryWriter writer)
            {
                MemoryStream currentWriter = new MemoryStream();
                XDocument document = new XDocument();
                XElement root = new XElement("root");

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

            public void Apply()
            {
                Hyperline.Settings.ResetSettings();

                for (int i = 0; i < hairLengthList.Length; i++)
                {
                    Hyperline.Settings.hairLengthList[i] = hairLengthList[i];
                    Hyperline.Settings.hairTypeList[i] = hairTypeList[i];
                    Hyperline.Settings.hairSpeedList[i] = hairSpeedList[i];
                    Hyperline.Settings.hairTextureSource[i] = hairTextureSource[i];
                    Hyperline.Settings.hairBangsSource[i] = hairBangsSource[i];
                    Hyperline.Settings.LoadCustomBangs(i);
                    Hyperline.Settings.LoadCustomTexture(i);
                    
                    foreach(KeyValuePair<uint, IHairType> hair in hairList[i])
                        Hyperline.Settings.hairList[i][hair.Key] = hair.Value.Clone();
                }
            }
        }

        public void LoadContent()
        {
            Logger.Log("Hyperline", "Attempting to load presets....\n");
            presets = new List<KeyValuePair<string, Preset>>();

            foreach (ModContent content in Everest.Content.Mods)
                foreach (ModAsset asset in content.List)
                {
                    if (Path.GetExtension(asset.PathVirtual).ToLower() == ".preset" && asset.PathVirtual.StartsWith("Hyperline/"))
                    {
                        string presetName = asset.PathVirtual.Substring(10);
                        presetName = presetName.Substring(0, presetName.Length - 7);
                        Preset preset = new Preset();
                        MemoryStream stream = new MemoryStream(asset.Data);
                        BinaryReader reader = new BinaryReader(stream);
                        preset.Read(reader);

                        presets.Add(new KeyValuePair<string, Preset>(presetName, preset));
                        Logger.Log(LogLevel.Info, "Hyperline", "Loaded preset " + presetName + " path " + asset.PathVirtual);
                    }
                }

            string savePath = Everest.PathSettings;
            foreach(string filename in Directory.GetFiles(savePath, "*.preset"))
            {
                string presetName = Path.GetFileNameWithoutExtension(filename);
                Preset preset = new Preset();

                FileStream fileStream = File.OpenRead(filename);
                MemoryStream memoryStream = new MemoryStream();
                fileStream.CopyTo(memoryStream);
                
                BinaryReader reader = new BinaryReader(File.OpenRead(filename));
                preset.Read(reader);
                presets.Add(new KeyValuePair<string, Preset>(presetName, preset));
                Logger.Log(LogLevel.Info, "Hyperline", "Loaded preset " + presetName + " path " + filename);
            }
        }
    }
}
