using CustomPlayerEffects;
using EffectOrderer.Extensions;
using EffectOrderer.Managers;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EffectOrderer.Patches;

[HarmonyPatch(typeof(StatusEffectBase), nameof(StatusEffectBase.ServerSetState))]
[HarmonyPriority(Priority.Last)]
internal static class StatusEffectBase_ServerSetState
{
    internal static bool Prefix(StatusEffectBase __instance, byte intensity, float duration, bool addDuration)
    {
        if (__instance.Classification == StatusEffectBase.EffectClassification.Technical) return true;

        CL.Debug($"{__instance.GetType().Name} set with Intensity {intensity} for {duration}s and AddDuration {addDuration}");
        try
        {
            __instance.GetManager().SetState(intensity, duration, addDuration);
        } catch (Exception e)
        {
            CL.Error(e);
        }

        return false;
    }
}
