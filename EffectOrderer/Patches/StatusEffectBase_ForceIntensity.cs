using CustomPlayerEffects;
using EffectOrderer.Extensions;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
