using System;
using System.Linq;
using UnityEngine.DedicatedServer;

internal class CommandLineArgumentsParser
{
    public int Port { get; }
    private const int DefaultPort = 7777;
    public int TargetFramerate { get; }
    private const int DefaultTargetFramerate = 30;

    private readonly string[] mArgs;

    /// <summary>
    /// Initializes the CommandLineArgumentsParser
    /// </summary>
    public CommandLineArgumentsParser() : this(Environment.GetCommandLineArgs())
    {
    }

    /// <summary>
    /// Initializes the CommandLineArgumentsParser
    /// </summary>
    /// <param name="arguments">Arguments to process</param>
    public CommandLineArgumentsParser(string[] arguments)
    {
        // Android fix
        mArgs = arguments ?? Array.Empty<string>();

        Port = Arguments.Port.HasValue ? Arguments.Port.Value : DefaultPort;
        TargetFramerate = Arguments.TargetFramerate.HasValue
            ? Arguments.TargetFramerate.Value
            : DefaultTargetFramerate;
    }

    /// <summary>
    /// Extracts a value for command line arguments provided
    /// </summary>
    /// <param name="argName"></param>
    /// <param name="defaultValue"></param>
    /// <param name="argumentAndValueAreSeparated"></param>
    /// <returns></returns>
    private string ExtractValue(string argName, string defaultValue = null,
        bool argumentAndValueAreSeparated = true)
    {
        if (argumentAndValueAreSeparated) {
            if (!mArgs.Contains(argName)) {
                return defaultValue;
            }

            var index = mArgs.ToList().FindIndex(0, a => a.Equals(argName));
            return mArgs[index + 1];
        }

        foreach (var argument in mArgs) {
            if (argument.StartsWith(argName)) //I.E: "-epiclocale=it"
            {
                return argument.Substring(argName.Length + 1, argument.Length - argName.Length - 1);
            }
        }
        return defaultValue;
    }

    private int ExtractValueInt(string argName, int defaultValue = -1)
    {
        var number = ExtractValue(argName, defaultValue.ToString());
        return Convert.ToInt32(number);
    }
}