using CustomPlayerEffects;
using EffectOrderer.Extensions;
using HarmonyLib;

namespace EffectOrderer.Patches;

[HarmonyPatch(typeof(StatusEffectBase), nameof(StatusEffectBase.ForceIntensity))]
[HarmonyPriority(Priority.Last)]
internal static class StatusEffectBase_ForceIntensity
{
    internal static bool Prefix(StatusEffectBase __instance, byte value)
    {
        if (__instance.Classification == StatusEffectBase.EffectClassification.Technical) return true;
        __instance.GetManager().ForceIntensity(value);

        return false;
    }
}
