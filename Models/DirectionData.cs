using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainCrewTIDWindow.Models {
    // <summary>
    /// 方向てこデータクラス
    /// </summary>
    public class DirectionData {
        /// <summary>
        /// 方向てこ名称
        /// </summary>
        public string Name { get; set; } = "";
        /// <summary>
        /// 方向てこの値
        /// </summary>
        public LCR State { get; set; } = LCR.Left;
    }
}
