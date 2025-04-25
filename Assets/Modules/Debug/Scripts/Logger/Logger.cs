using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Modules.Debug.Scripts.Logger
{
    public class Logger : IDisposable
    {
        private const int MAX_LOGS_COUNT = 5;
        private const string LOGS_FOLDER = "Logs";
        private const string LOGS_EXTENSION = "log";

        private List<string> _logs = new List<string>();

        public Logger()
        {
            Application.logMessageReceived += LogHandler;
        }

        public void Dispose()
        {
            Application.logMessageReceived -= LogHandler;

            SaveLogsFile();
            _logs.Clear();            
        }

        private void LogHandler(string condition, string stackTrace, LogType type)
        {
            _logs.Add(CreateMessage(condition, stackTrace, type));
        }

        private string CreateMessage(string condition, string stackTrace, LogType type)
        {
            var resultStacktrace = type == LogType.Exception || type == LogType.Error
                    ? stackTrace
                    : stackTrace.Length > 210
                        ? stackTrace[..200]
                        : stackTrace;
            return $"[{type.ToString()}] (UTC {GetTimeString(DateTime.UtcNow)}) :: {condition}\r\n{resultStacktrace}\r\n";
        }

        private string GetTimeString(DateTime time)
        {
            return $"{time:yyyy.MM.dd - H:mm:ss}";
        }

        private string GetFileName(DateTime time)
        {
            return $"{time:yyyyMMddHHmmss}.{LOGS_EXTENSION}";
        }

        private void SaveLogsFile()
        {
            var dir = Path.Combine(Application.persistentDataPath, LOGS_FOLDER);

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            string[] logsFiles = Directory.GetFiles(dir, $"*.{LOGS_EXTENSION}");
            if (logsFiles.Length > MAX_LOGS_COUNT - 1)
                for (int i = 0; i < (logsFiles.Length - (MAX_LOGS_COUNT - 1)); i++)
                    if (File.Exists(logsFiles[i]))
                        File.Delete(logsFiles[i]);

            var logsStr = string.Empty;
            for (int i = 0; i < _logs.Count; i++)
            {
                logsStr += _logs[i];
                if (i != _logs.Count - 1)
                    logsStr += Environment.NewLine;
            }

            string newLogsPath = Path.Combine(dir, GetFileName(DateTime.UtcNow));
            File.WriteAllText(newLogsPath, logsStr);
        }
    }
}
