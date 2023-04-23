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
            int appState = (int)sender;
            switch (appState)
            {
                case 1:
                    GetInputVideoFileName();
                    break;
                case 2:
                    GetOutputVideoFileName();
                    break;
                case 3:
                    GetInputSubtitleFileName();
                    break;
                case 4:
                    GetOutputSubtitleFileName();
                    break;
                default:
                    break;
            }
        }

        private void GetInputVideoFileName()
        {
                OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                State.InputVideoFileName = openFileDialog.FileName;
                }
        }

        private void GetOutputVideoFileName()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            if (saveFileDialog.ShowDialog() == true)
            {
                State.OutputVideoFileName = saveFileDialog.FileName;
            }
        }

        private void GetInputSubtitleFileName()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                State.InputSubtitleFileName = openFileDialog.FileName;
            }
        }

        private void GetOutputSubtitleFileName()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            if (saveFileDialog.ShowDialog() == true)
            {
                State.OutputSubtitleFileName = saveFileDialog.FileName;
                State.AppState = 5;
            }
                    }


    }

    // Workaround for compiler error "error MC3050: Cannot find the type 'local:Main'"
    // It seems that, although WPF's design-time build can see Razor components, its runtime build cannot.
    public partial class Main { }

    // Helpful guide on WCF: https://docs.microsoft.com/en-us/aspnet/core/blazor/hybrid/tutorials/wpf?view=aspnetcore-6.0
}
