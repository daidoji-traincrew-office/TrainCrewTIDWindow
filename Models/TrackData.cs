using System.Collections.ObjectModel;
using TrainCrewTIDWindow.Manager;
using TrainCrewTIDWindow.Settings;

namespace TrainCrewTIDWindow.Models {
    /// <summary>
    /// サーバやTRAIN CREW本体から取得した軌道回路の情報
    /// </summary>
    /// <param name="name">軌道回路名</param>
    /// <param name="lineSettings">このトラックの線の位置やファイル名などのデータ</param>
    /// <param name="numSettingsDown">このトラックの列車番号の位置などのデータ（下り列車用）</param>
    /// <param name="numSettingsUp">このトラックの列車番号の位置などのデータ（上り列車用）</param>
    /// <param name="train">在線している列車番号</param>
    /// <param name="isReserved">進路が信号により予約されているか</param>
    /// <param name="count">在線消失の際実際に在線を消すまでの猶予（チャタリング対策）</param>
    public class TrackData(string name, LineSetting[] lineSettings, NumberWindowSetting[] numSettingsDown, NumberWindowSetting[] numSettingsUp, string? train, bool isReserved, int count) {

        /// <summary>
        /// 軌道回路名
        /// </summary>
        public string Name {
            get;
            private set;
        } = name;

        /// <summary>
        /// このトラックの線の位置やファイル名などのデータ
        /// </summary>
        private readonly LineSetting[] lineSettings = lineSettings;

        /// <summary>
        /// このトラックの線の位置やファイル名などのデータ
        /// </summary>
        public ReadOnlyCollection<LineSetting> LineSettings => Array.AsReadOnly(lineSettings);

        /// <summary>
        /// このトラックの列車番号の位置などのデータ（下り列車用）
        /// </summary>
        private readonly NumberWindowSetting[] numSettingsDown = numSettingsDown;

        /// <summary>
        /// このトラックの列車番号の位置などのデータ（下り列車用）
        /// </summary>
        public ReadOnlyCollection<NumberWindowSetting> NumSettingsDown => Array.AsReadOnly(numSettingsDown);

        /// <summary>
        /// このトラックの列車番号の位置などのデータ（上り列車用）
        /// </summary>
        private readonly NumberWindowSetting[] numSettingsUp = numSettingsUp;

        /// <summary>
        /// このトラックの列車番号の位置などのデータ（上り列車用）
        /// </summary>
        public ReadOnlyCollection<NumberWindowSetting> NumSettingsUp => Array.AsReadOnly(numSettingsUp);

        /// <summary>
        /// 在線している列車番号
        /// </summary>
        public string? Train {
            get;
            private set;
        } = train;

        /// <summary>
        /// 進路が信号により予約されているか
        /// </summary>
        public bool IsReserved {
            get;
            private set;
        } = isReserved;

        /// <summary>
        /// 在線消失の際実際に在線を消すまでの猶予（チャタリング対策）
        /// 在線無しのデータが入力されるたびにカウントダウンし、0になると在線消失の処理が入る
        /// </summary>
        public int DeeCount { get; private set; } = train != "" || isReserved ? count > 1 ? count : 2 : 0;

        /// <summary>
        /// 在線中であるか
        /// </summary>
        public bool OnTrain => Train != null;



        /// <summary>
        /// サーバやTRAIN CREW本体から取得した軌道回路の情報
        /// </summary>
        /// <param name="name">軌道回路名</param>
        /// <param name="lineSettings">このトラックの線の位置やファイル名などのデータ</param>
        /// <param name="numSettingsDown">このトラックの列車番号の位置などのデータ（下り列車用）</param>
        /// <param name="numSettingsUp">このトラックの列車番号の位置などのデータ（上り列車用）</param>
        /// <param name="train">在線している列車番号</param>
        /// <param name="isReserved">進路が信号により予約されているか</param>
        public TrackData(string name, LineSetting[] lineSettingArray, NumberWindowSetting[] numSettingDown, NumberWindowSetting[] numSettingUp, string? train, bool isReserved) : this(name, lineSettingArray, numSettingDown, numSettingUp, train, isReserved, 0) { }

