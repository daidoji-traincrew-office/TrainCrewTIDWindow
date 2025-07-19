using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using TrainCrewTIDWindow.Models;

namespace TrainCrewTIDWindow.Manager
{
    /// <summary>
    /// 軌道回路管理用
    /// </summary>
    public class TrackManager {

        /// <summary>
        /// サーバやTRAIN CREW本体から取得した軌道回路の情報
        /// </summary>
        private readonly Dictionary<string, TrackData> trackDataDict = [];

        private readonly Dictionary<string, string> trainTCDict = [];

        /// <summary>
        /// TIDManagerオブジェクト
        /// </summary>
        private readonly TIDManager displayManager;

        /// <summary>
        /// DeeCountの初期値
        /// </summary>
        private int countStart = 2;


        /// <summary>
        /// サーバやTRAIN CREW本体から取得した軌道回路の情報
        /// </summary>
        public ReadOnlyDictionary<string, TrackData> TrackDataDict => trackDataDict.AsReadOnly();

        public string? GetTrain(string trackName) {
            foreach(var t in trainTCDict) {
                if(t.Value == trackName) {
                    return t.Key;
                }
            }
            return null;
        }

        public string? GetTrackForNum(string trainNumber) {
            if (trainTCDict.TryGetValue(trainNumber, out var v)) {
                return v;
            }
            return null;
        }

        /// <summary>
        /// DeeCountの初期値
        /// </summary>
        public int CountStart {
            get {
                return countStart;
            }
            set {
                countStart = value + 1;
            }
        }

        /// <summary>
        /// 軌道回路管理用
        /// </summary>
        /// <param name="displayManager">TIDManagerオブジェクト</param>
        public TrackManager(TIDManager displayManager) { 
            this.displayManager = displayManager;
        }


        /// <summary>
        /// 軌道回路のデータが更新された際に呼ぶ
        /// </summary>
        /// <param name="tcList">TrackCircuitDataのリスト</param>
        /// <returns>TID画面を更新する必要があるか</returns>
        public bool UpdateTCData(List<TrackCircuitData> tcList) {

            var updatedTID = false;
            var numOut = displayManager.NumSettingsOut.Where(n => n.PosX >= 0 && n.PosY >= 0).Select(n => n.TrackName).Distinct().ToArray();
            var numIn = displayManager.NumSettingsIn.Where(n => n.PosX >= 0 && n.PosY >= 0).Select(n => n.TrackName).Distinct().ToArray();
            lock (trackDataDict) {
                trainTCDict.Clear();
                foreach (var tc in tcList) {
                    if (tc == null/* || !tc.On && !tc.Lock || tc.Last != "" && !Regex.IsMatch(tc.Last, @"^([溝月レイルﾚｲﾙ]+|[回試臨]?[\d]{3,4}[ABCKST]?[XYZ]?)$")*/) {
                        continue;
                    }
                    /*Debug.WriteLine($"track {tc.Name}: {tc.Last} on:{tc.On} lock:{tc.Lock}");*/
                    if (!trackDataDict.TryAdd(tc.Name, new TrackData(tc.Name, displayManager, !tc.On ? null : tc.Last, tc.Lock, countStart))) {
                        if (tc.On || tc.Last == "") {
                            updatedTID |= trackDataDict[tc.Name].SetStates(!tc.On ? null : tc.Last, tc.Lock, countStart);
                        }
                    }
                    else {
                        updatedTID = true;
                    }
                    if (tc.On && int.TryParse(Regex.Replace(tc.Last, @"[^0-9]", ""), out var numBody)) {
                        var list = numBody % 2 == 1 ? numOut : numIn;
                        if (trainTCDict.Keys.Contains(tc.Last)) {
                            if(Array.IndexOf(list, tc.Name) > Array.IndexOf(list, trainTCDict[tc.Last])) {
                                trainTCDict[tc.Last] = tc.Name;
                            }
                        }
                        else {
                            trainTCDict.Add(tc.Last, tc.Name);
                        }
                    }
                }
            }

            foreach (var td in trackDataDict.ToArray()) {
                updatedTID |= td.Value.UpdateTrack();
            }

            //ログ爆弾注意
            /*if(trackDataDict.Keys.Any(t => trackDataDict[t].DeeCount == countStart - 2) && trackDataDict.Keys.All(t => trackDataDict[t].DeeCount >= countStart - 1)) {
                JsonDebugLogManager.OutputJsonTexts();
            }*/

            return updatedTID;
        }
    }
}
