namespace Celeste.Mod.Hyperline.UI;

using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using ActiveFont = global::Celeste.ActiveFont;
using Audio = global::Celeste.Audio;
using Dialog = global::Celeste.Dialog;
using Input = global::Celeste.Input;
using SFX = global::Celeste.SFX;

public class KeyboardInput : HMenuItem
{

    private bool useKeyboardInput;

    private bool containerAutoScroll;

    // keyboard letters based on user language
    private readonly List<string> letters;

    // the current input
    private string inputValue;
    private string initialValue;
    public string Value
    {
        get => inputValue;
        set
        {
            inputValue = value;
            onInput?.Invoke(value);
        }
    }

    public void SetInitialValue(string v) => initialValue = v;

    private bool UseKeyboardInput // this re-gets it from the settings, we need the other in place to know if we need to unhook
    {
        get
        {
            CoreModuleSettings settings = CoreModule.Instance._Settings as CoreModuleSettings;
            return (settings?.UseKeyboardForTextInput ?? false) && !forceControllerInput;
        }
    }

    public bool Valid { get; set; } = true;

    private readonly bool forceControllerInput;

    // length controls
    private int minLength;
    private readonly int maxLength;

    // on confirm (pass the input)
    private Action<string> onConfirm;
    // function to call when the user changes the input
    private Action<string> onInput;


    // sizing
    // widest and tallest letter
    private readonly Vector2 letterSize;
    // scale
    private readonly Vector2 scale;
    // size of the keyboard so we can easily calculate the size of the entire element, and allow us to center the input
    private readonly Vector2 keypadSize;
    private const float LowerPad = 10f;

    // selected character info
    private int selectedLine;
    private int selectedChar;

    // timing info
    private float timer;

    // selection for control characters (cancel, space, backspace, accept). if selectedLine > the number of lines in character, we are selecting a control character
    private List<string> controlChars;



    // colors ripped from the game's keyboard selection
    private readonly Color unselectColor = Color.LightGray;
    private readonly Color selectColorA = Calc.HexToColor("84FF54");
    private readonly Color selectColorB = Calc.HexToColor("FCFF59");

    // for inputs that directly interact with buttons, we need to wait the first frame to avoid double input.
    // when Update is called for the first time, the input is not yet cleared, so we need to wait for the next frame
    private bool wasFocused;
    private bool willExit;

    public KeyboardInput(string initialV, Action<string> onInput = null, Action<string> onConfirm = null, int minLength = 0, int maxLength = int.MaxValue, Vector2 sc = default, bool forceControllerInput = false)
    {
        initialValue = initialV;
        inputValue = initialValue;
        this.onInput = onInput;
        this.onConfirm = onConfirm;
        this.minLength = minLength;
        this.maxLength = maxLength;
        this.forceControllerInput = forceControllerInput;

        Selectable = true;
        // if scale is not set, default to 1
        scale = sc == default ? Vector2.One : sc;

        // now we get the letters based on the user language

        string letterChars = Dialog.Clean("name_letters");
        letters = letterChars.Split('\n').ToList();

        // calculate the widest letter and tallest letter, and make them letter size, this accounts for the scale
        foreach(string line in letters)
        {
            foreach(char c in line)
            {
                Vector2 size = ActiveFont.Measure(c.ToString());
                letterSize.X = Math.Max(size.X, letterSize.X);
                letterSize.Y = Math.Max(size.Y, letterSize.Y);
            }
        }

        // add the control characters
        controlChars = [ Dialog.Clean("name_back"), Dialog.Clean("name_space"), Dialog.Clean("name_backspace"), Dialog.Clean("name_accept") ];
        // for each, turn it to lowercase and make the first letter uppercase
        for (int i = 0; i < controlChars.Count; i++)
        {
            controlChars[i] = controlChars[i].ToLowerInvariant();
            controlChars[i] = string.Concat(controlChars[i].Substring(0, 1).ToUpperInvariant(), controlChars[i].AsSpan(1));
        }
        // apply the scale
        letterSize *= scale;
        // calculate the size of the keyboard. we need to know the size of the widest letter, the tallest letter, the scale, and the longest line
        keypadSize = new Vector2(letterSize.X * letters.Max(l => l.Length), letterSize.Y * letters.Count);
    }

    public KeyboardInput Change(Action<string> newOnInput)
    {
        onInput = newOnInput;
        return this;
    }

    public KeyboardInput Confirm(Action<string> newOnConfirm)
    {
        onConfirm = newOnConfirm;
        return this;
    }

