using System.CommandLine.IO;
using System.CommandLine;

namespace etsdevtest.cli;

/// <summary>
/// Custom console to manage IO for commands for testing and shell.
/// </summary>
public class CustomConsole : IConsole
{
    public class EmptyStreamWriter : IStandardStreamWriter
    {
        public EmptyStreamWriter() { }

        public void Write(string? value) { }
    }

    public class ErrorStreamWriter : IStandardStreamWriter
    {
        public ErrorStreamWriter() { }

        public void Write(string? value)
        {
            System.Console.Error.Write(value);
        }
    }

    EmptyStreamWriter mEmptyStreamWriter = new EmptyStreamWriter();
    ErrorStreamWriter mErrorStreamWriter = new ErrorStreamWriter();

    public IStandardStreamWriter Out => mEmptyStreamWriter;

    public IStandardStreamWriter Error => mErrorStreamWriter;

    public bool IsOutputRedirected => true;

    public bool IsErrorRedirected => false;

    public bool IsInputRedirected => true;
}
