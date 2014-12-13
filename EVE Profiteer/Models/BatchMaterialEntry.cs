namespace eZet.EveProfiteer.Models {
    public class BatchMaterialEntry {
        private readonly InvTypeMaterials _material;
        private readonly Asset _asset;

        public BatchMaterialEntry(InvTypeMaterials material, Asset asset) {
            _material = material;
            _asset = asset;
            init();
        }

        private void init() {
            ActualQuantity = BaseQuantity;
            TotalCost = CostPerUnit*ActualQuantity;
            CalculatedInventory = _asset.CalculatedQuantity;
            ActualInventory = _asset.InventoryQuantity;
        }

        public string TypeName { get { return _material.MaterialType.TypeName; } }

        public int BaseQuantity { get { return _material.Quantity; }}

        public int ActualQuantity { get; private set; }

        public decimal CostPerUnit { get { return _asset.LatestAverageCost; } }

        public decimal TotalCost { get; private set; }

        public int CalculatedInventory { get; private set; }
        public int ActualInventory { get; private set; }
        
    }
}