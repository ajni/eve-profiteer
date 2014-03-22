using Caliburn.Micro;
using eZet.Eve.EveProfiteer.Entities;
using System;
using System.Threading;

namespace eZet.Eve.EveProfiteer.ViewModels {
    public class AnalyzerProgressViewModel : PropertyChangedBase {

        public CancellationTokenSource CancelSource { get; private set; }

        private string _status;

        public string Status {
            get { return _status; }
            set { _status = value; NotifyOfPropertyChange(() => Status); }
        }

        private int _percent;

        public int Percent {
            get { return _percent; }
            set { _percent = value; NotifyOfPropertyChange(() => Percent); }
        }

        private ProgressType _progress;

        public ProgressType Progress {
            get {
                return _progress;
            }
            set {
                _progress = value;
                Percent = value.Percent;
                Status += value.Status + "\n";
            }
        }

        public AnalyzerProgressViewModel(CancellationTokenSource source) {
            CancelSource = source;
        }

        public IProgress<ProgressType> GetProgressReporter() {
            return new Progress<ProgressType>(t => Progress = t);
        }

    }
}
