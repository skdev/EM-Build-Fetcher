namespace EM_Build_Fetcher.program
{
    /// https://support.microsoft.com/en-gb/help/304888/list-of-error-codes-and-error-messages-for-windows-installer-processes
    public enum ExitCode
    {
        Success = 0,
        SucessNeedReboot = 3010,
        FunctionFailed = 1627
    }
}
