using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NightEngine.MonoGame;
using var game = new Example();
game.Run();
sealed class Counter : NightEngine.Scene
{
    public int Presses;
    public override void Update(NightEngine.Game game, NightEngine.Input input)
    {
        if (input.Hit(16)) Presses++;
        if (input.Hit(32)) game.QuitRequested = true;
    }
}
sealed class Example : Host
{
    private readonly Counter counter = new();
    private Texture2D pixel = null!;
#if DEBUG
    private DevBridge? bridge;
    protected override void UpdateBridge() => bridge?.Update();
    protected override void DrawBridge() => bridge?.Draw();
#endif
    public Example() : base(new NightEngine.Game()) { Simulation.Scenes.Replace(counter); }
    protected override void LoadContent()
    {
        base.LoadContent(); pixel = new(GraphicsDevice, 1, 1); pixel.SetData(new[] { Color.White });
#if DEBUG
        bridge = new(this, () => new[] { Simulation.Frame, counter.Presses }, () => { });
#endif
    }
    protected override int ReadKeys(KeyboardState keys) => (keys.IsKeyDown(Keys.Space) ? 16 : 0) | (keys.IsKeyDown(Keys.Escape) ? 32 : 0);
    protected override void Render()
    {
        GraphicsDevice.Clear(Color.Cyan); Batch.Begin(); Batch.Draw(pixel, new Rectangle(280, 200, 80, 80), counter.Presses % 2 == 0 ? Color.White : Color.Yellow); Batch.End();
    }
    protected override void UnloadContent()
    {
#if DEBUG
        bridge?.Dispose();
#endif
        pixel.Dispose(); base.UnloadContent();
    }
}
