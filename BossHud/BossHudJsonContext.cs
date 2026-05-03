using System.Text.Json.Serialization;
using BossHud.Model;

namespace BossHud;

[JsonSerializable(typeof(Entry[]))]
public partial class BossHudJsonContext : JsonSerializerContext;