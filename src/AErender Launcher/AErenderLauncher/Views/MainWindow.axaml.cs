using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using AErenderLauncher.Classes;
using AErenderLauncher.Classes.Project;
using AErenderLauncher.Classes.Rendering;
using AErenderLauncher.Controls;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;

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

        public static ObservableCollection<RenderThread> Threads { get; set; } = new ObservableCollection<RenderThread>();

        public MainWindow() {
            InitializeComponent();

            ExtendClientAreaToDecorationsHint = Helpers.Platform != OperatingSystemType.OSX;
            Root.RowDefinitions = Helpers.Platform == OperatingSystemType.OSX ? new RowDefinitions("0,32,*,48") : new RowDefinitions("32,32,*,48");
            // Threads.Add(new RenderThread("", "") {
            //     CompositionName = "Comp 1",
            //     CurrentFrame = 0, EndFrame = 200,
            // });
            // Threads.Add(new RenderThread("", "") {
            //     CompositionName = "Comp 1",
            //     CurrentFrame = 100, EndFrame = 200,
            // });
            // Threads.Add(new RenderThread("", "") {
            //     CompositionName = "Comp 1",
            //     CurrentFrame = 50, EndFrame = 200,
            // });
        }

        private async void Button_OnClick(object sender, RoutedEventArgs e) {
            // await MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams() {
            //     SystemDecorations = SystemDecorations.BorderOnly,
            //     
            //     ContentTitle = "Header",
            //     ContentMessage = "Message",
            //     ShowInCenter = true,
            //     ButtonDefinitions = ButtonEnum.Ok,
            //     CanResize = true
            // }).ShowDialog(this);
            TaskEditorPopup editor = new TaskEditorPopup(_task);
            await editor.ShowDialog(this);

        }

        private async void Launch_OnClick(object sender, RoutedEventArgs e) {
            ProjectItem[]? project = await ProjectParser.ParseProject("/Users/lilystilson/Yandex.Disk.localized/Development/Delphi/AErender Launcher/benchmark/Deneb - Mograph Icons/Mograph Icons.aep");

            if (project != null) {
                foreach (ProjectItem item in project) {
                    Debug.WriteLine($"Name = {item.Name}");
                }
            }
        }

        private async void SettingsButton_OnClick(object sender, RoutedEventArgs e) {
            SettingsWindow settings = new SettingsWindow();
            await settings.ShowDialog(this);
        }
    }
}