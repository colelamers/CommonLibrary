using System;
using System.Text;
using System.Threading.Tasks;
namespace CommonLibrary;

public enum LogLevel
{
    FATAL = 0,
    ALERT = 1,
    CRITICAL = 2,
    ERROR = 3,
    WARNING = 4,
    DEBUG = 5,
    NOTICE = 6,
    INFO = 7,
    TRACE = 8,
    TODO = 9,
    COUNT = 10
}

public class Logging
{
    protected string _logFilePath { get; set; }
    protected string _logFileName { get; set; }
    protected string _logFilePathAndName { get; set; }
    // Normally this value will be set by a config object so that it can be
    // easily changed without recompiling
    protected LogLevel _logLevel { get; set; } = LogLevel.COUNT;
    private readonly object _sync = new();
    public bool UseUtcTimestamps { get; set; } = false;

    
    public Logging(LogLevel logLevel)
    {
        // Go up one path from exec path
        this._logFilePath = Pathing.ExecutablePath + "../Logs/";
        this._logLevel = logLevel;
        this._logFileName = $"{DateTime.Now:yyyyMMdd}.log";
        this._logFilePathAndName = Path.Combine(this._logFilePath, this._logFileName);
        EnsureLogFile();
    }

    private void EnsureLogFile()
    {
        try
        {
            if (!Directory.Exists(this._logFilePath))
            {
                Directory.CreateDirectory(this._logFilePath);
            }

            Directory.CreateDirectory(this._logFilePath);

            if (!File.Exists(this._logFilePathAndName))
            {
                using (StreamWriter sw = File.CreateText(this._logFilePathAndName))
                {
                    sw.Flush();
                    sw.Close();
                }
            }
        }
        catch { }
    }

    public void Trace(string status)
    {
        Log(status, LogLevel.TRACE);
    }

    public void Fatal(string status)
    {
        Log(status, LogLevel.FATAL);
    }

    public void Notice(string status)
    {
        Log(status, LogLevel.NOTICE);
    }

    public void Error(string status)
    {
        Log(status, LogLevel.ERROR);
    }

    public void Debug(string status)
    {
        Log(status, LogLevel.DEBUG);
    }

    public void Alert(string status)
    {
        Log(status, LogLevel.ALERT);
    }

    public void Warn(string status)
    {
        Log(status, LogLevel.WARNING);
    }

    public void Info(string status)
    {
        Log(status, LogLevel.INFO);
    }
    public void Todo(string status)
    {
        Log(status, LogLevel.TODO);
    }
    
    public void Critical(string status)
    {
        Log(status, LogLevel.CRITICAL);
    }

    protected string FormatTimestamp()
    {
        DateTime utc = DateTime.UtcNow;
        string iso8601 = "yyyy-MM-ddTHH:mm:ss.fffK";
        if (this.UseUtcTimestamps)
        {
            return utc.ToString(iso8601);
        }
        else
        {
            return utc.ToLocalTime().ToString(iso8601);
        }
    }

    private void Log(string status, LogLevel logLevel) 
    {
        // If the log level of the message is less severe than the log level, 
        // skip logging. For example, if log level is WARNING, skip DEBUG, INFO,
        // and TRACE messages. If it's set to FATAL, only FATAL messages will 
        // be logged. 
        // Higher LogLevel means more verbose.
        if (logLevel > this._logLevel)
        {
            return;
        }

        // Lock to prevent multiple threads writing at once
        lock (this._sync)
        {
            using (StreamWriter streamWriter = File.AppendText(
                Path.GetFullPath(this._logFilePathAndName))) 
            {
                string write = $"{FormatTimestamp()} [{GetLogType(logLevel)}] - {status}";
                streamWriter.WriteLine(write);
            }
        }
    }

    public string GetLogType(LogLevel logLevel)
    {
        switch (logLevel)
        {
            case LogLevel.FATAL:
                return "FAT";
            case LogLevel.ALERT:
                return "ALT";
            case LogLevel.CRITICAL:
                return "CRT";
            case LogLevel.ERROR:
                return "ERR";
            case LogLevel.WARNING:
                return "WRN";
            case LogLevel.DEBUG:
                return "DBG";
            case LogLevel.NOTICE:
                return "NOT";
            case LogLevel.INFO:
                return "INF";
            case LogLevel.TRACE:
                return "TRC";
            case LogLevel.TODO:
                return "2DO";
            case LogLevel.COUNT:
            default:
                return "TYPE UNKNOWN! CRITICAL LOG ERROR!";
        }
    }
}

public class LoggingAsync : Logging
{
    private readonly SemaphoreSlim _asyncLock = new SemaphoreSlim(1, 1);

    public LoggingAsync(LogLevel logLevel) : base(logLevel)
    {
        
    }

    private async Task Log(string status, LogLevel logLevel)
    {
        // If the log level of the message is less severe than the log level, 
        // skip logging. For example, if log level is WARNING, skip DEBUG, INFO,
        // and TRACE messages. If it's set to FATAL, only FATAL messages will 
        // be logged. 
        // Higher LogLevel means more verbose.
        if (logLevel >= base._logLevel) 
        {
            return;
        }

        byte[] data = Encoding.UTF8.GetBytes(FormatTimestamp() + Environment.NewLine);

        await this._asyncLock.WaitAsync();
        try
        {
            await File.OpenWrite(base._logFilePathAndName).WriteAsync(data, 0, data.Length);
        }
        finally
        {
            this._asyncLock.Release();
        }
    }

    public new async Task Trace(string status)
    {
        await Log(status, LogLevel.TRACE);
    }

    public new async Task Fatal(string status)
    {
        await Log(status, LogLevel.FATAL);
    }

    public new async Task Notice(string status)
    {
        await Log(status, LogLevel.NOTICE);
    }

    public new async Task Error(string status)
    {
        await Log(status, LogLevel.ERROR);
    }

    public new async Task Debug(string status)
    {
        await Log(status, LogLevel.DEBUG);
    }

    public new async Task Alert(string status)
    {
        await Log(status, LogLevel.ALERT);
    }

    public new async Task Warn(string status)
    {
        await Log(status, LogLevel.WARNING);
    }

    public new async Task Info(string status)
    {
        await Log(status, LogLevel.INFO);
    }
    public new async Task Todo(string status)
    {
        await Log(status, LogLevel.TODO);
    }
    
    public new async Task Critical(string status)
    {
        await Log(status, LogLevel.CRITICAL);
    }
}
