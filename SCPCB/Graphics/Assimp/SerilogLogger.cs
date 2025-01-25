using Assimp;
using Serilog.Events;

namespace SCPCB.Graphics.Assimp;

// I'm not sure if this can be implemented any better, it seems like the C loggers have no access to explicit severity.
public class SerilogLogger : LogStream {
    static SerilogLogger() {
        Instance.Attach();
    }

    public static SerilogLogger Instance { get; } = new();

    // This breaks when models are loaded in parallel, but there's no solution to that really.
    public string ModelFile { get; set; }

    protected override void Dispose(bool disposing) {
        Detach();
        base.Dispose(disposing);
    }

    protected override void LogMessage(string msg, string userData) {
        const string SPLITTER = ", ";
        var splitterIndex = Math.Max(0, msg.IndexOf(SPLITTER));
        var severity = msg[..splitterIndex];
        Serilog.Log.Write(severity switch {
            // We downgrade their severity because Assimp is yapping too much.
            "Debug" => LogEventLevel.Verbose,
            "Info" => LogEventLevel.Debug,
            "Warn" => LogEventLevel.Warning,
            "Error" => LogEventLevel.Error,
            _ => LogEventLevel.Fatal,
        }, "Assimp ({Model}) {AssimpLog}", ModelFile, msg[(splitterIndex + SPLITTER.Length)..].Trim());
    }
}
