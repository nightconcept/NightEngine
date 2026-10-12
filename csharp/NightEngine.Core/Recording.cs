using System.Globalization;
using System.Text.Json;

namespace NightEngine;

public sealed class Recording(long seed, int width = 7, Target? target = null)
{
    public long Seed { get; } = seed;
    public int Width { get; } = width;
    public Target? Target { get; } = target;
    public List<int> Frames { get; } = [];
    public Dictionary<int, Pointer[]> Pointers { get; } = [];
    public Dictionary<int, string[]> Events { get; } = [];
    public Pointer[] At(int frame) => Pointers.GetValueOrDefault(frame, []);
    public string[] EventsAt(int frame) => Events.GetValueOrDefault(frame, []);
    public void Add(int code, Pointer[]? pointers = null, string[]? events = null)
    {
        if (pointers is { Length: > 0 }) Pointers[Frames.Count] = (Pointer[])pointers.Clone();
        if (events is { Length: > 0 }) Events[Frames.Count] = (string[])events.Clone();
        Frames.Add(code);
    }
    public string Json()
    {
        var data = new Dictionary<string, object?> { ["seed"] = Seed, ["width"] = Width, ["inputs"] = string.Concat(Frames.Select(c => c.ToString($"x{Width}"))) };
        if (Pointers.Count > 0) data["pointers"] = Pointers.ToDictionary(p => p.Key.ToString(), p => p.Value.Select(v => v.Row()).ToArray());
        if (Events.Count > 0) data["events"] = Events.ToDictionary(p => p.Key.ToString(), p => p.Value);
        if (Target != null) data["target"] = new { name = Target.Name, width = Target.Width, height = Target.Height, controls = Target.Inputs };
        return JsonSerializer.Serialize(data);
    }
    public void Save(string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        File.WriteAllText(path, Json() + "\n");
    }
    public static Recording Load(string path) => Parse(File.ReadAllText(path));
    public static Recording Parse(string text)
    {
        using var document = JsonDocument.Parse(text);
        var root = document.RootElement;
        int width = root.TryGetProperty("width", out var w) ? w.GetInt32() : 2;
        string inputs = root.GetProperty("inputs").GetString()!;
        if (width is < 1 or > 8 || inputs.Length % width != 0) throw new FormatException("Invalid replay width");
        Target? target = null;
        if (root.TryGetProperty("target", out var t))
            target = new(t.GetProperty("name").GetString()!, t.GetProperty("width").GetInt32(), t.GetProperty("height").GetInt32(), t.GetProperty("controls").EnumerateArray().Select(v => v.GetString()!).ToArray());
        var result = new Recording(root.GetProperty("seed").GetInt64(), width, target);
        for (int i = 0; i < inputs.Length; i += width) result.Frames.Add(int.Parse(inputs.AsSpan(i, width), NumberStyles.HexNumber));
        if (root.TryGetProperty("pointers", out var ps))
            foreach (var frame in ps.EnumerateObject())
                result.Pointers[int.Parse(frame.Name)] = frame.Value.EnumerateArray().Select(p =>
                    new Pointer(p[0].GetInt32(), p[1].GetInt32(), p[2].GetInt32(), p[3].GetInt32(), p[4].GetInt32(), p[5].GetString()!,
                        p.GetArrayLength() > 6 ? p[6].GetString() switch { "m" => "mouse", "p" => "pen", _ => "touch" } : "touch")).ToArray();
        if (root.TryGetProperty("events", out var es))
            foreach (var frame in es.EnumerateObject()) result.Events[int.Parse(frame.Name)] = frame.Value.EnumerateArray().Select(e => e.GetString()!).ToArray();
        if (target is not null && (target.Width < 0 || target.Height < 0 || target.Inputs.Length == 0 || target.Inputs.Any(c => c is not "keyboard" and not "mouse" and not "touch"))) throw new FormatException("Invalid replay target");
        foreach (var (frame, contacts) in result.Pointers)
        {
            if (frame < 0 || frame >= result.Frames.Count || contacts.Select(p => p.Id).Distinct().Count() != contacts.Length || contacts.Any(p => p.Phase is not "P" and not "H" and not "R" and not "C")) throw new FormatException("Invalid replay pointers");
        }
        if (result.Events.Keys.Any(frame => frame < 0 || frame >= result.Frames.Count)) throw new FormatException("Invalid replay event frame");
        return result;
    }
}
