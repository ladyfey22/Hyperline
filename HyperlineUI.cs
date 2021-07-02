using Celeste.Mod.UI;
using System.Collections.Generic;

namespace Celeste.Mod.Hyperline
{
    public class HyperlineUI
    {

        private HyperlineSettings Settings => Hyperline.Settings;
        private static int lastDash = 0;
        private static int currentPreset = 0;
        private TextMenu.Option<bool> enabledText;
        private TextMenu.Option<bool> allowMapHairText;
        private TextMenu.Option<bool> maddyCrownText;

        private List<List<List<TextMenu.Item>>> colorMenus; //format is  [Dashes][Type][ColorNum]
        private TextMenuExt.OptionSubMenu dashCountMenu;

        public HyperlineUI()
        {
            colorMenus = new List<List<List<TextMenu.Item>>>();
        }

        public string StringFromInt(int v)
        {
            return v.ToString();
        }

        public void EnabledToggled(bool enabled)
        {
            Hyperline.Settings.Enabled = enabled;
            if (!enabled)
                Hyperline.Instance.UnhookStuff();
            else
                Hyperline.Instance.HookStuff();
            allowMapHairText.Visible = enabled;
            dashCountMenu.Visible = enabled;
            maddyCrownText.Visible = enabled;
        }

        public void UpdateHairType(int dashCount, uint type)
        {
            lastDash = dashCount;
            IHairType[] hairTypes = Hyperline.Instance.hairTypes.GetHairTypes();
            for (int t = 0; t < colorMenus[dashCount].Count; t++)   //hair type
                for (int c = 0; c < colorMenus[dashCount][t].Count; c++)
                    colorMenus[dashCount][t][c].Visible = (type == hairTypes[t].GetHash());
        }

        public List<List<TextMenu.Item>> CreateDashCountMenu(TextMenu menu, bool inGame, int dashes, out TextMenuExt.EnumerableSlider<uint> typeSlider)
        {
            typeSlider = new TextMenuExt.EnumerableSlider<uint>("Type:", Hyperline.Instance.hairTypes.GetHairNames(), Settings.hairTypeList[dashes]);
            typeSlider.Change(v => { Settings.hairTypeList[dashes] = v; UpdateHairType(dashes, v); });
            List<List<TextMenu.Item>> returnV = new List<List<TextMenu.Item>>();
            foreach (KeyValuePair<uint, IHairType> hair in Hyperline.Settings.hairList[dashes])
            {
                returnV.Add(hair.Value.CreateMenu(menu, inGame));
            }
            return returnV;
        }

        public void CopyPreset()
        {
            if(currentPreset < Hyperline.Instance.presetManager.presets.Count)
            {
                Logger.Log("Hyperline", "Applying preset " + Hyperline.Instance.presetManager.presets[currentPreset].Key);
                Hyperline.Instance.presetManager.presets[currentPreset].Value.Apply();
                Audio.Play("event:/ui/main/button_back");
                OuiModOptions.Instance.Overworld.Goto<OuiModOptions>();
            }
        }

        public void CreatePresetMenu(TextMenu menu)
        {
            if (Hyperline.Instance.presetManager.presets.Count != 0)
            {
                List<KeyValuePair<uint, string>> enumerableList = new List<KeyValuePair<uint, string>>();
                for (uint i = 0; i < Hyperline.Instance.presetManager.presets.Count; i++)
                    enumerableList.Add(new KeyValuePair<uint, string>(i, Hyperline.Instance.presetManager.presets[(int)i].Key));
                TextMenuExt.EnumerableSlider<uint> slider = new TextMenuExt.EnumerableSlider<uint>("Preset:", enumerableList, 0);
                slider.Change(v => { currentPreset = (int)v; });
                menu.Add(slider);

                TextMenu.Button applyButton = new TextMenu.Button("Apply Preset");
                applyButton.Pressed(CopyPreset);
                menu.Add(applyButton);
            }
        }

