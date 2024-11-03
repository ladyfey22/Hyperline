namespace Celeste.Mod.Hyperline
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml.Linq;

    public class PresetManager
    {
        public Dictionary<string, Preset> Presets { get; private set; }

        public class Preset
        {
            public List<HyperlineSettings.DashSettings> DashList { get; set; }

            public Preset()
            {
                ResetSettings();
            }

            public void ResetSettings()
            {
                DashList = new List<HyperlineSettings.DashSettings>((int)Hyperline.MaxDashCount);

                for (int i = 0; i < Hyperline.MaxDashCount; i++)
                {
                    DashList.Add(new HyperlineSettings.DashSettings(i));
                }
            }

            public void Read(BinaryReader reader)
            {
                try
                {
                    MemoryStream currentReader = new(reader.ReadBytes((int)reader.BaseStream.Length));
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
                                    if (dash < Hyperline.MaxDashCount)
                                    {
                                        DashList[dash].Read(dashCountElement);
                                    }
                                }
                            }
                        }
                        else
                        {
                            Logger.Log(LogLevel.Warn, "Hyperline", "Hyperline settings XML missing dashs element.");
                        }
                    }
                    else
                    {
                        Logger.Log(LogLevel.Warn, "Hyperline", "Hyperline settings XML missing root element.");
                    }
                }
                catch (Exception exception)
                {
                    Logger.Log(LogLevel.Error, "Hyperline", "Error while loading save file...\n" + exception.ToString());
                }
            }

            public void Write(BinaryWriter writer)
            {
                MemoryStream currentWriter = new();
                XDocument document = new();
                XElement root = new("root");

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

            public void Apply()
            {
                Hyperline.Settings.ResetSettings();

                Hyperline.Settings.DashList = HyperlineSettings.CloneSettings(DashList);

                for (int i = 0; i < Hyperline.MaxDashCount; i++)
                {
                    Hyperline.Settings.LoadCustomBangs(i);
                    Hyperline.Settings.LoadCustomTexture(i);
                }
            }
        }

        public void LoadContent()
        {
            Logger.Log("Hyperline", "Attempting to load presets....\n");
            Presets = [];

            foreach (ModContent content in Everest.Content.Mods)
            {
                foreach (ModAsset asset in content.List)
                {
                    if (Path.GetExtension(asset.PathVirtual).ToLowerInvariant() == ".preset" && asset.PathVirtual.StartsWith("Hyperline/"))
                    {
                        string presetName = asset.PathVirtual[10..];
                        presetName = presetName[..^7];
                        Preset preset = new();
                        MemoryStream stream = new(asset.Data);
                        BinaryReader reader = new(stream);
                        preset.Read(reader);

                        Presets[presetName] = preset;
                        Logger.Log(LogLevel.Info, "Hyperline", "Loaded preset " + presetName + " path " + asset.PathVirtual);
                    }
                }
            }

            string savePath = Everest.PathSettings;
            foreach (string filename in Directory.GetFiles(savePath, "*.preset"))
            {
                string presetName = Path.GetFileNameWithoutExtension(filename);
                Preset preset = new();

                FileStream fileStream = File.OpenRead(filename);
                MemoryStream memoryStream = new();
                fileStream.CopyTo(memoryStream);

                BinaryReader reader = new(File.OpenRead(filename));
                preset.Read(reader);
                Presets[presetName] = preset;
                Logger.Log(LogLevel.Info, "Hyperline", "Loaded preset " + presetName + " path " + filename);
            }
        }
    }
}
