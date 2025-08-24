using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Text.RegularExpressions;
using TrainCrewTIDWindow.Models;
using TrainCrewTIDWindow.Settings;

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

        private readonly List<string> duplicatingTrains = [];

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

        public ReadOnlyCollection<string> DuplicatingTrains => duplicatingTrains.AsReadOnly();

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
                    if (tc == null/* || !tc.On && !tc.Lock || tc.Last != "" && !Regex.IsMatch(tc.Last, @"^([溝月レイルﾚｲﾙVague]+|[回試臨]?[\d]{3,4}[ABCKST]?[XYZ]?)$")*/) {
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

                    /*if (tc.On && int.TryParse(Regex.Replace(tc.Last, @"[^0-9]", ""), out var numBody)) {
                        var list = numBody % 2 == 1 ? numDown : numUp;
                        if (trainTCDict.ContainsKey(tc.Last)) {
                            if(Array.IndexOf(list, tc.Name) > Array.IndexOf(list, trainTCDict[tc.Last])) {
                                trainTCDict[tc.Last] = tc.Name;
                            }
                        }
                        else {
                            trainTCDict.Add(tc.Last, tc.Name);
                        }
                    }*/
                }

                /*foreach(var t in trainTCDict) {
                    var td = trackDataDict[t.Value];
                    if (int.TryParse(Regex.Replace(t.Key, @"[^0-9]", ""), out var numBody)) {
                        var list = numBody % 2 == 1 ? td.NumSettingsDown : td.NumSettingsUp;
                        foreach (var n in list) {
                            n.SetTrain(t.Key);
                        }
                    }
                }*/

                /*updatedTID |= displayManager.UpdateNumWindow();

                foreach(var n in displayManager.NumSettingsDown) {
                    updatedTID |= n.UpdateWindow();
                }
                foreach (var n in displayManager.NumSettingsUp) {
                    updatedTID |= n.UpdateWindow();
                }*/

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

        public bool UpdateNumberWindow() {
            duplicatingTrains.Clear();
            var updatedTID = false;

            var dict = new Dictionary<string, List<string>>();
            var numDown = displayManager.NumSettingsDown.Where(n => !n.NotDraw).Select(n => n.TrackName).Distinct();
            var numUp = displayManager.NumSettingsUp.Where(n => !n.NotDraw).Select(n => n.TrackName).Distinct();
            foreach (var t in trackDataDict.Values) {
                var train = t.Train;
                if(train != null) {
                    if (!dict.TryAdd(train, [t.Name])) {
                        dict[train].Add(t.Name);
                    }
                }
            }
            var connections = displayManager.TrackConnections;
            var pointDataDict = displayManager.Window.PointDataDict;
            foreach (var train in dict.Keys) {
                var onTracks = new List<string>();


                var tracks = dict[train];

                var isTrain = int.TryParse(Regex.Replace(train, @"[^0-9]", ""), out var numBody);

                if (!isTrain) {
                    onTracks.AddRange(tracks);
                }

                void DecideConnection(TrackConnectionSetting c, string t) {
                    if (c.Track != t && !tracks.Contains(c.Track)) {
                        return;
                    }
                    var locked = c.TargetTracks.All(target => target.Switches.All(s => pointDataDict.ContainsKey(s.SwitchName) && pointDataDict[s.SwitchName].IsLocked));
                    foreach (var target in c.TargetTracks) {

                        var v = true;
                        /*var locked = true;*/
                        if (locked) {
                            foreach (var s in target.Switches) {
                                if (pointDataDict.ContainsKey(s.SwitchName)) {
                                    var p = pointDataDict[s.SwitchName];
                                    /*locked &= p.IsLocked;*/
                                    if (p.IsLocked && p.IsReversed != s.Reversed) {
                                        v = false;
                                        break;
                                    }
                                }
                            }
                            if (!v) {
                                continue;
                            }
                            if (locked && target.Track != t && !tracks.Contains(target.Track)) {
                                break;
                            }
                            var nextT = t == c.Track || t != target.Track ? target.Track : c.Track;
                            onTracks.Add(nextT);
                            tracks.Remove(nextT);
                            foreach (var c2 in connections.Where(c2 => c2 != c && c2.ContainsTrack(nextT))) {
                                DecideConnection(c2, nextT);
                            }
                            break;
                        }
                        else {
                            if (target.Track != t && !tracks.Contains(target.Track)) {
                                break;
                            }
                            var nextT = t == c.Track || t != target.Track ? target.Track : c.Track;
                            onTracks.Add(nextT);
                            tracks.Remove(nextT);
                            foreach (var c2 in connections.Where(c2 => c2 != c && c2.ContainsTrack(nextT))) {
                                DecideConnection(c2, nextT);
                            }
                        }
                    }
                }

                var duplicating = 0;
                while (isTrain && tracks.Count > 0) {
                    duplicating++;
                    if(duplicating > 1) {
                        duplicatingTrains.Add(train);
                    }
                    var track = tracks[0];
                    onTracks.Add(track);
                    tracks.Remove(track);

                    foreach(var c in connections.Where(c => c.ContainsTrack(track)/*c.Track == track*/)) {
                        DecideConnection(c, track);
                    }

                    IEnumerable<NumberWindowSetting>? list = null;
                    var window = "";
                    var l = (numBody % 2 == 1 ? numDown : numUp).ToArray();
                    foreach (var t in onTracks) {
                        if (window != "" && Array.IndexOf(l, t) < Array.IndexOf(l, window)) {
                            continue;
                        }
                        window = t;
                    }

                    var td = trackDataDict[window];
                    list = numBody % 2 == 1 ? td.NumSettingsDown : td.NumSettingsUp;
                    var rule = "";
                    foreach (var numData in list) {

                        // 転轍器の状態で表示条件を判定
                        var r = numData.PointName != "" ? $"{numData.PointName}/{numData.Reversed}" : "";
                        if (r != "" && rule == "" && pointDataDict.ContainsKey(numData.PointName)) {
                            var point = pointDataDict[numData.PointName];
                            if (point.IsLocked && point.IsReversed == numData.Reversed) {
                                rule = r;
                            }
                        }

                        // 表示条件を満たさない場合は表示しない
                        if (rule != r) {
                            continue;
                        }
                        numData.SetTrain(train);
                    }
                }

                /*IEnumerable<NumberSetting>? list = null;
                if (isTrain) {
                    var window = "";
                    var l = (numBody % 2 == 1 ? numDown : numUp).ToArray();
                    foreach (var t in onTracks) {
                        if (window != "" && Array.IndexOf(l, t) < Array.IndexOf(l, window)) {
                            continue;
                        }
                        window = t;
                    }

                    var td = trackDataDict[window];
                    list = numBody % 2 == 1 ? td.NumSettingsDown : td.NumSettingsUp;
                    var rule = "";
                    foreach (var numData in list) {

                        // 転轍器の状態で表示条件を判定
                        var r = numData.PointName != "" ? $"{numData.PointName}/{numData.Reversed}" : "";
                        if (r != "" && rule == "" && pointDataDict.ContainsKey(numData.PointName)) {
                            var point = pointDataDict[numData.PointName];
                            if (point.IsLocked && point.IsReversed == numData.Reversed) {
                                rule = r;
                            }
                        }

                        // 表示条件を満たさない場合は表示しない
                        if (rule != r) {
                            continue;
                        }
                        numData.SetTrain(train);
                    }
                }*/
                /*else {
                    var tds = trackDataDict.Where(t => onTracks.Contains(t.Key)).ToDictionary();
                    foreach (var td in tds) {
                        var rule = "";
                        foreach (var numData in td.Value.NumSettingsDown) {

                            // 転轍器の状態で表示条件を判定
                            var r = numData.PointName != "" ? $"{numData.PointName}/{numData.Reversed}" : "";
                            if (r != "" && rule == "" && pointDataDict.ContainsKey(numData.PointName)) {
                                var point = pointDataDict[numData.PointName];
                                if (point.IsLocked && point.IsReversed == numData.Reversed) {
                                    rule = r;
                                }
                            }

                            // 表示条件を満たさない場合は表示しない
                            if (rule != r) {
                                continue;
                            }
                            numData.SetTrain(train);
                        }

                        rule = "";
                        foreach (var numData in td.Value.NumSettingsUp) {

                            // 転轍器の状態で表示条件を判定
                            var r = numData.PointName != "" ? $"{numData.PointName}/{numData.Reversed}" : "";
                            if (r != "" && rule == "" && pointDataDict.ContainsKey(numData.PointName)) {
                                var point = pointDataDict[numData.PointName];
                                if (point.IsLocked && point.IsReversed == numData.Reversed) {
                                    rule = r;
                                }
                            }

                            // 表示条件を満たさない場合は表示しない
                            if (rule != r) {
                                continue;
                            }
                            numData.SetTrain(train);
                        }
                    }
                }*/

                if (!isTrain) {
                    var tds = trackDataDict.Where(t => onTracks.Contains(t.Key)).ToDictionary();
                    foreach (var td in tds) {
                        var rule = "";
                        foreach (var numData in td.Value.NumSettingsDown) {

                            // 転轍器の状態で表示条件を判定
                            var r = numData.PointName != "" ? $"{numData.PointName}/{numData.Reversed}" : "";
                            if (r != "" && rule == "" && pointDataDict.ContainsKey(numData.PointName)) {
                                var point = pointDataDict[numData.PointName];
                                if (point.IsLocked && point.IsReversed == numData.Reversed) {
                                    rule = r;
                                }
                            }

                            // 表示条件を満たさない場合は表示しない
                            if (rule != r) {
                                continue;
                            }
                            numData.SetTrain(train);
                        }

                        rule = "";
                        foreach (var numData in td.Value.NumSettingsUp) {

                            // 転轍器の状態で表示条件を判定
                            var r = numData.PointName != "" ? $"{numData.PointName}/{numData.Reversed}" : "";
                            if (r != "" && rule == "" && pointDataDict.ContainsKey(numData.PointName)) {
                                var point = pointDataDict[numData.PointName];
                                if (point.IsLocked && point.IsReversed == numData.Reversed) {
                                    rule = r;
                                }
                            }

                            // 表示条件を満たさない場合は表示しない
                            if (rule != r) {
                                continue;
                            }
                            numData.SetTrain(train);
                        }
                    }
                }

            }
            updatedTID |= displayManager.UpdateNumWindow();

            foreach (var n in displayManager.NumSettingsDown) {
                updatedTID |= n.UpdateWindow();
            }
            foreach (var n in displayManager.NumSettingsUp) {
                updatedTID |= n.UpdateWindow();
            }




            return updatedTID;
        }
    }
}
