namespace Celeste.Mod.Hyperline;
using MonoMod.ModInterop;
using Lib;

/// <summary>
/// Provides export functions for other mods to import.
/// </summary>
[ModExportName("Hyperline")]
public static class HyperlineExports
{
    /// <summary>
    /// Adds a new hair type to the hair type list.
    /// </summary>
    /// <param name="hairType"></param>
    public static void AddHairType(IHairType hairType) => Hyperline.AddHairType(hairType);

    /// <summary>
    /// Called to replace the current preset temporarily with a triggered one, useful for creating custom triggers in a map.
    /// </summary>
    /// <param name="preset">The preset to override with. If null, clear the current preset.</param>
    public static void SetTriggerPreset(string preset) => Hyperline.TriggerManager.Trigger(preset);
}
