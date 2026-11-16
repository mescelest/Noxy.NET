using System.Text.Json;
using Noxy.NET.MediaPlayer.Domain.Responses.Setting;

namespace Noxy.NET.MediaPlayer.Presentation.Extensions;

public static class ResponseSettingExtension
{
    extension(ResponseSetting setting)
    {
        // The value arrives as the JSON the server sent. A value of another kind throws, naming the key and never the value.
        public bool GetBoolean()
        {
            JsonElement value = setting.GetElement();

            return value.ValueKind is JsonValueKind.True or JsonValueKind.False ? value.GetBoolean() : throw setting.WrongKind("a boolean");
        }

        public int GetInteger()
        {
            JsonElement value = setting.GetElement();

            return value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out int number) ? number : throw setting.WrongKind("an integer");
        }

        public string GetEnumName()
        {
            JsonElement value = setting.GetElement();

            return value.ValueKind == JsonValueKind.String && value.GetString() is { } name ? name : throw setting.WrongKind("an enum member name");
        }

        private JsonElement GetElement()
        {
            return setting.Value as JsonElement? ?? JsonSerializer.SerializeToElement(setting.Value);
        }

        private InvalidOperationException WrongKind(string expected)
        {
            return new($"The setting {setting.Key} does not hold {expected}.");
        }
    }
}
