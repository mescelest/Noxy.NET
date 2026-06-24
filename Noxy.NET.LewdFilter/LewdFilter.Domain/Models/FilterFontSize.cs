using System.Globalization;
using LewdFilter.Domain.Abstractions;

namespace LewdFilter.Domain.Models;

public record FilterFontSize(double? Value) : FilterEntity
{
    public const double Min = 18;
    public const double Max = 45;
    public const double Step = 1;

    public override string ToString()
    {
        return ((int?)Value ?? 0).ToString(CultureInfo.InvariantCulture);
    }
}
