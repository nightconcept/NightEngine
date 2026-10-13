using NightEngine;
public static class BoundaryTests
{
    public static void Check()
    {
        void Reject(string text) { try { Recording.FromJson(text); } catch (FormatException) { return; } throw new Exception("Accepted invalid replay"); }
        Reject("{\"seed\":0,\"width\":2,\"inputs\":\"00\",\"pointers\":{\"2\":[[1,0,0,0,0,\"P\"]]}}");
        Reject("{\"seed\":0,\"width\":2,\"inputs\":\"00\",\"pointers\":{\"0\":[[1,0,0,0,0,\"X\"]]}}");
        Reject("{\"seed\":0,\"width\":2,\"inputs\":\"00\",\"target\":{\"name\":\"android\",\"width\":640,\"height\":480,\"controls\":[\"shell\"]}}");
        var r = new Recording(42); r.Add(16, [new(1, 3, 4, 3, 4, "P")], ["pause"]); var copy = Recording.FromJson(r.ToJson());
        if (copy.Frames[0] != 16 || copy.At(0)[0].X != 3 || copy.EventsAt(0)[0] != "pause") throw new Exception("Replay round trip");
        // The exact text, so a game's saved replays stay the same.
        var full = new Recording(7, 2, new Target("android", 640, 480, ["touch", "mouse"])); full.Add(1); full.Add(255, [new(2, 5, 6, 1, 1, "H", "mouse"), new(3, 7, 8, 7, 8, "P")], ["dev level=4", "a\"<b>"]);
        string json = "{\"seed\":7,\"width\":2,\"inputs\":\"01ff\",\"pointers\":{\"1\":[[2,5,6,1,1,\"H\",\"m\"],[3,7,8,7,8,\"P\"]]},\"events\":{\"1\":[\"dev level=4\",\"a\\u0022\\u003Cb\\u003E\"]},\"target\":{\"name\":\"android\",\"width\":640,\"height\":480,\"controls\":[\"touch\",\"mouse\"]}}";
        if (full.ToJson() != json) throw new Exception("Replay JSON text: " + full.ToJson());
    }
}
