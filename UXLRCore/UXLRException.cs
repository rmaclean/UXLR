namespace UXLRCore
{
    using System;

    [Serializable]
    public class UXLRException : Exception
    {
        public UXLRException(string message, ExitCode exitCode) : base(message)
        {
            ExitCode = exitCode;
        }

        public UXLRException(string message, ExitCode exitCode, Exception inner) : base(message, inner)
        {
            ExitCode = exitCode;
        }

        public ExitCode ExitCode { get; }
    }
}