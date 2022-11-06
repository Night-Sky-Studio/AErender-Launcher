using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AErenderLauncher.Classes;
using AErenderLauncher.Classes.Project;
using AErenderLauncher.Classes.Rendering;
using AErenderLauncher.Classes.System;
using AErenderLauncher.Controls;
using AErenderLauncher.Views.Dialogs;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;
using Microsoft.CodeAnalysis.Diagnostics;
using static AErenderLauncher.App;

namespace AErenderLauncher.Views {
    public partial class MainWindow : Window {
        private RenderTask _task { get; set; } = new RenderTask {
            Project = "C:\\YandexDisk\\Acer\\Footages (AE)\\AErender Launcher Benchmark Projects\\Deneb - Mograph Icons\\Mograph Icons.aep",
            Output = "C:\\Users\\lunam\\Desktop\\[projectName]\\[compName].[fileExtension]",
            OutputModule = "Lossless",
            RenderSettings = "Best Settings",
            MissingFiles = true,
            Sound = true,
            Multiprocessing = false,
            CacheLimit = 100,
            MemoryLimit = 5,
            Compositions = new List<Composition>() {
                new Composition("Game Icons", new FrameSpan(0, 599), 1),
                new Composition("Web Icons", new FrameSpan(0, 599), 1),
                new Composition("Ecology Icons", new FrameSpan(0, 599), 1),
                new Composition("Medical Icons", new FrameSpan(0, 599), 1),
            }
        };

        public static ObservableCollection<RenderTask> Tasks { get; set; } = new ObservableCollection<RenderTask>();

        public static ObservableCollection<RenderThread> Threads { get; set; } = new ObservableCollection<RenderThread>();

        public MainWindow() {
            InitializeComponent();

            ExtendClientAreaToDecorationsHint = Helpers.Platform != OperatingSystemType.OSX;
            Root.RowDefinitions = Helpers.Platform == OperatingSystemType.OSX ? new RowDefinitions("0,32,*,48") : new RowDefinitions("32,32,*,48");
        }

        private async void NewTaskButton_OnClick(object sender, RoutedEventArgs e) {
            // open an aep
            OpenFileDialog dialog = new() {
                AllowMultiple = false,
                Directory = ApplicationSettings.DefaultProjectsPath,
                Filters = new() {
                    new() { Name = "After Effects project", Extensions = { "aep" } }
                },
                Title = "Open After Effects project"
            };
            
            string[]? result = await dialog.ShowAsync(this);

            if (result != null && result.Length > 0) {
                // parse dat
                
                if (await ParseProject(result[0]) is { } task) {
                    // add to tasks
                    Tasks.Add(task);
                }
            }
        }

        private async Task<RenderTask?> ParseProject(string ProjectPath) {
            if (AeProjectParser.CheckExists()) {
                ProjectItem[]? project = await ProjectParser!.ParseProject(ProjectPath);

                if (project != null) {
                    ApplicationSettings.LastProjectPath = ProjectPath;
                
                    ProjectImportDialog dialog = new(project);
                    RenderTask? task = await dialog.ShowDialog<RenderTask?>(this);

                    return task;
                }
            } else {
                await this.ShowGenericDialogAsync(new() {
                    Title = "Error",
                    Body = "Project parser somehow disappeared from AErender Launcher's folder.\nPlease, visit link bellow to fix this issue.",
                    Link = "https://aerenderlauncher.com/docs/Parser#ParserDisappeared",
                    CancelText = "Close"
                });
            }

            return null;
        }

        private async void Launch_OnClick(object sender, RoutedEventArgs e) {
            //
        }

        private async void SettingsButton_OnClick(object sender, RoutedEventArgs e) {
            SettingsWindow settings = new();
            await settings.ShowDialog(this);
        }

        private async void NewTaskEmpty_OnClick(object? sender, RoutedEventArgs e) {
            TaskEditorPopup editor = new();
            await editor.ShowDialog(this);
        }
    }
}