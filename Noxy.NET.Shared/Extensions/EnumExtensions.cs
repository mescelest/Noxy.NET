namespace Noxy.NET.Extensions;

public static class EnumExtensions
{
    public static bool IsFlagSet<T>(this T value, T flag) where T : struct, Enum
    {
        long lValue = Convert.ToInt64(value);
        long lFlag = Convert.ToInt64(flag);
        return (lValue & lFlag) != 0;
    }
}
