namespace CommonLibrary;

public enum LogLevel {
    FATAL,
    ALERT,
    CRITICAL,
    ERROR,
    WARNING,
    NOTICE,
    INFO,
    DEBUG,
    TRACE,
    TODO,
    COUNT
}

public class Logging {
    private string _logFilePath { get; set; }
    private string _logFileName { get; set; }
    private string _logFilePathAndName { get; set; }
    private bool _logInExecDir { get; set; }
    // _boEcas.LogDetail(MethodBase.GetCurrentMethod());

    public Logging() {
        // Go up one path from exec path
        _logFilePath = Pathing.ExecutablePath + "../Logs/";
        _logFileName = $"{DateTime.Now:yyyyMMdd}_Log.txt";
        _logFilePathAndName = Path.Combine(_logFilePath, _logFileName);
        WriteLogFile();
    }

    private void WriteLogFile() {
        // Creates the log directory if it doesn't exist
        if (!Directory.Exists(_logFilePath)) {
            Directory.CreateDirectory(_logFilePath);
        }

        if (!File.Exists(Path.GetFullPath(_logFilePathAndName))) {
            using (StreamWriter sw = File.CreateText(_logFilePathAndName)) {
                sw.Flush();
                sw.Close();
            }
        }
    }

    public void TraceLog(string status) {
        using (StreamWriter streamWriter = File.AppendText(Path.GetFullPath(_logFilePathAndName))) {
            string write = $"{DateTime.UtcNow} [{GetLogType(LogLevel.TRACE)}] - {status}";
            streamWriter.WriteLine(write);
        }
    }

    public void Log(string status, LogLevel logLevel) {
        using (StreamWriter streamWriter = File.AppendText(Path.GetFullPath(_logFilePathAndName))) {
            string write = $"{DateTime.UtcNow} [{GetLogType(logLevel)}] - {status}";
            streamWriter.WriteLine(write);
        }
    }

    public static string GetLogType(LogLevel logLevel) {
        switch (logLevel) {
            case LogLevel.FATAL:
                return "FATAL";
            case LogLevel.ALERT:
                return "ALERT";
            case LogLevel.CRITICAL:
                return "CRITICAL";
            case LogLevel.ERROR:
                return "ERROR";
            case LogLevel.WARNING:
                return "WARNING";
            case LogLevel.NOTICE:
                return "NOTICE";
            case LogLevel.INFO:
                return "INFO";
            case LogLevel.DEBUG:
                return "DEBUG";
            case LogLevel.TRACE:
                return "TRACE";
            case LogLevel.TODO:
                return "TODO";
            case LogLevel.COUNT:
            default:
                return "TYPE UNKNOWN! CRITICAL LOG ERROR!";
        }
    }
}
