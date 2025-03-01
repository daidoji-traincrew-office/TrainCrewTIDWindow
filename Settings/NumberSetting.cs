namespace TrainCrewTIDWindow.Settings {

    /// <summary>
    /// 列番表示枠についての設定
    /// </summary>
    /// <param name="trackName">軌道回路名</param>
    /// <param name="size">表示枠のサイズ</param>
    /// <param name="posX">画面上のx座標</param>
    /// <param name="posY">画面上のy座標</param>
    /// <param name="pointName">表示条件となる転轍器の名称</param>
    /// <param name="reversed">転轍器の状態（反位であるか）</param>
    public class NumberSetting(string trackName, NumberSize size, int posX, int posY, string pointName, bool reversed) {

        /// <summary>
        /// 軌道回路名
        /// </summary>
        public string TrackName { get; private set; } = trackName;

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
        public bool NotDraw => PosX <= -100 && PosY <= -100;

        /// <summary>
        /// 列番表示枠についての設定
        /// </summary>
        /// <param name="trackName">軌道回路名</param>
        /// <param name="size">表示枠のサイズ</param>
        /// <param name="posX">画面上のx座標</param>
        /// <param name="posY">画面上のy座標</param>
        public NumberSetting(string trackName, NumberSize size, int posX, int posY) : this(trackName, size, posX, posY, "", false) { }
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
