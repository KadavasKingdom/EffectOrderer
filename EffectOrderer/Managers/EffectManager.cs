using CustomPlayerEffects;
using HarmonyLib;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace EffectOrderer.Managers;

internal class EffectManager(StatusEffectBase effect)
{
    internal static EffectManager GetOrCreate(StatusEffectBase eff)
    => instances.GetValue(eff, _ => new(eff));

    // unsure why I don't see these used more, weak keys are nice to have
    private static ConditionalWeakTable<StatusEffectBase, EffectManager> instances = new();

    // Whether the current effect is temporarily disabled
    private bool _tempDisabled = false;
    // Effect value before being temporarily disabled
    private Effect _preDisable = Effect.zero;


    private Player owner => Player.Get(effect.Hub);
    private PlayerEffectsController controller => effect.Hub.playerEffectsController;
    // Current active effect (if temporarily disabled, the value before being temporarily disabled)
    Effect CurrentActive => _tempDisabled ? _preDisable : new(effect._intensity, effect._duration, effect._timeLeft);
    bool CurrentlyDisabled => effect.Intensity == 0 || (effect._timeLeft <= 0 && effect._duration != 0) || effect._timeLeft == -1f;

    // Intensity of permanent version of the effect
    internal byte permanentIntensity { get; private set; } = effect._duration == 0f ? effect._intensity : (byte)0;
    // All saved temporary effects, at most one for each intensity (you could have intensity 2 for 30s, and intensity 1 for 60s)
    // Does not include current effect
    private List<Effect> temporaryEffects = new();


    // use reflection to access a StatusEffectBase event (can't call them directly because John C# said so)
    void InvokeEvent(string eventName, params object[] parameters)
        => AccessTools.StaticFieldRefAccess<StatusEffectBase, Delegate>(eventName)?.DynamicInvoke(parameters);

    void TempDisable(Action then)
    {
        TempDisable();
        then();
        TempReenable();
    }
    void TempDisable()
    {
        _preDisable = CurrentActive;
        _tempDisabled = true;
        if (!CurrentActive.IsInfinite)
            temporaryEffects.Add(CurrentActive);
        RawSetEffect(Effect.zero);
    }

    void TempReenable()
    {
        RefreshEffect();
        _tempDisabled = false;
        _preDisable = Effect.zero;
    }

    internal void AddHighestTemporary(byte intensity, float duration)
    {
        TempDisable(() =>
        {
            var highest = GetHighestTemporary();
            if (!temporaryEffects.Remove(highest))
            {
                temporaryEffects.Add(new(intensity, duration, duration));
                return;
            }
            int newIntensity = highest.intensity + intensity;

            highest.intensity = (byte)Mathf.Clamp(newIntensity, 0, effect.MaxIntensity);
            highest.duration += duration;
            highest.timeLeft += duration;
            temporaryEffects.Add(highest);
            return;
        });
    }




    internal void RefreshEffect()
    {
        var highestEffect = GetHighestEffect();
        temporaryEffects.Remove(highestEffect);
        UpdateCurrentEffect(highestEffect);
    }

    internal void AddTemporaryEffect(byte intensity, float duration)
    {
        TempDisable(() =>
        {
            if (temporaryEffects.Any(x => x.intensity == intensity && x.timeLeft >= duration))
                return;

            temporaryEffects.Add(new(intensity, duration, duration));
            temporaryEffects.RemoveAll(x => x.intensity == intensity && x.timeLeft < duration);
            return;
        });
    }

    internal void AddDurTemporaryEffect(byte intensity, float duration)
    {
        TempDisable(() =>
        {
            var currentTempEffect = temporaryEffects.Where(x => x.intensity == intensity).FirstOrDefault();
            if (currentTempEffect != default)
                temporaryEffects.Remove(currentTempEffect);

            currentTempEffect.intensity = intensity;
            currentTempEffect.duration += duration;
            currentTempEffect.timeLeft += duration;
            temporaryEffects.Add(currentTempEffect);
        });
    }


    internal void SetPermanentEffect(byte intensity)
    {
        if (intensity > permanentIntensity)
        {
            permanentIntensity = intensity;
            RefreshEffect();
        }
    }

