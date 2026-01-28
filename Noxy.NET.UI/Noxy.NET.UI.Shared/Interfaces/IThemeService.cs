namespace Noxy.NET.UI.Interfaces;

public interface IThemeService<TType> where TType : struct, Enum
{
    Task<TType?> LoadTheme();
    TType? GetTheme();
    Task SetTheme(TType? type);

    event EventHandler<TType?> OnThemeChange;
}
