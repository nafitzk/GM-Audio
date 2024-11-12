using Gma;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace GuiApplication.ViewModels
{
    public class MixViewModel : ViewModelBase
    {
        private readonly MainViewModel _mvm;

        private readonly GmaMixTrack _track;
        public GmaMixTrack Track { get { return _track; } }

        public string Name => _track.Audio.Name;

        private float _volume = 0.0f;
        public float Volume
        {
            get { return _volume; }
            set { this.RaiseAndSetIfChanged(ref _volume, value); _track.Volume = value; }
        }

        public ReactiveCommand<Unit, Unit> StopCommand { get; }

        public MixViewModel(MainViewModel mvm, GmaMixTrack track)
        {
            _mvm = mvm;
            _track = track;

            Volume = track.Volume;

            track.OnTrackRemoved += OnTrackRemoved;

            StopCommand = ReactiveCommand.Create(() =>
            {
                _mvm.StopMixTrack(this);
            });
        }

        public void OnTrackRemoved()
        {
            _mvm.RemoveMixTrack(this);
        }
    }
}
