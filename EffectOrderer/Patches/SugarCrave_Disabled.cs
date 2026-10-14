using CustomPlayerEffects;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MEC;

namespace EffectOrderer.Patches;

// masterfully craft this patch to not error when compiled against non halloween builds (aka don't directly reference any halloween types)
// because of effectorderer and how marshmallow effect works (with blue candy), the marshmallow effect stays
[HarmonyPatch]
internal static class SugarCrave_Disabled
{
    private static void _dummy(StatusEffectBase _) {}
    internal static IEnumerable<MethodBase> TargetMethods()
    {
        Type? sugarCrave = Assembly.GetAssembly(typeof(ReferenceHub)).GetType("CustomPlayerEffects.SugarCrave");
        if (sugarCrave == null)
            return [typeof(SugarCrave_Disabled).GetMethod("_dummy", BindingFlags.Static | BindingFlags.NonPublic)!];

        return [sugarCrave.GetMethod("Disabled", BindingFlags.Instance | BindingFlags.NonPublic)!];
    }

    internal static void Prefix(StatusEffectBase __instance)
    {
        Timing.CallDelayed(1.2f, () =>
        {
            var mallowEffect = __instance.Hub.playerEffectsController.AllEffects.FirstOrDefault(x => x.GetType().Name == "MarshmallowEffect");
            if (mallowEffect != null && mallowEffect.Intensity > 0)
            {
                mallowEffect.DisableEffect();
            }
        });
    }
}