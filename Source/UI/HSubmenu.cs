namespace Celeste.Mod.Hyperline.UI
{

    using System;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;
    using Monocle;

    public class HSubMenu : HMenuItem
    {
        public List<TextMenu.Item> Items { get; private set; }

        public TextMenu.Item Current
        {
            get
            {
                if (Items.Count <= 0 || Selection < 0)
                {
                    return null;
                }
                return Items[Selection];
            }
            set => Selection = Items.IndexOf(value);
        }

        public int FirstPossibleSelection
        {
            get
            {
                for (int i = 0; i < Items.Count; i++)
                {
                    if (Items[i] != null && Items[i].Hoverable)
                    {
                        return i;
                    }
                }
                return 0;
            }
        }

        public int LastPossibleSelection
        {
            get
            {
                for (int i = Items.Count - 1; i >= 0; i--)
                {
                    if (Items[i] != null && Items[i].Hoverable)
                    {
                        return i;
                    }
                }
                return 0;
            }
        }

        public float ScrollTargetY
        {
            get
            {
                float num = Engine.Height - 150f - (Container.Height * Container.Justify.Y);
                float num2 = 150f + (Container.Height * Container.Justify.Y);
                return Calc.Clamp((Engine.Height / 2) + (Container.Height * Container.Justify.Y) - GetYOffsetOf(Current), num, num2);
            }
        }

        public float TitleHeight { get; private set; }

        public float MenuHeight { get; private set; }

        public HSubMenu(string label, bool enterOnSelect)
        {
            ConfirmSfx = "event:/ui/main/button_select";
            Label = label;
            Icon = GFX.Gui["downarrow"];
            Selectable = true;
            IncludeWidthInMeasurement = true;
            this.enterOnSelect = enterOnSelect;
            OnEnter = delegate
            {
                if (this.enterOnSelect)
                {
                    ConfirmPressed();
                }
            };
            Items = [];
            delayedAddItems = [];
            Selection = -1;
            ItemSpacing = 4f;
            ItemIndent = 20f;
            HighlightColor = Color.White;
            RecalculateSize();
        }

        public HSubMenu Add(TextMenu.Item item)
        {
            if (Container != null)
            {
                Items.Add(item);
                item.Container = Container;
                Container.Add(item.ValueWiggler = Wiggler.Create(0.25f, 3f, null, false, false));
                Container.Add(item.SelectWiggler = Wiggler.Create(0.25f, 3f, null, false, false));
                item.ValueWiggler.UseRawDeltaTime = item.SelectWiggler.UseRawDeltaTime = true;
                if (Selection == -1)
                {
                    FirstSelection();
                }

                if (item is HMenuItem hsubmenu)
                {
                    hsubmenu.Parent = this;
                }

                RecalculateSize();
                item.Added();
                return this;
            }
            delayedAddItems.Add(item);
            return this;
        }

        public HSubMenu Add(IEnumerable<TextMenu.Item> newItems)
        {
            foreach (TextMenu.Item item in newItems)
            {
                Add(item);
            }
            return this;
        }

        public HSubMenu Insert(int index, TextMenu.Item item)
        {
            if (Container != null)
            {
                Items.Insert(index, item);
                item.Container = Container;
                Container.Add(item.ValueWiggler = Wiggler.Create(0.25f, 3f, null, false, false));
                Container.Add(item.SelectWiggler = Wiggler.Create(0.25f, 3f, null, false, false));
                item.ValueWiggler.UseRawDeltaTime = item.SelectWiggler.UseRawDeltaTime = true;

                if (item is HMenuItem hsubmenu)
                {
                    hsubmenu.Parent = this;
                }

                if (Selection == -1)
                {
                    FirstSelection();
                }
                RecalculateSize();
                item.Added();
                return this;
            }
            delayedAddItems.Insert(index, item);
            return this;
        }

        public bool ContainsDelayedAddItem(TextMenu.Item item) => Container == null && delayedAddItems.Contains(item);

        public HSubMenu InsertDelayedAddItem(TextMenu.Item item, TextMenu.Item after)
        {
            if (Container == null && delayedAddItems.Contains(after))
            {
                delayedAddItems.Insert(delayedAddItems.IndexOf(after) + 1, item);
            }
            return this;
        }

        public HSubMenu Remove(TextMenu.Item item)
        {
            if (Container == null)
            {
                delayedAddItems.Remove(item);
                return this;
            }
            if (!Items.Remove(item))
            {
                return this;
            }



            if (item is HSubMenu hsubmenu)
            {
                hsubmenu.Parent = this;
            }
            item.Container = null;
            Container.Remove(item.ValueWiggler);
            Container.Remove(item.SelectWiggler);
            RecalculateSize();
            return this;
        }

        public void Clear() => Items = [];

        public int IndexOf(TextMenu.Item item) => Items.IndexOf(item);

        public void FirstSelection()
        {
            Selection = -1;
            MoveSelection(1, false);
        }

        public void LastSelection()
        {
            Selection = LastPossibleSelection;
            MoveSelection(0, false);
        }

        public void MoveSelection(int direction, bool wiggle = false)
        {
            int selection = Selection;
            direction = Math.Sign(direction);
            int num = 0;
            using (List<TextMenu.Item>.Enumerator enumerator = Items.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (enumerator.Current.Hoverable)
                    {
                        num++;
                    }
                }
            }
            for (; ; )
            {
                Selection += direction;
                if (enterOnSelect && (Selection < 0 || Selection >= Items.Count))
                {
                    break;
                }
                if (num > 2)
                {
                    if (Selection < 0)
                    {
                        Selection = Items.Count - 1;
                    }
                    else if (Selection >= Items.Count)
                    {
                        Selection = 0;
                    }
                }
                else if (Selection < 0 || Selection > Items.Count - 1)
                {
                    goto IL_00F3;
                }
                if (Current.Hoverable)
                {
                    goto IL_0124;
                }
            }
            Selection = selection;
            Exit();
            Container.MoveSelection(direction, true);
            return;
            IL_00F3:
            Selection = Calc.Clamp(Selection, 0, Items.Count - 1);
            IL_0124:
            if (!Current.Hoverable)
            {
                Selection = selection;
            }
            if (Selection != selection && Current != null)
            {
                if (selection >= 0 && Items[selection] != null && Items[selection].OnLeave != null)
                {
                    Items[selection].OnLeave();
                }
                Current.OnEnter?.Invoke();
                if (wiggle)
                {
                    Audio.Play(direction > 0 ? "event:/ui/main/rollover_down" : "event:/ui/main/rollover_up");
                    Current.SelectWiggler.Start();
                }
            }
        }

        public void RecalculateSize()
        {
            TitleHeight = ActiveFont.LineHeight;
            if (Items.Count < 1)
            {
                return;
            }
            LeftColumnWidth = RightColumnWidth = MenuHeight = 0f;
            foreach (TextMenu.Item item in Items)
            {
                if (item.IncludeWidthInMeasurement)
                {
                    LeftColumnWidth = Math.Max(LeftColumnWidth, item.LeftWidth());
                }
            }
            foreach (TextMenu.Item item2 in Items)
            {
                if (item2.IncludeWidthInMeasurement)
                {
                    RightColumnWidth = Math.Max(RightColumnWidth, item2.RightWidth());
                }
            }
            foreach (TextMenu.Item item3 in Items)
            {
                if (item3.Visible)
                {
                    MenuHeight += item3.Height() + Container.ItemSpacing;
                }
            }
            MenuHeight -= Container.ItemSpacing;
        }

        public float GetYOffsetOf(TextMenu.Item item)
        {
            float num = Container.GetYOffsetOf(this) - (Height() * 0.5f);
            if (item == null)
            {
                return num + (TitleHeight * 0.5f);
            }
            num += TitleHeight;
            foreach (TextMenu.Item item2 in Items)
            {
                if (item2.Visible)
                {
                    num += item2.Height() + ItemSpacing;
                }
                if (item2 == item)
                {
                    break;
                }
            }
            return num - (item.Height() * 0.5f) - ItemSpacing;
        }

        public override void Exit()
        {
            TextMenu.Item item = Current;
            item?.OnLeave?.Invoke();
            if (!Input.MenuUp.Repeating && !Input.MenuDown.Repeating)
            {
                Audio.Play("event:/ui/main/button_back");
            }
            base.Exit();
            Container.AutoScroll = containerAutoScroll;
        }

        public override string SearchLabel() => Label;

        public override void ConfirmPressed()
        {
            if (Items.Count > 0)
            {
                if (Input.MenuUp.Pressed)
                {
                    LastSelection();
                }
                else
                {
                    FirstSelection();
                }
                TakeFocus();
                containerAutoScroll = Container.AutoScroll;
                Container.AutoScroll = false;
                Enter();
                if (!Input.MenuUp.Repeating && !Input.MenuDown.Repeating)
                {
                    Audio.Play(ConfirmSfx);
                }
                base.ConfirmPressed();
            }
        }

        public override float LeftWidth() => ActiveFont.Measure(Label).X;

        public override float RightWidth() => Icon.Width;

        public override float Height()
        {
            if (Items.Count > 0)
            {
                return TitleHeight + (MenuHeight * Ease.QuadOut(ease));
            }
            return TitleHeight;
        }

        public override void Added()
        {
            base.Added();
            foreach (TextMenu.Item item in delayedAddItems)
            {
                Add(item);
            }
        }

        public override void LoseFocus()
        {
            base.LoseFocus();
            Logger.Log(LogLevel.Error, "Hyperline", "HSubmenu lost focus");
        }

        public override void Update()
        {
            ease = Calc.Approach(ease, ShouldRender ? 1f : 0f, Engine.RawDeltaTime * 4f);
            base.Update();
            if (Focused && ease > 0.9f)
            {
                if (Input.MenuDown.Pressed && (!Input.MenuDown.Repeating || Selection != LastPossibleSelection || enterOnSelect))
                {
                    MoveSelection(1, true);
                }
                else if (Input.MenuUp.Pressed && (!Input.MenuUp.Repeating || Selection != FirstPossibleSelection || enterOnSelect))
                {
                    MoveSelection(-1, true);
                }
                if (Current != null)
                {
                    if (Input.MenuLeft.Pressed)
                    {
                        Current.LeftPressed();
                    }
                    if (Input.MenuRight.Pressed)
                    {
                        Current.RightPressed();
                    }
                    if (Input.MenuConfirm.Pressed)
                    {
                        Current.ConfirmPressed();
                        Current.OnPressed?.Invoke();
                    }
                    if (Input.MenuJournal.Pressed && Current.OnAltPressed != null)
                    {
                        Current.OnAltPressed();
                    }
                }
                if (!Input.MenuConfirm.Pressed && (Input.MenuCancel.Pressed || Input.ESC.Pressed || Input.Pause.Pressed))
                {
                    Exit();
                }
            }
            foreach (TextMenu.Item item in Items)
            {
                item.OnUpdate?.Invoke();
                item.Update();
            }
            if (Settings.Instance.DisableFlashes)
            {
                HighlightColor = TextMenu.HighlightColorA;
            }
            else if (Engine.Scene.OnRawInterval(0.1f))
            {
                if (HighlightColor == TextMenu.HighlightColorA)
                {
                    HighlightColor = TextMenu.HighlightColorB;
                }
                else
                {
                    HighlightColor = TextMenu.HighlightColorA;
                }
            }
            if (Focused && containerAutoScroll)
            {
                if (Container.Height > Container.ScrollableMinSize)
                {
                    TextMenu container = Container;
                    container.Position.Y += (ScrollTargetY - Container.Position.Y) * (1f - (float)Math.Pow(0.009999999776482582, Engine.RawDeltaTime));
                    return;
                }
                Container.Position.Y = 540f;
            }
        }

        public override void Render(Vector2 position, bool highlighted)
        {
            Vector2 vector = new(position.X, position.Y - (Height() / 2f));
            float alpha = Container.Alpha;
            Color color = Disabled ? Color.DarkSlateGray : (highlighted ? Container.HighlightColor : Color.White) * alpha;
            Color color2 = Color.Black * (alpha * alpha * alpha);
            bool flag = Container.InnerContent == TextMenu.InnerContentMode.TwoColumn && !AlwaysCenter;
            Vector2 vector2 = vector + (Vector2.UnitY * TitleHeight / 2f) + (flag ? Vector2.Zero : new(Container.Width * 0.5f, 0f));
            Vector2 vector3 = flag ? new(0f, 0.5f) : new Vector2(0.5f, 0.5f);
            Vector2 vector4 = flag ? new(ActiveFont.Measure(Label).X + Icon.Width, 5f) : new Vector2((ActiveFont.Measure(Label).X / 2f) + Icon.Width, 5f);
            DrawIcon(vector2, Icon, vector4, true, (Disabled || Items.Count < 1 ? Color.DarkSlateGray : Focused ? Container.HighlightColor : Color.White) * alpha, 0.8f);
            ActiveFont.DrawOutline(Label, vector2, vector3, Vector2.One, color, 2f, color2);
            if (ShouldRender && ease > 0.9f)
            {
                Vector2 vector5 = new(vector.X + ItemIndent, vector.Y + TitleHeight + ItemSpacing);
                RecalculateSize();
                foreach (TextMenu.Item item in Items)
                {
                    if (item.Visible)
                    {
                        float num = item.Height();
                        Vector2 vector6 = vector5 + new Vector2(0f, (num * 0.5f) + (item.SelectWiggler.Value * 8f));
                        if (vector6.Y + (num * 0.5f) > 0f && vector6.Y - (num * 0.5f) < Engine.Height)
                        {
                            item.Render(vector6, Focused && Current == item);
                        }
                        vector5.Y += num + ItemSpacing;
                    }
                }
            }
        }

        private static void DrawIcon(Vector2 position, MTexture icon, Vector2 justify, bool outline, Color color, float scale)
        {
            if (outline)
            {
                icon.DrawOutlineCentered(position + justify, color);
                return;
            }
            icon.DrawCentered(position + justify, color, scale);
        }

        public string Label { get; set; }
        protected MTexture Icon { get; set; }
        private readonly List<TextMenu.Item> delayedAddItems;
        public int Selection { get; set; }
        public float ItemSpacing { get; set; }
        public float ItemIndent { get; set; }
        private Color HighlightColor { get; set; }
        public string ConfirmSfx { get; set; }
        public bool AlwaysCenter { get; set; }
        public float LeftColumnWidth { get; set; }
        public float RightColumnWidth { get; set; }
        private readonly bool enterOnSelect;
        private float ease;
        private bool containerAutoScroll;
    }

}
