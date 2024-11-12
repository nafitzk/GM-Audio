using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using GuiApplication.ViewModels;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.ReactiveUI;
using Avalonia.Controls.ApplicationLifetimes;
using GuiApplication.Views;
using System.Reactive;

namespace GuiApplication;

public partial class SceneView : ReactiveUserControl<SceneViewModel>
{
    public SceneView()
    {
        //InitializeComponent();
        AvaloniaXamlLoader.Load(this);

        if (Design.IsDesignMode) ViewModel = new SceneViewModel(null, new Gma.GmaScene("Test Scene"));

        AddHandler(DragDrop.DropEvent, GeneralDragDropHandler);

        this.WhenActivated(d => d(ViewModel!.ShowAddAudioDialog.RegisterHandler(DoShowAddAudioDialogAsync)));
        this.WhenActivated(d => d(ViewModel!.ShowAddListAudioDialog.RegisterHandler(DoShowAddListAudioDialogAsync)));
        this.WhenActivated(d => d(ViewModel!.ShowAddSequenceAudioDialog.RegisterHandler(DoShowAddSequenceAudioDialog)));
    }

    // audio has been dragged onto the open scene
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public async void GeneralDragDropHandler(object? sender, DragEventArgs e)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    {
        ViewModel!.DragDrop(sender, e);
    }

    public async Task DoShowAddAudioDialogAsync(InteractionContext<AddAudioViewModel, List<FileAudioViewModel>?> interaction)
    {
        var dialog = new AudioWindow();
        dialog.DataContext = interaction.Input;

        if(App.Current!.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime app)
        {
            var result = await dialog.ShowDialog<List<FileAudioViewModel>>(app.MainWindow!);
            interaction.SetOutput(result);
        }
    }

    public async Task DoShowAddListAudioDialogAsync(InteractionContext<FileListAudioViewModel, FileListAudioViewModel> interaction)
    {
        var dialog = new ListAudioWindow();
        dialog.DataContext = interaction.Input;

        if (App.Current!.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime app)
        {
            var result = await dialog.ShowDialog<FileListAudioViewModel>(app.MainWindow!);
            interaction.SetOutput(result);
        }
    }

    public async Task DoShowAddSequenceAudioDialog(InteractionContext<SequenceAudioViewModel, SequenceAudioViewModel> interaction)
    {
        var dialog = new SequenceBuilderWindow();
        dialog.DataContext = interaction.Input;

        if (App.Current!.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime app)
        {
            var result = await dialog.ShowDialog<SequenceAudioViewModel>(app.MainWindow!);
            interaction.SetOutput(result);
        }
    }
}