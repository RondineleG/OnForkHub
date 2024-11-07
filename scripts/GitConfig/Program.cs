using System;
using System.Diagnostics;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Applying shared Git configurations...");

        RunGitCommand("config --local include.path ../.gitconfig.local");

        Console.WriteLine("Configuration applied successfully!");
    }

    static void RunGitCommand(string command)
    {
        var processInfo = new ProcessStartInfo("git", command)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        var process = Process.Start(processInfo);
        process.WaitForExit();

        Console.WriteLine(process.StandardOutput.ReadToEnd());
        Console.Error.WriteLine(process.StandardError.ReadToEnd());
    }
}
