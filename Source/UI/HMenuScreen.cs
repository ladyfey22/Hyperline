namespace Celeste.Mod.Hyperline.UI;

using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Mod.UI;
using Monocle;

// License notice for maddie480, for Maddie's helping hand (thank you maddie)
/*
The MIT License (MIT)

Copyright (c) 2019 maddie480

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

// Everest license
/*
The MIT License (MIT)

Copyright (c) 2018 Everest Team

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/


/// <summary>
/// A base class for a new menu screen, allowing for easy integration with Everest's UI system.
/// Based on the Everest UI system and Extended Variants by maddie480
/// https://github.com/EverestAPI/Everest/blob/master/Celeste.Mod.mm/Mod/UI/OuiModOptions.cs
/// https://github.com/maddie480/ExtendedVariantMode/blob/master/UI/AbstractSubmenu.cs
/// </summary>
public abstract class HMenuScreen : Oui, OuiModOptions.ISubmenu {

    private TextMenu menu;

    private const float OnScreenX = 960f;
    private const float OffScreenX = 2880f;

    private float alpha;

    public string MenuName { get; protected set; }
    public string ButtonName { get; protected set;  }

    private Action backToParentMenu;
    private object[] parameters;

    /// <summary>
    /// Builds a submenu. The names expected here are dialog IDs.
    /// </summary>
    /// <param name="menuName">The title that will be displayed on top of the menu</param>
    /// <param name="buttonName">The name of the button that will open the menu from the parent submenu</param>
    protected HMenuScreen(string menuName, string buttonName)
    {
        MenuName = menuName;
        ButtonName = buttonName;
    }

    /// <summary>
    /// Adds all the submenu options to the TextMenu given in parameter.
    /// </summary>
    protected abstract void AddOptionsToMenu(TextMenu menu, bool inGame, object[] parameters);

    /// <summary>
    /// Gives the title that will be displayed on top of the menu.
    /// </summary>
    protected virtual string GetMenuName(object[] param) => Dialog.Clean(MenuName);

    /// <summary>
    /// Gives the name of the button that will open the menu from the parent submenu.
    /// </summary>
    public virtual string GetButtonName(object[] param)
    {
        return Dialog.Clean(ButtonName);
    }

    /// <summary>
    /// Builds the text menu, that can be either inserted into the pause menu, or added to the dedicated Oui screen.
    /// </summary>
    private TextMenu BuildMenu(bool inGame)
    {
        TextMenu menu = new();

        menu.Add(new TextMenu.Header(GetMenuName(parameters)));
        AddOptionsToMenu(menu, inGame, parameters);

        return menu;
    }

    // === some Oui plumbing

    public override IEnumerator Enter(Oui from)
    {
        // test
        Logger.Log(LogLevel.Error, "Hyperline/HMenuScreen", $"Entering {GetType().Name} from {from.GetType().Name}");
        menu = BuildMenu(false);
        Scene.Add(menu);

        menu.Visible = Visible = true;
        menu.Focused = false;

        for (float p = 0f; p < 1f; p += Engine.DeltaTime * 4f) {
            menu.X = OffScreenX + (-1920f * Ease.CubeOut(p));
            alpha = Ease.CubeOut(p);
            // wait frames to allow for the animation to be smooth
            Logger.Log("Hyperline/HMenuScreen", $"Waiting for animation to finish: {p}");
            yield return null;
        }

        menu.Focused = true;

        Logger.Log(LogLevel.Error, "Hyperline/HMenuScreen", $"Leaving base Enter");
    }

    public override IEnumerator Leave(Oui next)
    {
        Audio.Play(SFX.ui_main_whoosh_large_out);
        menu.Focused = false;

        for (float p = 0f; p < 1f; p += Engine.DeltaTime * 4f)
        {
            menu.X = OnScreenX + 1920f * Ease.CubeIn(p);
            alpha = 1f - Ease.CubeIn(p);
            yield return null;
        }

        menu.Visible = Visible = false;
        menu.RemoveSelf();
        menu = null;
    }

    public override void Update()
    {
        if (menu != null && menu.Focused && Selected && Input.MenuCancel.Pressed) {
            Audio.Play(SFX.ui_main_button_back);
            backToParentMenu();
        }

        base.Update();
    }

    public override void Render()
    {
        if (alpha > 0f) {
            Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black * alpha * 0.4f);
        }
        base.Render();
    }

    // === / some Oui plumbing

    /// <summary>
    /// Supposed to just contain "overworld.Goto<ChildType>()".
    /// </summary>
    protected abstract void GotoMenu(Overworld overworld);

    /// <summary>
    /// Builds a button that opens the menu with specified type when hit.
    /// </summary>
    /// <param name="parentMenu">The parent's TextMenu</param>
    /// <param name="inGame">true if we are in the pause menu, false if we are in the overworld</param>
    /// <param name="backToParentMenu">Action that will be called to go back to the parent menu</param>
    /// <param name="parameters">some arbitrary parameters that can be used to build the menu</param>
    /// <returns>A button you can insert in another menu</returns>
    public static TextMenu.Button BuildOpenMenuButton<T>(TextMenu parentMenu, bool inGame, Action backToParentMenu, object[] parameters) where T : HMenuScreen => GetOrInstantiateSubmenu<T>().BuildOpenMenuButton(parentMenu, inGame, backToParentMenu, parameters);

    private static T GetOrInstantiateSubmenu<T>() where T : HMenuScreen
    {
        if (OuiModOptions.Instance?.Overworld != null)
        {
            return OuiModOptions.Instance.Overworld.GetUI<T>();
        }

        // this is a very edgy edge case. but it still might happen. :maddyS:
        Logger.Log(LogLevel.Warn, "Hyperline/HMenuScreen", $"Overworld does not exist, instanciating submenu {typeof(T)} on the spot!");
        return (T) Activator.CreateInstance(typeof(T));
    }

    /// <summary>
    /// Method getting called on the Oui instance when the method just above is called.
    /// </summary>
    private TextMenu.Button BuildOpenMenuButton(TextMenu parentMenu, bool inGame, Action backToParentMenuNew, object[] param) {
        if (inGame) {
            // this is how it works in-game
            return (TextMenu.Button) new TextMenu.Button(GetButtonName(param)).Pressed(() => {
                Level level = Engine.Scene as Level;

                // set up the menu instance
                backToParentMenu = backToParentMenuNew;
                parameters = param;

                // close the parent menu
                parentMenu.RemoveSelf();

                // create our menu and prepare it
                TextMenu thisMenu = BuildMenu(true);

                // notify the pause menu that we aren't in the main menu anymore (hides the strawberry tracker)
                bool comesFromPauseMainMenu = level!.PauseMainMenuOpen;
                level.PauseMainMenuOpen = false;

                thisMenu.OnESC = thisMenu.OnCancel = () => {
                    // close this menu
                    Audio.Play(SFX.ui_main_button_back);

                    //ExtendedVariantsModule.Instance.SaveSettings();
                    thisMenu.Close();

                    // and open the parent menu back (this should work, right? we only removed it from the scene earlier, but it still exists and is intact)
                    // "what could possibly go wrong?" ~ famous last words
                    level.Add(parentMenu);

                    // restore the pause "main menu" flag to make strawberry tracker appear again if required.
                    level.PauseMainMenuOpen = comesFromPauseMainMenu;
                };

                thisMenu.OnPause = () => {
                    // we're unpausing, so close that menu, and save the mod Settings because the Mod Options menu won't do that for us
                    Audio.Play(SFX.ui_main_button_back);

                    //ExtendedVariantsModule.Instance.SaveSettings();
                    thisMenu.Close();

                    level.Paused = false;
                    Engine.FreezeTimer = 0.15f;
                };

                // finally, add the menu to the scene
                level.Add(thisMenu);
            });
        } else {
            // this is how it works in the main menu: way more simply than the in-game mess.
            return (TextMenu.Button) new TextMenu.Button(GetButtonName(param)).Pressed(() => {
                // set up the menu instance
                this.backToParentMenu = backToParentMenuNew;
                this.parameters = param;

                GotoMenu(OuiModOptions.Instance.Overworld);
            });
        }
    }
}
