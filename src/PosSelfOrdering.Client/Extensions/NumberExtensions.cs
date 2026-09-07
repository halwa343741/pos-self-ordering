using System.Globalization;

namespace PosSelfOrdering.Client.Extensions;

public static class NumberExtensions
{
    private static readonly CultureInfo IdCulture = new("id-ID");

    public static string ToIdr(this decimal amount)
    {
        return $"Rp {amount.ToString("N0", IdCulture)}";
    }

    public static string ToIdr(this int amount)
    {
        return $"Rp {amount.ToString("N0", IdCulture)}";
    }
}
