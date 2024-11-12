using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using GuiApplication.ViewModels;
using ReactiveUI;
using System;

namespace GuiApplication;

public partial class AudioWindow : ReactiveWindow<AddAudioViewModel>
{
    public AudioWindow()
    {
        InitializeComponent();

        if (Design.IsDesignMode) return;

        this.WhenActivated(action => action(ViewModel!.OkCommand.Subscribe(Close)));
        this.WhenActivated(action => action(ViewModel!.CancelCommand.Subscribe(Close)));
    }
}