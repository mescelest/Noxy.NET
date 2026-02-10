namespace Noxy.NET.EntityManagement.Presentation.Services;

public class SystemConstantService
{
    public string DateFormat { get; set; } = "dd.MM.yyyy";
    public string TimeFormat { get; set; } = "HH:mm";
    public string DateTimeFormat { get; set; } = "dd.MM.yyyy HH:mm";
    public string DefaultDateValue { get; set; } = "-";


    public string AsDate(DateTime? value)
    {
        return value.HasValue ? value.Value.ToString(DateFormat) : DefaultDateValue;
    }

    public string AsTime(DateTime? value)
    {
        return value.HasValue ? value.Value.ToString(TimeFormat) : DefaultDateValue;
    }

    public string AsDateTime(DateTime? value)
    {
        return value.HasValue ? value.Value.ToString(DateTimeFormat) : DefaultDateValue;
    }
}
