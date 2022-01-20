using System.Text.RegularExpressions;
using System.Diagnostics;

static class Chmod {
    private static readonly Process _proc = new Process() {
        StartInfo = new() {
            WindowStyle = ProcessWindowStyle.Hidden,
            FileName = "chmod",
            //WorkingDirectory = "path",
            //Arguments = $"args",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        }
    };

    public static void SetPerimissions(string path, string permission = "700") {
        if (!Regex.Match(permission, "^[0-7]{1,3}$").Success) {
            Console.WriteLine("Permission input is invalid! [0-7]{1,3}");
        }

        _proc.StartInfo.Arguments = $"{permission} {ExtensionMethods.EscapeForCommand(path)}";

        _proc.Start();
        _proc.WaitForExit();
        var output = _proc.StandardOutput.ReadToEnd();
        var error = _proc.StandardError.ReadToEnd();

        if (_proc.ExitCode != 0) {
            throw new Exception($"Error while changing filepermission (exitcode: {_proc.ExitCode}).", 
                new Exception(error));
        }
    }
}
