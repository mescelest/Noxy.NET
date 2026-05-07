namespace Noxy.NET.Extensions;

public static class IntegerExtensions
{
    public static bool? FromTriState(this int? value)
    {
        return value switch
        {
            1 => true,
            0 => false,
            _ => null
        };
    }

    public static bool? FromTriState(this int value)
    {
        return value switch
        {
            1 => true,
            0 => false,
            _ => null
        };
    }
}
