using NightEngine;
public static class BoundaryTests
{
    public static void Check()
    {
        void Reject(string text) { try { Recording.Parse(text); } catch (FormatException) { return; } throw new Exception("Accepted invalid replay"); }
        Reject("{\"seed\":0,\"width\":2,\"inputs\":\"00\",\"pointers\":{\"2\":[[1,0,0,0,0,\"P\"]]}}");
        Reject("{\"seed\":0,\"width\":2,\"inputs\":\"00\",\"pointers\":{\"0\":[[1,0,0,0,0,\"X\"]]}}");
        Reject("{\"seed\":0,\"width\":2,\"inputs\":\"00\",\"target\":{\"name\":\"android\",\"width\":640,\"height\":480,\"controls\":[\"shell\"]}}");
        var r = new Recording(42); r.Add(16, [new(1, 3, 4, 3, 4, "P")], ["pause"]); var copy = Recording.Parse(r.Json());
        if (copy.Frames[0] != 16 || copy.At(0)[0].X != 3 || copy.EventsAt(0)[0] != "pause") throw new Exception("Replay round trip");
    }
}
