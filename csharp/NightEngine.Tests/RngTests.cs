using System.Text.Json;
using NightEngine;

public static class RngTests
{
    public static void Check()
    {
        using var fixture = JsonDocument.Parse(File.ReadAllText("nightengine/contract/rng.json"));
        foreach (var vector in fixture.RootElement.GetProperty("rng").EnumerateArray())
        {
            var seed = vector.GetProperty("seed");
            var rng = seed.ValueKind == JsonValueKind.String ? new Rng(seed.GetString()!) : new Rng(seed.GetInt64());
            foreach (var expected in vector.GetProperty("randint(-5,150)").EnumerateArray())
                if (rng.Int(-5, 150) != expected.GetInt32()) throw new Exception($"RNG randint seed {seed}");
            foreach (var expected in vector.GetProperty("getrandbits(32)").EnumerateArray())
                if (rng.Bits(32) != expected.GetUInt32()) throw new Exception($"RNG bits seed {seed}");
        }
    }
}
