using System.Collections.ObjectModel;
using TrainCrewTIDWindow.Models;

namespace TrainCrewTIDWindow.Settings {

    /// <summary>
    /// 列番表示枠についての設定
    /// </summary>
    /// <param name="trackName">軌道回路名</param>
    /// <param name="nwd">列番表示枠</param>
    /// <param name="pointName">表示条件となる転轍器の名称</param>
    /// <param name="reversed">転轍器の状態（反位であるか）</param>
    public class NumberSetting(string trackName, NumberWindowData nwd, string pointName, bool reversed) {

        /// <summary>
        /// 軌道回路名
        /// </summary>
        public string TrackName { get; private set; } = trackName;

        private readonly List<NumberWindowData> windowDataList = [ nwd ]; 

        public ReadOnlyCollection<NumberWindowData> WindowDataList => windowDataList.AsReadOnly();

        /// <summary>
        /// 表示条件となる転轍器の名称
        /// </summary>
        public string PointName { get; private set; } = pointName;

        /// <summary>
        /// 転轍器の状態（反位であるか）
        /// </summary>
        public bool Reversed { get; private set; } = reversed;

        /// <summary>
        /// 表示条件（分岐器状態）が設定されているか
        /// </summary>
        public bool ExistPoint => PointName != "";

        /// <summary>
        /// 座標の数値が非表示とする条件に含まれるか（xy両方が-100以下だと非表示）
        /// </summary>
        public bool NotDraw => windowDataList.All(w => w.NotDraw);


        /// <summary>
        /// 列番表示枠についての設定
        /// </summary>
        /// <param name="trackName">軌道回路名</param>
        /// <param name="nwd">列番表示枠</param>
        public NumberSetting(string trackName, NumberWindowData nwd) : this(trackName, nwd, "", false) { }

        public void AddNumberWindow(NumberWindowData nwd) {
            windowDataList.Add(nwd);
        }


        /// <summary>
        /// 列車を設定する
        /// </summary>
        /// <param name="train">在線している列車番号</param>
        /// <returns>TID画面を更新する必要があるか</returns>
        public bool SetTrain(string train) {
            var v = false;
            foreach (var windowData in windowDataList) {
                if(windowData == null || windowData.Train != train && windowData.OnTrain) {
                    continue;
                }
                if (!windowData.OnTrain) {
                    v = true;
                }
                windowData.SetTrain(train);
                break;
            }
            return v;
        }

        /// <summary>
        /// 表示期限が切れた列車を消し表示を繰り上げる
        /// </summary>
        /// <returns>TID画面を更新する必要があるか</returns>
        public bool UpdateWindow() {
            var v = false;
            if (v) {
                var up = 0;
                for (var i = 0; i < windowDataList.Count; i++) {
                    if (!windowDataList[i].OnTrain) {
                        up++;
                    }
                    else if (up > 0) {
                        var t =windowDataList[i].DeleteTrain();
                        windowDataList[i - up].SetTrain(t);
                    }
                }
            }
            return v;
        }
    }

    /// <summary>
    /// 列番表示枠のサイズ
    /// </summary>
    public enum NumberSize {
        /// <summary>
        /// 列車番号+遅れ表示
        /// </summary>
        L,
        /// <summary>
        /// 列車番号のみ
        /// </summary>
        M,
        /// <summary>
        /// 運行番号のみ
        /// </summary>
        S
    }
}
