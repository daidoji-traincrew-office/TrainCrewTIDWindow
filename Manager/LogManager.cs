using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainCrewTIDWindow.Manager {
    public static class LogManager {

        private static StringBuilder log = new();

        private static bool updated = false;

        public static bool Output { get; private set; } = false;

        public static void AddInfoLog(string text) {
            log.Append($"{DateTime.Now.ToString()} [Info] ");
            log.AppendLine(text);
            updated = true;
        }


        public static void AddWarningLog(string text) {
            log.Append($"{DateTime.Now.ToString()} [Warning] ");
            log.AppendLine(text);
            updated = true;
        }


        public static void AddExceptionLog(Exception e) {
            log.Append($"{DateTime.Now.ToString()} [Error] ");
            log.AppendLine(e.GetType().FullName);
            log.AppendLine($"source: {e.Source}");
            log.AppendLine(e.Message);
            log.AppendLine(e.StackTrace);
            updated = true;
        }

        public static void OutputLog() {
            if (updated) {
                using (StreamWriter w = new(".\\ErrorLog.txt", false, new UTF8Encoding(false))) {
                    w.Write(log);
                }
                Output = true;
                updated = false;
            }
        }
    }
}
