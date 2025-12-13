using System.Text.Json.Serialization;
using JetBrainsRecentProjectCmdPal.Models;

namespace JetBrainsRecentProjectCmdPal.Serialization;

[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Metadata)]
[JsonSerializable(typeof(ProductInfo))]
[JsonSerializable(typeof(LaunchInfo))]
internal partial class AppJsonContext : JsonSerializerContext
{
}