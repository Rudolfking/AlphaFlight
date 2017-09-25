﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace AlphaConfigurator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
        }

        internal new static App Current => (App)Application.Current;

        public void Log(string logMessage)
        {
            Dispatcher.Invoke((Action)(() =>
            {
                if (MainWindow != null)
                {
                    (MainWindow as MainWindow).Log(logMessage);
                }
            }));
        }
    }
}
