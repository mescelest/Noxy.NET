namespace Noxy.NET.Extensions;

public static class BooleanExtensions
{
    public static int ToTriState(this bool? value)
    {
        return value switch
        {
            true => 1,
            false => 0,
            _ => -1
        };
    }
}
