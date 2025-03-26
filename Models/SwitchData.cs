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
