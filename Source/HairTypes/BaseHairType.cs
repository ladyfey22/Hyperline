namespace Celeste.Mod.Hyperline.HairTypes;
using Microsoft.Xna.Framework;
using Monocle;
using Lib;

public abstract class BaseHairType : IHairType
{
    private static int lastDashes;
    private static float hairFlashTimer;

    public static bool IsFlash() => hairFlashTimer > 0;

    public static Color LerpFlash(Color c) => Color.Lerp(c, Player.FlashHairColor, (float)System.Math.Sin(hairFlashTimer / 0.12f * MathHelper.Pi));

    /// <summary>
    /// Updates the hair, allowing for custom physics.
    /// </summary>
    /// <param name="orig">The default function for hair updating.</param>
    /// <param name="self">The player's hair.</param>
    /// <remarks>
    /// It is reccomended to look at the default hair after update before writing this.
    /// </remarks>
    public override void AfterUpdate(On.Celeste.PlayerHair.orig_AfterUpdate orig, PlayerHair self)
    {
        Player player = self.EntityAs<Player>();
        if (!(!Hyperline.Settings.DoFeatherColor && player.StateMachine.State == 19))
        {
            player.Hair.Color = Hyperline.GetCurrentColor(Hyperline.Instance.LastColor, player.Dashes, 0);
        }

        orig(self);
    }

    public override void PlayerUpdate(Color lastColor, Player player)
    {
        if (player != null)
        {
            player.OverrideHairColor = Hyperline.GetCurrentColor(Hyperline.Instance.LastColor, player.Dashes, 0);
            Hyperline.Instance.MaddyCrownSprite = null;
        }
    }

    public override void UpdateHair(On.Celeste.Player.orig_UpdateHair orig, Player self, bool applyGravity)
    {
        if (lastDashes != self.Dashes)
        {
            hairFlashTimer = 0.12f;
        }

        if (hairFlashTimer > 0f)
        {
            hairFlashTimer -= Engine.DeltaTime;
        }

        lastDashes = self.Dashes;
        orig(self, applyGravity);
    }
}
