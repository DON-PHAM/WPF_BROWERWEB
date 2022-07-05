namespace KDCLGD
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private string _appDataFolder;

        public string AppDataFolder
        {
            get
            {
                if (string.IsNullOrEmpty(_appDataFolder)) _appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EmbedAppData");
                if (!Directory.Exists(_appDataFolder)) Directory.CreateDirectory(_appDataFolder);
                return _appDataFolder;
            }
        }

        public App()
        {

        }
    }
}
