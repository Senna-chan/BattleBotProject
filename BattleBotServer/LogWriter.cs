using System;
using System.Collections.Generic;
using System.IO;

namespace BattleBotServer
{
    /// <summary>
    ///     A Logging class implementing the Singleton pattern and an internal Queue to be flushed perdiodically
    /// </summary>
    public class LogWriter
    {
        private static LogWriter instance;
        private static Queue<Log> logQueue;
        private static string logDir;
        private static readonly string logFile = "BattleLog.txt";
        private static readonly int maxLogAge = 99999999;
        private static readonly int queueSize = 1;
        private static DateTime LastFlushed = DateTime.Now;

        /// <summary>
        ///     Private constructor to prevent instance creation
        /// </summary>
        private LogWriter()
        {
        }

        /// <summary>
        ///     An LogWriter instance that exposes a single instance
        /// </summary>
        public static LogWriter Instance
        {
            get
            {
                // If the instance is null then create one and init the Queue
                if (Helpers.IsLinux)
                {
                    logDir = Environment.CurrentDirectory + "\\";
                }
                else
                {
                    logDir = Environment.CurrentDirectory + "/";
                }
                if (instance == null)
                {
                    instance = new LogWriter();
                    logQueue = new Queue<Log>();
                }
                return instance;
            }
        }


        /// <summary>
        ///     The single instance method that writes to the log file
        /// </summary>
        /// <param name="message">The message to write to the log</param>
        public void WriteToLog(object message)
        {
            // Lock the queue while writing to prevent contention for the log file
            lock (logQueue)
            {
                // Create the entry and push to the Queue
                var logEntry = new Log(message);
                logQueue.Enqueue(logEntry);

                // If we have reached the Queue Size then flush the Queue
                if (logQueue.Count >= queueSize || DoPeriodicFlush())
                {
                    FlushLog();
                }
            }
        }

        public void WriteToLog(string loglevel, object message)
        {
            // Lock the queue while writing to prevent contention for the log file
            lock (logQueue)
            {
                // Create the entry and push to the Queue
                var logEntry = new Log(loglevel, message);
                logQueue.Enqueue(logEntry);

                // If we have reached the Queue Size then flush the Queue
                if (logQueue.Count >= queueSize || DoPeriodicFlush())
                {
                    FlushLog();
                }
            }
        }

        private bool DoPeriodicFlush()
        {
            var logAge = DateTime.Now - LastFlushed;
            if (logAge.TotalSeconds >= maxLogAge)
            {
                LastFlushed = DateTime.Now;
                return true;
            }
            return false;
        }

        /// <summary>
        ///     Flushes the Queue to the physical log file
        /// </summary>
        private void FlushLog()
        {
            while (logQueue.Count > 0)
            {
                var entry = logQueue.Dequeue();
                var logPath = logDir + entry.LogDate + "_" + logFile;

                // This could be optimised to prevent opening and closing the file for each write
                using (var fs = File.Open(logPath, FileMode.Append, FileAccess.Write))
                {
                    using (var log = new StreamWriter(fs))
                    {
                        log.WriteLine("{0} [{1}]\t{2}", entry.LogTime, entry.LogLevel.ToUpper(), entry.Message);
                    }
                }
            }
        }
    }

    /// <summary>
    ///     A Log class to store the message and the Date and Time the log entry was created
    /// </summary>
    public class Log
    {
        public Log(object message)
        {
            LogLevel = "Debug";
            Message = message;
            LogDate = DateTime.Now.ToString("dd-MM-yyyy");
            LogTime = DateTime.Now.ToString("HH:mm:ss.fff tt");
        }

        public Log(string logLevel, object message)
        {
            LogLevel = logLevel;
            Message = message;
            LogDate = DateTime.Now.ToString("dd-MM-yyyy");
            LogTime = DateTime.Now.ToString("HH:mm:ss.fff tt");
        }

        public object Message { get; set; }
        public string LogTime { get; set; }
        public string LogDate { get; set; }
        public string LogLevel { get; set; }
    }
}