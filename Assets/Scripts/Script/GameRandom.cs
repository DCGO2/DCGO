using System;

/// <summary>
/// Deterministic PRNG for game-critical randomness (deck shuffling, sync keys, coin flips).
/// Uses Xoshiro256** with SplitMix64 seeding for a 256-bit internal state.
/// Both clients seed with the same value to produce identical sequences.
/// </summary>
public static class GameRandom
{
    private static ulong s0, s1, s2, s3;
    private static bool initialized = false;

    /// <summary>
    /// Seed the PRNG with a 64-bit value. Uses SplitMix64 to expand
    /// the seed into 256 bits of Xoshiro256** state.
    /// </summary>
    public static void Seed(long seed)
    {
        ulong s = (ulong)seed;
        s0 = SplitMix64(ref s);
        s1 = SplitMix64(ref s);
        s2 = SplitMix64(ref s);
        s3 = SplitMix64(ref s);

        // Ensure state is not all-zero (degenerate case)
        if (s0 == 0 && s1 == 0 && s2 == 0 && s3 == 0)
            s0 = 1;

        initialized = true;
    }

    /// <summary>
    /// Returns a random int in [min, exclusiveMax) with no modulo bias.
    /// Uses rejection sampling.
    /// </summary>
    public static int Range(int min, int exclusiveMax)
    {
        if (!initialized)
            throw new InvalidOperationException("GameRandom has not been seeded. Call GameRandom.Seed() first.");

        if (exclusiveMax <= min)
            return min;

        uint range = (uint)(exclusiveMax - min);

        if (range == 1)
            return min;

        // Rejection sampling to eliminate modulo bias
        // Threshold: values below this are in the biased zone
        uint threshold = (uint)((0x100000000UL - range) % range);

        uint raw;
        do
        {
            raw = NextUInt32();
        } while (raw < threshold);

        return min + (int)(raw % range);
    }

    /// <summary>
    /// Returns a random float in [min, max).
    /// </summary>
    public static float Range(float min, float max)
    {
        if (!initialized)
            throw new InvalidOperationException("GameRandom has not been seeded. Call GameRandom.Seed() first.");

        // Use 24 bits of randomness for float precision (IEEE 754 single has 23-bit mantissa)
        float t = (NextUInt32() >> 8) * (1.0f / (1 << 24));
        return min + (max - min) * t;
    }

    /// <summary>
    /// Returns true with the given probability [0..1].
    /// </summary>
    public static bool Probability(float p)
    {
        if (p >= 1f) return true;
        if (p <= 0f) return false;
        return Range(0f, 1f) <= p;
    }

    // --- Internal: Xoshiro256** ---

    private static ulong NextState()
    {
        // xoshiro256** result calculation
        ulong result = RotateLeft(s1 * 5, 7) * 9;

        ulong t = s1 << 17;

        s2 ^= s0;
        s3 ^= s1;
        s1 ^= s2;
        s0 ^= s3;

        s2 ^= t;
        s3 = RotateLeft(s3, 45);

        return result;
    }

    private static uint NextUInt32()
    {
        return (uint)(NextState() >> 32);
    }

    private static ulong RotateLeft(ulong x, int k)
    {
        return (x << k) | (x >> (64 - k));
    }

    // --- Internal: SplitMix64 (for seed expansion) ---

    private static ulong SplitMix64(ref ulong state)
    {
        ulong z = (state += 0x9E3779B97F4A7C15UL);
        z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
        z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
        return z ^ (z >> 31);
    }
}
