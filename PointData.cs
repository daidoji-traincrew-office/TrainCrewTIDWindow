using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainCrewTIDWindow {

    /// <summary>
    /// サーバから取得した転轍器の情報
    /// </summary>
    /// <param name="name">転轍器名</param>
    /// <param name="isLocked">鎖錠されているか</param>
    /// <param name="isReversed">反位であるか</param>
    public class PointData(string name, bool isLocked, bool isReversed) {

        /// <summary>
        /// 転轍器名
        /// </summary>
        public string Name { get; private set; } = name;

        /// <summary>
        /// 鎖錠されているか
        /// </summary>
        public bool IsLocked { get; private set; } = isLocked;

        /// <summary>
        /// 反位であるか
        /// </summary>
        public bool IsReversed { get; private set;} = isReversed;

        public void SetStates(bool isLocked, bool isReversed) {
            IsLocked = isLocked;
            IsReversed = isReversed;
        }
    }
}