    // Change current effect to newEffect, calling events and such
    bool UpdateCurrentEffect(Effect newEffect)
        => UpdateCurrentEffect(newEffect, out _);
    bool UpdateCurrentEffect(Effect newEffect, out Effect oldEffect)
    {
        oldEffect = CurrentActive;
        // set to old effect incase we were temporarily disabled
        RawSetEffect(oldEffect);
        // no need for sync or event calling if stuff is the exact same
        if (newEffect == oldEffect)
            return false;

        // call events and such
        PlayerEffectUpdatingEventArgs args = new(owner.ReferenceHub, effect, newEffect.intensity, newEffect.duration);
        PlayerEvents.OnUpdatingEffect(args);

        if (!args.IsAllowed)
            return false;
        newEffect.intensity = args.Intensity;
        newEffect.duration = args.Duration;
        newEffect.timeLeft += (args.Duration - newEffect.duration);

        RawSetEffect(newEffect);
        if (!_tempDisabled && oldEffect.intensity > 0 && oldEffect.timeLeft > 0f)
            temporaryEffects.Add(oldEffect);
        controller.ServerSyncEffect(effect);
        PlayerEvents.OnUpdatedEffect(new(owner.ReferenceHub, effect, newEffect.intensity, newEffect.duration));
        if (oldEffect.intensity == 0 && newEffect.intensity > 0)
        {
            effect.Enabled();
            InvokeEvent("OnEnabled", effect);
        }
        else if (newEffect.intensity == 0 && oldEffect.intensity > 0)
        {
            effect.Disabled();
            InvokeEvent("OnDisabled", effect);
        }

        if (oldEffect.intensity != newEffect.intensity)
        {
            effect.IntensityChanged(oldEffect.intensity, newEffect.intensity);
            InvokeEvent("OnIntensityChanged", effect, oldEffect.intensity, newEffect.intensity);
        }


        return true;
    }

    void RawSetEffect(Effect newEffect)
    {
        effect._intensity = newEffect.intensity;
        effect._duration = newEffect.duration;
        effect._timeLeft = newEffect.timeLeft;
    }

    // Get highest intensity effect, including current one and permanent one
    Effect GetHighestEffect()
    {
        Effect highestEffect = CurrentlyDisabled ? Effect.zero : CurrentActive;

        if (permanentIntensity >= highestEffect.intensity)
            highestEffect = new(permanentIntensity, 0f, 0f);

        var highestTemporary = GetHighestTemporary();
        if (highestTemporary.intensity > highestEffect.intensity)
            highestEffect = highestTemporary;


        return highestEffect;
    }

    // Highest intensity effect in temporaryEffects
    Effect GetHighestTemporary()
    {
        IEnumerable<Effect> highestBacked =
            from eff in temporaryEffects
            orderby eff.intensity, eff.timeLeft descending
            select eff;

        return highestBacked.FirstOrDefault();
    }

    // Replaces StatusEffectBase.SetState
    internal void SetState(byte intensity, float duration, bool addDuration)
    {
        if (intensity == 0)
        {
            DisableEffect();
            return;
        }

        if (duration == 0)
        {
            SetPermanentEffect(intensity);
            return;
        }

        if (addDuration)
        {
            AddDurTemporaryEffect(intensity, duration);
            return;
        }

        AddTemporaryEffect(intensity, duration);
    }


    // Equivalent to StatusEffectBase.DisableEffect
    void DisableEffect()
    {
        temporaryEffects.Clear();
        permanentIntensity = 0;
        if (effect._intensity != 0)
            RawSetEffect(new(effect._intensity, -1f, -1f));
        RefreshEffect();
    }

    // Replaces StatusEffectBase.Update
    internal void Update()
    {
        if (!effect.IsEnabled)
            return;

        if (effect._duration != 0f)
        {
            effect._timeLeft -= Time.deltaTime;
            if (effect._timeLeft < 0f)
            {
                RefreshEffect();
            }
        }


        effect.OnEffectUpdate();
    }

    // Replaces StatusEffectBase.ForceIntensity
    internal void ForceIntensity(byte value)
    {
        if (value == effect._intensity)
            return;

        if (value == 0)
        {
            DisableEffect();
            return;
        }

        TempDisable(() =>
        {
            var highest = GetHighestEffect();
            if (highest.IsInfinite && highest.intensity > 0)
                permanentIntensity = value;
            else
            {
                temporaryEffects.Remove(highest);
                highest.intensity = value;
                temporaryEffects.Add(highest);
            }
        });
    }
}
