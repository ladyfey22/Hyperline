
namespace Celeste.Mod.Hyperline.UI
{
    using System;
    using Monocle;
    using Microsoft.Xna.Framework;

    public class HairPreview : TextMenu.Item
    {
        private readonly MTexture hairTexture;

        private Vector2 scale = new(4, 4);
        private float time;
        private readonly int dashes;
        private const float Gap = 2;

        public HairPreview()
        {
            hairTexture = GFX.Game["characters/player/hair00"];
            Selectable = false;
        }

        public HairPreview(int dashes)
        {
            hairTexture = GFX.Game["characters/player/hair00"];
            Selectable = false;
            this.dashes = dashes;
        }

        private int GetHairCount() => Hyperline.Settings.DashList[dashes].HairLength;

        private float GetHairSpeed() => Hyperline.Settings.DashList[dashes].HairSpeed;
        private int GetHairPhase() => Hyperline.Settings.DashList[dashes].HairPhase;

        private IHairType GetHair() => Hyperline.Settings.DashList[dashes].HairList[Hyperline.Settings.DashList[dashes].HairType];

        public override float LeftWidth() => hairTexture.Width * GetHairCount() * scale.X;

        public override float Height() => hairTexture.Height * scale.Y;

        public override void Update()
        {
            base.Update();
            time += Engine.DeltaTime;
        }

        public override void Render(Vector2 position, bool highlighted)
        {
            for (int i = 0; i < GetHairCount(); i++)
            {
                float phaseShift = Math.Abs((i + GetHairPhase()) / ((float)GetHairCount()));
                float phase = phaseShift + (GetHairSpeed() / 20.0f * time);
                phase -= (float)Math.Floor(phase);

                IHairType previewHair = GetHair();
                if (previewHair != null)
                {
                    Color returnV = previewHair.GetColor(Color.Red, phase);
                    hairTexture.Draw(position + (Vector2.UnitX * Gap * i * scale.X), Vector2.Zero, returnV, scale);
                }
            }

        }
    }
}