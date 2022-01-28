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
    public static bool Start(string service)  => StopStart(service, true);

    /// <summary>
    /// Stops the service and changes the <seealso cref="IsActive(string)"/> state to false.
    /// </summary>
    /// <returns>True if service has been stopped</returns>
    public static bool Stop(string service) => StopStart(service, false);

    private static bool StopStart(string service, bool newState) {
        Console.WriteLine($"[DBG ] Systemd-StopStart: {(newState? "Start" : "Stop")} " +
                        $"service: '{service}'");

        // If currentState == newState
        if (IsActive(service) == newState) {
            Console.WriteLine($"[DBG ] Systemd-StopStart: Systemtd service already " +
                            $"{(newState? "started" : "stopped")}");
            return false;
        }
            

        _proc.StartInfo.Arguments = $"{(newState? "start" : "stop")} " + 
            $"{ExtensionMethods.EscapeForCommand(service)}";

        _proc.Start();

        _proc.StandardOutput.ReadToEnd();
        _proc.WaitForExit();

        if (_proc.ExitCode != 0 && _proc.ExitCode != 3) {
            throw new Exception($"Error while {(newState ? "starting" : "stopping")} service " +
                $"'{service}' (exitcode: {_proc.ExitCode}).");
        }

        Console.WriteLine($"[DBG ] Systemd-StopStart: Wait 5 seconds to check if " +
                        $"everything is okay");
        System.Threading.Thread.Sleep(5 * 1000); // Give 5 second to start the service.
        if (newState != IsActive(service)) {
            throw new Exception($"Error could not {(newState ? "start" : "stop")} service " +
                $"'{service}'.");
        }
        
        Console.WriteLine($"[DBG ] Systemd-StopStart: Service {(newState? "started" : "stoped")}");

        return true;
    }

    /// <summary>
    /// Checke the state of the Service
    /// </summary>
    /// <returns>True when the service is running.</returns>
    public static bool IsActive(string service) {
        Console.WriteLine($"[DBG ] Systemd-IsActive: Is service '{service}' active?");

        _proc.StartInfo.Arguments = $"is-active {ExtensionMethods.EscapeForCommand(service)}";
        _proc.Start();

        var isActive = _proc.StandardOutput.ReadToEnd() == "active\n";
        _proc.WaitForExit();

        if (_proc.ExitCode != 0 && _proc.ExitCode != 3) {
            throw new Exception($"Error while checking if service '{service}' " +
                $"is active (exitcode: {_proc.ExitCode}).",
                new Exception(_proc.StandardError.ReadToEnd()));
        }

        Console.WriteLine($"[DBG ] Systemd-IsActive: Service is {(isActive ? "" : "not ")}active");

        return isActive;
    }
}
