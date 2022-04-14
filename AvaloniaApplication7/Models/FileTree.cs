using Avalonia.Threading;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaApplication7.Models
{
    public class FileTree : ReactiveObject
    {
        private string _path;
        private string _name;
        private FileSystemWatcher? _watcher;
        private ObservableCollection<FileTree>? _children;
        private bool _isExpanded;
        public bool IsChecked { get; set; }
        public FileTree(string path,
            bool isRoot = false)
        {
            _path = path;
            _name = isRoot ? path : System.IO.Path.GetFileName(Path);
            _isExpanded = isRoot;
        }
        public string Path
        {
            get => _path;
            private set => this.RaiseAndSetIfChanged(ref _path, value);
        }
        public string Name
        {
            get => _name;
            private set => this.RaiseAndSetIfChanged(ref _name, value);
        }
        public bool IsExpanded
        {
            get => _isExpanded;
            set => this.RaiseAndSetIfChanged(ref _isExpanded, value);
        }
        public IReadOnlyList<FileTree> Children => _children ??= LoadChildren();

        private ObservableCollection<FileTree> LoadChildren()
        {
            var options = new EnumerationOptions { IgnoreInaccessible = true };
            var result = new ObservableCollection<FileTree>();

            foreach (var d in Directory.EnumerateDirectories(Path, "*", options))
            {
                result.Add(new FileTree(d, true));
            }

            foreach (var f in Directory.EnumerateFiles(Path, "*.jpg", options))
            {
                result.Add(new FileTree(f, false));
            }

            _watcher = new FileSystemWatcher
            {
                Path = Path,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.Size | NotifyFilters.LastWrite,
            };
            _watcher.EnableRaisingEvents = true;

            return result;
        }

    }
}