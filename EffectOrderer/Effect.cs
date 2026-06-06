namespace EffectOrderer;
internal record struct Effect(byte intensity, float duration, float timeLeft)
{
    internal static readonly Effect zero = new(0, 0f, 0f);
    // Effect.zero also counts as infinite
    internal readonly bool IsInfinite => duration == 0f;
};