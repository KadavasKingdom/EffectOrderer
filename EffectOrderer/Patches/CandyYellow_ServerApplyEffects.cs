using CustomPlayerEffects;
using EffectOrderer.Extensions;
using HarmonyLib;
using InventorySystem.Items.Usables.Scp330;
using PlayerStatsSystem;

namespace EffectOrderer.Patches;

// Yellow candy is a kind of special case with effects
[HarmonyPatch(typeof(CandyYellow), nameof(CandyYellow.ServerApplyEffects))]
internal static class CandyYellow_ServerApplyEffects
{
    internal static bool Prefix(CandyYellow __instance, ReferenceHub hub)
    {
        MovementBoost effect = hub.playerEffectsController.GetEffect<MovementBoost>();
        if (effect.Duration == 0 && effect.Intensity > 0)
        {
            hub.playerStats.GetModule<StaminaStat>().AddAmount(0.25f);
            hub.playerEffectsController.EnableEffect<Invigorated>(8f, true);
            effect.GetManager().AddHighestTemporary(10, 8f);
            return false;
        }
        return true;
    }
}
