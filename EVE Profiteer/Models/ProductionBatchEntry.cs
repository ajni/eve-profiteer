using System;
using System.Linq;

namespace eZet.EveProfiteer.Models {
    public class ProductionBatchEntry {

        private readonly ProductionBatch _productionBatch;

        public ProductionBatchEntry(ProductionBatch productionBatch) {
            _productionBatch = productionBatch;
            init();
        }

        private void init() {
            CostPerUnit = TotalCost / ProductionQuantity;
            MaterialCost = _productionBatch.BatchMaterials.Sum(material => material.TotalCost);
            OtherCost = _productionBatch.OtherCost;
            TotalCost = MaterialCost + OtherCost;
            QuantitySold = ProductionQuantity - QuantityLeft;
            AvgSellPrice = TotalSales / QuantitySold;
            ProfitPerUnit = AvgSellPrice - CostPerUnit;
            Margin = (double)(ProfitPerUnit / AvgSellPrice);


        }


        public int BatchId { get { return _productionBatch.Id; } }

        public int TypeId { get { return _productionBatch.ProductTypeId; } }

        public string ProductName { get { return _productionBatch.InvType.TypeName; } }

        public string BlueprintName { get { return _productionBatch.InvBlueprintType.BlueprintInvType.TypeName; } }

        public DateTime Date { get { return _productionBatch.Date; } }

        public int ProductionQuantity { get { return _productionBatch.ProductionQuantity; } }

        public decimal CostPerUnit { get; set; }

        public decimal MaterialCost { get; private set; }

        public decimal OtherCost { get; private set; }

        public decimal TotalCost { get; private set; }

        public decimal TotalSales { get { return _productionBatch.TotalSales; } }

        public int QuantitySold { get; private set; }

        public int QuantityLeft { get { return _productionBatch.QuantityLeft; } }

        public decimal AvgSellPrice { get; private set; }

        public decimal ProfitPerUnit { get; private set; }

        public double Margin { get; private set; }

        public int BlueprintMe { get { return _productionBatch.BlueprintME; } }

        public int CharacterMe { get { return _productionBatch.CharacterME; } }

    }
}
