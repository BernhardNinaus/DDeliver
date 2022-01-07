using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

class ServerConfig {
    public string? PathBase { get; set; }
    public string[]? DefaultBuildParam { get; set; }
    public Dictionary<string, ProjectConfig> Projects { get; set; } = new();
}

class ProjectConfig {
    public string GitRepo { get; set; } = string.Empty;
    public string OnBranch { get; set; } = "main";
    public string CSProjFile { get; set; } = string.Empty;
    public string[]? BuildParams { get; set; }
    public string OutputFolder { get; set; } = string.Empty;
    public string? SystemdService { get; set; }
}

static class ConfigProvider {
    public const string ConfigFilePath = "/etc/ddeliver/config.yml";

    public static readonly ServerConfig ExampleConfig = new ServerConfig() {
        PathBase = "/home/git/",
        DefaultBuildParam = new [] {
            "/property:GenerateFullPaths=true",
            "/consoleloggerparameters:NoSummary",
            "-c",
            "Release",
            "-r",
            "linux-x64",
            "-p:PublishSingleFile=true",
        },
        Projects = new() {
            { 
                "webserver-project", 
                new() {
                    GitRepo = "Test.git",
                    CSProjFile = "WebServer/WebServer.csproj",
                    OutputFolder = "/var/www/root/",
                    SystemdService = "webserver.service",
                    OnBranch = "main",
                    BuildParams = new[] {
                        "--self-contained",
                    }
                }
            }
        }
    };

    public static ServerConfig? GetConfig(IDeserializer deserializer) {
        if (!File.Exists(ConfigFilePath)) throw new Exception($"No config found at: {ConfigFilePath}");
        return deserializer.Deserialize<ServerConfig>(File.ReadAllText(ConfigFilePath));
    }
}
