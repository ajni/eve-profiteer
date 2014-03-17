using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace eZet.EveProfiteer.Models {
    public class ApiKey {

        private ICollection<ApiKeyEntity> entities;

        [Key]
        public int Id { get; set; }

        public long ApiKeyId { get; set; }

        public string VCode { get; set; }

        public string KeyType { get; set; }

        public virtual ICollection<ApiKeyEntity> Entities {
            get { return entities; }
            set { entities = value; }
        }

        public ApiKey() {
            entities = new HashSet<ApiKeyEntity>();
        }

    }
}
