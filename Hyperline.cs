using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.Hyperline
{
    public class Hyperline : EverestModule
    {

        public const uint MAX_DASH_COUNT = 10;
        public Hyperline()
        {
            UI = new HyperlineUI();
            LastColor = new Color();
            Instance = this;
            LastHairLength = 4;
            hairTypes = new HairTypeManager();
            AddType(new GradientHair());
            AddType(new PatternHair());
            AddType(new SolidHair());
            AddType(new RainbowHair());
        }

        public static Hyperline Instance;

        public HairTypeManager hairTypes;
        public override Type SettingsType => typeof(HyperlineSettings);
        public static HyperlineSettings Settings => (HyperlineSettings)Instance._Settings;

        public HyperlineUI UI;
        public override Type SaveDataType => null;

        Color LastColor;
        int LastHairLength;
        float Time;
        bool isHooked = false;
        Sprite MaddyCrownSprite;

        public override void CreateModMenuSection(TextMenu menu, bool inGame, EventInstance snapshot)
        {
            base.CreateModMenuSection(menu, inGame, snapshot);
            UI.CreateMenu(menu, inGame);
        }

        public void AddType(IHairType type)
        {
            hairTypes.AddHairType(type);
        }

        public override void Load()
        {
            HookStuff();
        }

        public override void Unload()
        {
            UnhookStuff();
        }

        private void UpdateMaddyCrown(Player player)
        {
            if (Settings.DoMaddyCrown && MaddyCrownSprite == null)
                foreach (Sprite sprite in player.Components.GetAll<Sprite>())
                {
                    if (sprite.Animations.ContainsKey("crown"))
                        MaddyCrownSprite = sprite;
                }
            if (Settings.DoMaddyCrown && MaddyCrownSprite != null)
                MaddyCrownSprite.SetColor(LastColor);
        }

        public static void OnLevelEntry(Session session, bool fromSaveData)
        {
        }

        public void UnhookStuff()
        {
            if (!isHooked || Settings.Enabled)
                return;
            On.Celeste.PlayerHair.GetHairTexture -= GetHairTexture;
            On.Celeste.PlayerHair.GetHairColor -= GetHairColor;
            On.Celeste.Player.GetTrailColor -= GetTrailColor;
            On.Celeste.PlayerHair.AfterUpdate -= PlayerHair_AfterUpdate;
            On.Celeste.DeathEffect.Draw -= Death;
            On.Celeste.Player.Added -= PlayerAdded;
            On.Celeste.Player.Update -= PlayerUpdate;
            Everest.Events.Level.OnEnter -= OnLevelEntry;
            isHooked = false;
        }
        public override void LoadContent(bool firstLoad)
        {
            Settings.LoadTextures();
        }

        public void HookStuff()
        {
            if (isHooked || !Settings.Enabled)
                return;
            On.Celeste.PlayerHair.GetHairTexture += GetHairTexture;
            On.Celeste.PlayerHair.GetHairColor += GetHairColor;
            On.Celeste.Player.GetTrailColor += GetTrailColor;
            On.Celeste.PlayerHair.AfterUpdate += PlayerHair_AfterUpdate;
            On.Celeste.DeathEffect.Draw += Death;
            On.Celeste.Player.Added += PlayerAdded;
            On.Celeste.Player.Update += PlayerUpdate;
            Everest.Events.Level.OnEnter += OnLevelEntry;
            isHooked = true;
        }

        public static MTexture GetHairTexture(On.Celeste.PlayerHair.orig_GetHairTexture orig, PlayerHair self, int index)
        {
            if (!(self.Entity is Player) || !Settings.Enabled)
                return orig(self, index);
            Player player = self.Entity as Player;
            if (player.Dashes >= MAX_DASH_COUNT || player.Dashes < 0)
                return orig(self, index);

            if (index == 0)  //bangs
            {
                if (Settings.hairBangs[player.Dashes] == null || Settings.hairBangs[player.Dashes].Count == 0)
                    return orig(self, index);
                return Settings.hairBangs[player.Dashes][self.GetSprite().HairFrame % Settings.hairBangs[player.Dashes].Count];
            }
            if (Settings.hairTextures[player.Dashes] == null || Settings.hairTextures[player.Dashes].Count == 0)
                return orig(self, index);
            return Settings.hairTextures[player.Dashes][index % Settings.hairTextures[player.Dashes].Count];
        }

        public static void PlayerAdded(On.Celeste.Player.orig_Added orig, Player self, Scene scene)
        {
            orig(self, scene);
            Instance.MaddyCrownSprite = null;
        }
        public static void PlayerUpdate(On.Celeste.Player.orig_Update orig, Player player)
        {
            if (Settings.Enabled)
            {
                player.OverrideHairColor = GetCurrentColor(player.Dashes, 0, player.Hair);
                Instance.MaddyCrownSprite = null;
            }
            else
                player.OverrideHairColor = null;
            orig(player);
        }

        public static void Death(On.Celeste.DeathEffect.orig_Draw orig, Vector2 position, Color color, float ease)
        {
            if (Settings.Enabled)
                color = Instance.LastColor;
            Instance.MaddyCrownSprite = null;
            orig(position, color, ease);
        }

        public void UpdateHairLength(PlayerHair self)
        {
            Player player = self.Entity as Player;
            if (Settings.Enabled && player != null)
            {
                if (player.StateMachine.State == 5)
                    player.Sprite.HairCount = 1;
                else if (player.StateMachine.State != 19)
                {
                    player.Sprite.HairCount = ((player.Dashes > 1) ? 5 : 4);
                    player.Sprite.HairCount += LastHairLength - 4;
                }
                else if (player.StateMachine.State == 19)
                    player.Sprite.HairCount = 7;
            }
        }

        public static Color GetHairColor(On.Celeste.PlayerHair.orig_GetHairColor orig, PlayerHair self, int index)
        {
            Color colorOrig = orig(self, index);
            if (!(self.Entity is Player) || !Settings.Enabled)
                return colorOrig;
            Player player = self.Entity as Player;
            if (player.Dashes >= MAX_DASH_COUNT || player.Dashes < 0)
                return colorOrig;
            Color ReturnC = GetCurrentColor(((Player)self.Entity).Dashes, index, self);
            if (index == 0)
            {
                Instance.UpdateMaddyCrown(self.Entity as Player);
                Instance.LastColor = ReturnC;
                Hyperline.Instance.LastHairLength = Settings.hairLengthList[((Player)self.Entity).Dashes];
            }
            return ReturnC;
        }

        public static Color GetCurrentColor(int dashes, int index, PlayerHair self)
        {
            if (dashes >= Settings.hairTypeList.Length)
                return new Color(0, 0, 0);
            uint type = Settings.hairTypeList[dashes];
            int speed = Settings.hairSpeedList[dashes];
            int length = Settings.hairLengthList[dashes];
            float phaseShift = Math.Abs(((float)index / ((float)length)) - 0.01f);
            float phase = phaseShift + speed / 20.0f * Hyperline.Instance.Time;
            phase -= (float)(Math.Floor(phase));
            Color returnV = new Color(0, 0, 0);
            if (Settings.hairList[dashes].ContainsKey(type) && Settings.hairList[dashes][type] != null)
            {
                returnV = Settings.hairList[dashes][type].GetColor(phase);
                if (returnV == null)
                    returnV = new Color(0, 0, 0);
            }
            else
            {
                Logger.Log("Hyperline", "Hair list didn't contain type " + type);
                throw new Exception("Hair list didn't contain type " + type);
            }
            return returnV;
        }

        private void PlayerHair_AfterUpdate(On.Celeste.PlayerHair.orig_AfterUpdate orig, PlayerHair self)
        {
            if ((self.Entity is Player) && Settings.Enabled)
            {
                Player player = (Player)self.Entity;
                player.Hair.Color = GetCurrentColor(player.Dashes, 0, player.Hair);
                Hyperline.Instance.Time += Engine.DeltaTime;
                MaddyCrownSprite = null;
                Instance.UpdateHairLength(self);
            }
            orig(self);
        }

        public override void Initialize()
        {
        }

        public static Color GetTrailColor(On.Celeste.Player.orig_GetTrailColor orig, Player self, bool wasDashB)
        {
            Color colorOrig = orig(self, wasDashB);
            if (Settings.Enabled)
                return GetCurrentColor(self.Dashes, 0, self.Hair);
            return colorOrig;
        }

    }
}
