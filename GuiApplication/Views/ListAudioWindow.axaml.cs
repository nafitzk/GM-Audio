using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Gma;
using GuiApplication.ViewModels;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Threading.Tasks;

namespace GuiApplication;

public partial class ListAudioWindow : ReactiveWindow<FileListAudioViewModel>
{
    public ListAudioWindow()
    {
        InitializeComponent();

        if (Design.IsDesignMode)
        {
            FileListAudio audio = new Gma.FileListAudio("list name", 0.5f, Gma.PlayMode.ONCE);

            ViewModel = new FileListAudioViewModel(null, audio);
            return;
        }
        this.WhenActivated(action => action(ViewModel!.OkCommand.Subscribe(Close)));
        this.WhenActivated(action => action(ViewModel!.CancelCommand.Subscribe(Close)));

        AddHandler(DragDrop.DropEvent, GeneralDragDropHandler);
    }

    // audio has been dragged onto the open scene
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public async void GeneralDragDropHandler(object? sender, DragEventArgs e)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    {
        ViewModel!.DragDrop(sender, e);
    }
}