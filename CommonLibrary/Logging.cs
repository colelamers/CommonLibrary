namespace CommonLibrary
{
    /*
     * Created by Cole Lamers 
     * 
     * This code sets up the debug logging library to be used  
     * 
     * 2020-12-06    First version
     * 2020-12-15    Revised the program to have a default constructor.
     *               Removed static from all functions. 
     * 2020-12-21    Revised the LogFilePath to just be \Logs\ because \Debug\Logs 
     *               was being placed in a new solutions \Debug\ folder.
     *               Removed colons in LogAction function between yyyMMdd and put hyphens.
     * 2023-08-12    Updated comments, made some minor code revisions for better readability.
     */
    public class Logging
    {
        private string _logFilePath { get; set; }
        private string _logFileName { get; set; }
        private string _logFilePathAndName { get; set; }

        // public int LogLevel { get; set; } // todo 4; implement one day. personally do not have a use rn

        /**
         * Default Constructor that sets the path values and then creates 
         * the log file or verifies it exists.
         */
        public Logging()
        {
            // Default Path starts in Debug folder of solutions
            _logFilePath = @"..\Logs\";
            _logFileName = $"{DateTime.Now:yyyyMMdd}_Log.txt";
            _logFilePathAndName = Path.Combine(_logFilePath, _logFileName);
            // LogLevel = logLevel;
            CreateDebugLogger();
        }

        /**
         * Creates/Verifies a path and log file. 
         * The debug file is written as the specific day the program is run
         */
        private void CreateDebugLogger()
        {
            // Creates the log directory if it doesn't exist
            if (!Directory.Exists(_logFilePath))
            {
                Directory.CreateDirectory(_logFilePath);
            }

            if (!File.Exists(Path.GetFullPath(_logFilePathAndName)))
            {
                using (StreamWriter sw = File.CreateText(_logFilePathAndName)) 
                {
                    sw.Flush();
                }
            }
        }

        /**
         * Accepts a string that will be written to the log file.
         * 
         * @param | string | "log this text to file" |
         * Whatever text you want added at the specific datetime to the log file.
         */
        public void Log(string status)
        {
            using (StreamWriter streamWriter = File.AppendText(Path.GetFullPath(_logFilePathAndName)))
            {
                streamWriter.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.ffff} - {status}");
            }
        }

        /**
         * Accepts a string that will be written to the log file.
         * 
         * @param | string | "log this text to file" |
         * Whatever text you want added at the specific datetime to the log file.
         * 
         * @param | Exception | 
         * Appends exception to log.
         */
        public void Log(string status, Exception ex)
        {
            using (StreamWriter streamWriter = File.AppendText(Path.GetFullPath(_logFilePathAndName)))
            {
                streamWriter.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.ffff} - {status}, {ex}");
            }
        }
    }
}
