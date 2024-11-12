using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using GuiApplication.ViewModels;
using ReactiveUI;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace GuiApplication.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        
        //var libraryViewBox = this.Get<ListBox>("LibraryView");
        //libraryViewBox.AddHandler(DragDrop.DropEvent, LibraryDragDropHandler);
    }

    public async void LibraryDragDropHandler(object? sender, DragEventArgs e)
    {
        if (e.Data.Contains(DataFormats.Files))
        {
            var files = e.Data.GetFiles() ?? Array.Empty<IStorageItem>();
            foreach(var file in files)
            {
                Debug.Print(file.Name);
            }
        }
        Debug.Print("Guess we droppin");
    }
}
