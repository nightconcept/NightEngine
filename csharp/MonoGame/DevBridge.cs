#if DEBUG
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
namespace NightEngine.MonoGame;

public sealed class DevBridge : IDisposable
{
    private sealed record Request(JsonElement Data, TaskCompletionSource<object?> Reply);
    private readonly TcpListener listener = new(IPAddress.Loopback, 12346);
    private readonly CancellationTokenSource stop = new();
    private readonly ConcurrentQueue<Request> requests = new();
    private readonly Host host; private readonly Func<object> snapshot; private readonly Action reload;
    private readonly List<Request> waiting = [];
    public DevBridge(Host host, Func<object> snapshot, Action reload) { this.host = host; this.snapshot = snapshot; this.reload = reload; listener.Start(); _ = Accept(); }
    private async Task Accept() { try { while (!stop.IsCancellationRequested) { var client = await listener.AcceptTcpClientAsync(stop.Token); _ = Serve(client); } } catch (OperationCanceledException) { } catch (SocketException) when (stop.IsCancellationRequested) { } }
    private async Task Serve(TcpClient client)
    {
        using (client) using (var reader = new StreamReader(client.GetStream())) using (var writer = new StreamWriter(client.GetStream()) { AutoFlush = true })
            try
            {
                string? line = await reader.ReadLineAsync(stop.Token); if (line is null || line.Length > 65536) return; using var doc = JsonDocument.Parse(line); var data = doc.RootElement.Clone();
                var reply = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously); requests.Enqueue(new(data, reply)); object? result = await reply.Task.WaitAsync(TimeSpan.FromSeconds(28), stop.Token);
                await writer.WriteLineAsync(JsonSerializer.Serialize(new { _reqId = data.GetProperty("_reqId").GetInt32(), result }));
            }
            catch (Exception e) when (e is JsonException or ArgumentException or InvalidOperationException or TimeoutException or OperationCanceledException or IOException) { if (!stop.IsCancellationRequested) try { await writer.WriteLineAsync(JsonSerializer.Serialize(new { error = e.Message })); } catch (IOException) { } }
    }
    private static int Frames(JsonElement data) { int n = data.TryGetProperty("frames", out var f) ? f.GetInt32() : 1; if (n < 1 || n > 600) throw new ArgumentException("frames must be 1..600"); return n; }
    public void Update()
    {
        foreach (var request in waiting.ToArray()) if (host.PendingSteps == 0) { request.Reply.TrySetResult(State()); waiting.Remove(request); }
        while (requests.TryDequeue(out var r)) try
            {
                var d = r.Data; string command = d.GetProperty("command").GetString()!; object? value = null;
                switch (command)
                {
                    case "get_objects": value = State(); break;
                    case "get_screenshot": screenshots.Enqueue(r); continue;
                    case "list_assets": value = Directory.Exists(Path.Combine(AppContext.BaseDirectory, "assets")) ? Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "assets"), "*", SearchOption.AllDirectories).Select(p => p.Replace('\\', '/')).Order().ToArray() : []; break;
                    case "reload_assets": reload(); value = "Assets reloaded"; break;
                    case "send_input":
                        if (host.Replay is not null) throw new InvalidOperationException("Replay input cannot be changed"); if (host.PendingSteps > 0) throw new InvalidOperationException("Wait for the previous step command");
                        int code = d.GetProperty("code").GetInt32(); if (code < 0 || (code & ~host.Simulation.InputMask) != 0) throw new ArgumentException("Input bits exceed the game mask");
                        var pointers = d.TryGetProperty("pointers", out var ps) ? ps.EnumerateArray().Select(p => new Pointer(p.GetProperty("id").GetInt32(), p.GetProperty("x").GetInt32(), p.GetProperty("y").GetInt32(), p.GetProperty("startX").GetInt32(), p.GetProperty("startY").GetInt32(), p.GetProperty("phase").GetString()!, p.TryGetProperty("kind", out var k) ? k.GetString()! : "touch")).ToArray() : [];
                        if (pointers.Any(p => p.Phase is not "P" and not "H" and not "R" and not "C" || p.Kind is not "touch" and not "mouse" and not "pen") || pointers.Select(p => p.Id).Distinct().Count() != pointers.Length) throw new ArgumentException("Invalid pointer input");
                        host.PendingSteps = Frames(d); host.Paused = true; host.InjectedInput = code; host.InjectedPointers = pointers; waiting.Add(r); continue;
                    case "run_action":
                        switch (d.GetProperty("action").GetString())
                        {
                            case "pause": host.Paused = true; break;
                            case "resume": host.Paused = false; host.InjectedInput = 0; break;
                            case "step": if (host.PendingSteps > 0) throw new InvalidOperationException("Step already pending"); host.Paused = true; host.InjectedInput = 0; host.PendingSteps = Frames(d); waiting.Add(r); continue;
                            case "save_replay": string path = d.GetProperty("path").GetString()!; if (string.IsNullOrWhiteSpace(path) || path.Length > 512) throw new ArgumentException("Invalid replay path"); host.Recording.Save(path); break;
                            default: throw new ArgumentException("Unknown action");
                        }
                        value = State(); break;
                    default: throw new ArgumentException("Unknown command");
                }
                r.Reply.TrySetResult(value);
            }
            catch (Exception e) when (e is ArgumentException or InvalidOperationException or KeyNotFoundException or FormatException or IOException) { r.Reply.TrySetException(e); }
    }
    private object State() => new { frame = host.Simulation.Frame, scene = host.Simulation.Scene.GetType().Name, paused = host.Paused, input = host.InjectedInput, cues = host.Simulation.Cues.ToArray(), hash = Trace.Hash(snapshot()), snapshot = snapshot() };
    private readonly Queue<Request> screenshots = new();
    public void Draw() { while (screenshots.TryDequeue(out var r)) r.Reply.TrySetResult(Convert.ToBase64String(host.Screenshot())); }
    public void Dispose() { stop.Cancel(); listener.Stop(); foreach (var r in waiting) r.Reply.TrySetCanceled(); stop.Dispose(); }
}
#endif
