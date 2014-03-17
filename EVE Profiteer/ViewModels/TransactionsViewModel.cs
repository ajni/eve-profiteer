using Caliburn.Micro;
using eZet.EveLib.EveOnline.Model.Character;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Services;

namespace eZet.EveProfiteer.ViewModels {
    public class TransactionsViewModel : Screen {

        private BindableCollection<WalletTransactions.Transaction> transactions;

        public ApiKey ApiKey { get; set; }

        public ApiKeyEntity Entity { get; set; }


        public BindableCollection<WalletTransactions.Transaction> Transactions {
            get { return transactions; }
            set { transactions = value; NotifyOfPropertyChange(() => Transactions); }
        }

        private readonly EveApiService eveApiService;

        public TransactionsViewModel(EveApiService eveApiService) {
            this.eveApiService = eveApiService;
        }

        public void Initialize(ApiKey key, ApiKeyEntity entity) {
            Transactions = new BindableCollection<WalletTransactions.Transaction>(eveApiService.GetAllTransactions(ApiKey, Entity));
        }




    }
}
