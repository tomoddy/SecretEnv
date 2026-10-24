using System.Text.Json;

// Take the secrets file path as an argument, or prompt for it if none was given.
string path = args.Length > 0 ? args[0] : Prompt();
path = path.Trim().Trim('"');
if (!File.Exists(path))
{
    Console.WriteLine($"File not found: {path}");
    return;
}

// Read the secrets and print each one as a systemd Environment= line.
using JsonDocument document = JsonDocument.Parse(File.ReadAllText(path));
foreach (KeyValuePair<string, string> secret in Flatten(document.RootElement, string.Empty))
    Console.WriteLine(ToEnvironmentLine(secret.Key, secret.Value));

// Prompts for the secrets file path.
static string Prompt()
{
    Console.Write("Secrets file: ");
    return Console.ReadLine() ?? string.Empty;
}

// Flattens nested JSON into configuration keys, e.g. Providers:MetOffice:ApiKey.
static IEnumerable<KeyValuePair<string, string>> Flatten(JsonElement element, string prefix)
{
    if (element.ValueKind == JsonValueKind.Object)
    {
        foreach (JsonProperty property in element.EnumerateObject())
            foreach (KeyValuePair<string, string> child in Flatten(property.Value, prefix.Length == 0 ? property.Name : $"{prefix}:{property.Name}"))
                yield return child;
    }
    else if (element.ValueKind == JsonValueKind.Array)
    {
        int index = 0;
        foreach (JsonElement item in element.EnumerateArray())
            foreach (KeyValuePair<string, string> child in Flatten(item, $"{prefix}:{index++}"))
                yield return child;
    }
    else
        yield return new(prefix, element.ValueKind == JsonValueKind.String ? element.GetString() ?? string.Empty : element.ValueKind == JsonValueKind.Null ? string.Empty : element.GetRawText());
}

// Builds an Environment= line, swapping : for __ and quoting values systemd would otherwise misread.
static string ToEnvironmentLine(string key, string value)
{
    string line = $"{key.Replace(":", "__")}={value.Replace("%", "%%")}";
    return line.Any(character => char.IsWhiteSpace(character) || character == '"' || character == '\\') ? $"Environment=\"{line.Replace("\\", "\\\\").Replace("\"", "\\\"")}\"" : $"Environment={line}";
}