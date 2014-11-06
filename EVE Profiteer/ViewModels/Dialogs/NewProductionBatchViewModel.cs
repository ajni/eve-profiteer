using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using Caliburn.Micro;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Services;
using eZet.EveProfiteer.Util;
using Microsoft.Practices.EnterpriseLibrary.Validation;

namespace eZet.EveProfiteer.ViewModels.Dialogs {
    public class NewProductionBatchViewModel : Screen, IDataErrorInfo {
        private readonly Services.EveProfiteerRepository _eveProfiteerRepository;
        private InvBlueprintType _blueprint;
        private ICollection<BatchMaterialEntry> _materials;
        private int _units;
        private int _runs;
        private int _blueprintMe;
        private readonly DataErrorInfoHelper _dataErrorInfoHelper;

        public NewProductionBatchViewModel(Services.EveProfiteerRepository eveProfiteerRepository) {
            _eveProfiteerRepository = eveProfiteerRepository;
            DisplayName = "New production batch";
            ProductionDate = DateTime.UtcNow;
            Blueprints = _eveProfiteerRepository.GetBlueprints().AsNoTracking().ToList();
            PropertyChanged += OnPropertyChanged;
            Blueprint = Blueprints.First();
            _dataErrorInfoHelper = new DataErrorInfoHelper(this);
            Runs = 1;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs args) {
            if (args.PropertyName == "Blueprint") {
                Runs = 1;
                updateMaterials();
            }
            if (args.PropertyName == "Runs") {
                Units = (int)(Runs * Blueprint.ProductInvType.PortionSize);
            }
        }

        public ICollection<InvBlueprintType> Blueprints { get; private set; }

        [Required]
        public InvBlueprintType Blueprint {
            get { return _blueprint; }
            set {
                if (Equals(value, _blueprint)) return;
                _blueprint = value;
                NotifyOfPropertyChange();
            }
        }

        [Required]
        [Range(0, int.MaxValue)]
        public int BlueprintMe {
            get { return _blueprintMe; }
            set {
                if (value == _blueprintMe) return;
                _blueprintMe = value;
                NotifyOfPropertyChange();
            }
        }

        [Required]
        public int CharacterPe { get; set; }

        [Required]
        public DateTime ProductionDate { get; set; }

        [Range(1, int.MaxValue)]
        public int Runs {
            get { return _runs; }
            set {
                if (value == _runs) return;
                _runs = value;
                NotifyOfPropertyChange(() => Runs);
            }
        }

        public ICollection<BatchMaterialEntry> Materials {
            get { return _materials; }
            set {
                if (Equals(value, _materials)) return;
                _materials = value;
                NotifyOfPropertyChange();
            }
        }

        public int Units {
            get { return _units; }
            set {
                if (value == _units) return;
                _units = value;
                NotifyOfPropertyChange();
            }
        }


        public decimal OtherCost { get; set; }

        public decimal TotalCost { get; set; }

        public bool UserDefinedMaterialCost { get; set; }

        public void updateMaterials() {
            var list = new List<BatchMaterialEntry>();
            foreach (var material in Blueprint.ProductInvType.InvTypeMaterials) {
                var asset = material.MaterialType.Assets.SingleOrDefault(f => f.ApiKeyEntity_Id == ApplicationHelper.ActiveEntity.Id);
                if (asset == null) asset = new Asset();
                list.Add(new BatchMaterialEntry(material, asset));
            }
            Materials = list;
        }


        string IDataErrorInfo.Error {
            get { return _dataErrorInfoHelper.Error; }
        }

        string IDataErrorInfo.this[string columnName] {
            get { return new DataErrorInfoHelper(this)[columnName]; }
        }
    }
}
