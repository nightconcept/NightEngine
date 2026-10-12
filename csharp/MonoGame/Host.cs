using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
namespace NightEngine.MonoGame;

public abstract class Host : Microsoft.Xna.Framework.Game
{
    public NightEngine.Game Simulation { get; }
    public Recording Recording { get; }
    public Recording? Replay;
    protected SpriteBatch Batch = null!;
    protected RenderTarget2D Screen = null!;
    private readonly Dictionary<int, (int X, int Y)> starts = [];
    private bool mouseDown;
    public string? FramesDirectory;
    public int CaptureEvery = 60;
    public int StopAfter;
    public bool Paused;
    public int PendingSteps;
    public int InjectedInput;
    public Pointer[] InjectedPointers = [];
    protected Host(NightEngine.Game simulation)
    {
        Simulation = simulation; Recording = new(simulation.Seed, 7, simulation.Target);
        _ = new GraphicsDeviceManager(this) { PreferredBackBufferWidth = simulation.ScreenSize.Width, PreferredBackBufferHeight = simulation.ScreenSize.Height, SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight };
        TargetElapsedTime = TimeSpan.FromSeconds(1.0 / 30); IsMouseVisible = true; Window.AllowUserResizing = true;
#if DEBUG
        Window.Title = "NightEngine MonoGame";
#endif
    }
    protected override void LoadContent() { Batch = new(GraphicsDevice); Screen = new(GraphicsDevice, Simulation.ScreenSize.Width, Simulation.ScreenSize.Height, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents); }
    protected abstract int ReadKeys(KeyboardState keys);
    protected abstract void Render();
    protected virtual int AutomaticInput() => 0;
    protected virtual void PlayCues(IReadOnlyList<string> cues) { }
    protected Letterbox View => new(GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight, Simulation.ScreenSize.Width, Simulation.ScreenSize.Height);
    private Pointer[] ReadPointers()
    {
        List<Pointer> result = []; var view = View;
        void Contact(int id, int x, int y, string phase, string kind)
        {
            var pos = view.Logical(x, y);
            if (phase == "P" && pos is { } begin) starts[id] = begin;
            if (!starts.TryGetValue(id, out var start)) return;
            var point = pos ?? (-1000, -1000); result.Add(new(id, point.Item1, point.Item2, start.X, start.Y, pos is null && phase == "R" ? "C" : phase, kind));
            if (phase is "R" or "C") starts.Remove(id);
        }
        foreach (var p in TouchPanel.GetState()) Contact(p.Id, (int)p.Position.X, (int)p.Position.Y, p.State == TouchLocationState.Pressed ? "P" : p.State == TouchLocationState.Released ? "R" : "H", "touch");
        var mouse = Mouse.GetState(); bool down = mouse.LeftButton == ButtonState.Pressed;
        if (down || mouseDown) Contact(-1, mouse.X, mouse.Y, down ? mouseDown ? "H" : "P" : "R", "mouse"); mouseDown = down;
        return result.ToArray();
    }
    protected override void Update(GameTime time)
    {
#if DEBUG
        UpdateBridge();
#endif
        if (!IsActive) { starts.Clear(); mouseDown = false; return; }
        if (Paused && PendingSteps == 0) return;
        if (PendingSteps > 0) PendingSteps--;
        int frame = Simulation.Frame;
        if (Replay is not null && frame >= Replay.Frames.Count || StopAfter > 0 && frame >= StopAfter) { Exit(); return; }
        int code = Replay is null ? ReadKeys(Keyboard.GetState()) | AutomaticInput() | InjectedInput : Replay.Frames[frame];
        Pointer[] pointers = Replay is null ? ReadPointers().Concat(InjectedPointers).ToArray() : Replay.At(frame);
        InjectedPointers = []; string[] events = Replay?.EventsAt(frame) ?? [];
        Recording.Add(code, pointers, events); PlayCues(Simulation.Step(code, pointers, events));
        if (Simulation.QuitRequested) Exit(); base.Update(time);
    }
    protected override void Draw(GameTime time)
    {
        var size = Simulation.ScreenSize;
        if (Screen.Width != size.Width || Screen.Height != size.Height) { Screen.Dispose(); Screen = new(GraphicsDevice, size.Width, size.Height, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents); }
        GraphicsDevice.SetRenderTarget(Screen); GraphicsDevice.Clear(Color.Black); Render(); GraphicsDevice.SetRenderTarget(null); GraphicsDevice.Clear(Color.Black);
        var view = View; Batch.Begin(samplerState: SamplerState.PointClamp); Batch.Draw(Screen, new Rectangle(view.X, view.Y, view.Width, view.Height), Color.White); Batch.End();
        if (FramesDirectory is not null && Simulation.Frame % CaptureEvery == 0) { Directory.CreateDirectory(FramesDirectory); using var file = File.Create(Path.Combine(FramesDirectory, $"frame{Simulation.Frame:D5}.png")); Screen.SaveAsPng(file, Screen.Width, Screen.Height); }
#if DEBUG
        DrawBridge();
#endif
        base.Draw(time);
    }
    public byte[] Screenshot() { using var buffer = new MemoryStream(); Screen.SaveAsPng(buffer, Screen.Width, Screen.Height); return buffer.ToArray(); }
    protected override void OnDeactivated(object sender, EventArgs args) { starts.Clear(); mouseDown = false; base.OnDeactivated(sender, args); }
    protected override void UnloadContent() { Screen.Dispose(); Batch.Dispose(); base.UnloadContent(); }
#if DEBUG
    protected virtual void UpdateBridge() { }
    protected virtual void DrawBridge() { }
#endif
}
