using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using DynamicData;
using Gma;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace GuiApplication.ViewModels
{
    public class FileAudioViewModel : AudioViewModel
    {
        protected string _path;
        public string Path
        {
            get => _path;
            set { this.RaiseAndSetIfChanged(ref _path, value); ((FileAudio)_audio).File = value; }
        }

        public FileAudioViewModel(MainViewModel mvm, FileAudio audio) : base(mvm, audio)
        {
            _path = audio.File;
        }
    }

    public class FileListAudioItemViewModel : ViewModelBase
    {
        private string _file = string.Empty;
        public string File
        {
            get => _file;
            set => this.RaiseAndSetIfChanged(ref _file, value);
        }

        public FileListAudioItemViewModel(string file)
        {
            File = file;
        }
    }

    public class FileListAudioViewModel : AudioViewModel
    {
        private bool _random = false;
        public bool Random
        {
            get => _random;
            set { this.RaiseAndSetIfChanged(ref _random, value); ((FileListAudio)_audio).Shuffle = value; } 
        }

        private float _intervalMin = 0.0f;
        public float IntervalMin
        {
            get => _intervalMin;
            set { this.RaiseAndSetIfChanged(ref _intervalMin, value); ((FileListAudio)_audio).IntervalMin = value; }
        }

        private float _intervalMax = 0.0f;
        public float IntervalMax
        {
            get => _intervalMax;
            set { this.RaiseAndSetIfChanged(ref _intervalMax, value); ((FileListAudio)_audio).IntervalMax = value; }
        }

        public ObservableCollection<FileListAudioItemViewModel> Files { get; } = new ObservableCollection<FileListAudioItemViewModel>();

        public ReactiveCommand<Unit, Unit> ClearCommand { get; }
        public ReactiveCommand<Unit, Unit> SortCommand { get; }
        public ReactiveCommand<FileListAudioItemViewModel, Unit> RemoveItemCommand { get; }

        public ReactiveCommand<Unit, FileListAudioViewModel> OkCommand { get; }
        public ReactiveCommand<Unit, FileListAudioViewModel> CancelCommand { get; }

        public FileListAudioViewModel(MainViewModel mvm, FileListAudio audio) : base(mvm, audio)
        {
            Random = audio.Shuffle;
            IntervalMin = audio.IntervalMin;
            IntervalMax = audio.IntervalMax;

            foreach(var file in audio.Files)
            {
                Files.Add(new FileListAudioItemViewModel(file));
            }

            if (Design.IsDesignMode)
            {
                Files.Add(new FileListAudioItemViewModel("item_1_this_one_will_be_really_long_i_think_dont_worry_about_it"));
                Files.Add(new FileListAudioItemViewModel("item 2"));
                Files.Add(new FileListAudioItemViewModel("item 3"));
            }

            SortCommand = ReactiveCommand.Create(() =>
            {
                var list = Files.ToList();
                list.Sort((a, b) => a.File.CompareTo(a.File));

                Files.Clear();
                Files.Add(list);
            });

            ClearCommand = ReactiveCommand.Create(() =>
            {
                Files.Clear();
            });

            RemoveItemCommand = ReactiveCommand.Create((FileListAudioItemViewModel item) =>
            {
                Files.Remove(item);
            });

            OkCommand = ReactiveCommand.Create(() =>
            {
                foreach(var file in Files)
                {
                    ((FileListAudio)Audio).Files.Add(file.File);
                }

                return this;
            });

            CancelCommand = ReactiveCommand.Create(() =>
            {
                Files.Clear();
                return this;
            });
        }

        public void DragDrop(object? sender, DragEventArgs e)
        {
            if (e.Data.Contains(DataFormats.Files))
            {
                var files = e.Data.GetFiles() ?? Array.Empty<IStorageItem>();
                foreach (var file in files)
                {
                    Files.Add(new FileListAudioItemViewModel(file.Path.LocalPath));
                }
            }
        }
    }

    public class AudioViewModel : ViewModelBase
    {
        protected readonly MainViewModel _mvm;

        // gma model
        protected readonly GmaBaseAudio _audio;
        public GmaBaseAudio Audio { get { return _audio; } }

        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set { this.RaiseAndSetIfChanged(ref _name, value); _audio.Name = value; }
        }

        protected float _volume = 0.0f;
        public float Volume { get => _volume; set { this.RaiseAndSetIfChanged(ref _volume, value); _audio.Volume = value; } }

        protected bool _loop = false;
        public bool Loop
        {
            get { return _loop; }
            set { this.RaiseAndSetIfChanged(ref _loop, value); _audio.Mode = value ? PlayMode.LOOP : PlayMode.ONCE; }
        }

        private float _fadeInTime = 0.0f;
        public float FadeInTime 
        {
            get => _fadeInTime;
            set { this.RaiseAndSetIfChanged(ref _fadeInTime, value); _audio.FadeInTime = value; } 
        }

        private float _fadeOutTime = 0.0f;
        public float FadeOutTime
        {
            get => _fadeOutTime;
            set { this.RaiseAndSetIfChanged(ref _fadeOutTime, value); _audio.FadeOutTime = value; }
        }

        public ReactiveCommand<Unit, Unit> PlayCommand { get; }
        public ReactiveCommand<Unit, Unit> StopCommand { get; }
        public ReactiveCommand<Unit, Unit> SelectCommand { get; }
        public ReactiveCommand<Unit, Unit> PreviewPlayCommand { get; }
        public ReactiveCommand<Unit, Unit> PreviewStopCommand { get; }

        public AudioViewModel(MainViewModel mvm, GmaBaseAudio audio)
        {
            _mvm = mvm;
            _audio = audio;

            Name = audio.Name;
            Volume = audio.Volume;
            Loop = audio.Mode == PlayMode.LOOP;
            FadeInTime = audio.FadeInTime;
            FadeOutTime = audio.FadeOutTime;

            PlayCommand = ReactiveCommand.Create(() =>
            {
                _mvm.PlayAudio(this);
            });

            StopCommand = ReactiveCommand.Create(() =>
            {

            });

            SelectCommand = ReactiveCommand.Create(() =>
            {
                _mvm.SelectAudio(this);
            });

            PreviewPlayCommand = ReactiveCommand.Create(() =>
            {
                _mvm.PreviewPlay(this);
            });

            PreviewStopCommand = ReactiveCommand.Create(() =>
            {
                _mvm.PreviewStop();
            });
        }
    }
}
