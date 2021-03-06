using System.ComponentModel;
using System.Diagnostics;

class DeliveryService {
    private readonly ServerConfig _serverConfig;

    public DeliveryService(ServerConfig serverConfig) {
        _serverConfig = serverConfig;
    }

    public void Deliver(string projectName) {
        var tmpProjectPath = "";
        try {
            Console.WriteLine($"[INFO] ####################### Deliver project {projectName}");

            if (!_serverConfig.Projects.ContainsKey(projectName)) {
                throw new Exception($"Could not find project '{projectName}' in configuration.");
            }
            var project = _serverConfig.Projects[projectName];

            tmpProjectPath = $"/tmp/{projectName}";
            var tmpCompileOutput = $"{tmpProjectPath}/compileOutput";

            Console.WriteLine($"[INFO] Ensure tmpProjectPaht '{tmpProjectPath}'");
            EnsureEmptyFolder(tmpProjectPath);

            Console.WriteLine($"[INFO] Clone git repo '{project.GitRepo}' -> {project.OnBranch}");
            CloneGitRepo(tmpProjectPath, project.GitRepo, project.OnBranch);

            Console.WriteLine($"[INFO] Compile Project");
            CompileDotNet(tmpProjectPath, tmpCompileOutput, project);

            var needsStart = false;
            Console.WriteLine($"[INFO] Systemtd service available?");
            if (!string.IsNullOrWhiteSpace(project.SystemdService)) {
                needsStart = Systemd.Stop(project.SystemdService);
                if (needsStart) Console.WriteLine($"[INFO] Systemtd service needs a restart later");
            }

            Console.WriteLine($"[INFO] Clean or create output folder? {project.OutputFolder}");
            if (project.CleanOutpuFolder || !Directory.Exists(project.OutputFolder)) {
                EnsureEmptyFolder(project.OutputFolder);
            }

            Console.WriteLine($"[INFO] Need to set user and group?");
            if (!string.IsNullOrWhiteSpace(project.UserGroup)) {
                Chown.SetUserGroup(project.UserGroup, tmpCompileOutput, true);
            }

            Console.WriteLine($"[INFO] Copy to output folder");
            CopyCompilationToOutput(tmpCompileOutput, project.OutputFolder);

            if (!string.IsNullOrWhiteSpace(project.SystemdService) && needsStart) {
                Console.WriteLine($"[INFO] Restart systemtd service");
                Systemd.Start(project.SystemdService);
            }

        } catch (Exception ex) {
            Console.Error.WriteLine($"[ERRO] Expection while delivering project '{projectName}':");

            Console.Error.WriteLine($"\t{ex.Message}");
            if (ex.InnerException is not null) 
                Console.Error.WriteLine($"\t{ex.InnerException.Message.Replace("\n", "\n\t")}");

            Console.Error.WriteLine("[ERRO] Project will be skipped.");
        } finally {
            if (Directory.Exists(tmpProjectPath))
                Directory.Delete(tmpProjectPath, true);
            Console.WriteLine($"[INFO] Done {projectName}");
        }
    }

    private void EnsureEmptyFolder(string tmpProjectPath) {
        if (Directory.Exists(tmpProjectPath)) {
            Console.WriteLine($"[DBG ] EnsureEmptyFolder: Folder {tmpProjectPath} exists, delete");
            Directory.Delete(tmpProjectPath, true);
        }
        Directory.CreateDirectory(tmpProjectPath);
        Console.WriteLine($"[DBG ] EnsureEmptyFolder: Folder {tmpProjectPath} created");
    }

    private void CloneGitRepo(string tmpProjectPath, string gitPath, string branch) {
        branch = ExtensionMethods.EscapeForCommand(branch);
        gitPath = ExtensionMethods.EscapeForCommand(gitPath);

        using var proc = new Process() {
            StartInfo = new() {
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "git",
                WorkingDirectory = tmpProjectPath,
                Arguments = $"clone -b {branch} {gitPath} .",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            }
        };

        proc.Start();

        _ = proc.StandardOutput.ReadToEnd();
        var error = proc.StandardError.ReadToEnd();
        proc.WaitForExit();

        if (proc.ExitCode != 0) {
            throw new Exception($"Error while loading git project (exitcode: {proc.ExitCode}).", 
                new Exception(error));
        }
    }

    private void CompileDotNet(string projectPath, string outputPaht, ProjectConfig config) {
        var allParams = _serverConfig.DefaultBuildParam.Concat(config.BuildParams);
        var buildArguments = GetBuildParam(allParams);

        using var proc = new Process() {
            StartInfo = new() {
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "dotnet",
                WorkingDirectory = projectPath,
                Arguments = $"publish {config.CSProjFile} -o {outputPaht} {buildArguments}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            }
        };

        proc.Start();
        
        var output = proc.StandardOutput.ReadToEnd();

        var error = proc.StandardError.ReadToEnd();
        proc.WaitForExit();

        if (proc.ExitCode != 0) {
            throw new Exception($"Error while loading git project (exitcode: {proc.ExitCode}).", 
                new Exception(error));
        }
    }

    private string GetBuildParam(IEnumerable<string> param) {
        IEnumerable<string> CleanParam(IEnumerable<string> parm, string option, int countArg) {
            var ret = parm.ToList();
            var i = ret.IndexOf(option);
            if (i == -1) return parm;
            ret.RemoveRange(i, ++countArg);
            return ret;
        };
        
        param = CleanParam(param, "-o", 1);
        param = CleanParam(param, "--output", 1);
        return string.Join(' ', param);
    }

    private void CopyCompilationToOutput(string sorucePath, string destinationPath) {
        sorucePath = ExtensionMethods.EscapeForCommand(sorucePath);
        destinationPath = ExtensionMethods.EscapeForCommand(destinationPath);

        using var proc = new Process() {
            StartInfo = new() {
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "cp",
                Arguments = $"-r {sorucePath}/. {destinationPath}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            }
        };

        proc.Start();
        proc.WaitForExit();
        var output = proc.StandardOutput.ReadToEnd();
        var error = proc.StandardError.ReadToEnd();

        if (proc.ExitCode != 0) {
            throw new Exception($"Error while copying files to Outputfolder (exitcode: {proc.ExitCode}).", 
                new Exception(error));
        }
    }
}
