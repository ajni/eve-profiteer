using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace eZet.EveProfiteer.Models {
    public class ApiKeyEntity {

        private ICollection<ApiKey> apiKeys;


        [Key]
        public int Id { get; set; }

        public long EntityId { get; set; }
        
        public string Name { get; set; }

        public string Type { get; set; }

        public string ImagePath { get; set; }

        public bool IsActive { get; set; }

        public virtual ICollection<ApiKey> ApiKeys {
            get { return apiKeys; }
            set { apiKeys = value; }
        }

        public ApiKeyEntity() {
            apiKeys = new HashSet<ApiKey>();
        }
    }
}