        /// <summary>
        /// サーバやTRAIN CREW本体から取得した軌道回路の情報
        /// </summary>
        /// <param name="name">軌道回路名</param>
        /// <param name="lineSettings">このトラックの線の位置やファイル名などのデータ</param>
        /// <param name="numSettingsDown">このトラックの列車番号の位置などのデータ（下り列車用）</param>
        /// <param name="numSettingsUp">このトラックの列車番号の位置などのデータ（上り列車用）</param>
        /// <param name="train">在線している列車番号</param>
        /// <param name="count">在線消失の際実際に在線を消すまでの猶予（チャタリング対策）</param>
        public TrackData(string name, LineSetting[] lineSettingArray, NumberWindowSetting[] numSettingDown, NumberWindowSetting[] numSettingUp, string? train, int count) : this(name, lineSettingArray, numSettingDown, numSettingUp, train, false, count) { }

        /// <summary>
        /// サーバやTRAIN CREW本体から取得した軌道回路の情報
        /// </summary>
        /// <param name="name">軌道回路名</param>
        /// <param name="displayManager">TIDManagerオブジェクト</param>
        /// <param name="train">在線している列車番号</param>
        /// <param name="count">在線消失の際実際に在線を消すまでの猶予（チャタリング対策）</param>
        public TrackData(string name, TIDManager displayManager, string? train, int count) : this(name, displayManager.LineSettings.Where(d => d.TrackName == name).ToArray(), displayManager.NumSettingsDown.Where(d => d.TrackName == name).ToArray(), displayManager.NumSettingsUp.Where(d => d.TrackName == name).ToArray(), train, false, count) { }

        /// <summary>
        /// サーバやTRAIN CREW本体から取得した軌道回路の情報
        /// </summary>
        /// <param name="name">軌道回路名</param>
        /// <param name="displayManager">TIDManagerオブジェクト</param>
        /// <param name="train">在線している列車番号</param>
        /// <param name="isReserved">進路が信号により予約されているか</param>
        /// <param name="count">在線消失の際実際に在線を消すまでの猶予（チャタリング対策）</param>
        public TrackData(string name, TIDManager displayManager, string? train, bool isReserved, int count) : this(name, displayManager.LineSettings.Where(d => d.TrackName == name).ToArray(), displayManager.NumSettingsDown.Where(d => d.TrackName == name).ToArray(), displayManager.NumSettingsUp.Where(d => d.TrackName == name).ToArray(), train, isReserved, count) { }

        /// <summary>
        /// 在線情報を設定する
        /// </summary>
        /// <param name="train">在線している列車番号</param>
        /// <param name="isReserved">進路が信号により予約されているか</param>
        /// <param name="count">在線消失の際実際に在線を消すまでの猶予（チャタリング対策）</param>
        /// <returns>TID画面を更新する必要があるか</returns>
        public bool SetStates(string? train, bool isReserved, int count) {
            var v = train != Train || train == null && IsReserved != isReserved;
            IsReserved = isReserved;


            /*// ちょっと書き変えかけ気味のまま凍結
            if (train == "" && OnTrain)
            {
                Train = train;
                if (!isReserved) {

                }
                return true;
            }*/
            //ログ爆弾注意
            /*if (train == Train && DeeCount < count - 1) {
                JsonDebugLogManager.OutputJsonTexts();
            }*/

            Train = train;
            DeeCount = OnTrain || IsReserved ? (count > 1 ? count : 2) : 0;

            return v;

        }

        /// <summary>
        /// 在線情報を設定する
        /// </summary>
        /// <param name="train">在線している列車番号</param>
        /// <param name="count">在線消失の際実際に在線を消すまでの猶予（チャタリング対策）</param>
        /// <returns>TID画面を更新する必要があるか</returns>
        public bool SetStates(string train, int count) {
            return SetStates(train, false, count);
        }

        /// <summary>
        /// 在線情報を設定する
        /// </summary>
        /// <param name="train">在線している列車番号</param>
        /// <param name="isReserved">進路が信号により予約されているか</param>
        /// <returns>TID画面を更新する必要があるか</returns>
        public bool SetStates(string train, bool isReserved) {
            return SetStates(train, isReserved, 0);
        }

        /// <summary>
        /// 表示期限が切れた列車を消す
        /// </summary>
        /// <returns>TID画面を更新する必要があるか</returns>
        public bool UpdateTrack() {
            if (DeeCount > 0 && --DeeCount <= 0) {
                DeeCount = 0;
                Train = null;
                IsReserved = false;
                return true;
            }
            return false;
        }

        public override string ToString() {
            return $"{Name}:{Train}:{IsReserved}";
        }
    }
}
