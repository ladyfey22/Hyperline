
namespace Celeste.Mod.Hyperline.UI
{
    using System;
    using Microsoft.Xna.Framework;
    using Monocle;

    public class ColorSubmenu : HSubMenu
    {
        private Action<HSVColor> onValueChange;
        // TEXTURES
        private readonly MTexture hairTexture; // player hair texture

        // sub items
        private readonly ColorControl colorControl;
        private readonly TextMenu.Button textInput;
        private HSVColor color;
        private readonly TextMenu menu;

        public ColorSubmenu(TextMenu menu, string label, HSVColor color, bool inGame) : base(label, false)
        {
            this.color = color;
            this.menu = menu;

            hairTexture = GFX.Game["characters/player/hair00"];

            colorControl = new ColorControl("Color Selector", color.H, color.S, color.V).Change((h, s, v) => OnChange(new(h, s, v)));
            textInput = new("Code Input: " + color.ToHSVString());
            textInput.Pressed(() =>
            {
                Audio.Play(SFX.ui_main_savefile_rename_start);
                menu.SceneAs<Overworld>().Goto<Mod.UI.OuiModOptionString>().Init<Mod.UI.OuiModOptions>(color.ToHSVString(), v => OnChange(new(v)), 9);
            });

            textInput.Disabled = inGame;
            Add([colorControl, textInput]);
            Selectable = true;
        }

        public ColorSubmenu Change(Action<HSVColor> newOnValueChange)
        {
            onValueChange = newOnValueChange;
            return this;
        }

        private void OnChange(HSVColor c)
        {
            color = c;
            colorControl.SetHSV(color.H, color.S, color.V);
            textInput.Pressed(() =>
            {
                Audio.Play(SFX.ui_main_savefile_rename_start);
                menu.SceneAs<Overworld>().Goto<Mod.UI.OuiModOptionString>().Init<Mod.UI.OuiModOptions>(color.ToHSVString(), v => OnChange(new(v)), 9);
            });
            textInput.Label = "Code Input: " + color.ToHSVString();

            onValueChange?.Invoke(color);
        }

        public override void Render(Vector2 position, bool highlighted)
        {
            base.Render(position, highlighted);
            Vector2 vector = new(position.X, position.Y - (Height() / 2f));
            bool flag = Container.InnerContent == TextMenu.InnerContentMode.TwoColumn && !AlwaysCenter;
            Vector2 vector2 = vector + (Vector2.UnitY * TitleHeight / 2f) + (flag ? Vector2.Zero : new(Container.Width * 0.5f, 0f));
            // get the width of the text
            float textWidth = ActiveFont.Measure(Label).X + (Icon.Width * 2);
            // draw the hair sample at the location just after the text
            hairTexture.DrawCentered(vector2 + (textWidth * Vector2.UnitX), color.ToColor(), 4);
        }
    }
}
