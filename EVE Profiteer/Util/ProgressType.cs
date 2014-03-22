
namespace eZet.Eve.EveProfiteer.Entities {
    public struct ProgressType {
        public int Percent { get; private set; }

        public string Status { get; private set; }

        public ProgressType(int percent, string status) : this() {
            Percent = percent;
            Status = status;
        }
    }
}
