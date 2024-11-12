using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Gma;
using GuiApplication.ViewModels;

namespace GuiApplication;

public partial class AudioView : ReactiveUserControl<AudioViewModel>
{
    public AudioView()
    {
        InitializeComponent();

        if (Design.IsDesignMode)
        {
            ViewModel = new AudioViewModel(null, new FileAudio("test path", "test audio", 0.5f, PlayMode.ONCE));
        }
    }
}