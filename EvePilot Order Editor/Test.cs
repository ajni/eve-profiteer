using System.IO;

namespace eZet.EvePilotOrderEditor {
    public class Test {

        public static void Main(string[] args) {

            var data = readFile("../../samples/BuyOrders.xml");

        }

        public static string readFile(string path) {
            var data = File.ReadAllText(path);
            return data;
        }

    }
}
