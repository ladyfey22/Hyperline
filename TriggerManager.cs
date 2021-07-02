using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace Celeste.Mod.Hyperline
{
    public class TriggerManager
    {
        private IHairType[] hairList;
        private int[] hairSpeedList;
        private int[] hairLengthList;
        private List<MTexture>[] hairTextures;
        private List<MTexture>[] hairBangs;

        private Dictionary<int, IHairType> hairChanges;
        private Dictionary<int, int> hairLengthChanges;
        private Dictionary<int, int> hairSpeedChanges;

        public TriggerManager()
        {
            hairLengthList = new int[Hyperline.MAX_DASH_COUNT];
            hairSpeedList = new int[Hyperline.MAX_DASH_COUNT];
            hairTextures = new List<MTexture>[Hyperline.MAX_DASH_COUNT];
            hairBangs = new List<MTexture>[Hyperline.MAX_DASH_COUNT];
            hairList = new IHairType[Hyperline.MAX_DASH_COUNT];

            hairChanges = new Dictionary<int, IHairType>();
            hairLengthChanges = new Dictionary<int, int>();
            hairSpeedChanges = new Dictionary<int, int>();
        }

        public void LoadFromSettings()
        {
            HyperlineSettings settings = Hyperline.Settings;
            for (int i = 0; i < settings.hairList.Length; i++)
            {
                hairList[i] = settings.hairList[i][settings.hairTypeList[i]];
                hairSpeedList[i] = settings.hairSpeedList[i];
                hairLengthList[i] = settings.hairLengthList[i];
                hairBangs[i] = settings.hairBangs[i];
                hairTextures[i] = settings.hairTextures[i];
            }
        }

        public void UpdateFromChanges()
        {
            if (!Hyperline.Settings.AllowMapHairColors)
                return;
            foreach (KeyValuePair<int, IHairType> hair in hairChanges)
                hairList[hair.Key] = hair.Value;
            foreach (KeyValuePair<int, int> speed in hairSpeedChanges)
                hairSpeedList[speed.Key] = speed.Value;
            foreach (KeyValuePair<int, int> length in hairLengthChanges)
                hairLengthList[length.Key] = length.Value;
        }

        public void SetTrigger(IHairType hair, int dashCount, int length, int speed)
        {
            if (dashCount < Hyperline.MAX_DASH_COUNT)
            {
                hairChanges[dashCount] = hair;
                hairLengthChanges[dashCount] = length;
                hairSpeedChanges[dashCount] = speed;
            }
            UpdateFromChanges();
        }

        public void ResetTrigger(int dashCount)
        {
            if (hairChanges.ContainsKey(dashCount))
                hairChanges.Remove(dashCount);
            if (hairLengthChanges.ContainsKey(dashCount))
                hairLengthChanges.Remove(dashCount);
            if (hairSpeedChanges.ContainsKey(dashCount))
                hairSpeedChanges.Remove(dashCount);
            LoadFromSettings();
            UpdateFromChanges();
        }

        public IHairType GetHair(int dashCount)
        {
            if (dashCount >= Hyperline.MAX_DASH_COUNT || dashCount < 0)
                return hairList[Hyperline.MAX_DASH_COUNT - 1];
            return hairList[dashCount];
        }

        public int GetHairLength(int dashCount)
        {
            if (dashCount >= Hyperline.MAX_DASH_COUNT || dashCount < 0)
                return hairLengthList[Hyperline.MAX_DASH_COUNT - 1];
            return hairLengthList[dashCount];
        }

        public int GetHairSpeed(int dashCount)
        {
            if (dashCount >= Hyperline.MAX_DASH_COUNT || dashCount < 0)
                return hairSpeedList[Hyperline.MAX_DASH_COUNT - 1];
            return hairSpeedList[dashCount];
        }

        public void ResetChanges()
        {
            hairChanges = new Dictionary<int, IHairType>();
            hairLengthChanges = new Dictionary<int, int>();
            hairSpeedChanges = new Dictionary<int, int>();
        }

        public static void OnLevelEntry(Session session, bool fromSaveData)
        {
            Hyperline.Instance.triggerManager.LoadFromSettings();
            if (fromSaveData)
                Hyperline.Instance.triggerManager.UpdateFromChanges();
            else
                Hyperline.Instance.triggerManager.ResetChanges();
        }

        public static void OnLevelTransitionTo(Level level, LevelData next, Vector2 direction)
        {
            Hyperline.Instance.triggerManager.LoadFromSettings();
            Hyperline.Instance.triggerManager.UpdateFromChanges();
        }

        public static void Load()
        {
            Everest.Events.Level.OnEnter += OnLevelEntry;
            Everest.Events.Level.OnTransitionTo += OnLevelTransitionTo;
        }

        public static void Unload()
        {
            Everest.Events.Level.OnEnter -= OnLevelEntry;
            Everest.Events.Level.OnTransitionTo -= OnLevelTransitionTo;
        }


        public void Read(BinaryReader reader)
        {
            MemoryStream currentReader = new MemoryStream(reader.ReadBytes((int)reader.BaseStream.Length));
            XDocument document = XDocument.Load(currentReader);
            XElement root = document.Element("root");
            if(root != null)
            {
                XElement hairChangesElement = root.Element("hairChanges");
                if (hairChangesElement != null)
                {
                    foreach (XElement dashCountElement in hairChangesElement.Elements("dash"))
                    {
                        XAttribute dashAttr = dashCountElement.Attribute("count");
                        if (dashAttr != null)
                        {
                            int dash = (int)dashAttr;
                            if (dash < Hyperline.MAX_DASH_COUNT)
                            {
                                foreach (XElement hairElement in dashCountElement.Elements())
                                {
                                    uint type = Hashing.FNV1Hash(hairElement.Name.LocalName);
                                    IHairType currentHair = Hyperline.Instance.hairTypes.CreateNewHairType(type);
                                    if (currentHair != null)
                                    {
                                        currentHair.Read(hairElement);
                                        hairChanges[dash] = currentHair;
                                    }
                                }
                            }
                        }
                    }
                }

                XElement speedChangesElement = root.Element("speedChanges");
                if (speedChangesElement != null)
                {
                    foreach (XElement dashCountElement in speedChangesElement.Elements("dash"))
                    {
                        XAttribute dashAttr = dashCountElement.Attribute("count");
                        if (dashAttr != null)
                        {
                            int dash = (int)dashAttr;
                            if (dash < Hyperline.MAX_DASH_COUNT)
                            {
                                XElement speedElement = dashCountElement.Element("speed");
                                if (speedElement != null)
                                    hairSpeedChanges[dash] = (int)speedElement;
                            }
                        }
                    }
                }

                XElement lengthChangesElement = root.Element("speedChanges");
                if (speedChangesElement != null)
                {
                    foreach (XElement dashCountElement in lengthChangesElement.Elements("dash"))
                    {
                        XAttribute dashAttr = dashCountElement.Attribute("count");
                        if (dashAttr != null)
                        {
                            int dash = (int)dashAttr;
                            if (dash < Hyperline.MAX_DASH_COUNT)
                            {
                                XElement lengthElement = dashCountElement.Element("length");
                                if (lengthElement != null)
                                    hairLengthChanges[dash] = (int)lengthElement;
                            }
                        }
                    }
                }
            }
        }

        public void Write(BinaryWriter writer)
        {
            MemoryStream currentWriter = new MemoryStream();
            XDocument document = new XDocument();
            XElement root = new XElement("root");

            XElement hairChangesElement = new XElement("hairChanges");

            foreach (KeyValuePair<int, IHairType> currentHair in hairChanges)
            {
                XElement dashCountElement = new XElement("dash", new XAttribute("count", currentHair.Key));
                XElement hairElement = new XElement(currentHair.Value.GetId());
                currentHair.Value.Write(hairElement);
                dashCountElement.Add(hairElement);
                hairChangesElement.Add(dashCountElement);
            }
            root.Add(hairChangesElement);

            XElement speedChangesElement = new XElement("speedChanges");
            foreach(KeyValuePair<int, int> currentSpeed in hairSpeedChanges)
            {
                XElement dashCountElement = new XElement("dash", new XAttribute("count", currentSpeed.Key));
                XElement speedElement = new XElement("speed", currentSpeed.Value);

                dashCountElement.Add(speedElement);
                speedChangesElement.Add(dashCountElement);
            }
            root.Add(speedChangesElement);

            XElement lengthChangesElement = new XElement("lengthChanges");
            foreach(KeyValuePair<int, int> currentLength in hairLengthChanges)
            {
                XElement dashCountElement = new XElement("dash", new XAttribute("count", currentLength.Key));
                XElement lengthElement = new XElement("length", currentLength.Value);

                dashCountElement.Add(lengthElement);
                lengthChangesElement.Add(dashCountElement);
            }
            root.Add(lengthChangesElement);

            document.Add(root);
            document.Save(currentWriter);

            byte[] bytes = currentWriter.ToArray();
            writer.Write(bytes, 0, bytes.Length);
        }
    }
}
