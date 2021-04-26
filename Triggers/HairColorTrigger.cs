using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.Hyperline.Triggers
{
    [CustomEntity("Hyperline/HairColorTrigger")]
    public class HairColorTrigger : Trigger
    {
        private struct HairChange
        {
            public HairChange(int spd, int lngth, IHairType tp)
            {
                hairSpeed = spd;
                hairLength = lngth;
                hairType = tp;
            }

            public int hairSpeed;
            public int hairLength;
            public IHairType hairType;
        }

        private bool resetOnLeave;
        private Dictionary<int, HairChange> hairChanges;

        public HairColorTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            resetOnLeave = data.Bool("resetOnLeave");
            hairChanges = new Dictionary<int, HairChange>();
            ParseHairChanges(data.Attr("hairChanges"));
        }

        private void ParseHairChanges(string v)
        {
            string[] tokens = v.Split(';');
            for (int i = 0; i < tokens.Length && i + 4 < tokens.Length; i += 5)
            {
                try
                {
                    int dashCount = int.Parse(tokens[i]);
                    dashCount = dashCount > HyperlineSettings.MAX_HAIR_LENGTH ? HyperlineSettings.MAX_HAIR_LENGTH - 1 : dashCount;
                    int hairLength = int.Parse(tokens[i + 1]);
                    int hairSpeed = int.Parse(tokens[i + 2]);
                    string hairName = tokens[i + 3];

                    IHairType type = Hyperline.Instance.hairTypes.GetType(hairName);
                    if (type == null)
                    {
                        Logger.Log("Hyperline", "HairColorTrigger no " + hairName + " type found for trigger parsing");
                        type = new SolidHair();
                    }
                    else
                        type = type.CreateNew(tokens[i + 4]);
                    hairChanges[dashCount] = new HairChange(hairSpeed, hairLength, type);
                }
                catch (Exception ex)
                {
                    Logger.Log("Hyperline", "HairColorTrigger error parsing...\n" + ex);
                }
            }
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            foreach (KeyValuePair<int, HairChange> hair in hairChanges)
                Hyperline.Instance.triggerManager.SetTrigger(hair.Value.hairType, hair.Key, hair.Value.hairLength, hair.Value.hairSpeed);
        }

        public override void OnLeave(Player player)
        {
            base.OnLeave(player);
            if (resetOnLeave)
                foreach (KeyValuePair<int, HairChange> hair in hairChanges)
                    Hyperline.Instance.triggerManager.ResetTrigger(hair.Key);
        }
    }
}
