using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrainCrewTIDWindow.Models;

namespace TrainCrewTIDWindow.Settings {
    public class SignalDirectionSetting(string name, LCR type, string lever1Name, string lever2Name) {

        /// <summary>
        /// 名称（不使用）
        /// </summary>
        public string Name { get; private set; } = name;

        /// <summary>
        /// 向き（R/L）
        /// </summary>
        public LCR Type { get; private set; } = type;

        /// <summary>
        /// 方向てこ名称1
        /// </summary>
        public string Lever1Name { get; private set; } = lever1Name;

        /// <summary>
        /// 方向てこ名称2
        /// </summary>
        public string Lever2Name { get; private set; } = lever2Name;
    }
}
