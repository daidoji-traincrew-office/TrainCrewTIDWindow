
namespace TrainCrewTIDWindow.Models {
    public class TrainStateData {
        public long Id { get; set; }
        public string TrainNumber { get; set; } = string.Empty;
        public int DiaNumber { get; set; }
        public string FromStationId { get; set; } = string.Empty;
        public string ToStationId { get; set; } = string.Empty;
        public int Delay { get; set; }
        public ulong? DriverId { get; set; }
    }
}
