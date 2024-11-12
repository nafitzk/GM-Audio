using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Gma;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Windows.Input;

namespace GuiApplication.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    // gma models
    private readonly GmaLibrary _library;
    private readonly GmaMix _mix = new GmaMix();

    // library
    public ObservableCollection<SceneViewModel> SceneList { get; } = new ObservableCollection<SceneViewModel>();

    public ObservableCollection<Gma.GmaPlayerType> MixPlayers { get; } = new ObservableCollection<Gma.GmaPlayerType>(Enum.GetValues<Gma.GmaPlayerType>());

    private Gma.GmaPlayerType _playerType = GmaPlayerType.Default;
    public Gma.GmaPlayerType PlayerType
    {
        get { return _playerType; }
        set { this.RaiseAndSetIfChanged(ref _playerType, value); _mix.SetPlayer(value); }
    }

    // scene tab
    private string _newSceneName = string.Empty;
    public string NewSceneName
    {
        get => _newSceneName;
        set 
        { 
            this.RaiseAndSetIfChanged(ref _newSceneName, value);
            NewSceneNameValid = !string.IsNullOrEmpty(NewSceneName);
        }
    }

    private bool _newSceneNameValid = false;
    public bool NewSceneNameValid
    {
        get => _newSceneNameValid;
        set => this.RaiseAndSetIfChanged(ref _newSceneNameValid, value);
    }

    // active scenes
    public ObservableCollection<SceneViewModel> ActiveScenes { get; } = new ObservableCollection<SceneViewModel>();

    // inspector
    private AudioViewModel _activeAudio;
    public AudioViewModel ActiveAudio { get => _activeAudio; set => this.RaiseAndSetIfChanged(ref _activeAudio, value); }

    // mix
    public ObservableCollection<MixViewModel> MixList { get; } = new ObservableCollection<MixViewModel>();

    public ReactiveCommand<Unit, Unit> ClearMixCommand { get;}

    public MainViewModel()
    {
        _library = GmaLibrary.LoadLibrary();
        _library.OnSceneCreated += OnSceneCreated;

        // load scenes
        foreach (GmaScene scene in _library.scenes)
        {
            SceneList.Add(new SceneViewModel(this, scene));
        }

        _mix.OnMixTrackAdded += OnMixTrackAdded;
        _mix.OnMixTrackRemoved += OnMixTrackRemoved;

        ClearMixCommand = ReactiveCommand.Create(() =>
        {
            _mix.Clear();

            //for(var mixItem in MixList)
            //{
            //    mixItem.StopCommand.Execute().Subscribe();
            //}
        });

        // clean up whatever needs cleaning up
        if (App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.ShutdownRequested += Shutdown;
        }

        if (Design.IsDesignMode)
        {
            SceneViewModel svm = new SceneViewModel(this, new GmaScene("test scene"));
            AudioViewModel avm = new AudioViewModel(this, new FileAudio("test path", "library audio", 0.5f, PlayMode.ONCE));

            SceneList.Add(svm);
            ActiveScenes.Add(svm);

            MixList.Add(new MixViewModel(this, new GmaMixTrack(new FileAudio("G:\\Projects\\GMAudio_OLD\\test\\test2.wav", "test audio", 1.0f, PlayMode.ONCE))));

            //FileAudio audio = new FileAudio("test path", "list name", 0.5f, Gma.PlayMode.ONCE);
            FileListAudio audio = new FileListAudio("list name", 0.5f, Gma.PlayMode.ONCE);

            //ActiveAudio = new FileAudioViewModel(this, audio);
            ActiveAudio = new FileListAudioViewModel(this, audio);
        }
    }

    public void Shutdown(object? sender, ShutdownRequestedEventArgs e)
    {
        _library.Shutdown();
        _mix.Dispose();
    }

    public void OnSceneCreated(GmaScene scene)
    {
        SceneViewModel svm = new SceneViewModel(this, scene);

        SceneList.Add(new SceneViewModel(this, scene));
        ActiveScenes.Add(svm);
    }

    public void OnMixTrackAdded(GmaMixTrack track)
    {
        MixList.Add(new MixViewModel(this, track));
    }

    public void OnMixTrackRemoved(GmaMixTrack track)
    {

    }

    public void StopMixTrack(MixViewModel mix)
    {
        _mix.Stop(mix.Track);
    }

    public void RemoveMixTrack(MixViewModel mix)
    {
        if (MixList.Remove(mix))
        {
            
        }
    }

    public void AddNewScene()
    {
        if(string.IsNullOrEmpty(_newSceneName))
        {
            return;
        }
        _library.CreateScene(_newSceneName);
        NewSceneName = string.Empty;
    }

    public void OpenScene(SceneViewModel svm)
    {
        if (!ActiveScenes.Contains(svm))
        {
            ActiveScenes.Add(svm);
        }
    }

    public void CloseScene(SceneViewModel svm)
    {
        ActiveScenes.Remove(svm);
    }

    public void SetActiveAudio(AudioViewModel audio)
    {
        ActiveAudio = audio;
    }
    public void AddAudio(GmaBaseAudio audio, GmaScene? scene)
    {   
        if(scene != null) _library.AddAudio(scene, audio);
        else _library.AddAudio(audio);
    }

    public void DeleteAudio(GmaBaseAudio audio, GmaScene scene)
    {
        _library.DeleteAudio(scene, audio);
    }

    public void SelectAudio(AudioViewModel avm)
    {
        ActiveAudio = avm;
    }

    internal void PlayAudio(AudioViewModel avm)
    {
        _mix.Play(avm.Audio);
    }

    internal void PreviewPlay(AudioViewModel audioViewModel)
    {
        _mix.PreviewPlay(audioViewModel.Audio);
    }

    internal void PreviewStop()
    {
        _mix.PreviewStop();
    }
}
