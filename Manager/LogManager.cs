using System.Text;

namespace TrainCrewTIDWindow.Manager {
    public static class LogManager {

        private static readonly StringBuilder log = new();

        private static bool updated = false;

        public static bool Output { get; private set; } = false;

        public static bool NeededWarning { get; private set; } = false;

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


        public static void AddExceptionLog(HttpRequestException e) {
            log.Append($"{DateTime.Now.ToString()} [Error] ");
            log.AppendLine(e.GetType().FullName);
            log.AppendLine($"status: {e.StatusCode}");
            log.AppendLine($"source: {e.Source}");
            log.AppendLine(e.Message);
            log.AppendLine(e.StackTrace);
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

        public static void OutputLog(bool neededWarning = false) {
            if (updated) {
                using (StreamWriter w = new(".\\ErrorLog.txt", false, new UTF8Encoding(false))) {
                    w.Write(log);
                }
                Output = true;
                updated = false;
                NeededWarning |= neededWarning;
            }
        }
    }
}
