// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using Web.Data;

namespace SubtitleDubberApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddWpfBlazorWebView();
            Resources.Add("services", serviceCollection.BuildServiceProvider());
            State.AppStateChanged += State_AppStateChanged;
        }

        private void State_AppStateChanged(object sender, EventArgs e)
        {
            StateType appState = (StateType)sender;
            switch (appState)
            {
                case StateType.SelectInputVideoFile:
                    GetInputVideoFileName();
                    break;
                case StateType.SelectOutputVideoFile:
                    GetOutputVideoFileName();
                    break;
                case StateType.SelectInputSubtitleFile:
                    GetInputSubtitleFileName();
                    break;
                case StateType.SelectOutputSubtitleFile:
                    GetOutputSubtitleFileName();
                    break;
                default:
                    break;
            }
        }

        private void GetInputVideoFileName()
        {
                var openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                State.InputVideoFileName = openFileDialog.FileName;
                }
        }

        private void GetOutputVideoFileName()
        {
            var saveFileDialog = new SaveFileDialog();
            if (saveFileDialog.ShowDialog() == true)
            {
                State.OutputVideoFileName = saveFileDialog.FileName;
            }
        }

        private void GetInputSubtitleFileName()
        {
            var openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                State.InputSubtitleFileName = openFileDialog.FileName;
            }
        }

        private void GetOutputSubtitleFileName()
        {
            var saveFileDialog = new SaveFileDialog();
            if (saveFileDialog.ShowDialog() == true)
            {
                State.OutputSubtitleFileName = saveFileDialog.FileName;
                State.AppState = StateType.SelectedOutputSubtitleFile;
            }
                    }


    }

    // Workaround for compiler error "error MC3050: Cannot find the type 'local:Main'"
    // It seems that, although WPF's design-time build can see Razor components, its runtime build cannot.
    public partial class Main { }

    // Helpful guide on WCF: https://docs.microsoft.com/en-us/aspnet/core/blazor/hybrid/tutorials/wpf?view=aspnetcore-6.0
}
