using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace eZet.EveProfiteer.Models {
    public sealed class ApiKey {

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long ApiKeyId { get; set; }

        public string VCode { get; set; }

        public string KeyType { get; set; }

        public ICollection<ApiKeyEntity> Entities { get; set; }

        public ApiKey() {
            Entities = new HashSet<ApiKeyEntity>();
        }

    }
}
