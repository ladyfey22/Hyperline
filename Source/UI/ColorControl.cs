
namespace Celeste.Mod.Hyperline.UI
{
    using System;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Monocle;

    public class ColorControl : HMenuItem, IDisposable
    {
        private const int HSVBoxSize = 400;
        private readonly string label;
        private float h, s, v;
        private Action<float, float, float> onValueChange;
        private bool containerAutoScroll;
        private bool isReleased; // first entry we need to wait until the user stops pressing confirm before we allow another confirm

        private const float ValueSatScrollSpeed = 0.02f;
        private const float HSVScrollSpeed = 1f;
        private const float HSVScrollSpeedFast = 5f;
        private const float ValueSatScollSpeedFast = 0.1f;

        // TEXTURES
        private readonly MTexture gradientTexture; // hue gradient texture
        private readonly MTexture arrowTexture; // 
        private readonly MTexture selector;

        private MenuState menuState; // current state of the menu when focussed


        private static RenderTarget2D renderTarget;
        private static SpriteBatch spriteBatch;
        private static Texture2D hsvTexture;
        private static Effect hsvEffect;
        private const string ShaderName = "Effects/HSVEffect.cso";

        private enum MenuState
        {
            HueHovered,
            HueSelected,
            ValueSatHovered,
            ValueSatSelected
        }


        private readonly TextMenu.Button hueButton;
        private readonly TextMenu.Button valueButton;

        public override void Added()
        {
            base.Added();
            SetupButton(hueButton);
            SetupButton(valueButton);
        }

        public ColorControl(string label, float h, float s, float v)
        {
            this.label = label;
            this.h = h;
            this.s = s;
            this.v = v;
            gradientTexture = GFX.Gui["Hyperline/Gradient"];
            selector = GFX.Gui["Hyperline/selector"];
            arrowTexture = GFX.Gui["downarrow"];
            CreateRenderTarget();

            hueButton = new TextMenu.Button("Hue");
            valueButton = new TextMenu.Button("Value/Saturation");

            Selectable = true;
        }

        private void SetupButton(TextMenu.Item item)
        {
            if (Container != null)
            {
                item.Container = Container;
                Container.Add(item.ValueWiggler = Wiggler.Create(0.25f, 3f, null, false, false));
                Container.Add(item.SelectWiggler = Wiggler.Create(0.25f, 3f, null, false, false));
                item.ValueWiggler.UseRawDeltaTime = item.SelectWiggler.UseRawDeltaTime = true;
                item.Added();
            }
        }


        public ColorControl Change(Action<float, float, float> onValueChange)
        {
            this.onValueChange = onValueChange;
            return this;
        }

        public override void ConfirmPressed()
        {
            containerAutoScroll = Container.AutoScroll;
            Enter();
            Container.AutoScroll = false;
            isReleased = false; // wait until its released before we do anything with confirm
            menuState = MenuState.HueHovered;
            RegenerateTexture(); // regen the texture for the current selection

            Audio.Play("event:/ui/main/button_select");
            base.ConfirmPressed();
        }

