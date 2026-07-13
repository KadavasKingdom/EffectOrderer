# Effect Orderer
This plugin makes use of custom effect managers (overwriting StatusEffectBase.<ServerSetState, Update, ForceIntensity> for all effects EXCEPT technical ones).

These managers allow several instances of a status effect to "stack", where one is executed after another. Permanent effects will not be overwritten by temporary 
ones, unless the temporary one has a higher intensity. When that temporary effect runs out, the intensity will return to the permanent intensity.

## Examples/footguns:
### Using ServerSetState to lower intensity
ServerSetState will not allow you to lower the intensity of an effect. ForceIntensity must be used instead.
```cs
player.EnableEffect<Slowness>(10, 0f); // player has slowness 10
player.EnableEffect<Slowness>(5, 0f); // In vanilla, player has slowness 5. With EffectOrderer, player has slowness 10 instead.

player.GetEffect<Slowness>().Intensity = 5; // In vanilla and with EffectOrderer, player now has slowness 5.
```

### Yellow candy
Yellow candy doesn't follow the usual rules of effects, adding intensity and duration specifically to temporary MovementBoost. It works intuitively, only applying its effect to active temporary effects and not permanent MovementBoost.

### Cancellable permanent effects
If you don't know how long an effect will last, you can use a cancellable permanent effect to cancel it when it's done. 
```cs
public class StainedManager(Player player) {
  Action _cancelAction;
  public void OnPlayerEnteredTantrum() {
    _cancelAction = player.AddCancellablePermanentEffect<Stained>(1);
  }

  public void OnPlayerExitedTantrum() {
    _cancelAction();
  }
}
```
This is useful for when an effect must be enabled and forcefully disabled, but you want to ignore other instances of the effect that may be created during the interim.

Another example is an item that grants movement boost when held out. You can add a cancellable 10% movement boost, and when the player unequips the item that cancellation is invoked.
