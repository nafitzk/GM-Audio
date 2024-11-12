using Avalonia.Controls;
using Gma;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace GuiApplication.ViewModels
{
    public class SequenceAudioViewModel : AudioViewModel
    {
        public ObservableCollection<SequenceNodeViewModel> Nodes { get; set; } = new ObservableCollection<SequenceNodeViewModel>();

        private SequenceNodeViewModel? _selectedNode;
        public SequenceNodeViewModel? SelectedNode 
        {
            get { return _selectedNode; }
            set { this.RaiseAndSetIfChanged(ref _selectedNode, value); } 
        }

        public ReactiveCommand<string, Unit> AddNodeCommand { get; }
        public ReactiveCommand<SequenceNodeViewModel, Unit> RemoveNodeCommand { get; }

        public SequenceAudioViewModel(MainViewModel mvm, SequenceAudio audio) : base(mvm, audio)
        {
            if (Design.IsDesignMode)
            {
                Nodes.Add(new SequenceNodeViewModel(_mvm, "the file", 0, "FileAudio"));
                Nodes.Add(new SequenceNodeViewModel(_mvm, "the list", 2, "FileListAudio"));

                SelectedNode = Nodes[0];
            }

            AddNodeCommand = ReactiveCommand.Create((string type) =>
            {
                SequenceNodeViewModel snvm = new SequenceNodeViewModel(_mvm, "new node", 0, type);

                Nodes.Add(snvm);
                //((SequenceAudio)Audio).Nodes.Add(snvm.Node);
            });

            RemoveNodeCommand = ReactiveCommand.Create((SequenceNodeViewModel snvm) =>
            {
                if (Nodes.Remove(snvm))
                {
                    //if (!((SequenceAudio)Audio).Nodes.Remove(snvm.Node))
                    //{
                    //    Debug.Print("uhh");
                    //}
                }
                else
                {
                    Debug.Print("uhh");
                }
            });
        }
    }

    public class SequenceNodeViewModel : ViewModelBase
    {
        private MainViewModel _mvm;

        private float _startTime = 0;
        public float StartTime { get { return _startTime; } set { this.RaiseAndSetIfChanged(ref _startTime, value); } }

        public AudioViewModel Audio { get; set; }

        // base model
        //public SequenceNode Node { get; set; }

        public SequenceNodeViewModel(MainViewModel mvm, string name, float time, string type)
        {
            _mvm = mvm;
            StartTime = time;

            if (type.Equals("FileAudio"))
            {
                Audio = new FileAudioViewModel(_mvm, new FileAudio());
                //Node = new SequenceNode(StartTime, );
            }
            else if (type.Equals("FileListAudio"))
            {
                Audio = new FileListAudioViewModel(_mvm, new FileListAudio());
                //Node = new SequenceNode(StartTime, new FileListAudio());
            }
            else
            {
                Audio = new FileAudioViewModel(_mvm, new FileAudio());
                //Node = new SequenceNode(StartTime, new FileAudio());
            }

            Audio.Name = name;
        }
    }
}
