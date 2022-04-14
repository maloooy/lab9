using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using AvaloniaApplication7.Models;
using ReactiveUI;
using Avalonia.Data.Converters;
using System.Globalization;

namespace AvaloniaApplication7.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private FileTree? _root;
        private string _selectedDrive;
        private string? _selectedPath;
        Bitmap? _viewableImage = null;
        bool _areButtonsActive = false;

        bool AreButtonsActive
        {
            get => _areButtonsActive;
            set => this.RaiseAndSetIfChanged(ref _areButtonsActive, value);
        }

        public MainWindowViewModel()
        {
            var assetLoader = AvaloniaLocator.Current.GetService<IAssetLoader>();

            Drives = DriveInfo.GetDrives().Select(x => x.Name).ToList();
            _selectedDrive = "C:\\";
            Source = new HierarchicalTreeDataGridSource<FileTree>(Array.Empty<FileTree>())
            {
                Columns =
                {
                    new HierarchicalExpanderColumn<FileTree>(
                        new TemplateColumn<FileTree>(
                            "Èìÿ",
                            new FuncDataTemplate<FileTree>(FileNameTemplate, true),
                            new GridLength(1, GridUnitType.Star)
                            ),
                        x => x.Children,
                        x => x.IsExpanded)
                }
            };

            Source.RowSelection!.SingleSelect = false;
            Source.RowSelection.SelectionChanged += SelectionChanged;

            this.WhenAnyValue(x => x.SelectedDrive)
                .Subscribe(x =>
                {
                    _root = new FileTree(_selectedDrive, isRoot: true);
                    Source.Items = new[] { _root };
                });
        }

        public IList<string> Drives { get; }

        public string SelectedDrive
        {
            get => _selectedDrive;
            set => this.RaiseAndSetIfChanged(ref _selectedDrive, value);
        }

        public string? SelectedPath
        {
            get => _selectedPath;
            set => SetSelectedPath(value);
        }

        public Bitmap? ViewableImage
        {
            get => _viewableImage;
            set => this.RaiseAndSetIfChanged(ref _viewableImage, value);
        }

        public HierarchicalTreeDataGridSource<FileTree> Source { get; }

        private IControl FileNameTemplate(FileTree node, INameScope ns)
        {
            return new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                Children =
                {
                    new Image
                    {
                        [!Image.SourceProperty] = new MultiBinding
                        {
                            Bindings =
                            {
                                new Binding(nameof(node.IsExpanded)),
                            }
                        },
                        Margin = new Thickness(0, 0, 4, 0),
                        VerticalAlignment = VerticalAlignment.Center,
                    },
                    new TextBlock
                    {
                        [!TextBlock.TextProperty] = new Binding(nameof(FileTree.Name)),
                        VerticalAlignment = VerticalAlignment.Center,
                    }
                }
            };
        }

        private void SetSelectedPath(string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                Source.RowSelection!.Clear();
                return;
            }

            var path = value;
            var components = new Stack<string>();
            DirectoryInfo? d = null;

            if (File.Exists(path))
            {
                var f = new FileInfo(path);
                components.Push(f.Name);
                d = f.Directory;
            }
            else if (Directory.Exists(path))
            {
                d = new DirectoryInfo(path);
            }

            while (d is not null)
            {
                components.Push(d.Name);
                d = d.Parent;
            }

            var index = IndexPath.Unselected;

            if (components.Count > 0)
            {
                var drive = components.Pop();
                var driveIndex = Drives.FindIndex(x => string.Equals(x, drive, StringComparison.OrdinalIgnoreCase));

                if (driveIndex >= 0)
                    SelectedDrive = Drives[driveIndex];

                FileTree? node = _root;
                index = new IndexPath(0);

                while (node is not null && components.Count > 0)
                {
                    node.IsExpanded = true;

                    var component = components.Pop();
                    var i = node.Children.FindIndex(x => string.Equals(x.Name, component, StringComparison.OrdinalIgnoreCase));
                    node = i >= 0 ? node.Children[i] : null;
                    index = i >= 0 ? index.Append(i) : default;

                }
            }
            Source.RowSelection!.SelectedIndex = index;
        }
        private void SelectionChanged(object? sender, TreeSelectionModelSelectionChangedEventArgs<FileTree> e)
        {
            var selectedPath = Source.RowSelection?.SelectedItem?.Path;


            this.RaiseAndSetIfChanged(ref _selectedPath, selectedPath, nameof(SelectedPath));

            foreach (var i in e.DeselectedItems)
                System.Diagnostics.Trace.WriteLine($"Deselected '{i?.Path}'");
            foreach (var i in e.SelectedItems)
                System.Diagnostics.Trace.WriteLine($"Selected '{i?.Path}'");

            try
            {
                FileInfo fileInfo = new FileInfo(selectedPath);
                using (FileStream fs = fileInfo.OpenRead())
                {
                    try
                    {
                        this.ViewableImage = Bitmap.DecodeToWidth(fs, 500);
                        string mypath = selectedPath.Substring(0, selectedPath.LastIndexOf('\\'));
                        string[] files = Directory.GetFiles(mypath);
                        var jpgs = Array.FindAll(files, (fileName) => fileName.Contains(".jpg"));
                        if (jpgs.Length > 1)
                        {
                            AreButtonsActive = true;
                        }


                    }
                    catch (Exception ex)
                    {
                        this.ViewableImage = null;
                        AreButtonsActive = false;
                    }
                }
            }
            catch
            {
                this.ViewableImage = null;
                AreButtonsActive = false;
            }

        }

    }
}