    private void AppendChar(char c)
    {
        if(Value.Length < maxLength)
        {
            Value += c;
            // make a sound. space has a special sound
            Audio.Play(c == ' ' ? SFX.ui_main_rename_entry_space : SFX.ui_main_rename_entry_char);
        }
        else
        {
            Audio.Play(SFX.ui_main_button_invalid);
        }
    }


    public override void Update()
    {
        base.Update();

        if(!wasFocused && Focused)
        {
            wasFocused = true;
            return;
        }


        if (!Focused)
        {
            return ;
        }

        // escape
        if (Input.ESC.Pressed)
        {
            Leave(false);
        }

        if(UseKeyboardInput) // if we are using the keyboard, we don't need to update the selection
        {
            return;
        }

        if (Input.MenuDown.Pressed)
        {
            selectedLine = Math.Min(selectedLine +1, letters.Count); // extra line for control characters
            // if we are on the control characters, we need to make sure we are not selecting a character that doesn't exist
            if (selectedLine == letters.Count)
            {
                selectedChar = Math.Min(selectedChar, controlChars.Count - 1);
            }
        }

        if (Input.MenuUp.Pressed)
        {
            selectedLine = Math.Max(selectedLine - 1, 0);

            // check the selected character to make sure it's not out of bounds
            if(selectedChar >= letters[selectedLine].Length)
            {
                selectedChar = letters[selectedLine].Length - 1;
            }
        }

        // on left and right we need to find the selected character, skipping spaces that are used for padding
        if (Input.MenuLeft.Pressed)
        {
            if (selectedLine < letters.Count)
            {
                do
                {
                    selectedChar = (selectedChar + letters[selectedLine].Length - 1) % letters[selectedLine].Length;
                } while (letters[selectedLine][selectedChar] == ' ');
            }
            else
            {
                selectedChar = Math.Max(0, selectedChar - 1);
            }
        }

        if (Input.MenuRight.Pressed)
        {
            if (selectedLine < letters.Count)
            {
                do
                {
                    selectedChar = (selectedChar + 1) % letters[selectedLine].Length;
                } while (letters[selectedLine][selectedChar] == ' ');
            }
            else
            {
                selectedChar = Math.Min(controlChars.Count - 1, selectedChar + 1);
            }
        }

        // confirm
        if (Input.MenuConfirm.Pressed)
        {
            // means we are choosing a character/control character

            // if it's a normal character, append it to the input
            if (selectedLine < letters.Count)
            {
                AppendChar(letters[selectedLine][selectedChar]);
            }
            else
            {
                // if it's a control selection, handle it
                switch (selectedChar)
                {
                    case 0: // back
                        Leave(false);
                        break;
                    case 1: // space
                        AppendChar(' ');
                        break;
                    case 2: // backspace
                        Backspace();
                        break;
                    case 3: // accept
                        Leave(true);
                        break;
                }
            }
        }

        if (Input.MenuCancel.Pressed)
        {
            if(Value.Length > 0)
            {
                Backspace();
            }
            else
            {
                Leave(false);
            }
        }

        timer += Engine.DeltaTime;
    }

    // when confirmed, focus
    public override void ConfirmPressed()
    {
        Enter();

        Audio.Play("event:/ui/main/button_select");
        base.ConfirmPressed();
    }

    public override void Enter()
    {
        containerAutoScroll = Container.AutoScroll;
        base.Enter();
        Container.AutoScroll = false;
        inputValue = initialValue;

        // set the initial value
        selectedLine = 0;
        selectedChar = 0;

        useKeyboardInput = UseKeyboardInput;
        if (useKeyboardInput)
        {
            TextInput.OnInput += OnTextInput;
        }

        // disable debug commands

        Engine.Commands.Enabled = false;

        wasFocused = false;
    }

    public void Leave(bool confirm)
    {
        if(confirm) // if we are confirming on exit, pass the input
        {
            onConfirm?.Invoke(Value);
            // the user may have changed the input, so we need to update the initial value
            initialValue = Value;
        }
        else
        {
            Value = initialValue; // if we are not confirming, reset the input. we use Value instead of inputValue because we want to call the onInput function
        }
        Exit();
    }


    public override void Exit()
    {
        base.Exit();
        Audio.Play("event:/ui/main/button_back");
        if (useKeyboardInput)
        {
            TextInput.OnInput -= OnTextInput;
        }
        // enable debug commands if the engine allows it
        Engine.Commands.Enabled = Celeste.PlayMode == Celeste.PlayModes.Debug;

        Container.AutoScroll = containerAutoScroll;
    }

