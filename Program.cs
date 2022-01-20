using YamlDotNet.Serialization;

void printUsage() {
    var f = ConfigProvider.ConfigFilePath;

    Console.WriteLine(@$"Usage:
    -d / --deliver <project-name>[ <project-name>]  Delivers the project(s)

    --d-all                                         Delivers all configured projects

    --d-git-repo <git-repo-path>[ <git-repo-path>]  Delivers all projects from the given git repo.
      
    --hook                                          Hooks all configured projects.
                                                     If there is already a hook present,
                                                     simply call this executable with the -g param.

    --example-config                                Prints the example config
                                                     ddleiver --example-config > {f}");
}

if (!args.Any()) {
    Console.WriteLine("No Arguments provided!");
    printUsage();
    return;
}

var serializer = new SerializerBuilder().Build();
var deserializer = new DeserializerBuilder().Build();

if (args[0] == "--example-config") {
    var yaml = serializer.Serialize(ConfigProvider.ExampleConfig);
    Console.WriteLine(yaml);
    return;
}

ServerConfig serverConfig;
try { 
    serverConfig = ConfigProvider.GetConfig(deserializer);
} catch (Exception ex) {
    Console.WriteLine(@$"Could not load configuration at: {ConfigProvider.ConfigFilePath}
    Expection: {ex.Message}
        ({ex.InnerException?.Message})");
    return;
}

var deliverer = new DeliveryService(serverConfig);

switch (args[0]) {
    case "-h":
    case "--help":
        printUsage();
        return;

    case "-d":
    case "--deliver":
        if (args.Length == 1)
            throw new Exception("No prject(s) provided to deliver!");

        foreach (var projectName in args[1..])
            deliverer.Deliver(projectName);

        return;

    case "--d-all":
        foreach (var projectName in serverConfig.Projects.Keys)
            deliverer.Deliver(projectName);

        return;

    case "--d-git-repo":
        if (args.Length == 1)
            throw new Exception("No prject(s) provided to deliver!");

        var effectedRepos = args[1..];

        foreach (var (projectName, _) in serverConfig.Projects
                                            .Where(w => effectedRepos.Contains(w.Value.GitRepo)))
            deliverer.Deliver(projectName);

        return;

    case "--hook":
        throw new NotImplementedException("--hook not implemented");

    default:
        Console.WriteLine("Command not found!");
        printUsage();
        break;
}
