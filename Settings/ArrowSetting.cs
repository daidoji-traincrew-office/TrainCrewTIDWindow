namespace TrainCrewTIDWindow.Settings
{
    /// <summary>
    /// 単線区間の方向てこ状態を示す矢印の位置やファイル名などについての設定
    /// </summary>
    /// <param name="name">名称（暫定）</param>
    /// <param name="type">向き（R/L）</param>
    /// <param name="fileName">ファイル名称</param>
    /// <param name="posX">画面上のx座標</param>
    /// <param name="posY">画面上のy座標</param>
    public class ArrowSetting(string name, ArrowType type, string fileName, int posX, int posY)
    {

        /// <summary>
        /// 名称（暫定）
        /// </summary>
        public string Name { get; private set; } = name;

        /// <summary>
        /// 向き（R/L）
        /// </summary>
        public ArrowType Type { get; private set; } = type;

        /// <summary>
        /// ファイル名称
        /// </summary>
        public string FileName { get; private set; } = fileName;

        /// <summary>
        /// 画面上のx座標
        /// </summary>
        public int PosX { get; private set; } = posX;

        /// <summary>
        /// 画面上のy座標
        /// </summary>
        public int PosY { get; private set; } = posY;
    }

    /// <summary>
    /// 矢印の向き
    /// </summary>
    public enum ArrowType
    {
        /// <summary>
        /// 右→
        /// </summary>
        R,
        /// <summary>
        /// ←左
        /// </summary>
        L
    }
}
