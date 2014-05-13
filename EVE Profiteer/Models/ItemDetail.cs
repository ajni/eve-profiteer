using System.ComponentModel.DataAnnotations;
using eZet.EveOnlineDbModels;

namespace eZet.EveProfiteer.Models {
    public class ItemDetail {

        [Key]
        public int Id { get; set; }

        public string Status { get; set; }

        public InvType InvType { get; set; }



    }
}
