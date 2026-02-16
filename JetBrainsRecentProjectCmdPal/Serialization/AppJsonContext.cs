using System.Text.Json.Serialization;
using JetBrainsRecentProjectCmdPal.Models;

namespace JetBrainsRecentProjectCmdPal.Serialization;

[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Metadata, PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(ProductInfo))]
[JsonSerializable(typeof(LaunchInfo))]
internal sealed partial class AppJsonContext : JsonSerializerContext
{
}