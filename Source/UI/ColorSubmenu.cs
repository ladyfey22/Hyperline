
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
        private readonly DropdownControlTray textInput;

        //tooltip textmenu item
        private readonly TextMenuExt.SubHeaderExt tooltip;

        private HSVColor color;
        private readonly TextMenu menu;

        public ColorSubmenu(TextMenu menu, string label, HSVColor color, bool inGame) : base(label, false)
        {
            this.color = color;
            this.menu = menu;

            hairTexture = GFX.Game["characters/player/hair00"];

            colorControl = new ColorControl("Color Selector", color.H, color.S, color.V).Change((h, s, v) => OnChange(new(h, s, v)));
            // createcoloreditor expects something that gives an hsv color, and a lambda that sets the hsv color
            textInput = HyperlineUI.CreateColorEditor(menu, "HSV/RGB Input: ", () => color, OnChange);

            tooltip = new("HSV Format HHHSSSVVV or RGB Format RRGGBB");
            Add([colorControl, textInput, tooltip]);
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
