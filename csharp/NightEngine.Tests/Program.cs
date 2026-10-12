using System.Reflection;

Directory.SetCurrentDirectory(AppContext.BaseDirectory);

var checks = typeof(Program).Assembly.GetTypes().Select(t => t.GetMethod("Check", BindingFlags.Public | BindingFlags.Static)).Where(m => m != null).ToArray();
if (checks.Length == 0) throw new Exception("No checks found");
foreach (var check in checks)
{
    check!.Invoke(null, null);
    Console.WriteLine($"PASS {check.DeclaringType!.Name}");
}
