namespace Celeste.Mod.Hyperline;
using MonoMod.ModInterop;

/// <summary>
/// Provides export functions for other mods to import.
/// If you do not need to export any functions, delete this class and the corresponding call
/// to ModInterop() in <see cref="Hyperline.Load"/>
/// </summary>
[ModExportName("Hyperline")]
public static class HyperlineExports
{
    public static void AddHairType(IHairType hairType) => Hyperline.AddHairType(hairType);

    /// <summary>
    /// Called to replace the current preset temporarily with a triggered one, useful for creating custom triggers in a map.
    /// </summary>
    /// <param name="preset">The preset to override with. If null, clear the current preset.</param>
    public static void SetTriggerPreset(string preset) => Hyperline.TriggerManager.Trigger(preset);
}
