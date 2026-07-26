using System.Text.Json.Serialization;
using BossHud.Model;

namespace BossHud;

[JsonSerializable(typeof(JsonEntry[]))]
public partial class BossHudJsonContext : JsonSerializerContext;