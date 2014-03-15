using eZet.EveLib.EveOnline;

namespace eZet.EveProfiteer.Models {
    public class ApiKeyEntity {

        public long Id { get; private set; }
        
        public string Name { get; private set; }

        public string Type { get; private set; }

        public string ImagePath { get; private set; }

        public bool IsActive { get; set; }

        public long ApiKeyId { get; set; }

        public ApiKeyEntity(string name, long id, string type, string imagepath) {
            Name = name;
            Id = id;
            Type = type;
            ImagePath = imagepath;
        }


    }
}
