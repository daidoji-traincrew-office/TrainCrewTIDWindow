using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainCrewTIDWindow {
    /// <summary>
    /// サーバやTRAIN CREW本体から取得した軌道回路の情報
    /// </summary>
    /// <param name="name">軌道回路名</param>
    /// <param name="lineSettings">このトラックの線の位置やファイル名などのデータ</param>
    /// <param name="numSettingsOut">このトラックの列車番号の位置などのデータ（下り列車用）</param>
    /// <param name="numSettingsIn">このトラックの列車番号の位置などのデータ（上り列車用）</param>
    /// <param name="train">在線している列車番号</param>
    /// <param name="isReserved">進路が信号により予約されているか</param>
    /// <param name="count">在線消失の際実際に在線を消すまでの猶予（チャタリング対策）</param>
    public class TrackData(string name, LineSetting[] lineSettings, NumberSetting[] numSettingsOut, NumberSetting[] numSettingsIn, string train, bool isReserved, int count) {

        /// <summary>
        /// 軌道回路名
        /// </summary>
        public string Name {
            get; 
            private set; } = name;

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
        private readonly NumberSetting[] numSettingsOut = numSettingsOut;

        /// <summary>
        /// このトラックの列車番号の位置などのデータ（下り列車用）
        /// </summary>
        public ReadOnlyCollection<NumberSetting> NumSettingsOut => Array.AsReadOnly(numSettingsOut);

        /// <summary>
        /// このトラックの列車番号の位置などのデータ（上り列車用）
        /// </summary>
        private readonly NumberSetting[] numSettingsIn = numSettingsIn;

        /// <summary>
        /// このトラックの列車番号の位置などのデータ（上り列車用）
        /// </summary>
        public ReadOnlyCollection<NumberSetting> NumSettingsIn => Array.AsReadOnly(numSettingsIn);

        /// <summary>
        /// 在線している列車番号
        /// </summary>
        public string Train {
            get;
            private set; } = train;

        /// <summary>
        /// 進路が信号により予約されているか
        /// </summary>
        public bool IsReserved {
            get;
            private set; } = isReserved;

        /// <summary>
        /// 在線消失の際実際に在線を消すまでの猶予（チャタリング対策）
        /// 在線無しのデータが入力されるたびにカウントダウンし、0になると在線消失の処理が入る
        /// </summary>
        public int DeeCount { get; private set; } = count > 0 ? count : (train != "" ? 1 : 0);

        /// <summary>
        /// 在線中であるか
        /// </summary>
        public bool OnTrain => Train != "";

        /// <summary>
        /// サーバやTRAIN CREW本体から取得した軌道回路の情報
        /// </summary>
        /// <param name="name">軌道回路名</param>
        /// <param name="lineSettings">このトラックの線の位置やファイル名などのデータ</param>
        /// <param name="numSettingsOut">このトラックの列車番号の位置などのデータ（下り列車用）</param>
        /// <param name="numSettingsIn">このトラックの列車番号の位置などのデータ（上り列車用）</param>
        /// <param name="train">在線している列車番号</param>
        /// <param name="isReserved">進路が信号により予約されているか</param>
        public TrackData(string name, LineSetting[] lineSettingArray, NumberSetting[] numSettingOut, NumberSetting[] numSettingIn, string train, bool isReserved): this(name, lineSettingArray, numSettingOut, numSettingIn, train ,isReserved, 0) { }

        /// <summary>
        /// サーバやTRAIN CREW本体から取得した軌道回路の情報
        /// </summary>
        /// <param name="name">軌道回路名</param>
        /// <param name="lineSettings">このトラックの線の位置やファイル名などのデータ</param>
        /// <param name="numSettingsOut">このトラックの列車番号の位置などのデータ（下り列車用）</param>
        /// <param name="numSettingsIn">このトラックの列車番号の位置などのデータ（上り列車用）</param>
        /// <param name="train">在線している列車番号</param>
        /// <param name="count">在線消失の際実際に在線を消すまでの猶予（チャタリング対策）</param>
        public TrackData(string name, LineSetting[] lineSettingArray, NumberSetting[] numSettingOut, NumberSetting[] numSettingIn, string train, int count) : this(name, lineSettingArray, numSettingOut, numSettingIn, train, false, count) { }

        /// <summary>
        /// 在線情報を設定する
        /// </summary>
        /// <param name="train">在線している列車番号</param>
        /// <param name="isReserved">進路が信号により予約されているか</param>
        /// <param name="count">在線消失の際実際に在線を消すまでの猶予（チャタリング対策）</param>
        public void SetStates(string train, bool isReserved, int count) {
            if(train == "" && OnTrain) {
                --DeeCount;
            }
            IsReserved = isReserved;

            // DeeCountが0以下でない場合、在線無しのデータが入っても上書きしない
            if(train == "" && DeeCount > 0) {
                return;
            }

            Train = train;
            DeeCount = OnTrain ? (count > 0 ? count : 1) : 0;

        }

        /// <summary>
        /// 在線情報を設定する
        /// </summary>
        /// <param name="train">在線している列車番号</param>
        /// <param name="count">進路が信号により予約されているか</param>
        public void SetStates(string train, int count) {
            SetStates(train, false, count);
        }

        /// <summary>
        /// 在線情報を設定する
        /// </summary>
        /// <param name="train">在線している列車番号</param>
        /// <param name="isReserved">在線消失の際実際に在線を消すまでの猶予（チャタリング対策）</param>
        public void SetStates(string train, bool isReserved) {
            SetStates(train, isReserved, 0);
        }

        public override string ToString() {
            return $"{Name}:{Train}:{IsReserved}";
        }
    }
}
