using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using DynamicData;
using Gma;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace GuiApplication.ViewModels
{
    public class AddAudioViewModel : ViewModelBase
    {
        protected readonly MainViewModel _mvm;

        public ObservableCollection<FileAudioViewModel> AudioList { get; } = new ObservableCollection<FileAudioViewModel>();

        public ReactiveCommand<Unit, List<FileAudioViewModel>> OkCommand {get;}
        public ReactiveCommand<Unit, List<FileAudioViewModel>> CancelCommand {get;}

        public AddAudioViewModel(MainViewModel mvm, List<Uri> fileList)
        {
            _mvm = mvm;

            foreach(var file in fileList)
            {
                FileAudio audio = new FileAudio(file.LocalPath, System.IO.Path.GetFileName(file.LocalPath), 0.5f, PlayMode.ONCE);

                AudioList.Add(new FileAudioViewModel(mvm, audio));
            }

            OkCommand = ReactiveCommand.Create(() =>
            {
                return AudioList.ToList();
            });
            CancelCommand = ReactiveCommand.Create(() =>
            {
                AudioList.Clear();
                return AudioList.ToList();
            });
        }
    }
}
