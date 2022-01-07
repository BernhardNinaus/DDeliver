using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

void printUsage() {
    var f = ConfigProvider.ConfigFilePath;

    Console.WriteLine(@$"Usage:
    -d / --deliver <project-name>[,<project-name>]  Delivers the project(s)

    --d-all                                         Delivers all configured projects

    -g / --git-repo <git-repo-path>                 Delivers all projects from the given git repo.
    
    --hook                                          Hooks all configured projects.
                                                     If there is already a hook present,
                                                     simply call this executable with the -g param.

    --remove-repo <git-repo-path>                   Removes project and deletes hook
                                                     (May not be able to write config file,
                                                     new config will be printed out)

    --remove-proj <project-name>[,<project-name>]   Removes project(s) from configuration
                                                     (May not be able to write config file,
                                                     new config will be printed out)

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


ServerConfig? currentConf = null;
try { 
    currentConf = ConfigProvider.GetConfig(deserializer);
} catch (Exception ex) {
    Console.WriteLine(@$"Could not load configuration at: {ConfigProvider.ConfigFilePath}
    Expection: {ex.Message}
        ({ex.InnerException?.Message})");
    return;
}

switch (args[0]) {
    case "-h":
    case "--help":
        printUsage();
        return;

    case "-d":
    case "--deliver":
        throw new NotImplementedException("-d / --deliver not implemented");

    case "--d-all":
        throw new NotImplementedException("--d-all not implemented");

    case "-g":
    case "--git-repo":
        throw new NotImplementedException("-g / --git-repo not implemented");


    case "--hook":
        throw new NotImplementedException("--hook not implemented");

    case "--remove-repo":
        throw new NotImplementedException("--remove-repo not implemented");

    case "--remove-proj":
        throw new NotImplementedException("--remove-proj not implemented");

    default:
        Console.WriteLine("Command not found!");
        printUsage();
        break;
}
