using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using db_manager.Models;

namespace db_manager.Services;

public class ConfigService
{
    private readonly string _baseDir = AppDomain.CurrentDomain.BaseDirectory;
    private readonly string _configDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config");

    public ConfigService()
    {
        Directory.CreateDirectory(_configDir);
    }

    public List<DatabaseConfig> LoadDatabaseConfigs()
    {
        var configs = new List<DatabaseConfig>();
        var files = new[] {
            ("mysql.json", DatabaseType.MySql),
            ("mongodb.json", DatabaseType.MongoDb),
            ("redis.json", DatabaseType.Redis)
        };

        foreach (var (fileName, dbType) in files)
        {
            var config = LoadConfig<DatabaseConfig>(fileName);
            if (config == null) continue;

            config.Type = dbType;
            configs.Add(config);
            GenerateConfig(config);
        }

        return configs;
    }

    private T? LoadConfig<T>(string fileName) where T : class
    {
        var path = Path.Combine(_configDir, fileName);
        return File.Exists(path)
            ? JsonSerializer.Deserialize<T>(File.ReadAllText(path))
            : null;
    }

    private void GenerateConfig(DatabaseConfig config)
    {
        var configPath = Path.GetFullPath(Path.Combine(_baseDir, config.ConfigPath));
        if (File.Exists(configPath)) return;

        Directory.CreateDirectory(Path.GetDirectoryName(configPath)!);
        var content = config.Type switch
        {
            DatabaseType.MySql => GenerateMySqlConfig(config),
            DatabaseType.MongoDb => GenerateMongoConfig(config),
            DatabaseType.Redis => GenerateRedisConfig(config),
            _ => string.Empty
        };

        File.WriteAllText(configPath, content);
        LogManager.Log($"生成{config.Type}配置文件: {configPath}");
    }

    private string GenerateMySqlConfig(DatabaseConfig config)
    {
        return $"""
                [client]
                default-character-set=utf8

                [mysqld]
                port = {config.Port}
                basedir = {Path.GetFullPath(Path.Combine(_baseDir, config.Home)).Replace("\\", "/")}
                datadir = {Path.GetFullPath(Path.Combine(_baseDir, config.DataPath)).Replace("\\", "/")}
                max_connections = 200
                character-set-server = utf8
                default-storage-engine = INNODB
                explicit_defaults_for_timestamp = 1
                """;
    }

    private string GenerateMongoConfig(DatabaseConfig config)
    {
        return $"""
                systemLog:
                  destination: file
                  path: {Path.GetFullPath(Path.Combine(_baseDir, config.LogPath, "mongo.log")).Replace("\\", "/")}
                  logAppend: true
                storage:
                  dbPath: {Path.GetFullPath(Path.Combine(_baseDir, config.DataPath)).Replace("\\", "/")}
                net:
                  bindIp: 127.0.0.1
                  port: {config.Port}
                """;
    }

    private static string GenerateRedisConfig(DatabaseConfig _)
    {
        return $"""
                bind 127.0.0.1 -::1
                protected-mode yes
                port 6379
                tcp-backlog 511
                timeout 0
                tcp-keepalive 300
                daemonize no
                pidfile ./redis.pid
                loglevel notice
                logfile ""
                databases 16
                always-show-logo no
                set-proc-title yes
                locale-collate ""
                stop-writes-on-bgsave-error yes
                rdbcompression yes
                rdbchecksum yes
                dbfilename dump.rdb
                rdb-del-sync-files no
                dir ./
                replica-serve-stale-data yes
                replica-read-only yes
                repl-diskless-sync yes
                repl-diskless-sync-delay 5
                repl-diskless-sync-max-replicas 0
                repl-diskless-load disabled
                repl-disable-tcp-nodelay no
                replica-priority 100
                acllog-max-len 128
                lazyfree-lazy-eviction no
                lazyfree-lazy-expire no
                lazyfree-lazy-server-del no
                replica-lazy-flush no
                lazyfree-lazy-user-del no
                lazyfree-lazy-user-flush no
                oom-score-adj no
                oom-score-adj-values 0 200 800
                disable-thp yes
                appendonly no
                appendfilename "appendonly.aof"
                appenddirname "appendonlydir"
                appendfsync everysec
                no-appendfsync-on-rewrite no
                auto-aof-rewrite-percentage 100
                auto-aof-rewrite-min-size 64mb
                aof-load-truncated yes
                aof-use-rdb-preamble yes
                aof-timestamp-enabled no
                slowlog-log-slower-than 10000
                slowlog-max-len 128
                latency-monitor-threshold 0
                notify-keyspace-events ""
                hash-max-listpack-entries 512
                hash-max-listpack-value 64
                list-max-listpack-size -2
                list-compress-depth 0
                set-max-intset-entries 512
                set-max-listpack-entries 128
                set-max-listpack-value 64
                zset-max-listpack-entries 128
                zset-max-listpack-value 64
                hll-sparse-max-bytes 3000
                stream-node-max-bytes 4096
                stream-node-max-entries 100
                activerehashing yes
                client-output-buffer-limit normal 0 0 0
                client-output-buffer-limit replica 256mb 64mb 60
                client-output-buffer-limit pubsub 32mb 8mb 60
                hz 10
                dynamic-hz yes
                aof-rewrite-incremental-fsync yes
                rdb-save-incremental-fsync yes
                jemalloc-bg-thread yes
                """;
    }

    public void ValidatePaths()
    {
        foreach (var path in from config in LoadDatabaseConfigs()
                 select new[] {
                     Path.Combine(_baseDir, config.Home),
                     Path.Combine(_baseDir, config.Bin),
                     Path.GetDirectoryName(Path.Combine(_baseDir, config.ExePath)),
                     Path.GetDirectoryName(Path.Combine(_baseDir, config.ConfigPath)),
                     Path.Combine(_baseDir, config.DataPath),
                     Path.GetDirectoryName(Path.Combine(_baseDir, config.LogPath)),
                     Path.GetDirectoryName(Path.Combine(_baseDir, config.PidPath))
                 }
                 into paths
                 from path in paths
                 where !string.IsNullOrEmpty(path) && !Directory.Exists(path)
                 select path)
        {
            Directory.CreateDirectory(path);
        }
    }
}