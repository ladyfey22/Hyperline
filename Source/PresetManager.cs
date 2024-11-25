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
                DashList = new((int)Hyperline.MaxDashCount);

                for (int i = 0; i < Hyperline.MaxDashCount; i++)
                {
                    DashList.Add(new(i));
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

            public static Preset CopyFromSettings()
            {
                Preset preset = new() { DashList = HyperlineSettings.CloneSettings(Hyperline.Settings.DashList) };
                return preset;
            }
        }


        public bool SavePreset(string preset, Preset presetData)
        {
            // we want Hyperline to have its own folder in the settings folder
            string savePath = Path.Combine(Everest.PathSettings, "Hyperline");
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }

            string saveFile = Path.Combine(savePath, preset + ".preset");
            Logger.Log(LogLevel.Info, "Hyperline", $"Attempting to save preset {preset} to Hyperline folder\n");

            try
            {
                using FileStream fileStream = File.Open(saveFile, FileMode.Create);
                using BinaryWriter writer = new(fileStream);
                presetData.Write(writer);
            }
            catch (Exception exception)
            {
                Logger.Log(LogLevel.Error, "Hyperline", "Error while saving preset...\n" + exception);
                return false;
            }

            // if we got here, the preset was saved successfully, add it to the list
            Presets[preset] = presetData; // will overwrite if it already exists

            return true;
        }

        public void DeletePreset(string preset)
        {
            string savePath = Path.Combine(Everest.PathSettings, "Hyperline");
            string saveFile = Path.Combine(savePath, preset + ".preset");

            if (File.Exists(saveFile))
            {
                File.Delete(saveFile);
                Presets.Remove(preset);
            }
        }

        public void LoadPresetsFromPath(string path)
        {
            // load presets from a specific path, making sure to check path and file extensions
            if (!Directory.Exists(path))
            {
                return;
            }

            foreach (string filename in Directory.GetFiles(path, "*.preset"))
            {
                string presetName = Path.GetFileNameWithoutExtension(filename);
                Preset preset = new();

                FileStream fileStream = File.OpenRead(filename);

                MemoryStream memoryStream = new();
                fileStream.CopyTo(memoryStream);

                BinaryReader reader = new(File.OpenRead(filename));
                preset.Read(reader);
                Presets[presetName] = preset;
                Logger.Log(LogLevel.Info, "Hyperline", "Loaded preset " + presetName);
            }
        }

        public void LoadContent()
        {
            Logger.Log(LogLevel.Info, "Hyperline", "Attempting to load presets....\n");
            Presets = [];

            foreach (ModContent content in Everest.Content.Mods)
            {
                foreach (ModAsset asset in content.List)
                {
                    if (Path.GetExtension(asset.PathVirtual)?.ToLowerInvariant() != ".preset" ||
                        !asset.PathVirtual.StartsWith("Hyperline/"))
                    {
                        continue;
                    }

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

            string savePath = Everest.PathSettings;
            if (Directory.Exists(savePath))
            {
                LoadPresetsFromPath(savePath);
            }

            string hyperlinePath = Path.Combine(Everest.PathSettings, "Hyperline");
            if (Directory.Exists(hyperlinePath))
            {
                LoadPresetsFromPath(hyperlinePath);
            }
            else
            {
                // make the folder if it doesn't exist, it won't have any presets in it yet so we don't need to load anything
                Directory.CreateDirectory(hyperlinePath);
            }
        }
    }
}
