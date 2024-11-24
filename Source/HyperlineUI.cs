namespace Celeste.Mod.Hyperline
{
    using global::Celeste.Mod.UI;
    using System.Collections.Generic;
    using System.Linq;
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

        public void CreatePresetMenu(TextMenu menu)
        {
            if (Hyperline.PresetManager.Presets.Count != 0)
            {
                presetList.Clear();
                uint i = 0;
                foreach (KeyValuePair<string, PresetManager.Preset> preset in Hyperline.PresetManager.Presets)
                {
                    presetList.Add(new(i, preset.Key));
                    i++;
                }

                menu.Add(new TextMenuExt.EnumerableSlider<uint>("Preset:", presetList, 0).Change(v => currentPreset = (int)v));
                menu.Add(new TextMenu.Button("Apply Preset").Pressed(CopyPreset));
            }
        }

        public static void SetHairLength(int dashCount, int hairLength) => Hyperline.Settings.DashList[dashCount].HairLength = hairLength;

        public static void SetHairSpeed(int dashCount, int hairSpeed) => Hyperline.Settings.DashList[dashCount].HairSpeed = hairSpeed;

        public static void SetHairPhase(int dashCount, int hairPhase) => Hyperline.Settings.DashList[dashCount].HairPhase = hairPhase;

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
            CreatePresetMenu(menu);

            colorMenus = [];    //dashes
            dashCountMenu = new("Dashes");
            dashCountMenu.SetInitialSelection(lastDash);

            dashCountMenu.Change(v => UpdateHairType(v, Hyperline.Settings.DashList[v].HairType));
            for (int counterd = 0; counterd < Hyperline.MaxDashCount; counterd++)
            {
                int r = counterd;
                List<TextMenu.Item> dashMenu = [];

                colorMenus.Add(CreateDashCountMenu(menu, inGame, counterd, out TextMenuExt.EnumerableSlider<uint> hairTypeMenu));

                // TextMenu.Item textureButton = new DropdownControlTray("Custom Texture: " + Hyperline.Settings.DashList[r].HairTextureSource, new KeyboardInput(Hyperline.Settings.DashList[r].HairTextureSource, null, null, 0, 12, forceControllerInput: true));

                TextMenu.Item textureButton = new TextMenu.Button("Custom Texture: " + Hyperline.Settings.DashList[counterd].HairTextureSource).Pressed(() =>
                {
                    Audio.Play(SFX.ui_main_savefile_rename_start);
                    menu.SceneAs<Overworld>().Goto<OuiModOptionString>().Init<OuiModOptions>(Hyperline.Settings.DashList[r].HairTextureSource, v => { Hyperline.Settings.DashList[r].HairTextureSource = v; Hyperline.Settings.LoadCustomTexture(r); }, 12);
                });
                textureButton.Disabled = inGame;
                // title, buttonName, value, onValueChange, maxValueLength, minValueLength
                /*
                var counterd1 = counterd;
                TextMenu.Item textureButton =
                    HSubMenuString.BuildOpenMenuButton<HSubMenuString>(menu, inGame, inGame ? null : () => OuiModOptions.Instance.Overworld.Goto<OuiModOptions>(), ["Custom Textures: ", "Custom Texture: " + Hyperline.Settings.DashList[counterd].HairTextureSource, Hyperline.Settings.DashList[counterd].HairTextureSource, (string v) => { Hyperline.Settings.DashList[counterd1].HairTextureSource = v; Hyperline.Settings.LoadCustomTexture(counterd1); }, 12, 1]);
*/
                TextMenu.Item bangsButton = new TextMenu.Button("Custom Bangs: " + Hyperline.Settings.DashList[counterd].HairBangsSource).Pressed(() =>
                {
                    Audio.Play(SFX.ui_main_savefile_rename_start);
                    menu.SceneAs<Overworld>().Goto<OuiModOptionString>().Init<OuiModOptions>(Hyperline.Settings.DashList[r].HairBangsSource, v => { Hyperline.Settings.DashList[r].HairBangsSource = v; Hyperline.Settings.LoadCustomBangs(r); }, 12);
                });
                bangsButton.Disabled = inGame;

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
