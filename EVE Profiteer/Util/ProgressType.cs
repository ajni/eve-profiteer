namespace eZet.EveProfiteer.Util {
    public struct ProgressType {
        public ProgressType(int percent, string status) : this() {
            Percent = percent;
            Status = status;
        }

        public int Percent { get; private set; }

        public string Status { get; private set; }
    }
}