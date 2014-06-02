using System;
using System.Threading;
using Caliburn.Micro;
using eZet.Eve.EveProfiteer.Entities;

namespace eZet.EveProfiteer.ViewModels.Dialogs {
    public class AnalyzerProgressViewModel : PropertyChangedBase {
        private int _percent;
        private ProgressType _progress;
        private string _status;

        public AnalyzerProgressViewModel(CancellationTokenSource source) {
            CancelSource = source;
        }

        public CancellationTokenSource CancelSource { get; private set; }

        public string Status {
            get { return _status; }
            set {
                _status = value;
                NotifyOfPropertyChange(() => Status);
            }
        }

        public int Percent {
            get { return _percent; }
            set {
                _percent = value;
                NotifyOfPropertyChange(() => Percent);
            }
        }

        public ProgressType Progress {
            get { return _progress; }
            set {
                _progress = value;
                Percent = value.Percent;
                Status += value.Status + "\n";
            }
        }

        public IProgress<ProgressType> GetProgressReporter() {
            return new Progress<ProgressType>(t => Progress = t);
        }
    }
}