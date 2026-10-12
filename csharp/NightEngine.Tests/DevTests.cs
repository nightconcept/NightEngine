using System.Text.Json;
using NightEngine;

public static class DevTests
{
    public static void Check()
    {
        using var doc = JsonDocument.Parse(File.ReadAllText("nightengine/contract/dev.json"));
        var root = doc.RootElement;
        string[] Strings(string name) => root.GetProperty(name).EnumerateArray().Select(e => e.GetString()!).ToArray();
        if (!Strings("pairs").Select(Inputs.DevEvent).SequenceEqual(Strings("events"))) throw new Exception("Dev event contract mismatch");
        foreach (var pair in Strings("invalid"))
        {
            try { Inputs.DevEvent(pair); throw new Exception($"Dev pair {pair} should fail"); }
            catch (ArgumentException) { }
        }
        var expected = root.GetProperty("dev").EnumerateObject().ToDictionary(p => p.Name, p => p.Value.GetString()!);
        var found = new Input(0, 0, 0, [], Strings("frame")).Dev();
        if (found.Count != expected.Count || found.Any(p => expected.GetValueOrDefault(p.Key) != p.Value)) throw new Exception("Dev pairs contract mismatch");
        if (new Input(0, 0, 0, [], []).Dev().Count != 0) throw new Exception("Dev pairs without events");
    }
}
