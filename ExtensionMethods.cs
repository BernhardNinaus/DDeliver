using System.Net.NetworkInformation;
public static class ExtensionMethods {
    public static string EscapeForCommand(this string command) 
        => $"\"{command.Replace("\"", "\\\"")}\"";
}
