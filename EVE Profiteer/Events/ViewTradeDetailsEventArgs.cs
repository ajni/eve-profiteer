namespace eZet.EveProfiteer.Events {
    public class ViewTradeDetailsEventArgs {
        public int TypeId { get; private set; }

        public ViewTradeDetailsEventArgs(int typeId) {
            TypeId = typeId;
        }
    }
}
