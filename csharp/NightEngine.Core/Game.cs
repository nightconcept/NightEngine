namespace NightEngine;

public static class Buttons
{
    public const int Up = 1, Down = 2, Left = 4, Right = 8, A = 16, B = 32, C = 64, Menu = 128, All = 255;
}

public record Pointer(int Id, int X, int Y, int StartX, int StartY, string Phase, string Kind = "touch")
{
    public bool Active => Phase is not "R" and not "C";
    public object[] Row() => Kind == "touch" ? [Id, X, Y, StartX, StartY, Phase] : [Id, X, Y, StartX, StartY, Phase, Kind[..1]];
}

public record Target(string Name = "desktop", int Width = 0, int Height = 0, string[]? Controls = null)
{
    public string[] Inputs => Controls ?? ["keyboard", "mouse", "touch"];
    public bool TouchOnly => Inputs.Length == 1 && Inputs[0] == "touch";
    public bool Accepts(Pointer p) => p.Kind == "mouse" ? Inputs.Contains("mouse") || TouchOnly : Inputs.Contains("touch");
}

public record Input(int Held, int Pressed, int Repeat, Pointer[] Pointers, string[] Events)
{
    public bool Down(int bit) => (Held & bit) != 0;
    public bool Hit(int bit) => (Pressed & bit) != 0;
    public bool Nav(int bit) => (Repeat & bit) != 0;
}

public sealed class InputTracker(int mask = 255, int[]? repeat = null, int delay = 14, int rate = 5)
{
    private int previous;
    private readonly Dictionary<int, int> ages = (repeat ?? []).ToDictionary(b => b, _ => 0);
    public Input Feed(int code, Pointer[]? pointers = null, string[]? events = null)
    {
        code &= mask;
        int pressed = code & ~previous, ticks = pressed;
        foreach (int bit in ages.Keys)
        {
            ages[bit] = (code & bit) != 0 ? ages[bit] + 1 : 0;
            int elapsed = ages[bit] - delay;
            if (ages[bit] > 0 && elapsed >= 0 && elapsed % rate == 0) ticks |= bit;
        }
        previous = code;
        return new(code, pressed, ticks, pointers ?? [], events ?? []);
    }
}

public class Scene
{
    public virtual bool Overlay => false;
    public virtual void Update(Game game, Input input) { }
}

public sealed class SceneStack
{
    private readonly List<Scene> items = [];
    public Scene Top => items[^1];
    public void Push(Scene scene) => items.Add(scene);
    public void Pop(Scene scene) => items.Remove(scene);
    public void Replace(params Scene[] scenes) { items.Clear(); items.AddRange(scenes); }
    public Scene[] Visible()
    {
        int start = items.Count - 1;
        while (start > 0 && items[start].Overlay) start--;
        return items.Skip(Math.Max(0, start)).ToArray();
    }
}

public class Game
{
    public long Seed { get; }
    public Target Target { get; }
    public Rng Rng { get; }
    public int Frame { get; private set; }
    public int InputMask { get; }
    public SceneStack Scenes { get; } = new();
    public Scene Scene => Scenes.Top;
    public List<string> Cues { get; } = [];
    public bool QuitRequested;
    private readonly InputTracker tracker;
    public Game(long seed = 0, Target? target = null, int mask = 255, int[]? repeat = null, int delay = 14, int rate = 5)
    {
        Seed = seed; Target = target ?? new(); Rng = new(seed); InputMask = mask;
        tracker = new(mask, repeat, delay, rate);
    }
    public virtual (int Width, int Height) ScreenSize => (Target.Width == 0 ? 640 : Target.Width, Target.Height == 0 ? 480 : Target.Height);
    public void Cue(string name) => Cues.Add(name);
    protected virtual void BeforeScene(Input input) { }
    protected virtual void AfterScene() { }
    public IReadOnlyList<string> Step(int code, Pointer[]? pointers = null, string[]? events = null)
    {
        Cues.Clear();
        var input = tracker.Feed(code, (pointers ?? []).Where(Target.Accepts).ToArray(), events);
        BeforeScene(input); Scene.Update(this, input); AfterScene(); Frame++;
        return Cues;
    }
}
