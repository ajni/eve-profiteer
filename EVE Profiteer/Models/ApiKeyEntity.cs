using System.Collections.Generic;

namespace eZet.EveProfiteer.Models {
    public class ApiKeyEntity {

        public long Id { get; set; }
        
        public string Name { get; set; }

        public string Type { get; set; }

        public string ImagePath { get; set; }

        public bool IsActive { get; set; }

        public ICollection<ApiKey> ApiKeys { get; set; }

        public ApiKeyEntity() {
            ApiKeys = new HashSet<ApiKey>();
        }
    }
}
