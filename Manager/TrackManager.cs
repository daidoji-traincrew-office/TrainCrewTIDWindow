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
            var numDown = displayManager.NumSettingsDown.Where(n => !n.NotDraw).Select(n => n.TrackName).Distinct().ToArray();
            var numUp = displayManager.NumSettingsUp.Where(n => !n.NotDraw).Select(n => n.TrackName).Distinct().ToArray();
            lock (trackDataDict) {
                trainTCDict.Clear();
                foreach (var tc in tcList) {
                    if (tc == null/* || !tc.On && !tc.Lock || tc.Last != "" && !Regex.IsMatch(tc.Last, @"^([溝月レイルﾚｲﾙ]+|[回試臨]?[\d]{3,4}[ABCKST]?[XYZ]?)$")*/) {
                        continue;
                    }
                    /*Debug.WriteLine($"track {tc.Name}: {tc.Last} on:{tc.On} lock:{tc.Lock}");*/
                    var td = new TrackData(tc.Name, displayManager, !tc.On ? null : tc.Last, tc.Lock, countStart);
                    if (!trackDataDict.TryAdd(tc.Name, td)) {
                        if (tc.On || tc.Last == "") {
                            td = trackDataDict[tc.Name];
                            updatedTID |= td.SetStates(!tc.On ? null : tc.Last, tc.Lock, countStart);
                        }
                    }
                    else {
                        updatedTID = true;
                    }
                    if (tc.On && int.TryParse(Regex.Replace(tc.Last, @"[^0-9]", ""), out var numBody)) {
                        var list = numBody % 2 == 1 ? numDown : numUp;
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

                foreach(var t in trainTCDict) {
                    var td = trackDataDict[t.Value];
                    if (int.TryParse(Regex.Replace(t.Key, @"[^0-9]", ""), out var numBody)) {
                        var list = numBody % 2 == 1 ? td.NumSettingsDown : td.NumSettingsUp;
                        foreach (var n in list) {
                            n.SetTrain(t.Key);
                        }
                    }
                }

                updatedTID |= displayManager.UpdateNumWindow();

                foreach(var n in displayManager.NumSettingsDown) {
                    updatedTID |= n.UpdateWindow();
                }
                foreach (var n in displayManager.NumSettingsUp) {
                    updatedTID |= n.UpdateWindow();
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
