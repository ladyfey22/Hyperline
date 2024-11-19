namespace Celeste.Mod.Hyperline
{
    using FMOD.Studio;
    using Microsoft.Xna.Framework;
    using Monocle;
    using MonoMod.ModInterop;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using MonoMod.RuntimeDetour;

    public class Hyperline : EverestModule
    {

        public const uint MaxDashCount = 10;
        public static Hyperline Instance { get; private set; }

        //Everest Type Definitions
        public override Type SettingsType => typeof(HyperlineSettings);
        public static HyperlineSettings Settings => (HyperlineSettings)Instance._Settings;
        public override Type SessionType => typeof(HyperlineSession);
        public static HyperlineSession Session => (HyperlineSession)Instance._Session;
        public override Type SaveDataType => null;

        // Hair Source
        private IHairSource HairSource { get; set; }

        // Manager Classes
        public static PresetManager PresetManager { get; private set; }
        public static HairTypeManager HairTypes { get; private set; }
        public static HyperlineUI Ui { get; private set; }
        public static TriggerManager TriggerManager { get; set; }

        public Color LastColor { get; set; }
        private int lastHairLength;
        private float time;
        public Sprite MaddyCrownSprite { get; set; }

        // vanilla particle definitions
        private static ParticleType defaultPDashA;
        private static ParticleType defaultPDashB;
        private static ParticleType playerParticle;


        // Whether on the next render, the players override hair should be cleared
        public bool ShouldClearOverride { get; set; }

        public Hyperline()
        {
            Ui = new();
            PresetManager = new();
            TriggerManager = new();

            LastColor = new();
            Instance = this;
            lastHairLength = 4;

            //add the default hair types
            HairTypes = new();
            AddHairType([new GradientHair(), new PatternHair(), new SolidHair(), new RainbowHair(), new DefaultHair()]);


            HairSource = new HairSourceList([new TriggerHairSource(), new SettingsHairSource()]);

            // create the default particles
            defaultPDashA = new()
            {
                Color = Calc.HexToColor("44B7FF"),
                Color2 = Calc.HexToColor("75c9ff"),
                ColorMode = ParticleType.ColorModes.Blink,
                FadeMode = ParticleType.FadeModes.Late,
                LifeMin = 1f,
                LifeMax = 1.8f,
                Size = 1f,
                SpeedMin = 10f,
                SpeedMax = 20f,
                Acceleration = new(0f, 8f),
                DirectionRange = 1.0471976f
            };

            defaultPDashB = new(defaultPDashA)
            {
                Color = Calc.HexToColor("AC3232"),
                Color2 = Calc.HexToColor("e05959")
            };

            playerParticle = new(defaultPDashA);
        }

        public override void CreateModMenuSection(TextMenu menu, bool inGame, EventInstance snapshot)
        {
            // use our own custom menu system due to the hair type system
            base.CreateModMenuSection(menu, inGame, snapshot);
            Ui.CreateMenu(menu, inGame);
        }

        public static void AddHairType(IHairType type) => HairTypes.AddHairType(type);

        public static void AddHairType(IEnumerable<IHairType> hairTypes)
        {
            foreach (IHairType hairType in hairTypes)
            {
                AddHairType(hairType);
            }
        }

        private static void LoadDetour()
        {
            using (DetourHelper.GenerateDetourContext(typeof(PlayerHair), "GetHairColor", before: ["SkinModHelperPlus", "Prideline"], after: ["JackalHelper"]))
            {
                On.Celeste.PlayerHair.GetHairColor += GetHairColor;
            }

            using (DetourHelper.GenerateDetourContext(typeof(Player), "GetTrailColor", before: ["SkinModHelperPlus", "Prideline"], after: ["JackalHelper"]))
            {
                On.Celeste.Player.GetTrailColor += GetTrailColor;
            }

            using (DetourHelper.GenerateDetourContext(typeof(PlayerHair), "GetHairTexture", before: ["SkinModHelperPlus", "Cateline", "Bunneline"]))
            {
                On.Celeste.PlayerHair.GetHairTexture += GetHairTexture;
            }
        }

        public override void Load()
        {
            Logger.Log(LogLevel.Info, "Hyperline", "Starting hyperline Version " + Settings.Version[0] + "-" + Settings.Version[1] + "-" + Settings.Version[2]);

            On.Celeste.PlayerHair.AfterUpdate += PlayerHair_AfterUpdate;

            On.Celeste.Player.Render += PlayerRender;
            On.Celeste.Player.Added += PlayerAdded;
            On.Celeste.PlayerHair.Render += RenderHair;
            On.Celeste.Player.UpdateHair += UpdateHair;
            On.Celeste.Player.DashUpdate += DashUpdate;
            On.Celeste.Player.Update += PlayerUpdate;

            TriggerManager.Hook();
        }

        public override void Unload()
        {
            On.Celeste.PlayerHair.GetHairTexture -= GetHairTexture;
            On.Celeste.PlayerHair.GetHairColor -= GetHairColor;
            On.Celeste.Player.GetTrailColor -= GetTrailColor;
            On.Celeste.PlayerHair.AfterUpdate -= PlayerHair_AfterUpdate;
            On.Celeste.Player.Render -= PlayerRender;
            On.Celeste.Player.Added -= PlayerAdded;
            On.Celeste.PlayerHair.Render -= RenderHair;
            On.Celeste.Player.UpdateHair -= UpdateHair;
            On.Celeste.Player.DashUpdate -= DashUpdate;
            On.Celeste.Player.Update -= PlayerUpdate;
            TriggerManager.Unhook();
        }

        private void UpdateMaddyCrown(Player player)
        {
            if (Settings.DoMaddyCrown && MaddyCrownSprite == null)
            {
                foreach (Sprite sprite in player.Components.GetAll<Sprite>())
                {
                    if (sprite.Animations.ContainsKey("crown"))
                    {
                        MaddyCrownSprite = sprite;
                    }
                }
            }

            if (Settings.DoMaddyCrown && MaddyCrownSprite != null)
            {
                MaddyCrownSprite.SetColor(LastColor);
            }
        }


        public override void LoadContent(bool firstLoad)
        {
            Settings.LoadTextures();
            PresetManager.LoadContent();
        }

        private static int DashUpdate(On.Celeste.Player.orig_DashUpdate orig, Player self)
        {
            // we need to set the particles to the right color on dash
            if (Settings.Enabled)
            {
                playerParticle.Color = Instance.LastColor;
                playerParticle.Color2 = Instance.LastColor;

                Player.P_DashA = playerParticle;
                Player.P_DashB = playerParticle;
                Player.P_DashBadB = playerParticle;
            }
            int returnV = orig(self);

            if (!Settings.Enabled)
            {
                return returnV;
            }

            Player.P_DashA = defaultPDashA;
            Player.P_DashB = defaultPDashB;
            Player.P_DashBadB = defaultPDashB;

            return returnV;
        }

        private static void UpdateHair(On.Celeste.Player.orig_UpdateHair orig, Player self, bool applyGravity)
        {
            if (!Settings.Enabled || self.Dashes > MaxDashCount)
            {
                orig(self, applyGravity); // we draw default if the dash count is over hyperline's max, or if it is disabled
                return;
            }

            IHairType hair = Instance.HairSource.GetHair(self.Dashes);
            if (hair != null)
            {
                hair.UpdateHair(orig, self, applyGravity);
            }
            else
            {
                orig(self, applyGravity);
            }
        }

        public static void RenderHair(On.Celeste.PlayerHair.orig_Render orig, PlayerHair self)
        {
            if (self.Entity is not Player player || !Settings.Enabled || player.Dashes > MaxDashCount)
            {
                orig(self);
                return;
            }

            IHairType hair = Instance.HairSource.GetHair(player.Dashes);
            if (hair != null)
            {
                hair.Render(orig, self);
            }
            else
            {
                orig(self);
            }
        }

        public static MTexture GetHairTexture(On.Celeste.PlayerHair.orig_GetHairTexture orig, PlayerHair self, int index)
        {
            if (self.Entity is not Player player || !Settings.Enabled)
            {
                return orig(self, index);
            }

            if (player.Dashes is >= (int)MaxDashCount or < 0)
            {
                return orig(self, index);
            }

            if (index == 0)  //bangs
            {
                List<MTexture> hairBangs = Instance.HairSource.GetBangsTextures(player.Dashes);
                if (hairBangs == null || hairBangs.Count == 0)
                {
                    return orig(self, index);
                }

                return hairBangs[self.Sprite.HairFrame % hairBangs.Count];
            }

            List<MTexture> hairTextures = Instance.HairSource.GetHairTextures(player.Dashes);
            if (hairTextures == null || hairTextures.Count == 0)
            {
                return orig(self, index);
            }

            return hairTextures[index % hairTextures.Count];
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
                if (!(!Settings.DoFeatherColor && player.StateMachine.State == 19)) //ignore it if we are in the feather and the setting for that is not enabled
                {
                    Instance.MaddyCrownSprite = null;
                    IHairType currentHair = Instance.HairSource.GetHair(player.Dashes);
                    currentHair?.PlayerUpdate(Instance.LastColor, player);
                }
                else
                {
                    player.OverrideHairColor = null;
                }
            }

            if (Instance.ShouldClearOverride)
            {
                Instance.ShouldClearOverride = false;
                player.OverrideHairColor = null;
            }
            orig(player);
        }

        // needed to play the death effect for the player in the correct color
        public static void PlayerRender(On.Celeste.Player.orig_Render orig, Player player)
        {
            if (Settings.Enabled && !SaveData.Instance.Assists.InvisibleMotion && player.StateMachine.State == 14)
            {
                Instance.MaddyCrownSprite = null;
                // if we are in the dead state, enabled, and not invisible, draw the death effect in hyperlines color
                DeathEffect.Draw(player.Center + player.deadOffset, Instance.LastColor, player.introEase);
            }
            else
            {
                orig(player);
            }
        }

        public void UpdateHairLength(PlayerHair self)
        {
            if (!Settings.Enabled || self.Entity is not Player player)
            {
                return;
            }

            if (player.StateMachine.State == 5)
            {
                player.Sprite.HairCount = 1;
            }
            else if (player.StateMachine.State != 19)
            {
                player.Sprite.HairCount = 4;//player.Dashes > 1 ? 5 : 4;
                player.Sprite.HairCount += lastHairLength - 4;
            }
            else if (player.StateMachine.State == 19)
            {
                player.Sprite.HairCount = 7; // feather
            }
        }

        public static Color GetHairColor(On.Celeste.PlayerHair.orig_GetHairColor orig, PlayerHair self, int index)
        {
            Color colorOrig = orig(self, index);
            // not a player, not enabled, dashes above hyperline's max or below zero, or we are in feather and feather is not enabled
            if (self.Entity is not Player player || !Settings.Enabled || player.Dashes >= MaxDashCount || player.Dashes < 0 || (!Settings.DoFeatherColor && player.StateMachine.State == 19))
            {
                return colorOrig;
            }

            Color returnC = GetCurrentColor(colorOrig, player.Dashes, index);
            if (index == 0)
            {
                Instance.UpdateMaddyCrown(player);
                Instance.LastColor = returnC; // set last color to the current bangs color
                Instance.lastHairLength = Instance.HairSource.GetHairLength(player.Dashes);
            }

            if (IHairType.IsFlash() && Settings.DoDashFlash)
            {
                return Player.FlashHairColor; // if the player's hair is currently flashing, do the flash
            }

            return returnC;
        }

        public static Color GetCurrentColor(Color colorOrig, int dashes, int index)
        {
            if (dashes >= MaxDashCount) // just a double check, may not be necessary
            {
                return colorOrig;
            }

            int speed = Instance.HairSource.GetHairSpeed(dashes);
            int length = Instance.HairSource.GetHairLength(dashes);
            int hairPhase = Instance.HairSource.GetHairPhase(dashes);
            float phaseShift = Math.Abs((index + hairPhase) / (float)length);
            float phase = phaseShift + (speed / 20.0f * Instance.time);
            phase -= (float)Math.Floor(phase); // phase needs to be between [0 - 1]
            Color returnV = new(0, 0, 0);

            IHairType hair = Instance.HairSource.GetHair(dashes);
            if (hair != null)
            {
                returnV = hair.GetColor(colorOrig, phase);
            }

            return returnV;
        }

        private static void PlayerHair_AfterUpdate(On.Celeste.PlayerHair.orig_AfterUpdate orig, PlayerHair self)
        {
            if (self.Entity is not Player player || !Settings.Enabled || player.Dashes > MaxDashCount)
            {
                orig(self);
                return;
            }

            Instance.time += Engine.DeltaTime;
            Instance.MaddyCrownSprite = null;
            Instance.UpdateHairLength(self);

            IHairType hair = Instance.HairSource.GetHair(player.Dashes);
            if (hair != null)
            {
                hair.AfterUpdate(orig, self);
            }
            else
            {
                orig(self); //this really shouldn't happen, only happens when using the load command from the main menu, so we'll reload the trigger manager
            }
        }

        public override void Initialize()
        {
            typeof(HyperlineExports).ModInterop();
            LoadDetour();
            //Logger.Log(LogLevel.Debug, "Hyperline", $"Detour info: " + DetourHelper.DumpDetours(typeof(PlayerHair), "GetHairColor", [typeof(int)]));
        }

        public static Color GetTrailColor(On.Celeste.Player.orig_GetTrailColor orig, Player self, bool wasDashB)
        {
            Color colorOrig = orig(self, wasDashB);
            return Settings.Enabled ? GetCurrentColor(Instance.LastColor, self.Dashes, 0) : colorOrig;
        }

    }
}
