using CustomPlayerEffects;
using HarmonyLib;

namespace EffectOrderer.Patches;

//Removes the annoying max hp constantly being set by the Scp109Resurrected effect
[HarmonyPatch(typeof(Scp1509Resurrected), nameof(Scp1509Resurrected.OnEffectUpdate))]
internal static class Scp1509Resurrected_OnEffectUpdate
{
    internal static bool Prefix(Scp1509Resurrected __instance)
    {
        CL.Info("OVERRIDING UPDATE");
        return false;
    }
}
