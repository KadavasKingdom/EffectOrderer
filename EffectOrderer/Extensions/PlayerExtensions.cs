using CustomPlayerEffects;
using UnityEngine;

namespace EffectOrderer.Extensions
{
    public static class PlayerExtensions
    {
        /// <summary>
        /// Sets the effect of type <typeparamref name="T"/> to the current permanent value plus <paramref name="intensity"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="player"></param>
        /// <param name="intensity"></param>
        /// <returns>The new intensity</returns>
        public static byte AddPermanentEffect<T>(this Player player, byte intensity) where T: StatusEffectBase
        {
            var effect = player.GetEffect<T>();
            if (effect == null)
                return 0;

            var manager = effect.GetManager();
            manager.SetPermanentEffect((byte)Mathf.Clamp(intensity + manager.permanentIntensity, 0, 255));
            return manager.permanentIntensity;
        }

        /// <summary>
        /// Creates a cancellable permanent effect on <paramref name="player"/> with intensity <paramref name="intensity"/>
        /// </summary>
        /// <param name="player"></param>
        /// <param name="intensity"></param>
        /// <returns>Action which, when called, removes the permanent effect</returns>
        public static Action AddCancellablePermanentEffect<T>(this Player player, byte intensity) where T : StatusEffectBase
            => AddCancellablePermanentEffect(player, player.GetEffect<T>()!, intensity);

        /// <summary>
        /// Creates a cancellable permanent effect on <paramref name="player"/> with intensity <paramref name="intensity"/>
        /// </summary>
        /// <param name="player"></param>
        /// <param name="intensity"></param>
        /// <returns>Action which, when called, removes the permanent effect</returns>
        public static Action AddCancellablePermanentEffect(this Player player, StatusEffectBase effect, byte intensity)
            => effect.GetManager().AddCancellablePermanentEffect(intensity);
    }
}
