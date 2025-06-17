namespace Celeste.Mod.Hyperline.Imports
{
    using System;
    using MonoMod.ModInterop;

    [ModImportName("SkinModHelperPlus")]
    public static class SkinModHelperPlus
    {
        public static Action<PlayerHair, bool> SetHairConfigColor_Active;
        public static Action<PlayerHair, bool> SetHairConfigLengths_Active;
    }
}
