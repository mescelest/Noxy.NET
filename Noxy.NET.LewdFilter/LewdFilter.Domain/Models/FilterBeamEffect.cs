using LewdFilter.Domain.Enums;

namespace LewdFilter.Domain.Models;

public record FilterBeamEffect(FilterColorNameEnum Color, bool IsTemporary = false);
