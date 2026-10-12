using NightEngine;
public static class ViewportTests
{
    public static void Check()
    {
        var view = new Letterbox(1920, 1080, 640, 480);
        if ((view.X, view.Y, view.Width, view.Height) != (240, 0, 1440, 1080)) throw new Exception("Letterbox bounds");
        if (view.Logical(960, 540) != (320, 240) || view.Logical(0, 0) is not null) throw new Exception("Pointer mapping");
        var portrait = new Letterbox(1080, 1920, 854, 480);
        if (portrait.Logical(540, 960) != (427, 240)) throw new Exception("Portrait centering");
    }
}
