
namespace TrainCrewTIDWindow.Models {
    public class TrainData(string number, int delayMinutes) {

        public string Number {
            get;
            private set;
        } = number;

        public int DelayMinutes {
            get;
            private set;
        } = delayMinutes;

        public bool Markup {
            get;
            set;
        } = false;

        /// <summary>
        /// 在線消失の際実際に在線を消すまでの猶予（チャタリング対策）
        /// 在線無しのデータが入力されるたびにカウントダウンし、0になると在線消失の処理が入る
        /// </summary>
        public int DeeCount { get; private set; } = 2;

        public bool SetStates(int delayMinutes, int count = 2) {
            var v = false;
            if (delayMinutes >= 0) {
                v = DelayMinutes != delayMinutes;
                DelayMinutes = delayMinutes;
            }
            DeeCount = count > 1 ? count : 2;

            return v;
        }

        /// <summary>
        /// 表示期限が切れた列車を消す
        /// </summary>
        /// <returns>列車が消失したか</returns>
        public bool UpdateTrack() {
            if (DeeCount > 0 && --DeeCount <= 0) {
                DeeCount = 0;
                return true;
            }
            return false;
        }
    }
}
