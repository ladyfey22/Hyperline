using Celeste.Mod.UI;
using System.Collections.Generic;

namespace Celeste.Mod.Hyperline
{
    public class HyperlineUI
    {

        private HyperlineSettings Settings => Hyperline.Settings;
        private static int lastDash = 0;
        private TextMenu.Option<bool> enabledText;
        private TextMenu.Option<bool> allowMapHairText;
        private TextMenu.Option<bool> maddyCrownText;

        List<List<List<TextMenu.Item>>> ColorMenus; //format is  [Dashes][Type][ColorNum]
        TextMenuExt.OptionSubMenu DashCountMenu;

        public HyperlineUI()
        {
            ColorMenus = new List<List<List<TextMenu.Item>>>();
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
            DashCountMenu.Visible = enabled;
            maddyCrownText.Visible = enabled;
        }

        public void UpdateHairType(int dashCount, uint type)
        {
            lastDash = dashCount;
            IHairType[] hairTypes = Hyperline.Instance.hairTypes.GetHairTypes();
            for (int t = 0; t < ColorMenus[dashCount].Count; t++)   //hair type
                for (int c = 0; c < ColorMenus[dashCount][t].Count; c++)
                    ColorMenus[dashCount][t][c].Visible = (type == hairTypes[t].GetHash());
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
            enabledText = new TextMenu.OnOff(Dialog.Clean("MODOPTIONS_HYPERLINE_ENABLED"), Settings.Enabled).Change(EnabledToggled);
            allowMapHairText = new TextMenu.OnOff(Dialog.Clean("MODOPTIONS_HYPERLINE_ALLOWMAPHAIR"), Settings.AllowMapHairColors).Change(v => Settings.AllowMapHairColors = v);
            maddyCrownText = new TextMenu.OnOff("Maddy Crown Support:", Settings.DoMaddyCrown).Change(v => { Settings.DoMaddyCrown = v; });
            menu.Add(enabledText);
            menu.Add(allowMapHairText);
            menu.Add(maddyCrownText);
            ColorMenus = new List<List<List<TextMenu.Item>>>();    //dashes
            DashCountMenu = new TextMenuExt.OptionSubMenu("Dashes");
            DashCountMenu.SetInitialSelection(lastDash);

            DashCountMenu.Change(v => { UpdateHairType(v, Settings.hairTypeList[v]); });
            for (int counterd = 0; counterd < Hyperline.MAX_DASH_COUNT; counterd++)
            {
                int r = counterd;
                List<TextMenu.Item> Menu = new List<TextMenu.Item>();
                TextMenuExt.EnumerableSlider<uint> HairTypeMenu;
                ColorMenus.Add(CreateDashCountMenu(menu, inGame, counterd, out HairTypeMenu));
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

                Menu.Add(new TextMenu.Slider("Speed:", StringFromInt, -40, 40, Settings.hairSpeedList[counterd]).Change(v => { SetHairSpeed(r, v); }));
                Menu.Add(new TextMenu.Slider("Length:", StringFromInt, HyperlineSettings.MIN_HAIR_LENGTH, HyperlineSettings.MAX_HAIR_LENGTH, Settings.hairLengthList[counterd]).Change(v => { SetHairLength(r, v); }));
                Menu.Add(HairTypeMenu);
                if (!inGame)
                {
                    for (int i = 0; i < ColorMenus[counterd].Count; i++)
                        for (int j = 0; j < ColorMenus[counterd][i].Count; j++)
                            Menu.Add(ColorMenus[counterd][i][j]);
                }
                DashCountMenu.Add(counterd.ToString(), Menu);
            }
            menu.Add(DashCountMenu);
            UpdateHairType(lastDash, Settings.hairTypeList[lastDash]);
            EnabledToggled(Settings.Enabled);
        }
    }
}
