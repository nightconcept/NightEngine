using System.Text.Json;
using NightEngine;

public static class FrameTests
{
    private sealed class Counter : Scene
    {
        public int Value;
        public override void Update(Game game, Input input)
        {
            if (input.Down(16)) Value++;
            if (input.Hit(16)) game.Cue("act");
        }
    }
    public static void Check()
    {
        using var doc = JsonDocument.Parse(File.ReadAllText("nightengine/contract/behavior.json"));
        var game = new Game(); var counter = new Counter(); game.Scenes.Replace(counter);
        var expected = doc.RootElement.GetProperty("states");
        void Match(int frame)
        {
            object?[] state = [game.Frame, counter.Value, game.Cues];
            if (Trace.Canon(state) != expected[frame].GetRawText().Replace("\n", "").Replace(" ", ""))
                throw new Exception($"Frame contract mismatch at {frame}");
        }
        Match(0);
        int i = 0;
        foreach (var code in doc.RootElement.GetProperty("codes").EnumerateArray()) { game.Step(code.GetInt32()); Match(++i); }
        var recording = Recording.Load("nightengine/contract/events.json");
        if (recording.EventsAt(2).Length != 2 || Recording.Parse(recording.Json()).Frames.Count != 3)
            throw new Exception("Replay event contract mismatch");
        object?[] value = [new object?[] { "abc", 1.5, 3, -0.0, true, null, new[] { "q\"x\\" } }];
        using var rng = JsonDocument.Parse(File.ReadAllText("nightengine/contract/rng.json"));
        if (Trace.Canon(value) != rng.RootElement.GetProperty("canon").GetProperty("text").GetString() || Trace.Hash(value) != "e648789b")
            throw new Exception("Canonical hash contract mismatch");
        if (Trace.Canon(new[] { 1e-5, 1e20, -0.0 }) != "[1.0000000000000001e-05,1e+20,0]")
            throw new Exception("Canonical exponent encoding mismatch");
    }
}
