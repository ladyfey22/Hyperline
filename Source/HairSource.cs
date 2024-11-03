namespace Celeste.Mod.Hyperline
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a source of hyperline hair info.
    /// </summary>
    public abstract class IHairSource
    {
        public abstract HyperlineSettings.DashSettings GetDash(int dashes);

        public virtual IHairType GetHair(int dashes)
        {
            HyperlineSettings.DashSettings hairSettings = GetDash(dashes);
            if (hairSettings.HairList.TryGetValue(hairSettings.HairType, out IHairType value))
            {
                return value;
            }
            return null;
        }

        public virtual int GetHairLength(int dashes)
        {
            HyperlineSettings.DashSettings hairSettings = GetDash(dashes);
            return hairSettings != null ? hairSettings.HairLength : 4;
        }

        public virtual int GetHairSpeed(int dashes)
        {
            HyperlineSettings.DashSettings hairSettings = GetDash(dashes);
            return hairSettings != null ? hairSettings.HairSpeed : 0;
        }
    }

    public class SettingsHairSource : IHairSource
    {
        public override HyperlineSettings.DashSettings GetDash(int dashes)
        {
            if (dashes is < 0 or >= (int)Hyperline.MaxDashCount)
            {
                return null;
            }

            return Hyperline.Settings.DashList[dashes];
        }
    }

    public class HairSourceList(List<IHairSource> hairSources) : IHairSource
    {
        public List<IHairSource> HairSources { get; set; } = hairSources;

        public override HyperlineSettings.DashSettings GetDash(int dashes)
        {
            HyperlineSettings.DashSettings returnV = null;
            for (int i = 0; i < HairSources.Count && returnV == null; i++)
            {
                returnV = HairSources[i].GetDash(dashes);
            }
            return returnV;
        }
    }

    public class TriggerHairSource : IHairSource
    {
        public override HyperlineSettings.DashSettings GetDash(int dashes)
        {
            if (Hyperline.TriggerManager != null && Hyperline.TriggerManager.CurrentPreset != null && dashes >= 0 && dashes < (int)Hyperline.MaxDashCount)
            {
                return Hyperline.TriggerManager.CurrentPreset.DashList[dashes];
            }
            return null;
        }
    }
}