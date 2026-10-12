namespace NightEngine;
public sealed class Letterbox
{
    public int X { get; } public int Y { get; } public int Width { get; } public int Height { get; }
    private readonly int logicalWidth, logicalHeight;
    public Letterbox(int width, int height, int logicalWidth, int logicalHeight)
    {
        if (width <= 0 || height <= 0 || logicalWidth <= 0 || logicalHeight <= 0) throw new ArgumentOutOfRangeException(nameof(width));
        this.logicalWidth = logicalWidth; this.logicalHeight = logicalHeight;
        double scale = Math.Min((double)width / logicalWidth, (double)height / logicalHeight);
        Width = Math.Max(1, (int)(logicalWidth * scale)); Height = Math.Max(1, (int)(logicalHeight * scale)); X = (width - Width) / 2; Y = (height - Height) / 2;
    }
    public (int X, int Y)? Logical(int x, int y) => x < X || x >= X + Width || y < Y || y >= Y + Height ? null : ((x - X) * logicalWidth / Width, (y - Y) * logicalHeight / Height);
}
