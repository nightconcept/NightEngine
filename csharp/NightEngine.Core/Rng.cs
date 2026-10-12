using System.Text;

namespace NightEngine;

public sealed class Rng
{
    public uint[] State { get; private set; }
    public Rng(string seed) : this(Hash(seed)) { }
    public Rng(long seed = 0)
    {
        uint x = unchecked((uint)seed);
        State = new uint[4];
        for (int i = 0; i < 4; i++)
        {
            x = unchecked(x + 0x9e3779b9);
            uint z = unchecked((x ^ (x >> 16)) * 0x85ebca6b);
            z = unchecked((z ^ (z >> 13)) * 0xc2b2ae35);
            State[i] = z ^ (z >> 16);
        }
        if (State.All(s => s == 0)) State[0] = 1;
    }
    private static uint Hash(string text)
    {
        uint h = 0x811c9dc5;
        foreach (byte b in Encoding.UTF8.GetBytes(text)) h = unchecked((h ^ b) * 0x01000193);
        return h;
    }
    private static uint Rotate(uint v, int bits) => (v << bits) | (v >> (32 - bits));
    public uint Next()
    {
        var s = State;
        uint result = unchecked(Rotate(s[1] * 5, 7) * 9), t = s[1] << 9;
        s[2] ^= s[0]; s[3] ^= s[1]; s[1] ^= s[2]; s[0] ^= s[3]; s[2] ^= t; s[3] = Rotate(s[3], 11);
        return result;
    }
    public uint Getrandbits(int count)
    {
        if (count is < 1 or > 32) throw new ArgumentOutOfRangeException(nameof(count));
        return Next() >> (32 - count);
    }
    public int Randint(int low, int high)
    {
        long n = (long)high - low + 1;
        if (n is < 1 or > 4294967296L) throw new ArgumentOutOfRangeException(nameof(high));
        long limit = 4294967296L - 4294967296L % n;
        uint value;
        do { value = Next(); } while (value >= limit);
        return (int)(low + value % n);
    }
    public double Random() => Next() / 4294967296.0;
    public Rng Copy() => new() { State = (uint[])State.Clone() };
}
