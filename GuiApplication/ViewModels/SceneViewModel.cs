using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using Gma;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;

namespace GuiApplication.ViewModels;

public class SceneViewModel : ViewModelBase
{
    // gma model
    private readonly GmaScene _scene;

    private readonly MainViewModel _mvm;

    public string Name => _scene.name;

    public ObservableCollection<AudioViewModel> AudioList { get; } = new ObservableCollection<AudioViewModel>();

    // interactions
    public Interaction<AddAudioViewModel, List<FileAudioViewModel>?> ShowAddAudioDialog { get; } = new Interaction<AddAudioViewModel, List<FileAudioViewModel>?>();
    public Interaction<FileListAudioViewModel, FileListAudioViewModel> ShowAddListAudioDialog { get; } = new Interaction<FileListAudioViewModel, FileListAudioViewModel>();
    public Interaction<SequenceAudioViewModel, SequenceAudioViewModel> ShowAddSequenceAudioDialog { get; } = new Interaction<SequenceAudioViewModel, SequenceAudioViewModel>();

    //commands
    public ReactiveCommand<List<Uri>,Unit> AddAudioCommand { get; }
    public ReactiveCommand<Unit, Unit> AddListAudioCommand { get; }
    public ReactiveCommand<Unit, Unit> AddSequenceAudioCommand { get; }

    public ReactiveCommand<AudioViewModel, Unit> RemoveAudioCommand {  get; }

    public SceneViewModel(MainViewModel mvm, GmaScene scene)
    {
        _mvm = mvm;
        _scene = scene;

        foreach (GmaBaseAudio audio in _scene.sceneAudio)
        {
            //AudioViewModel? avm = _mvm.GetAudioViewModel(audio);
            //if(avm != null) AudioList.Add(avm);
            AudioList.Add(GetViewModelType(audio));
        }

        // commands
        AddAudioCommand = ReactiveCommand.CreateFromTask(async (List<Uri> input) =>
        {
            var aavm = new AddAudioViewModel(_mvm, input);

            var result = await ShowAddAudioDialog.Handle(aavm) ?? new List<FileAudioViewModel>();

            if (result!.Count > 0)
            {
                foreach (var item in result)
                {
                    _mvm.AddAudio(item.Audio, _scene);
                }
            }
        });

        AddListAudioCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var avm = new FileListAudioViewModel(_mvm, new FileListAudio(string.Empty, 0.5f, PlayMode.ONCE));
            var result = await ShowAddListAudioDialog.Handle(avm);

            if(result != null && result.Files.Count > 0)
            {
                _mvm.AddAudio(result.Audio, _scene);
            }
        });

        AddSequenceAudioCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var avm = new SequenceAudioViewModel(_mvm, new SequenceAudio("", 0.5f, PlayMode.ONCE));
            var result = await ShowAddSequenceAudioDialog.Handle(avm);

            if(result != null)
            {

            }
        });

        RemoveAudioCommand = ReactiveCommand.Create((AudioViewModel avm) =>
        {
            if (AudioList.Remove(avm))
            {
                _mvm.DeleteAudio(avm.Audio, _scene);
            }
        });

        // gma callbacks
        _scene.OnAudioAdded += OnAudioAdded;

        if (Design.IsDesignMode)
        {
            AudioList.Add(new AudioViewModel(_mvm, new FileAudio("scene audio path 1","scene audio 1", 0.5f, PlayMode.ONCE)));
            AudioList.Add(new AudioViewModel(_mvm, new FileAudio("scene audio path 2","scene audio 2", 0.5f, PlayMode.ONCE)));
            AudioList.Add(new AudioViewModel(_mvm, new FileAudio("scene audio path 3","scene audio 3", 0.5f, PlayMode.ONCE)));
            AudioList.Add(new AudioViewModel(_mvm, new FileAudio("scene audio path 4","scene audio 4", 0.5f, PlayMode.ONCE)));
            AudioList.Add(new AudioViewModel(_mvm, new FileAudio("scene audio path 5","scene audio 5", 0.5f, PlayMode.ONCE)));
        }
    }

    private void OnAudioAdded(GmaBaseAudio audio)
    {
        //AudioViewModel? avm = _mvm.GetAudioViewModel(audio);
        //if (avm != null) AudioList.Add(avm);
        AudioList.Add(GetViewModelType(audio));
    }

    private AudioViewModel GetViewModelType(GmaBaseAudio audio)
    {
        if(audio is FileAudio fileAudio)
        {
            return new FileAudioViewModel(_mvm, fileAudio);
        }
        else if(audio is FileListAudio fileListAudio)
        {
            return new FileListAudioViewModel(_mvm, fileListAudio);
        }

        return new AudioViewModel(_mvm, audio);
    }

    // process drag drop event
    public async void DragDrop(object? sender, DragEventArgs e)
    {
        if (e.Data.Contains(DataFormats.Files))
        {
            var files = e.Data.GetFiles() ?? Array.Empty<IStorageItem>();
            List<Uri> fileUris = new List<Uri>();
            foreach (var file in files)
            {
                fileUris.Add(file.Path);
            }

            if (fileUris.Count > 0)
            {
                await AddAudioCommand.Execute(fileUris);
            }
        }
    }
}
