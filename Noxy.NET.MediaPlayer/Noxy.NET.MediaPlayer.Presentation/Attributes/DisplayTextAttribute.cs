using System.ComponentModel;

namespace Noxy.NET.MediaPlayer.Presentation.Attributes;

// The name of a form field, read from PresentationTexts by the name of its text. It is a DisplayName, which is what a form looks for, so the labels of a model are not literals.
[AttributeUsage(AttributeTargets.Property)]
public class DisplayTextAttribute(string key) : DisplayNameAttribute
{
    public override string DisplayName => PresentationTexts.ResourceManager.GetString(key, PresentationTexts.Culture) ?? throw new InvalidOperationException($"There is no text named {key}.");
}
