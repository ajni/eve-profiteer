namespace eZet.Eve.OrderIoHelper {
    public class Test {

        public static void Main(string[] args) {
            var buyorders = OrderIoHelper.ReadBuyOrders("../../samples");
            var sellorders = OrderIoHelper.ReadSellOrders("../../samples");
            OrderIoHelper.Write("../../samples", buyorders, sellorders);
        }



    }
}
