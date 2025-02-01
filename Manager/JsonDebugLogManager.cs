using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainCrewTIDWindow.Manager
{
    public static class JsonDebugLogManager {

        private const int MAX_SIZE = 2;

        private static List<string> jsonTexts = new List<string>(MAX_SIZE);

        private static bool alreadyOutput = false;

        public static void AddJsonText(string jsonText) {
            if (jsonTexts.Count >= MAX_SIZE) {
                jsonTexts.RemoveAt(0);
            }
            jsonTexts.Add(jsonText);
            alreadyOutput = false;
        }

        public static void OutputJsonTexts() {
            if (!alreadyOutput) {
                var timeStamp = DateTime.Now.ToString("yyyy-MM-dd-HHmmss.fff");
                Directory.CreateDirectory("jsonLog");
                for (int i = 0; i < jsonTexts.Count; i++) {
                    File.WriteAllText($"jsonLog/{timeStamp}_{i}.json", jsonTexts[i]);
                }
                alreadyOutput = true;
            }
        }
    }
}
