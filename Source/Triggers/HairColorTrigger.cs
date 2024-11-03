namespace Celeste.Mod.Hyperline.Triggers
{
    using global::Celeste.Mod.Entities;
    using Microsoft.Xna.Framework;

    [CustomEntity("Hyperline/HairColorTrigger")]
    public class HairColorTrigger(EntityData data, Vector2 offset) : Trigger(data, offset)
    {

        private readonly bool resetOnLeave = data.Bool("resetOnLeave");
        private readonly string preset = data.Attr("preset");

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            Hyperline.TriggerManager.Trigger(preset);
        }

        public override void OnLeave(Player player)
        {
            base.OnLeave(player);
            if (resetOnLeave)
            {
                Hyperline.TriggerManager.ResetTrigger();
            }
        }
    }
}