    public void OnTextInput(char c)
    {
        if (c == (char)13)
        {
            // Enter - confirm.
            Leave(true);
        }
        else if (c == (char)8)
        {
            // Backspace - trim.
            Backspace();
        }
        else if (c == (char)22)
        {
            // Paste.
            Value += TextInput.GetClipboardText();
            if (Value.Length > maxLength)
            {
                Value = Value.Substring(0, maxLength);
            }
        }
        else if (c == (char)127)
        {
            // Delete. We treat it as backspace.
            Backspace();
        }
        else if (c == ' ')
        {
            AppendChar(' ');
        }
        else if (!char.IsControl(c))
        {
            // Any other character - append.
            if (ActiveFont.FontSize.Characters.ContainsKey(c))
            {
                AppendChar(c);
            }
            else
            {
                Audio.Play(SFX.ui_main_button_invalid);
            }
        }
    }
    private void Backspace()
    {
        if (Value.Length > 0)
        {
            Value = Value.Substring(0, Value.Length - 1);
            Audio.Play(SFX.ui_main_rename_entry_backspace);
        }
        else
        {
            Audio.Play(SFX.ui_main_button_invalid);
        }
    }

    public override float LeftWidth() => keypadSize.X;

    // we hide the on screen keyboard if we are using the keyboard for input
    public override float Height() => (UseKeyboardInput ? letterSize.Y : keypadSize.Y) + (letterSize.Y * 2) + (LowerPad * 2);

    private void DrawKeyboardControl(string text, Vector2 center, Vector2 justify, Vector2 textScale, bool selected)
    {
        ActiveFont.DrawOutline(text, center, justify, textScale, GetTextColor(selected), 2f, Color.Gray);
    }

    private Color GetTextColor(bool selected)
    {
        if (selected)
        {
            return Calc.BetweenInterval(timer, 0.1f) ? selectColorA : selectColorB;
        }

        return unselectColor;
    }

    public override void Render(Vector2 position, bool highlighted)
    {
        base.Render(position, highlighted);

        // render the input value centered in the middle of the keyboard, at the top
        Vector2 currentPosition = position - new Vector2(0, Height() / 2);

        Color textColor = Valid ? Color.White : Color.Red;
        ActiveFont.DrawOutline(Value, currentPosition + new Vector2(keypadSize.X / 2, letterSize.Y / 2), new(0.5f, 0.5f), scale, textColor, 2, Color.Black);
        currentPosition.Y += letterSize.Y + LowerPad;

        if(UseKeyboardInput)
        {
            // draw a message that the keyboard is being used. in the future use Dialog.Clean, for now, hardcode it
            ActiveFont.DrawOutline("Using Keyboard Input.", currentPosition + new Vector2(keypadSize.X / 2, letterSize.Y / 2), new(0.5f, 0.5f), scale, Color.Gray, 2, Color.Black);

            // add tooltip for Enter and ESC right below the "Using Keyboard Input" message
            currentPosition.Y += letterSize.Y + LowerPad;

            const float controlScale = 0.6f;
            float enterWidth = ButtonUI.Width(Dialog.Clean("name_accept"), Input.Pause) * scale.X * controlScale;
            ButtonUI.Render(currentPosition + new Vector2(keypadSize.X / 2 - enterWidth, letterSize.Y / 2), Dialog.Clean("name_accept"), Input.Pause, scale.X * controlScale, justifyX: 0.5f);

            // adjust the position.x to the right of the enter button
            float escWidth = ButtonUI.Width(Dialog.Clean("name_back"), Input.ESC) * scale.X * controlScale;
            // draw to the right of the enter button
            ButtonUI.Render(currentPosition + new Vector2(keypadSize.X / 2 + escWidth / 2, letterSize.Y / 2), Dialog.Clean("name_back"), Input.ESC, scale.X * controlScale, justifyX: 0.5f);

            return; // we don't need to render the keyboard if we are using the keyboard
        }


        // render the keyboard
        for (int lineIndex = 0; lineIndex < letters.Count; lineIndex++)
        {
            string line = letters[lineIndex];
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                bool selected = selectedLine == lineIndex && selectedChar == i;
                DrawKeyboardControl(c.ToString(), currentPosition + new Vector2(letterSize.X / 2, letterSize.Y / 2), new(0.5f,0.5f), scale, selected);
                currentPosition.X += letterSize.X;
            }

            currentPosition.X = position.X;
            currentPosition.Y += letterSize.Y;
        }

        // render the controls (back, space, backspace, accept)
        for (int i = 0; i < controlChars.Count; i++)
        {
            bool selected = selectedLine == letters.Count && selectedChar == i;
            Vector2 textSize = ActiveFont.Measure(controlChars[i]);
            DrawKeyboardControl(controlChars[i], currentPosition + new Vector2(textSize.X / 2, letterSize.Y / 2), new(0.5f, 0.5f), scale, selected);
            currentPosition.X += textSize.X + letterSize.X;
        }

    }
}
