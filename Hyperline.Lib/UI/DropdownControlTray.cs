namespace Celeste.Mod.Hyperline.Lib.UI;

using Microsoft.Xna.Framework;
using Monocle;

// a button that, when pressed, opens a tray containing a single control
public class DropdownControlTray : HMenuItem
{
    private bool containerAutoScroll;
    protected MTexture Icon { get; set; }
    public HMenuItem Control { get; set; }
    public string Label { get; set; }

    const float padding = 4f;

    private float ease;

    private Color highlightColor;

    public DropdownControlTray(string label, HMenuItem control)
    {
        Control = control;
        Label = label;
        Icon = GFX.Gui["downarrow"];
        Selectable = true;
    }

    public override void ConfirmPressed()
    {
        Enter();
        Audio.Play(SFX.ui_main_button_select);
        base.ConfirmPressed();
    }

    public override float Height() => ActiveFont.LineHeight + (Ease.QuadOut(ease)  * Control.Height());

    public override void Enter()
    {
        containerAutoScroll = Container.AutoScroll;
        Container.AutoScroll = false;

        Control.Added();

        highlightColor = TextMenu.HighlightColorA;

        base.Enter();
        // open the tray
        Control.Enter();
        ease = 0f;
    }

    public override void Added()
    {
        base.Added();

        // control should only be added after the menu is initialized
        // set the parent of the control
        Control.Parent = this;
        Control.Container = Container;

        Control.Added();
    }

    public override void Exit()
    {
        Audio.Play("event:/ui/main/button_back");
        base.Exit();
        Container.AutoScroll = containerAutoScroll;
    }


    public override void Render(Vector2 position, bool highlighted)
    {
        base.Render(position, highlighted);

        if (Settings.Instance.DisableFlashes)
        {
            highlightColor = TextMenu.HighlightColorA;
        }
        else if (Engine.Scene.OnRawInterval(0.1f))
        {
            highlightColor = highlightColor == TextMenu.HighlightColorA ? TextMenu.HighlightColorB : TextMenu.HighlightColorA;
        }

        float alpha = Container.Alpha;
        Color color = Disabled ? Color.DarkSlateGray : (highlighted ? Container.HighlightColor : Color.White) * alpha;
        Color strokeColor = Color.Black * (alpha * alpha * alpha);

        ease = Calc.Approach(ease, ShouldRender ? 1f : 0f, Engine.RawDeltaTime * 4f);

        Vector2 topLeft = new(position.X, position.Y - (Height() / 2f));
        // draw the label
        Vector2 labelPosition = topLeft + new Vector2(ActiveFont.Measure(Label).X / 2, Container.ItemSpacing + (ActiveFont.LineHeight / 2));
        ActiveFont.DrawOutline(Label, labelPosition, new(0.5f, 0.5f), Vector2.One, color, 2f, strokeColor);
        // draw the icon right of the label
        Color iconColor = (Disabled ? Color.DarkSlateGray : (Focused || Control.Focused) ? Container.HighlightColor : Color.White) * alpha;
        Icon.Draw(topLeft + new Vector2(ActiveFont.Measure(Label).X + (Icon.Width / 2f), ActiveFont.LineHeight / 2), new(0.5f, 0.5f), iconColor);
        // draw the control if it's open
        if (Control.ShouldRender && ease > 0.9f) // ease in
        {
            Control.Render(topLeft + new Vector2(0f, ActiveFont.LineHeight) + (Vector2.UnitY * Control.Height() / 2), highlighted);
        }
    }

    public override void Update()
    {
        base.Update();

        // if we are focused, it means the control handed focus back to us. close the tray.
        if (Focused && !Control.Focused)
        {
            Exit();
        }
        Control.Update();
    }
}
