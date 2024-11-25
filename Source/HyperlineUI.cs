namespace Celeste.Mod.Hyperline
{
    using System;
    using global::Celeste.Mod.UI;
    using System.Collections.Generic;
    using System.Linq;
    using On.Monocle;
    using UI;

    public class HyperlineUI
    {
        private static int lastDash;
        private static int currentPreset;
        private TextMenu.Option<bool> enabledText;
        private TextMenu.Option<bool> allowMapHairText;
        private TextMenu.Option<bool> maddyCrownText;
        private TextMenu.Option<bool> doFeatherColorText;
        private TextMenu.Option<bool> doDashFlashText;

        private List<List<List<TextMenu.Item>>> colorMenus; //format is  [Dashes][Type][ColorNum]
        private UI.HOptionSubmenu dashCountMenu;

        private readonly List<KeyValuePair<uint, string>> presetList;

        public HyperlineUI()
        {
            colorMenus = [];
            presetList = [];
        }


        public static string StringFromInt(int v) => v.ToString();

        public void EnabledToggled(bool enabled)
        {
            Hyperline.Settings.Enabled = enabled;
            allowMapHairText.Visible = enabled;
            dashCountMenu.Visible = enabled;
            maddyCrownText.Visible = enabled;
            doFeatherColorText.Visible = enabled;
            doDashFlashText.Visible = enabled;

            Hyperline.Instance.ShouldClearOverride = true;
        }

        public void UpdateHairType(int dashCount, uint type)
        {
            lastDash = dashCount;
            IHairType[] hairTypes = Hyperline.HairTypes.GetHairTypes();
            for (int t = 0; t < colorMenus[dashCount].Count; t++)   //hair type
            {
                for (int c = 0; c < colorMenus[dashCount][t].Count; c++)
                {
                    colorMenus[dashCount][t][c].Visible = type == hairTypes[t].GetHash();
                }
            }
        }

        public List<List<TextMenu.Item>> CreateDashCountMenu(TextMenu menu, bool inGame, int dashes, out TextMenuExt.EnumerableSlider<uint> typeSlider)
        {
            typeSlider = new("Type:", Hyperline.HairTypes.GetHairNames(), Hyperline.Settings.DashList[dashes].HairType);
            typeSlider.Change(v => { Hyperline.Settings.DashList[dashes].HairType = v; UpdateHairType(dashes, v); });
            List<List<TextMenu.Item>> returnV = [];
            foreach (KeyValuePair<uint, IHairType> hair in Hyperline.Settings.DashList[dashes].HairList)
            {
                List<TextMenu.Item> items = hair.Value.CreateMenu(menu, inGame);
                items.Add(new UI.HairPreview(dashes));
                returnV.Add(items);
            }
            return returnV;
        }

        public void CopyPreset()
        {
            if (currentPreset < Hyperline.PresetManager.Presets.Count)
            {
                Logger.Log("Hyperline", "Applying preset " + presetList[currentPreset].Key);
                Hyperline.PresetManager.Presets[presetList[currentPreset].Value].Apply();
                Audio.Play("event:/ui/main/button_back");
                OuiModOptions.Instance.Overworld.Goto<OuiModOptions>();
            }
        }

        public HMenuItem CreatePresetMenu()
        {
            HSubMenu presetMenu = new("Presets", false);
            presetList.Clear();
            uint i = 0;
            foreach (KeyValuePair<string, PresetManager.Preset> preset in Hyperline.PresetManager.Presets)
            {
                presetList.Add(new(i, preset.Key));
                i++;
            }

            TextMenu.Button applyButton = new("Apply Preset");
            applyButton.Disabled = presetList.Count == 0;
            applyButton.Pressed(CopyPreset);

            TextMenu.Button saveButton = new("Save Preset");
            saveButton.Pressed(() =>
            {
                if(Hyperline.PresetManager.Presets.Count == 0)
                {
                    Logger.Log("Hyperline", "No presets to save.");
                    return;
                }

                Logger.Log("Hyperline", "Saving preset " + Hyperline.PresetManager.Presets.Count);
                Hyperline.PresetManager.SavePreset(presetList[currentPreset].Value, PresetManager.Preset.CopyFromSettings());
                Audio.Play("event:/ui/main/savefile_select");
                OuiModOptions.Instance.Overworld.Goto<OuiModOptions>();
            });
            saveButton.Disabled = presetList.Count == 0;

            TextMenu.Button deleteButton = new("Delete Preset");
            deleteButton.Disabled = presetList.Count == 0;


            TextMenuExt.EnumerableSlider<uint> p = new("Preset:", presetList, 0);
            p.Change(v => currentPreset = (int)v);
            presetMenu.Add(p);
            p.Disabled = presetList.Count == 0;
            Action updatePresetList = () =>
            {
                presetMenu.Remove(p);
                presetList.Clear();
                // for whatever reason it's internally stored as a tuple, so we have to recreate it.

                uint j = 0;
                foreach (KeyValuePair<string, PresetManager.Preset> preset in Hyperline.PresetManager.Presets)
                {
                    presetList.Add(new(j, preset.Key));
                    j++;
                }
                p = new("Preset:", presetList, 0);
                p.Disabled = presetList.Count == 0;
                p.Change(v => currentPreset = (int)v);
                // we need to insert it at the start
                presetMenu.Insert(0, p);

                applyButton.Disabled = presetList.Count == 0;
                saveButton.Disabled = presetList.Count == 0;
                deleteButton.Disabled = presetList.Count == 0;
                p.Disabled = presetList.Count == 0;
            };

            deleteButton.Pressed(() =>
            {
                Logger.Log("Hyperline", "Deleting preset " + presetList[currentPreset].Key);
                Hyperline.PresetManager.DeletePreset(presetList[currentPreset].Value);
                Audio.Play("event:/ui/main/savefile_delete");
                OuiModOptions.Instance.Overworld.Goto<OuiModOptions>();
                updatePresetList.Invoke();
            });


            presetMenu.Add(applyButton);
            presetMenu.Add(saveButton);
            presetMenu.Add(deleteButton);

            // create a new preset from the current settings. This should be a keyboard dropdown tray, and on confirm, it should save the preset.
            KeyboardInput presetInput = new("", null, null, 0, 12);
            DropdownControlTray presetButton = new("New Preset: ", presetInput);

            presetInput.Confirm(v =>
            {
                Logger.Log("Hyperline", "Saving preset " + v);
                Hyperline.PresetManager.SavePreset(v, PresetManager.Preset.CopyFromSettings());
                Audio.Play("event:/ui/main/savefile_select");
                OuiModOptions.Instance.Overworld.Goto<OuiModOptions>();
                updatePresetList.Invoke();
            });
            presetMenu.Add(presetButton);

            return presetMenu;
        }

        public static void SetHairLength(int dashCount, int hairLength) => Hyperline.Settings.DashList[dashCount].HairLength = hairLength;

        public static void SetHairSpeed(int dashCount, int hairSpeed) => Hyperline.Settings.DashList[dashCount].HairSpeed = hairSpeed;

        public static void SetHairPhase(int dashCount, int hairPhase) => Hyperline.Settings.DashList[dashCount].HairPhase = hairPhase;


        // create a texture editor using the keyboard, parameters are a get and set for the value
        public static DropdownControlTray CreateTextureEditor(TextMenu menu, string buttonName,
            Func<string> getValue, Action<string> setValue)
        {
            KeyboardInput textureInput = new(getValue(), null, null, 0, 12);
            DropdownControlTray textureButton = new(buttonName + getValue(), textureInput);
            textureInput.Confirm(v =>
            {
                // log
                Logger.Log(LogLevel.Info, "Hyperline", "Setting custom texture to " + v);
                if (string.IsNullOrEmpty(v) || !HyperlineSettings.HasAtlasSubtexture("hyperline/" + v))
                {
                    // clear the texture if it's invalid
                    setValue("");
                    textureInput.Value = getValue();
                    textureButton.Label = buttonName + getValue();
                    Logger.Log(LogLevel.Warn, "Hyperline", "Invalid custom texture, clearing.");
                    return;
                }

                setValue(v);
                textureInput.Value = getValue();
                textureButton.Label = buttonName + getValue();
                Logger.Log(LogLevel.Info, "Hyperline", "Custom texture set to " + v);
            });

            textureInput.Change(v =>
            {
                // validate and set Valid on the input
                textureInput.Valid = string.IsNullOrEmpty(v) || HyperlineSettings.HasAtlasSubtexture("hyperline/" + v);
            });

            return textureButton;
        }

        // equivalent to the above, but for color text input.
        // HSVColor ToString and FromString are used to convert the color to and from a string. bool FromString(string) is used to validate the input.
        public static DropdownControlTray CreateColorEditor(TextMenu menu, string buttonName,
            Func<HSVColor> getValue, Action<HSVColor> setValue)
        {
            KeyboardInput colorInput = new(getValue().ToHSVString(), null, null, 0, 9);
            DropdownControlTray colorButton = new(buttonName + getValue().ToHSVString(), colorInput);
            colorInput.Confirm(v =>
            {
                // log
                Logger.Log(LogLevel.Info, "Hyperline", "Setting custom color to " + v);
                HSVColor color = new();
                if (!color.FromString(v))
                {
                    // revert to the previous color if the input is invalid
                    setValue(getValue());
                    colorInput.Value = getValue().ToHSVString();
                    colorButton.Label = buttonName + getValue().ToHSVString();
                    Logger.Log(LogLevel.Warn, "Hyperline", "Invalid custom color, reverting.");
                    return;
                }

                setValue(color);
                colorInput.Value = getValue().ToHSVString();
                colorButton.Label = buttonName + getValue().ToHSVString();
                Logger.Log(LogLevel.Info, "Hyperline", "Custom color set to " + v);
            });

            colorInput.Change(v =>
            {
                // validate and set Valid on the input
                colorInput.Valid = new HSVColor().FromString(v);
            });

            return colorButton;
        }

        public void CreateMenu(TextMenu menu, bool inGame)
        {
            currentPreset = 0;
            enabledText = new TextMenu.OnOff(Dialog.Clean("MODOPTIONS_HYPERLINE_ENABLED"), Hyperline.Settings.Enabled).Change(EnabledToggled);
            allowMapHairText = new TextMenu.OnOff(Dialog.Clean("MODOPTIONS_HYPERLINE_ALLOWMAPHAIR"), Hyperline.Settings.AllowMapHairColors).Change(v => Hyperline.Settings.AllowMapHairColors = v);
            maddyCrownText = new TextMenu.OnOff("Maddy Crown Support:", Hyperline.Settings.DoMaddyCrown).Change(v => Hyperline.Settings.DoMaddyCrown = v);
            doFeatherColorText = new TextMenu.OnOff("Do Feather Color", Hyperline.Settings.DoFeatherColor).Change(v => Hyperline.Settings.DoFeatherColor = v);
            doDashFlashText = new TextMenu.OnOff("Do Dash Flash", Hyperline.Settings.DoDashFlash).Change(v => Hyperline.Settings.DoDashFlash = v);
            menu.Add(enabledText);
            menu.Add(allowMapHairText);
            menu.Add(maddyCrownText);
            menu.Add(doFeatherColorText);
            menu.Add(doDashFlashText);
            menu.Add(CreatePresetMenu());

            colorMenus = [];    //dashes
            dashCountMenu = new("Dashes");
            dashCountMenu.SetInitialSelection(lastDash);

            dashCountMenu.Change(v => UpdateHairType(v, Hyperline.Settings.DashList[v].HairType));
            for (int counterd = 0; counterd < Hyperline.MaxDashCount; counterd++)
            {
                int r = counterd;
                List<TextMenu.Item> dashMenu = [];

                colorMenus.Add(CreateDashCountMenu(menu, inGame, counterd, out TextMenuExt.EnumerableSlider<uint> hairTypeMenu));

                DropdownControlTray textureButton = CreateTextureEditor(menu, "Custom Texture: ", () => Hyperline.Settings.DashList[r].HairTextureSource, v => { Hyperline.Settings.DashList[r].HairTextureSource = v; Hyperline.Settings.LoadCustomTexture(r); });

                DropdownControlTray bangsButton = CreateTextureEditor(menu, "Custom Bangs: ", () => Hyperline.Settings.DashList[r].HairBangsSource, v => { Hyperline.Settings.DashList[r].HairBangsSource = v; Hyperline.Settings.LoadCustomBangs(r); });

                dashMenu.Add(textureButton);
                dashMenu.Add(bangsButton);

                dashMenu.Add(new TextMenu.Slider("Speed:", StringFromInt, HyperlineSettings.MinHairSpeed, HyperlineSettings.MaxHairSpeed, Hyperline.Settings.DashList[counterd].HairSpeed).Change(v => SetHairSpeed(r, v)));
                dashMenu.Add(new TextMenu.Slider("Length:", StringFromInt, HyperlineSettings.MinHairLength, Hyperline.Settings.HairLengthSoftCap, Hyperline.Settings.DashList[counterd].HairLength).Change(v => SetHairLength(r, v)));
                dashMenu.Add(new TextMenu.Slider("Phase: ", StringFromInt, HyperlineSettings.MinHairPhase, HyperlineSettings.MaxHairPhase, Hyperline.Settings.DashList[counterd].HairPhase).Change(v => SetHairPhase(r, v)));
                dashMenu.Add(hairTypeMenu);

                for (int i = 0; i < colorMenus[counterd].Count; i++)
                {
                    dashMenu.AddRange(colorMenus[counterd][i].AsEnumerable());
                }

                dashCountMenu.Add(counterd.ToString(), dashMenu);
            }
            menu.Add(dashCountMenu);
            UpdateHairType(lastDash, Hyperline.Settings.DashList[lastDash].HairType);
            EnabledToggled(Hyperline.Settings.Enabled);
        }
    }
}
