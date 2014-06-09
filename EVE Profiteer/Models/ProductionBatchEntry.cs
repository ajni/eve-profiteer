using System;

namespace eZet.EveProfiteer.Models {
    public class ProductionBatchEntry {

        public int BatchId { get; private set; }

        public int TypeId { get; private set; }

        public string TypeName { get; private set; }

        public DateTime Date { get; private set; }

        public int Quantity { get; private set; }

        public decimal CostPerUnit { get; set; }

        public decimal TotalCost { get; set; }

        public int QuantitySold { get; private set; }

        public decimal AvgSellPrice { get; private set; }

        public decimal ProfitPerUnit { get; private set; }

        public double Margin { get; private set; }

        public int BlueprintMe { get; private set; }
}
}
