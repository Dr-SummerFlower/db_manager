using System.IO;
using System.Text.Json.Serialization;

namespace db_manager.Models;

public class DatabaseConfig
{
  [JsonConstructor]
  public DatabaseConfig()
  {
  }

  [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;

  // [JsonPropertyName("icon")] public string Icon { get; set; } = string.Empty;
  [JsonPropertyName("home")] public string Home { get; set; } = string.Empty;
  [JsonPropertyName("bin")] public string Bin { get; set; } = string.Empty;
  [JsonPropertyName("exePath")] public string ExePath { get; set; } = string.Empty;
  [JsonPropertyName("dataPath")] public string DataPath { get; set; } = string.Empty;
  [JsonPropertyName("logPath")] public string LogPath { get; set; } = string.Empty;
  [JsonPropertyName("configPath")] public string ConfigPath { get; set; } = string.Empty;
  [JsonPropertyName("port")] public int Port { get; set; }
  [JsonPropertyName("user")] public string User { get; set; } = string.Empty;
  [JsonPropertyName("password")] public string Password { get; set; } = string.Empty;

  [JsonIgnore] public DatabaseType Type { get; set; }
  [JsonIgnore] public string PidPath => Path.Combine("pids", $"{Type.ToString().ToLower()}.pid");
}

public enum DatabaseType
{
  MySql,
  MongoDb,
  Redis
}
