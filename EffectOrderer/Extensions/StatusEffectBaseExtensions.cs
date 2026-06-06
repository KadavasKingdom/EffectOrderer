using CustomPlayerEffects;
using EffectOrderer.Managers;

namespace EffectOrderer.Extensions;

internal static class StatusEffectBaseExtensions
{
    internal static EffectManager GetManager(this StatusEffectBase effect) => EffectManager.GetOrCreate(effect);
}