        public override void Render(Vector2 position, bool highlighted)
        {
            // draw the label

            Vector2 topLeft = new(position.X, position.Y - (Height() / 2f)); //get the top left instead of the middle
            Vector2 labelPosition = topLeft + (Vector2.UnitY * ActiveFont.LineHeight / 2f);
            Vector2 textJustify = new(0.0f, 0.5f);

            float alpha = Container.Alpha;
            Color titleColor = Disabled ? Color.DarkSlateGray : ((highlighted ? Container.HighlightColor : Color.White) * alpha);
            Color outlineColor = Color.Black * (alpha * alpha * alpha);
            ActiveFont.DrawOutline(label, labelPosition, textJustify, Vector2.One, titleColor, 2f, outlineColor);


            spriteBatch.Begin();
            if (Focused)
            {
                Vector2 basePosition = labelPosition + (Vector2.UnitY * ActiveFont.LineHeight * 1.25f); // add an extra gap of 0.5 of a line height between the label and the start of the color menu


                // we need to make sure we add the arrow texture height offset
                // start by drawing the text
                Vector2 hueTextPosition = basePosition + new Vector2(0, (ActiveFont.LineHeight / 2) + arrowTexture.Height);
                Vector2 valueTextPosition = basePosition + new Vector2(gradientTexture.Width * 6.0f, (ActiveFont.LineHeight / 2) + arrowTexture.Height);

                hueButton.Render(hueTextPosition, menuState == MenuState.HueHovered && Focused);
                valueButton.Render(valueTextPosition, menuState == MenuState.ValueSatHovered && Focused);

                // we need the centers to center things up
                Vector2 hueTextPositionCenter = hueTextPosition + (Vector2.UnitX * hueButton.LeftWidth() / 2);
                Vector2 valueTextPositionCenter = valueTextPosition + (Vector2.UnitX * valueButton.LeftWidth() / 2);

                Vector2 hueArrowPosition = hueTextPositionCenter + (Vector2.UnitY * -ActiveFont.LineHeight); // center the arrow on the text
                Vector2 valueArrowPosition = valueTextPositionCenter + (Vector2.UnitY * -ActiveFont.LineHeight);

                // draw the arrow
                Vector2 arrowPosition = (menuState is MenuState.HueHovered or MenuState.HueSelected) ? hueArrowPosition : valueArrowPosition;
                Color arrowColor = (menuState is MenuState.HueSelected or MenuState.ValueSatSelected) ? Color.Yellow : Color.White; // if we are selecting something, highlight it to inform the user
                arrowTexture.DrawCentered(arrowPosition, arrowColor);

                // now using the centered hue text position and the centered value text position, draw the color selector and value selector
                Vector2 huePosition = hueTextPositionCenter + new Vector2(-gradientTexture.Width / 2, ActiveFont.LineHeight * 1.25f);
                Vector2 valuePosition = valueTextPositionCenter + new Vector2(-renderTarget.Width / 2, ActiveFont.LineHeight * 1.25f);

                gradientTexture.DrawOutline(huePosition);
                Draw.SpriteBatch.Draw(renderTarget, valuePosition, Color.White);


                // finally, draw the selectors
                Vector2 hueSelectorPosition = huePosition + new Vector2(gradientTexture.Width / 2, h / 360f * gradientTexture.Height); // scale the height based on the current hue
                Vector2 satValueSelectorPosition = valuePosition + new Vector2(s * renderTarget.Width, (1.0f - v) * renderTarget.Height); //scale the width and height based on hue/value, value is inverted due to its rendering

                // draw the selectors
                selector.DrawCentered(hueSelectorPosition, Color.Black, 2);
                selector.DrawCentered(satValueSelectorPosition, Color.Black, 2);
            }
            spriteBatch.End();
        }

        public override float Height() => ActiveFont.LineHeight + (Focused ? (HSVBoxSize + ActiveFont.LineHeight + arrowTexture.Height + (ActiveFont.LineHeight * 1.5f)) : 0);

        public override float LeftWidth() => ActiveFont.Measure(label).X;

        private static Texture2D TextureDotCreate(GraphicsDevice device)
        {
            Color[] data = [new Color(255, 255, 255, 255)];
            return TextureFromColorArray(device, data, 1, 1);
        }

        private static Texture2D TextureFromColorArray(GraphicsDevice device, Color[] data, int width, int height)
        {
            Texture2D tex = new(device, width, height);
            tex.SetData(data);
            return tex;
        }

        private void RegenerateTexture()
        {
            EffectParameter hueParam = hsvEffect.Parameters["hue"];
            hueParam?.SetValue(h / 360.0f);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, hsvEffect);
            Celeste.Graphics.GraphicsDevice.SetRenderTarget(renderTarget);
            spriteBatch.Draw(hsvTexture, new Rectangle(0, 0, HSVBoxSize, HSVBoxSize), Color.White);
            spriteBatch.End();
            Celeste.Graphics.GraphicsDevice.SetRenderTarget(null);
        }

