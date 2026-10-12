using System.Collections;
using System.Globalization;
using System.Text;

namespace NightEngine;

public static class Trace
{
    public static string Canon(object? value)
    {
        if (value == null) return "null";
        if (value is bool b) return b ? "true" : "false";
        if (value is string s) return "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
        if (value is IDictionary) throw new ArgumentException("Snapshots must use lists, not maps");
        if (value is IEnumerable sequence) return "[" + string.Join(",", sequence.Cast<object?>().Select(Canon)) + "]";
        if (value is not (byte or sbyte or short or ushort or int or uint or long or ulong or float or double or decimal))
            throw new ArgumentException("Unsupported snapshot value");
        double number = Convert.ToDouble(value, CultureInfo.InvariantCulture);
        return number == 0 ? "0" : number.ToString("G17", CultureInfo.InvariantCulture).ToLowerInvariant();
    }
    public static string Digest(object? value)
    {
        uint crc = 0xffffffff;
        foreach (byte b in Encoding.UTF8.GetBytes(Canon(value)))
        {
            crc ^= b;
            for (int i = 0; i < 8; i++) crc = (crc >> 1) ^ ((crc & 1) == 1 ? 0xedb88320u : 0);
        }
        return (~crc).ToString("x8", CultureInfo.InvariantCulture);
    }
}
