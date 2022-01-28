using System.Text.RegularExpressions;
using System.Diagnostics;

static class Chown {
    private static readonly Process _proc = new Process() {
        StartInfo = new() {
            WindowStyle = ProcessWindowStyle.Hidden,
            FileName = "chown",
            //WorkingDirectory = "path",
            //Arguments = $"args",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        }
    };

    public static void SetUserGroup(string userGroup, string path, bool recursive = false) {
        if (!Regex.Match(userGroup, "[_a-z][-0-9_a-z]*:[_a-z][-0-9_a-z]*").Success) {
            Console.WriteLine("User:Group not a vild name ([_a-z][-0-9_a-z]*:[_a-z][-0-9_a-z]*)");
        }

        _proc.StartInfo.Arguments = $"{(recursive ? "-R" : "")} {userGroup} " + 
            $"{ExtensionMethods.EscapeForCommand(path)}";

        _proc.Start();
        _proc.WaitForExit();
        var output = _proc.StandardOutput.ReadToEnd();
        var error = _proc.StandardError.ReadToEnd();

        if (_proc.ExitCode != 0) {
            throw new Exception($"Error while changing user and group " + 
                                $"(exitcode: {_proc.ExitCode}).", 
                new Exception(error));
        }
    }
}
