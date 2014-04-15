using System.ComponentModel.DataAnnotations;

namespace eZet.EveProfiteer.Models {
    public class ItemDetail {

        [Key]
        public int Id { get; set; }

        public string Status { get; set; }

        public Item Item { get; set; }



    }
}
