using System.Diagnostics;

static class Systemd {
    private static readonly Process _proc = new Process() {
        StartInfo = new() {
            WindowStyle = ProcessWindowStyle.Hidden,
            FileName = "systemctl",
            //WorkingDirectory = "path",
            //Arguments = $"args",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        }
    };

    /// <summary>
    /// Starts the service and changes the <seealso cref="IsActive(string)"/> state to true.
    /// </summary>
    /// <returns>True if service has been started</returns>
    public static bool Start(string service)  => StopStart(service, false);

    /// <summary>
    /// Stops the service and changes the <seealso cref="IsActive(string)"/> state to false.
    /// </summary>
    /// <returns>True if service has been stopped</returns>
    public static bool Stop(string service) => StopStart(service, true);

    private static bool StopStart(string service, bool newState) {
        // If currentState == newState
        if (IsActive(service) == newState) return false;

        _proc.StartInfo.Arguments = $"{(newState? "stop" : "start")} " + 
            $"{ExtensionMethods.EscapeForCommand(service)}";
        _proc.Start();

        _proc.StandardOutput.ReadToEnd();
        _proc.WaitForExit();

        if (_proc.ExitCode != 0) {
            throw new Exception($"Error while {(newState ? "starting" : "stopping")} service " +
                $"'{service}' (exitcode: {_proc.ExitCode}).");
        }

        if (newState == IsActive(service)) {
            throw new Exception($"Error could not {(newState ? "start" : "stop")} service " +
                $"'{service}' (exitcode: {_proc.ExitCode}).");
        }

        return true;
    }

    /// <summary>
    /// Checke the state of the Service
    /// </summary>
    /// <returns>True when the service is running.</returns>
    public static bool IsActive(string service) {
        _proc.StartInfo.Arguments = $"is-activ {ExtensionMethods.EscapeForCommand(service)}";
        _proc.Start();

        var isActive = _proc.StandardOutput.ReadToEnd() == "inactive\n";
        _proc.WaitForExit();

        if (_proc.ExitCode != 0) {
            throw new Exception($"Error while checking if service '{service}' " +
                $"is active (exitcode: {_proc.ExitCode}).",
                new Exception(_proc.StandardError.ReadToEnd()));
        }

        return isActive;
    }
}
