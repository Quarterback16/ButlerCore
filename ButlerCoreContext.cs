using Microsoft.Extensions.Logging;

namespace ButlerCore
{
    /// <summary>
    ///  A custom config object
    /// </summary>
    public class ButlerCoreContext
    {
        public ILogger? Logger { get; set; }
        public StreamWriter? StreamWriter { get; set; }
        public ConnectionStrings? ConnectionStrings { get; set; }
        public string[]? AppNames { get; set; }
        public string[]? Exceptions { get; set; }
        public string? ConnectTo { get; set; }
        public DateTime? StartDateTime { get; set; }
        public int? Frequency { get; set; }
        public bool? Beep { get; set; }
        public bool? Stats { get; set; }
        public string? KnockOffTime { get; set; }
        public string? LogFile { get; set; }
        public string? ResultsFile { get; set; }
        public string? DropBoxFolder { get; set; }
    }

    public class ConnectionStrings
    {
        public string? Dev { get; set; }
        public string? Prod { get; set; }
    }
}
