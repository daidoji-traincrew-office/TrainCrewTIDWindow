using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainCrewTIDWindow.Models {

    public class SwitchData {
        public NRC State { get; set; } = NRC.Center;
        public string Name { get; set; } = "";
    }
    public enum NRC {
        Normal,
        Reversed,
        Center
    }
}
