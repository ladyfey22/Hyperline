using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.Hyperline
{
    public class Hyperline : EverestModule
    {

        public const uint MAX_DASH_COUNT = 10;

        public static Hyperline Instance;

        public override Type SettingsType => typeof(HyperlineSettings);
        public static HyperlineSettings Settings => (HyperlineSettings)Instance._Settings;

        public override Type SessionType => typeof(HyperlineSession);
        public static HyperlineSession Session => (HyperlineSession)Instance._Session;

        public override Type SaveDataType => null;

        public TriggerManager triggerManager;
        public PresetManager presetManager;
        public HairTypeManager hairTypes;
        public HyperlineUI UI;

        public Color lastColor;
        private int lastHairLength;
        private float time;
        private bool isHooked = false;
        public Sprite maddyCrownSprite;

        // vanilla particle definitions
        public static ParticleType DefaultPDashA;
        public static ParticleType DefaultPDashB;
        public static ParticleType DefaultPDashBadB;

        public ParticleType PlayerParticle;
        bool shouldClearOverride = false;

        public Hyperline()
        {
            UI = new HyperlineUI();
            triggerManager = new TriggerManager();
            presetManager = new PresetManager();

            lastColor = new Color();
            Instance = this;
            lastHairLength = 4;
            //add the default hair types
            hairTypes = new HairTypeManager();
            AddHairType(new GradientHair());
            AddHairType(new PatternHair());
            AddHairType(new SolidHair());
            AddHairType(new RainbowHair());
            AddHairType(new DefaultHair());

            DefaultPDashA = new ParticleType {
                Color = Calc.HexToColor("44B7FF"),
                Color2 = Calc.HexToColor("75c9ff"),
                ColorMode = ParticleType.ColorModes.Blink,
                FadeMode = ParticleType.FadeModes.Late,
                LifeMin = 1f,
                LifeMax = 1.8f,
                Size = 1f,
                SpeedMin = 10f,
                SpeedMax = 20f,
                Acceleration = new Vector2(0f, 8f),
                DirectionRange = 1.0471976f
            };

            DefaultPDashB = new ParticleType(DefaultPDashA) {
                Color = Calc.HexToColor("AC3232"),
                Color2 = Calc.HexToColor("e05959")
            };

            DefaultPDashBadB = new ParticleType(DefaultPDashA) {
                Color = Calc.HexToColor("9B3FB5"),
                Color2 = Calc.HexToColor("CC8EE2")
            };

            PlayerParticle = new ParticleType(DefaultPDashA) {
            };

        }

        public override void CreateModMenuSection(TextMenu menu, bool inGame, EventInstance snapshot)
        {
            base.CreateModMenuSection(menu, inGame, snapshot);
            UI.CreateMenu(menu, inGame);
        }

        public void AddHairType(IHairType type)
        {
            hairTypes.AddHairType(type);
        }

        public override void Load()
        {
            Logger.Log(LogLevel.Info, "Hyperline", "Starting hyperline Version " + Hyperline.Settings.version[0] + "-" + Hyperline.Settings.version[1] + "-" + Hyperline.Settings.version[2]);
            HookStuff();
            On.Celeste.Player.Update += PlayerUpdate;
        }

        public override void Unload()
        {
            UnhookStuff();
            On.Celeste.Player.Update -= PlayerUpdate;
        }

        private void UpdateMaddyCrown(Player player)
        {
            if (Settings.DoMaddyCrown && maddyCrownSprite == null)
                foreach (Sprite sprite in player.Components.GetAll<Sprite>())
                {
                    if (sprite.Animations.ContainsKey("crown"))
                        maddyCrownSprite = sprite;
                }
            if (Settings.DoMaddyCrown && maddyCrownSprite != null)
                maddyCrownSprite.SetColor(lastColor);
        }


        public override void LoadContent(bool firstLoad)
        {
            Settings.LoadTextures();
            presetManager.LoadContent();
        }

        private static void OnUnpause(On.Celeste.Level.orig_EndPauseEffects orig, Level self)
        {
            Instance.triggerManager.LoadFromSettings();
            Instance.triggerManager.UpdateFromChanges();
            orig(self);
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
            On.Celeste.Level.EndPauseEffects -= OnUnpause;
            On.Celeste.PlayerHair.Render -= RenderHair;
            On.Celeste.Player.UpdateHair -= UpdateHair;
            On.Celeste.Player.DashUpdate -= DashUpdate;
            shouldClearOverride = true;

            TriggerManager.Unload();
            isHooked = false;
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
            On.Celeste.Level.EndPauseEffects += OnUnpause;
            On.Celeste.PlayerHair.Render += RenderHair;
            On.Celeste.Player.UpdateHair += UpdateHair;
            On.Celeste.Player.DashUpdate += DashUpdate;
            TriggerManager.Load();
            isHooked = true;
        }

        private int DashUpdate(On.Celeste.Player.orig_DashUpdate orig, Player self)
        {
            if (Settings.Enabled)
            {
                PlayerParticle.Color = lastColor;
                PlayerParticle.Color2 = lastColor;

                Player.P_DashA = PlayerParticle;
                Player.P_DashB = PlayerParticle;
                Player.P_DashBadB = PlayerParticle;
            }
            int returnV = orig(self);

            if (Settings.Enabled)
            {
                Player.P_DashA = DefaultPDashA;
                Player.P_DashB = DefaultPDashB;
                Player.P_DashBadB = DefaultPDashB;
            }

            return returnV;
        }

        private void UpdateHair(On.Celeste.Player.orig_UpdateHair orig, Player self, bool applyGravity)
        {
            if(!Settings.Enabled || self.Dashes > MAX_DASH_COUNT)
            {
                orig(self, applyGravity);
                return;
            }

            IHairType hair = Instance.triggerManager.GetHair(self.Dashes);
            if (hair != null)
                hair.UpdateHair(orig, self, applyGravity);
            else
                orig(self, applyGravity);

        }

        public void RenderHair(On.Celeste.PlayerHair.orig_Render orig, PlayerHair self)
        {
            if (!(self.Entity is Player) || !Settings.Enabled || (self.Entity as Player).Dashes > MAX_DASH_COUNT)
            {
                orig(self);
                return;
            }

            Player player = self.Entity as Player;
            IHairType hair = Instance.triggerManager.GetHair(player.Dashes);
            if(hair != null)
                hair.Render(orig, self);
            else
                orig(self);
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
            Instance.maddyCrownSprite = null;
        }

        public static void PlayerUpdate(On.Celeste.Player.orig_Update orig, Player player)
        {
            if (Settings.Enabled)
            {
                if (!(!Settings.DoFeatherColor && player.StateMachine.State == 19))
                {
                    Instance.maddyCrownSprite = null;
                    IHairType currentHair = Instance.triggerManager.GetHair(player.Dashes);
                    if (currentHair != null)
                        currentHair.PlayerUpdate(Instance.lastColor, player);
                }
                else
                    player.OverrideHairColor = null;
            }
            
            if(Instance.shouldClearOverride)
            {
                Instance.shouldClearOverride = false;
                player.OverrideHairColor = null;
            }
            orig(player);
        }

        public static void Death(On.Celeste.DeathEffect.orig_Draw orig, Vector2 position, Color color, float ease)
        {
            if (Settings.Enabled)
                color = Instance.lastColor;
            Instance.maddyCrownSprite = null;
            orig(position, color, ease);
        }

        public void UpdateHairLength(PlayerHair self)
        {
            if (Settings.Enabled && self.Entity is Player)
            {
                Player player = self.Entity as Player;
                if (player.StateMachine.State == 5)
                    player.Sprite.HairCount = 1;
                else if (player.StateMachine.State != 19)
                {
                    player.Sprite.HairCount = ((player.Dashes > 1) ? 5 : 4);
                    player.Sprite.HairCount += lastHairLength - 4;
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

            if (player.Dashes >= MAX_DASH_COUNT || player.Dashes < 0 || (!Settings.DoFeatherColor && player.StateMachine.State == 19))
                return colorOrig;

            Color ReturnC = GetCurrentColor(colorOrig, ((Player)self.Entity).Dashes, index, self);
            if (index == 0)
            {
                Instance.UpdateMaddyCrown(self.Entity as Player);
                Instance.lastColor = ReturnC;
                Instance.lastHairLength = Instance.triggerManager.GetHairLength(((Player)self.Entity).Dashes);
            }

            if (IHairType.IsFlash() && Settings.DoDashFlash)
                return Player.FlashHairColor;

            return ReturnC;
        }

        public static Color GetCurrentColor(Color colorOrig, int dashes, int index, PlayerHair self)
        {
            if (dashes >= MAX_DASH_COUNT)
                return colorOrig;

            int speed = Instance.triggerManager.GetHairSpeed(dashes);
            int length = Instance.triggerManager.GetHairLength(dashes);
            float phaseShift = Math.Abs(((float)index / ((float)length)) - 0.01f);
            float phase = phaseShift + speed / 20.0f * Hyperline.Instance.time;
            phase -= (float)(Math.Floor(phase));
            Color returnV = new Color(0, 0, 0);

            IHairType hair = Instance.triggerManager.GetHair(dashes);
            if (hair != null)
            {
                returnV = hair.GetColor(colorOrig, phase);
                if (returnV == null)
                    returnV = new Color(0, 0, 0);
            }
            return returnV;
        }

        private void PlayerHair_AfterUpdate(On.Celeste.PlayerHair.orig_AfterUpdate orig, PlayerHair self)
        {
            if (!(self.Entity is Player) || !Settings.Enabled || (self.Entity as Player).Dashes > MAX_DASH_COUNT)
            {
                orig(self);
                return;
            }

            Player player = (Player)self.Entity;
            Instance.time += Engine.DeltaTime;
            maddyCrownSprite = null;
            Instance.UpdateHairLength(self);

            IHairType hair = Instance.triggerManager.GetHair(player.Dashes);
            if (hair != null)
                hair.AfterUpdate(orig, self);
            else
            {
                orig(self); //this really shouldn't happen, only happens when using the load command from the main menu, so we'll reload the trigger manager
                triggerManager.LoadFromSettings();
            }
        }

        public override void Initialize()
        {
        }

        public static Color GetTrailColor(On.Celeste.Player.orig_GetTrailColor orig, Player self, bool wasDashB)
        {
            Color colorOrig = orig(self, wasDashB);
            if (Settings.Enabled)
                return GetCurrentColor(Instance.lastColor, self.Dashes, 0, self.Hair);
            return colorOrig;
        }

    }
}
