using CommandSystem;
using HarmonyLib;
using LabApi.Events.Handlers;
using LabApi.Loader.Features.Plugins;
using MEC;
using PlayerStatsSystem;
using Unity.Collections;
using UnityEngine;

namespace EffectOrderer;

public class EffectOrderer : Plugin
{
    public override string Name => "EffectOrderer";

    public override string Description => "Manages permanent and temporary effects";

    public override string Author => "KadavasKingdom";

    public override Version Version => new(0, 1);

    public override Version RequiredApiVersion => LabApi.Features.LabApiProperties.CurrentVersion;

    Harmony? harmony;
    public override void Enable()
    {
        harmony = new(Name);
        harmony.PatchAll();
    }
    public override void Disable()
    {
        harmony!.UnpatchAll(Name);
    }
}
