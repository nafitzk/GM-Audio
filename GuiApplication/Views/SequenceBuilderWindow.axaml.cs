using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Gma;
using GuiApplication.ViewModels;

namespace GuiApplication;

public partial class SequenceBuilderWindow : ReactiveWindow<SequenceAudioViewModel>
{
    public SequenceBuilderWindow()
    {
        InitializeComponent();

        if(Design.IsDesignMode)
        {
            ViewModel = new SequenceAudioViewModel(null, new SequenceAudio("sequence audio", 0.5f, PlayMode.ONCE));
        }
    }
}