namespace Celeste.Mod.Hyperline.UI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Xna.Framework;
    using Monocle;


    public class HOptionSubmenu : HMenuItem
    {
        public List<Tuple<string, List<TextMenu.Item>>> Menus { get; private set; }

        public List<TextMenu.Item> CurrentMenu
        {
            get
            {
                if (Menus.Count <= 0)
                {
                    return null;
                }
                return Menus[MenuIndex].Item2;
            }
        }

        public TextMenu.Item Current
        {
            get
            {
                if (CurrentMenu.Count <= 0 || Selection < 0)
                {
                    return null;
                }
                return CurrentMenu[Selection];
            }
        }

        public int FirstPossibleSelection
        {
            get
            {
                for (int i = 0; i < CurrentMenu.Count; i++)
                {
                    if (CurrentMenu[i] != null && CurrentMenu[i].Hoverable)
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
                for (int i = CurrentMenu.Count - 1; i >= 0; i--)
                {
                    if (CurrentMenu[i] != null && CurrentMenu[i].Hoverable)
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
                float num = Engine.Height - 150 - (Container.Height * Container.Justify.Y);
                float num2 = 150f + (Container.Height * Container.Justify.Y);
                return Calc.Clamp((Engine.Height / 2f) + (Container.Height * Container.Justify.Y) - GetYOffsetOf(Current), num, num2);
            }
        }

        public float TitleHeight { get; private set; }
        public float MenuHeight { get; private set; }

        public HOptionSubmenu(string label)
        {
            ConfirmSfx = "event:/ui/main/button_select";
            Label = label;
            Icon = GFX.Gui["downarrow"];
            Selectable = true;
            IncludeWidthInMeasurement = true;
            MenuIndex = 0;
            Menus = [];
            delayedAddMenus = [];
            Selection = -1;
            ItemSpacing = 4f;
            ItemIndent = 20f;
            highlightColor = Color.White;
            RecalculateSize();
        }

        public HOptionSubmenu Add(string label, List<TextMenu.Item> items)
        {
            if (Container != null)
            {
                if (items != null)
                {
                    foreach (TextMenu.Item item in items)
                    {
                        item.Container = Container;
                        Container.Add(item.ValueWiggler = Wiggler.Create(0.25f, 3f, null, false, false));
                        Container.Add(item.SelectWiggler = Wiggler.Create(0.25f, 3f, null, false, false));
                        item.ValueWiggler.UseRawDeltaTime = item.SelectWiggler.UseRawDeltaTime = true;

                        if (item is HMenuItem hItem)
                        {
                            hItem.Parent = this;
                        }

                        item.Added();
                    }
                    Menus.Add(new(label, items));
                }
                else
                {
                    Menus.Add(new(label, []));
                }
                if (Selection == -1)
                {
                    FirstSelection();
                }
                RecalculateSize();
                return this;
            }
            delayedAddMenus.Add(new(label, items));
            return this;
        }

        public HOptionSubmenu SetInitialSelection(int index)
        {
            InitialSelection = index;
            return this;
        }

        public void Clear() => Menus = [];

        public void FirstSelection()
        {
            Selection = -1;
            if (CurrentMenu.Count > 0)
            {
                MoveSelection(1, true);
            }
        }

        public void MoveSelection(int direction, bool wiggle = false)
        {
            int selection = Selection;
            direction = Math.Sign(direction);
            int num = 0;
            using (List<TextMenu.Item>.Enumerator enumerator = CurrentMenu.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (enumerator.Current is { Hoverable: true })
                    {
                        num++;
                    }
                }
            }
            for (; ; )
            {
                Selection += direction;
                if (num > 2)
                {
                    if (Selection < 0)
                    {
                        Selection = CurrentMenu.Count - 1;
                    }
                    else if (Selection >= CurrentMenu.Count)
                    {
                        Selection = 0;
                    }
                }
                else if (Selection < 0 || Selection > CurrentMenu.Count - 1)
                {
                    break;
                }
                if (Current.Hoverable)
                {
                    goto IL_00E5;
                }
            }
            Selection = Calc.Clamp(Selection, 0, CurrentMenu.Count - 1);
            IL_00E5:
            if (!Current.Hoverable)
            {
                Selection = selection;
            }

            if (Selection == selection || Current == null)
            {
                return;
            }

            if (selection >= 0 && CurrentMenu[selection] != null && CurrentMenu[selection].OnLeave != null)
            {
                CurrentMenu[selection].OnLeave();
            }

            Current.OnEnter?.Invoke();
            if (wiggle)
            {
                Audio.Play(direction > 0 ? "event:/ui/main/rollover_down" : "event:/ui/main/rollover_up");
                Current.SelectWiggler.Start();
            }
        }

        public void RecalculateSize()
        {
            TitleHeight = ActiveFont.LineHeight;
            LeftColumnWidth = RightColumnWidth = menuHeight = 0f;
            if (Menus.Count < 1 || CurrentMenu == null)
            {
                return;
            }
            foreach (TextMenu.Item item in CurrentMenu)
            {
                if (item.IncludeWidthInMeasurement)
                {
                    LeftColumnWidth = Math.Max(LeftColumnWidth, item.LeftWidth());
                }
            }
            foreach (TextMenu.Item item2 in CurrentMenu)
            {
                if (item2.IncludeWidthInMeasurement)
                {
                    RightColumnWidth = Math.Max(RightColumnWidth, item2.RightWidth());
                }
            }
            foreach (TextMenu.Item item3 in CurrentMenu)
            {
                if (item3.Visible)
                {
                    menuHeight += item3.Height() + Container.ItemSpacing;
                }
            }
            menuHeight -= Container.ItemSpacing;
        }

        public float GetYOffsetOf(TextMenu.Item item)
        {
            float num = Container.GetYOffsetOf(this) - (Height() * 0.5f);
            if (item == null)
            {
                return num + (TitleHeight * 0.5f);
            }
            num += TitleHeight;
            foreach (TextMenu.Item item2 in CurrentMenu)
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

        public override string SearchLabel() => Label;

        public HOptionSubmenu Change(Action<int> onValueChange)
        {
            OnValueChange = onValueChange;
            return this;
        }

        // Token: 0x0600367A RID: 13946 RVA: 0x001515B0 File Offset: 0x0014F7B0
        public override void LeftPressed()
        {
            if (MenuIndex <= 0)
            {
                return;
            }

            Audio.Play("event:/ui/main/button_toggle_off");
            MenuIndex--;
            lastDir = -1;
            ValueWiggler.Start();
            FirstSelection();
            Action<int> onValueChange = OnValueChange;
            onValueChange?.Invoke(MenuIndex);
        }

        // Token: 0x0600367B RID: 13947 RVA: 0x00151610 File Offset: 0x0014F810
        public override void RightPressed()
        {
            if (MenuIndex >= Menus.Count - 1)
            {
                return;
            }

            Audio.Play("event:/ui/main/button_toggle_on");
            MenuIndex++;
            lastDir = 1;
            ValueWiggler.Start();
            FirstSelection();
            Action<int> onValueChange = OnValueChange;
            onValueChange?.Invoke(MenuIndex);
        }

        public override void ConfirmPressed()
        {
            if (CurrentMenu.Count <= 0)
            {
                return;
            }

            containerAutoScroll = Container.AutoScroll;
            Container.AutoScroll = false;
            Enter();
            FirstSelection();
        }

        public override float LeftWidth() => ActiveFont.Measure(Label).X;

        public override float RightWidth()
        {
            float num = 0f;
            foreach (string text in Menus.Select(tuple => tuple.Item1))
            {
                num = Math.Max(num, ActiveFont.Measure(text).X);
            }
            return num + 120f;
        }

        public override float Height() => TitleHeight + Math.Max(MenuHeight, 0f);

        public override void Added()
        {
            base.Added();
            foreach (Tuple<string, List<TextMenu.Item>> tuple in delayedAddMenus)
            {
                Add(tuple.Item1, tuple.Item2);
            }
            MenuIndex = InitialSelection;
        }

        public override void Update()
        {
            MenuHeight = Calc.Approach(MenuHeight, menuHeight, Engine.RawDeltaTime * Math.Abs(MenuHeight - menuHeight) * 8f);
            sine += Engine.RawDeltaTime;
            base.Update();
            if (CurrentMenu != null)
            {
                if (Focused)
                {
                    if (!wasFocused)
                    {
                        wasFocused = true;
                    }
                    else
                    {
                        if (Input.MenuDown.Pressed && (!Input.MenuDown.Repeating || Selection != LastPossibleSelection))
                        {
                            MoveSelection(1, true);
                        }
                        else if (Input.MenuUp.Pressed && (!Input.MenuUp.Repeating || Selection != FirstPossibleSelection))
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
                            TextMenu.Item item = Current;
                            item?.OnLeave?.Invoke();
                            Exit();
                            Audio.Play("event:/ui/main/button_back");
                            Container.AutoScroll = containerAutoScroll;
                        }
                    }
                }
                else
                {
                    wasFocused = false;
                }
                foreach (Tuple<string, List<TextMenu.Item>> tuple in Menus)
                {
                    foreach (TextMenu.Item item2 in tuple.Item2)
                    {
                        item2.OnUpdate?.Invoke();
                        item2.Update();
                    }
                }
                if (Settings.Instance.DisableFlashes)
                {
                    highlightColor = TextMenu.HighlightColorA;
                }
                else if (Engine.Scene.OnRawInterval(0.1f))
                {
                    highlightColor = highlightColor == TextMenu.HighlightColorA ? TextMenu.HighlightColorB : TextMenu.HighlightColorA;
                }

                if (!Focused || !containerAutoScroll)
                {
                    return;
                }

                if (Container.Height > Container.ScrollableMinSize)
                {
                    TextMenu container = Container;
                    container.Position.Y += (ScrollTargetY - Container.Position.Y) * (1f - (float)Math.Pow(0.009999999776482582, Engine.RawDeltaTime));
                    return;
                }
                Container.Position.Y = 540f;
            }
        }

        // Token: 0x06003682 RID: 13954 RVA: 0x00151B60 File Offset: 0x0014FD60
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
            MTexture icon = Icon;
            const bool flag2 = true;
            Color color3;
            if (!Disabled)
            {
                List<TextMenu.Item> currentMenu = CurrentMenu;
                if (currentMenu is not { Count: < 1 })
                {
                    color3 = Focused ? Container.HighlightColor : Color.White;
                    goto IL_019D;
                }
            }
            color3 = Color.DarkSlateGray;
            IL_019D:
            DrawIcon(vector2, icon, vector4, flag2, color3 * alpha, 0.8f);
            ActiveFont.DrawOutline(Label, vector2, vector3, Vector2.One, color, 2f, color2);
            if (Menus.Count > 0)
            {
                float num = RightWidth();
                ActiveFont.DrawOutline(Menus[MenuIndex].Item1, vector2 + new Vector2(Container.Width - (num * 0.5f) + (lastDir * ValueWiggler.Value * 8f), 0f), new(0.5f, 0.5f), Vector2.One * 0.8f, color, 2f, color2);
                Vector2 vector7 = Vector2.UnitX * (highlighted ? (float)Math.Sin(sine * 4f) * 4f : 0f);
                Color color4 = MenuIndex > 0 ? color : Color.DarkSlateGray * alpha;
                Vector2 vector8 = vector2 + new Vector2(Container.Width - num + 40f + (lastDir < 0 ? -ValueWiggler.Value * 8f : 0f), 0f) - (MenuIndex > 0 ? vector7 : Vector2.Zero);
                ActiveFont.DrawOutline("<", vector8, new(0.5f, 0.5f), Vector2.One, color4, 2f, color2);
                color4 = MenuIndex < Menus.Count - 1 ? color : Color.DarkSlateGray * alpha;
                vector8 = vector2 + new Vector2(Container.Width - 40f + (lastDir > 0 ? ValueWiggler.Value * 8f : 0f), 0f) + (MenuIndex < Menus.Count - 1 ? vector7 : Vector2.Zero);
                ActiveFont.DrawOutline(">", vector8, new(0.5f, 0.5f), Vector2.One, color4, 2f, color2);
            }

            if (CurrentMenu == null)
            {
                return;
            }

            Vector2 vector9 = new(vector.X + ItemIndent, vector.Y + TitleHeight + ItemSpacing);
            float y = vector9.Y;
            RecalculateSize();
            foreach (TextMenu.Item item in CurrentMenu)
            {
                if (!item.Visible)
                {
                    continue;
                }

                float num2 = item.Height();
                Vector2 vector10 = vector9 + new Vector2(0f, (num2 * 0.5f) + (item.SelectWiggler.Value * 8f));
                if (vector10.Y - y < MenuHeight && vector10.Y + (num2 * 0.5f) > 0f && vector10.Y - (num2 * 0.5f) < Engine.Height)
                {
                    item.Render(vector10, Focused && Current == item);
                }
                vector9.Y += num2 + ItemSpacing;
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
        private MTexture Icon { get; set; }
        private readonly List<Tuple<string, List<TextMenu.Item>>> delayedAddMenus;
        public int MenuIndex { get; set; }
        private int InitialSelection { get; set; }
        public int Selection { get; set; }
        private int lastDir;
        private float sine;
        public Action<int> OnValueChange { get; set; }
        public float ItemSpacing { get; set; }
        public float ItemIndent { get; set; }
        private Color highlightColor;
        public string ConfirmSfx { get; set; }
        public bool AlwaysCenter { get; set; }
        public float LeftColumnWidth { get; set; }
        public float RightColumnWidth { get; set; }
        private float menuHeight;
        private bool wasFocused;
        private bool containerAutoScroll;
    }
}