        public void SetHairLength(int dashCount, int hairLength)
        {
            Settings.hairLengthList[dashCount] = hairLength;
        }

        public void SetHairSpeed(int dashCount, int hairSpeed)
        {
            Settings.hairSpeedList[dashCount] = hairSpeed;
        }

        public void CreateMenu(TextMenu menu, bool inGame)
        {
            currentPreset = 0;
            enabledText = new TextMenu.OnOff(Dialog.Clean("MODOPTIONS_HYPERLINE_ENABLED"), Settings.Enabled).Change(EnabledToggled);
            allowMapHairText = new TextMenu.OnOff(Dialog.Clean("MODOPTIONS_HYPERLINE_ALLOWMAPHAIR"), Settings.AllowMapHairColors).Change(v => Settings.AllowMapHairColors = v);
            maddyCrownText = new TextMenu.OnOff("Maddy Crown Support:", Settings.DoMaddyCrown).Change(v => { Settings.DoMaddyCrown = v; });
            menu.Add(enabledText);
            menu.Add(allowMapHairText);
            menu.Add(maddyCrownText);
            CreatePresetMenu(menu);

            colorMenus = new List<List<List<TextMenu.Item>>>();    //dashes
            dashCountMenu = new TextMenuExt.OptionSubMenu("Dashes");
            dashCountMenu.SetInitialSelection(lastDash);

            dashCountMenu.Change(v => { UpdateHairType(v, Settings.hairTypeList[v]); });
            for (int counterd = 0; counterd < Hyperline.MAX_DASH_COUNT; counterd++)
            {
                int r = counterd;
                List<TextMenu.Item> Menu = new List<TextMenu.Item>();
                TextMenuExt.EnumerableSlider<uint> HairTypeMenu;
                colorMenus.Add(CreateDashCountMenu(menu, inGame, counterd, out HairTypeMenu));
                if (!inGame)
                {
                    Menu.Add(new TextMenu.Button("Custom Texture: " + Settings.hairTextureSource[counterd]).Pressed(() =>
                    {
                        Audio.Play(SFX.ui_main_savefile_rename_start);
                        menu.SceneAs<Overworld>().Goto<OuiModOptionString>().Init<OuiModOptions>(Settings.hairTextureSource[r], v => { Settings.hairTextureSource[r] = v; Settings.LoadCustomTexture(r); }, 12);
                    }));
                    Menu.Add(new TextMenu.Button("Custom Bangs: " + Settings.hairBangsSource[counterd]).Pressed(() =>
                    {
                        Audio.Play(SFX.ui_main_savefile_rename_start);
                        menu.SceneAs<Overworld>().Goto<OuiModOptionString>().Init<OuiModOptions>(Settings.hairBangsSource[r], v => { Settings.hairBangsSource[r] = v; Settings.LoadCustomBangs(r); }, 12);
                    }));
                }

                Menu.Add(new TextMenu.Slider("Speed:", StringFromInt, HyperlineSettings.MIN_HAIR_SPEED, HyperlineSettings.MAX_HAIR_SPEED, Settings.hairSpeedList[counterd]).Change(v => { SetHairSpeed(r, v); }));
                Menu.Add(new TextMenu.Slider("Length:", StringFromInt, HyperlineSettings.MIN_HAIR_LENGTH, HyperlineSettings.MAX_HAIR_LENGTH, Settings.hairLengthList[counterd]).Change(v => { SetHairLength(r, v); }));
                Menu.Add(HairTypeMenu);
                if (!inGame)
                {
                    for (int i = 0; i < colorMenus[counterd].Count; i++)
                        for (int j = 0; j < colorMenus[counterd][i].Count; j++)
                            Menu.Add(colorMenus[counterd][i][j]);
                }
                dashCountMenu.Add(counterd.ToString(), Menu);
            }
            menu.Add(dashCountMenu);
            UpdateHairType(lastDash, Settings.hairTypeList[lastDash]);
            EnabledToggled(Settings.Enabled);
        }
    }
}
