using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

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
            hairChanges[dashCount] = hair;
            hairLengthChanges[dashCount] = length;
            hairSpeedChanges[dashCount] = speed;
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
            return hairList[dashCount];
        }

        public int GetHairLength(int dashCount)
        {
            return hairLengthList[dashCount];
        }

        public int GetHairSpeed(int dashCount)
        {
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
    }
}
