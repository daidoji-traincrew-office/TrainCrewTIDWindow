using TrainCrewTIDWindow.Models;

namespace TrainCrewTIDWindow.Settings {
    public class SignalSwitchSetting(string switchName, NRC state) {
        public string SwitchName { get; } = switchName;
        public NRC State { get; } = state;
    }
}
