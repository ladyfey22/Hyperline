namespace Celeste.Mod.Hyperline
{
    using System;
    using System.IO;
    using System.Xml.Linq;

    public class TriggerManager
    {
        public string CurrentPresetName { get; set; }
        public PresetManager.Preset CurrentPreset { get; set; }

        public TriggerManager()
        {
        }

        public void Trigger(string preset)
        {
            if (string.IsNullOrEmpty(preset))
            {
                CurrentPresetName = null;
                CurrentPreset = null;
                return;
            }

            CurrentPresetName = preset;
            Hyperline.PresetManager.Presets.TryGetValue(preset, out PresetManager.Preset p);
            CurrentPreset = p;
        }

        public void ResetTrigger()
        {
            CurrentPresetName = null;
            CurrentPreset = null;
        }


        public void Read(BinaryReader reader)
        {
            MemoryStream currentReader = new(reader.ReadBytes((int)reader.BaseStream.Length));
            XDocument document = XDocument.Load(currentReader);
            XElement root = document.Element("root");
            if (root != null)
            {
                string presetName = (string)document.Element("preset") ?? "";
                Trigger(presetName);
            }
        }

        public void Write(BinaryWriter writer)
        {
            MemoryStream currentWriter = new();
            XDocument document = new();
            XElement root = new("root");

            root.Add(new XElement("preset", CurrentPresetName));
            document.Add(root);
            document.Save(currentWriter);

            byte[] bytes = currentWriter.ToArray();
            writer.Write(bytes, 0, bytes.Length);
        }
    }
}