using YamlDotNet.Serialization;

class ServerConfig {
    public IEnumerable<string> DefaultBuildParam { get; set; } = Array.Empty<string>();
    public Dictionary<string, ProjectConfig> Projects { get; set; } = new();
}

class ProjectConfig {
    public string GitRepo { get; set; } = string.Empty;
    public string OnBranch { get; set; } = "main";
    public string CSProjFile { get; set; } = string.Empty;
    public IEnumerable<string> BuildParams { get; set; } = Array.Empty<string>();
    public string OutputFolder { get; set; } = string.Empty;
    public bool CleanOutpuFolder { get; set; } = false;
    public string? SystemdService { get; set; }
    public string? UserGroup { get; set; }
}

static class ConfigProvider {
    public static string ConfigFilePath = "/etc/ddeliver/config.yml";

    public static readonly ServerConfig ExampleConfig = new ServerConfig() {
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
                    GitRepo = "/home/git/webserver.git",
                    CSProjFile = "WebServer/WebServer.csproj",
                    OutputFolder = "/var/www/root/",
                    SystemdService = "webserver.service",
                    OnBranch = "main",
                    BuildParams = new[] {
                        "--self-contained",
                    },
                    UserGroup = "www-data:www-data"
                }
            }
        }
    };

    public static ServerConfig GetConfig(IDeserializer deserializer) {
        if (!File.Exists(ConfigFilePath)) 
            throw new Exception($"No config found at: {ConfigFilePath}");
        return deserializer.Deserialize<ServerConfig>(File.ReadAllText(ConfigFilePath));
    }
}
