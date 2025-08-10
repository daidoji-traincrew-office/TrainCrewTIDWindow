using System.Collections.ObjectModel;

namespace TrainCrewTIDWindow.Settings {
    public class TrackConnectionSetting(string track, List<TargetTrack> targets) {

        public TrackConnectionSetting(string track, string target, List<IfSwitch> switches): this(track, [new(target, switches)]) { }

        public TrackConnectionSetting(string track, string target, string switchName, bool reversed) : this(track, target, [new(switchName, reversed)]) { }
        public TrackConnectionSetting(string track, string target) : this(track, [new(target)]) { }

        public TrackConnectionSetting(string track) : this(track, []) { }

        public string Track { get; private set; } = track;

        private readonly List<TargetTrack> targetTracks = targets;

        public ReadOnlyCollection<TargetTrack> TargetTracks => targetTracks.AsReadOnly();

        public bool Enable => targetTracks.Count > 0;

        public void AddTarget(string target, List<IfSwitch> switches) {
            targetTracks.Add(new(target, switches));
        }

        public void AddTarget(string target) {
            AddTarget(target, []);
        }

        public bool ContainsTrack(string t) {
            return Track == t || targetTracks.Any(tt => tt.Track == t);
        }
    }

    public class TargetTrack(string track, List<IfSwitch> switches) {

        public TargetTrack(string track) : this(track, []) { }

        public string Track { get; private set; } = track;

        private readonly List<IfSwitch> switches = switches;

        public ReadOnlyCollection<IfSwitch> Switches => switches.AsReadOnly();

        public void AddSwitch(string switchName, bool reversed) {
            switches.Add(new(switchName, reversed));
        }
    }

    public class IfSwitch (string switchName, bool reversed) {
        public string SwitchName { get; private set; } = switchName;

        /// <summary>
        /// 転轍器の状態（反位であるか）
        /// </summary>
        public bool Reversed { get; private set; } = reversed;
    }
}
