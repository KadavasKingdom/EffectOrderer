using CustomPlayerEffects;
using EffectOrderer.Extensions;
using HarmonyLib;
namespace EffectOrderer.Patches;

[HarmonyPatch(typeof(StatusEffectBase), nameof(StatusEffectBase.Update))]
[HarmonyPriority(Priority.Last)]
internal static class StatusEffectBase_Update
{
    internal static bool Prefix(StatusEffectBase __instance)
    {
        if (__instance.Classification == StatusEffectBase.EffectClassification.Technical) return true;
         
        __instance.GetManager().Update();
        return false;
    }
}
