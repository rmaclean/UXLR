using System;

namespace UXLRCore
{
    [Flags]
    public enum ExitCode
    {
        Success = 0,
        RootDirectoryNotFound = 1,
        ResourceFileNotFound = 2,
        StyleFileNotFound = 4,
        ShowHelp = 8
    }
}