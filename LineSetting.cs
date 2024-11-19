using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainCrewTIDWindow {
    /// <summary>
    /// 軌道回路の状態を表示する線についての設定
    /// </summary>
    /// <param name="trackName">軌道回路名</param>
    /// <param name="fileName">ファイル名称（_Y、_Rなど、状態部分を含まない）</param>
    /// <param name="posX">画面上のx座標</param>
    /// <param name="posY">画面上のy座標</param>
    /// <param name="pointName">表示条件となる転轍器の名称</param>
    /// <param name="reversed">転轍器の状態（反位であるか）</param>
    public class LineSetting(string trackName, string fileName, int posX, int posY, string pointName, bool reversed) {

        /// <summary>
        /// 軌道回路名
        /// </summary>
        public string TrackName { get; private set; } = trackName;

        /// <summary>
        /// ファイル名称（_Y、_Rなど、状態部分を含まない）
        /// </summary>
        public string FileName { get; private set; } = fileName;

        /// <summary>
        /// ファイル名称（赤線）
        /// </summary>
        public string FileNameR { get; private set; } = $"{fileName}_R";

        /// <summary>
        /// ファイル名称（黄線）
        /// </summary>
        public string FileNameY { get; private set; } = $"{fileName}_Y";

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
        public bool IsDefault => PointName == "";

        /// <summary>
        /// 軌道回路の状態を表示する線についての設定
        /// </summary>
        /// <param name="trackName">軌道回路名</param>
        /// <param name="fileName">ファイル名称（_Y、_Rなど、状態部分を含まない）</param>
        /// <param name="posX">画面上のx座標</param>
        /// <param name="posY">画面上のy座標</param>
        public LineSetting(string trackName, string fileName, int posX, int posY) : this(trackName, fileName, posX, posY, "", false) { }

    }
}
