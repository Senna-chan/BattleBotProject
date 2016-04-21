using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;

namespace BattleBotClientWPF
{
    /// <summary>
    /// A Logging class implementing the Singleton pattern and an internal Queue to be flushed perdiodically
    /// </summary>
    public class LogWriter
    {
        private static LogWriter _instance;
        private static Queue<Log> _logQueue;
        private static string _logDir = Environment.CurrentDirectory+"/";
        private static string _logFile = "BattleLog.txt";
        private static int _maxLogAge = 99999999;
        private static int _queueSize = 1;
        private static DateTime _lastFlushed = DateTime.Now;

        /// <summary>
        /// Private constructor to prevent instance creation
        /// </summary>
        private LogWriter() { }

        /// <summary>
        /// An LogWriter instance that exposes a single instance
        /// </summary>
        public static LogWriter Instance
        {
            get
            {
                // If the instance is null then create one and init the Queue
                if (_instance == null)
                {
                    _instance = new LogWriter();
                    _logQueue = new Queue<Log>();
                }
                return _instance;
            }
        }

        

        /// <summary>
        /// The single instance method that writes to the log file
        /// </summary>
        /// <param name="message">The message to write to the log</param>
        public void WriteToLog(string message)
        {
            // Lock the queue while writing to prevent contention for the log file
            lock (_logQueue)
            {
                // Create the entry and push to the Queue
                Log logEntry = new Log(message);
                _logQueue.Enqueue(logEntry);

                // If we have reached the Queue Size then flush the Queue
                if (_logQueue.Count >= _queueSize || DoPeriodicFlush())
                {
                    FlushLog();
                }
            }
        }

        public void WriteToLog(string loglevel, string message)
        {
            // Lock the queue while writing to prevent contention for the log file
            lock (_logQueue)
            {
                // Create the entry and push to the Queue
                Log logEntry = new Log(loglevel, message);
                _logQueue.Enqueue(logEntry);

                // If we have reached the Queue Size then flush the Queue
                if (_logQueue.Count >= _queueSize || DoPeriodicFlush())
                {
                    FlushLog();
                }
            }
        }

        private bool DoPeriodicFlush()
        {
            TimeSpan logAge = DateTime.Now - _lastFlushed;
            if (logAge.TotalSeconds >= _maxLogAge)
            {
                _lastFlushed = DateTime.Now;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Flushes the Queue to the physical log file
        /// </summary>
        private void FlushLog()
        {
            while (_logQueue.Count > 0)
            {
                Log entry = _logQueue.Dequeue();
                string logPath = _logDir + entry.LogDate + "_" + _logFile;

                // This could be optimised to prevent opening and closing the file for each write
                using (FileStream fs = File.Open(logPath, FileMode.Append, FileAccess.Write))
                {
                    using (StreamWriter log = new StreamWriter(fs))
                    {
                        log.WriteLine(string.Format("{0} [{1}]\t{2}", entry.LogTime, entry.LogLevel.ToUpper(), entry.Message));
                    }
                }
            }
        }
    }

    /// <summary>
    /// A Log class to store the message and the Date and Time the log entry was created
    /// </summary>
    public class Log
    {
        public string Message   { get; set; }
        public string LogTime   { get; set; }
        public string LogDate   { get; set; }
        public string LogLevel  { get; set; }

        public Log(string message)
        {
            LogLevel    = "Debug";
            Message     = message;
            LogDate     = DateTime.Now.ToString("dd-MM-yyyy");
            LogTime     = DateTime.Now.ToString("HH:mm:ss.fff tt");
        }
        public Log(string logLevel, string message)
        {
            LogLevel    = logLevel;
            Message     = message;
            LogDate     = DateTime.Now.ToString("dd-MM-yyyy");
            LogTime     = DateTime.Now.ToString("HH:mm:ss.fff tt");
        }
    }
}