using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainCrewTIDWindow.Settings
{
    /// <summary>
    /// 踏切の位置やファイル名などについての設定
    /// </summary>
    /// <param name="name">踏切名</param>
    /// <param name="fileName">ファイル名称（_G、_Rなど、状態部分を含まない）</param>
    /// <param name="posX">画面上のx座標</param>
    /// <param name="posY">画面上のy座標</param>
    public class CrossingSetting(string name, string fileName, int posX, int posY)
    {
        /// <summary>
        /// 踏切名
        /// </summary>
        public string Name { get; private set; } = name;

        /// <summary>
        /// ファイル名称（_G、_Rなど、状態部分を含まない）
        /// </summary>
        public string FileName { get; private set; } = fileName;

        /// <summary>
        /// ファイル名称（赤）
        /// </summary>
        public string FileNameR { get; private set; } = $"{fileName}_R";

        /// <summary>
        /// ファイル名称（緑）
        /// </summary>
        public string FileNameG { get; private set; } = $"{fileName}_G";

        /// <summary>
        /// 画面上のx座標
        /// </summary>
        public int PosX { get; private set; } = posX;

        /// <summary>
        /// 画面上のy座標
        /// </summary>
        public int PosY { get; private set; } = posY;
    }
}
