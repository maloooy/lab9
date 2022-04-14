using Avalonia.Controls;
using System;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using AvaloniaApplication7.ViewModels;

namespace AvaloniaApplication7.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private void SelectedPath_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var vm = (MainWindowViewModel)DataContext!;
                vm.SelectedPath = ((TextBox)sender!).Text;

            }
        }
        private void NextItem(object? sender, RoutedEventArgs e)
        {
            var keyboard = KeyboardDevice.Instance;
            var inputManager = InputManager.Instance;
            this.FindControl<TreeDataGrid>("fileViewer").Focus();
            inputManager.ProcessInput(new RawKeyEventArgs(keyboard, (ulong)DateTime.Now.Ticks, this, RawKeyEventType.KeyDown, Key.Down, RawInputModifiers.None));
        }

        private void PredItem(object? sender, RoutedEventArgs e)
        {
            var keyboard = KeyboardDevice.Instance;
            var inputManager = InputManager.Instance;
            this.FindControl<TreeDataGrid>("fileViewer").Focus();
            inputManager.ProcessInput(new RawKeyEventArgs(keyboard, (ulong)DateTime.Now.Ticks, this, RawKeyEventType.KeyDown, Key.Up, RawInputModifiers.None));
        }

    }
}