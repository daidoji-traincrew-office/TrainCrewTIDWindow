using TrainCrewTIDWindow.Settings;

namespace TrainCrewTIDWindow.Models {
    /// <summary>
    /// 列番表示枠
    /// </summary>
    /// <param name="size">表示枠のサイズ</param>
    /// <param name="posX">画面上のx座標</param>
    /// <param name="posY">画面上のy座標</param>
    public class NumberWindowData(NumberSize size, int posX, int posY) {

        /// <summary>
        /// 表示枠のサイズ
        /// </summary>
        public NumberSize Size { get; private set; } = size;

        /// <summary>
        /// 画面上のx座標
        /// </summary>
        public int PosX { get; private set; } = posX;

        /// <summary>
        /// 画面上のy座標
        /// </summary>
        public int PosY { get; private set; } = posY;

        /// <summary>
        /// 表示する列車番号
        /// </summary>
        public string? Train {
            get;
            private set;
        } = null;

        /// <summary>
        /// 在線消失の際実際に在線を消すまでの猶予
        /// 在線無しのデータが入力されるたびにカウントダウンし、0になると在線消失の処理が入る
        /// </summary>
        public int DeeCount { get; private set; } =  0;

        /// <summary>
        /// 在線中であるか
        /// </summary>
        public bool OnTrain => Train != null;

        /// <summary>
        /// 座標の数値が非表示とする条件に含まれるか（xy両方が-100以下だと非表示）
        /// </summary>
        public bool NotDraw => PosX <= -100 && PosY <= -100;

        /// <summary>
        /// 列車を設定する
        /// </summary>
        /// <param name="train">在線している列車番号</param>
        /// <param name="count">在線消失の際実際に在線を消すまでの猶予（チャタリング対策）</param>
        /// <returns>TID画面を更新する必要があるか</returns>
        public bool SetTrain(string train, int count = 2) {
            var v = train != Train;
            Train = train;
            DeeCount = OnTrain ? (count > 1 ? count : 2) : 0;

            return v;
        }

        /// <summary>
        /// 表示期限が切れた列車を消す
        /// </summary>
        /// <returns>TID画面を更新する必要があるか</returns>
        public bool UpdateWindow() {
            if (DeeCount > 0 && --DeeCount <= 0) {
                DeeCount = 0;
                Train = null;
                return true;
            }
            return false;
        }

        public string DeleteTrain() {
            var v = Train;
            DeeCount = 0;
            Train = null;
            return v ?? "";
        }

        public Size GetSize() {
            return Size switch {
                NumberSize.L => new Size(59, 11),
                NumberSize.M => new Size(47, 11),
                NumberSize.S => new Size(29, 11),
                _ => new Size(0, 0),
            };
        }

    }
}