        private static void CreateRenderTarget()
        {
            // if we are already initialized, we don't need to again, since only one color picker can be active at a time
            if (renderTarget != null)
            {
                return;
            }

            renderTarget = new RenderTarget2D(Celeste.Graphics.GraphicsDevice, HSVBoxSize, HSVBoxSize);
            spriteBatch = new SpriteBatch(Celeste.Graphics.GraphicsDevice);
            hsvTexture = TextureDotCreate(Celeste.Graphics.GraphicsDevice);

            ModAsset shaderCode = Everest.Content.Get(ShaderName);
            if (shaderCode == null)
            {
                Logger.Log(LogLevel.Error, "Hyperline", "Failed to load shader asset " + ShaderName);
            }
            else
            {
                try
                {
                    hsvEffect = new Effect(Celeste.Graphics.GraphicsDevice, shaderCode.Data);
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Error, "Hyperline", "Failed to load the shader " + ShaderName);
                    Logger.Log(LogLevel.Error, "Hyperline", "Exception \n " + ex.ToString());
                }
            }
        }

        public override void Update()
        {
            base.Update();

            if (Focused)
            {
                bool isFastHeld = Input.MenuConfirm.Check;
                float hsvSpeed = isFastHeld ? HSVScrollSpeedFast : HSVScrollSpeed;
                float valueSatSpeed = isFastHeld ? ValueSatScollSpeedFast : ValueSatScrollSpeed;

                if (Input.MenuDown.Pressed)
                {
                    if (menuState == MenuState.HueSelected) // we have hue selecting enabled, so do that instead
                    {
                        h = Math.Min(h + hsvSpeed, 359f); // bound it to 359
                        RegenerateTexture();
                        onValueChange?.Invoke(h, s, v);
                        Audio.Play("event:/ui/main/button_toggle_off");
                    }
                    else
                    if (menuState == MenuState.ValueSatSelected) // selecting value/saturation
                    {
                        v = Math.Max(v - valueSatSpeed, 0f);
                        onValueChange?.Invoke(h, s, v);
                        Audio.Play("event:/ui/main/button_toggle_off");
                    }
                    // if there is nothing selected, up and down does nothing
                }

                if (Input.MenuUp.Pressed)
                {
                    if (menuState == MenuState.HueSelected)
                    {
                        h = Math.Max(h - hsvSpeed, 0f); // bound it to zero
                        RegenerateTexture();
                        onValueChange?.Invoke(h, s, v);
                        Audio.Play("event:/ui/main/button_toggle_off");
                    }
                    else // we are selecting value instead
                    if (menuState == MenuState.ValueSatSelected)
                    {
                        v = Math.Min(v + valueSatSpeed, 1); // bound it to 100
                        onValueChange?.Invoke(h, s, v);
                        Audio.Play("event:/ui/main/button_toggle_off");
                    }
                }

                if (Input.MenuLeft.Pressed) // saturation, or menu
                {
                    if (menuState == MenuState.ValueSatSelected)
                    {
                        s = Math.Max(s - valueSatSpeed, 0);
                        onValueChange?.Invoke(h, s, v);
                        Audio.Play("event:/ui/main/button_toggle_off");
                    }
                    else
                    if (menuState == MenuState.ValueSatHovered)
                    {
                        menuState = MenuState.HueHovered;
                        onValueChange?.Invoke(h, s, v);
                        Audio.Play("event:/ui/main/button_toggle_off");
                    }
                }

                if (Input.MenuRight.Pressed)
                {
                    if (menuState == MenuState.ValueSatSelected)
                    {
                        s = Math.Min(s + valueSatSpeed, 1f);
                        onValueChange?.Invoke(h, s, v);
                        Audio.Play("event:/ui/main/button_toggle_off");
                    }
                    else
                    if (menuState == MenuState.HueHovered)
                    {
                        menuState = MenuState.ValueSatHovered;
                        onValueChange?.Invoke(h, s, v);
                        Audio.Play("event:/ui/main/button_toggle_off");
                    }
                }

                if (Input.MenuConfirm.Pressed && isReleased)
                {
                    Audio.Play("event:/ui/main/button_select");
                    if (menuState == MenuState.HueHovered)
                    {
                        menuState = MenuState.HueSelected;
                        Audio.Play("event:/ui/main/button_select");
                        isReleased = false;
                    }
                    else
                    if (menuState == MenuState.ValueSatHovered)
                    {
                        menuState = MenuState.ValueSatSelected;
                        Audio.Play("event:/ui/main/button_select");
                        isReleased = false;
                    }
                    RegenerateTexture();
                }

                if (!Input.MenuConfirm.Pressed && (Input.MenuCancel.Pressed || Input.ESC.Pressed || Input.Pause.Pressed))
                {
                    if (menuState is MenuState.HueHovered or MenuState.ValueSatHovered)
                    {
                        Focused = false;
                        Exit();
                        Container.AutoScroll = containerAutoScroll;
                        onValueChange?.Invoke(h, s, v);
                    }
                    else
                    {
                        if (menuState == MenuState.HueSelected)
                        {
                            menuState = MenuState.HueHovered;
                        }
                        else
                        if (menuState == MenuState.ValueSatSelected)
                        {
                            menuState = MenuState.ValueSatHovered;
                        }
                    }

                    Audio.Play("event:/ui/main/button_back");
                }
                isReleased = isReleased || !Input.MenuConfirm.Pressed;

                hueButton.Update();
                valueButton.Update();
            }
        }

        public void SetHSV(float h, float s, float v)
        {
            this.h = h;
            this.s = s;
            this.v = v;
        }

        public void Dispose()
        {
            hsvEffect.Dispose();
            hsvTexture.Dispose();
            renderTarget.Dispose();
        }
    }
}