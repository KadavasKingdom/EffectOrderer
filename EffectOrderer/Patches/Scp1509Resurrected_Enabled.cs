using CustomPlayerEffects;
using HarmonyLib;
using PlayerRoles;

namespace EffectOrderer.Patches;

//Removes the annoying max hp constantly being set by the Scp109Resurrected effect
[HarmonyPatch(typeof(Scp1509Resurrected), nameof(Scp1509Resurrected.Enabled))]
internal static class Scp1509Resurrected_Enabled
{
    internal static bool Prefix(Scp1509Resurrected __instance)
    {
        if (__instance.Hub.IsSCP())
        {
            CL.Info("IS SCP ENABLED");
            return false;
        }
        else
            return true;
    }
}
