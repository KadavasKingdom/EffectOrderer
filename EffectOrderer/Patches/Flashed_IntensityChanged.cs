using CustomPlayerEffects;
using HarmonyLib;

namespace EffectOrderer.Patches;

// status effects in vanilla are such a joke. this function does nothing in basegame
[HarmonyPatch(typeof(Flashed), nameof(Flashed.IntensityChanged))]
public static class Flashed_IntensityChanged
{
    public static bool Prefix()
    {
        return false;
    }
}
