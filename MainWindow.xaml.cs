namespace KDCLGD
{
    using CefSharp.Wpf;
    using Microsoft.Win32;
    using Newtonsoft.Json;
    using RestSharp;
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.Diagnostics;
    using System.Net;
    using System.Reflection;
    using System.Windows;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string TempFile { get; set; }
        public ProgressBarWindow ProgressBarWindow { get; set; }
        private WebClient _downloader;

        bool isFirstLoad = false;

        public MainWindow()
        {
            CefSettings settings = new CefSettings();
            settings.CachePath = (Application.Current as App).AppDataFolder;
            settings.CefCommandLineArgs.Add("ignore-certificate-errors");
            CefSharp.Cef.Initialize(settings);
            InitializeComponent();
            webView.Address = ConfigurationManager.AppSettings.Get("Domain");
            webView.Loaded += WebView_Loaded;
            webView.LoadError += WebView_LoadError;
            Loaded += MainWindow_Loaded;
        }

        private void WebView_LoadError(object sender, CefSharp.LoadErrorEventArgs e)
        {
            if (e.ErrorCode != CefSharp.CefErrorCode.Aborted)
            {
                App.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    webView.Visibility = Visibility.Hidden;
                    refresh.Visibility = Visibility.Hidden;
                    error.Visibility = Visibility.Visible;
                    backBtn.Visibility = Visibility.Visible;
                }));
            }
        }

        private void WebView_Loaded(object sender, RoutedEventArgs e)
        {
            webView.Visibility = Visibility.Visible;
            refresh.Visibility = Visibility.Hidden;
            error.Visibility = Visibility.Hidden;
            backBtn.Visibility = Visibility.Hidden;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            CheckUpdateVersion();
            string role = "";
            switch (ConfigurationManager.AppSettings.Get("Role"))
            {
                case "School":
                    role = "Phiên bản dành cho đơn vị cấp trường";
                    break;
                case "EducationDepartment":
                    role = "Phiên bản dành cho đơn vị cấp phòng";
                    break;
                case "DepartmentOfEducation":
                    role = "Phiên bản dành cho đơn vị cấp sở";
                    break;
                default:
                    break;
            }
            Title = ConfigurationManager.AppSettings.Get("Name") + " - " + role;
            webView.DownloadHandler = new MyDownloadHandler();
            //webView.RequestHandler = new CustomRequestHandler(ConfigurationManager.AppSettings.Get("Role"));
            //webView.MenuHandler = new DisableContextMenuHandler();
        }

        /// <summary>
        /// Kiểm tra thử có cập nhật hay không
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private void CheckUpdateVersion()
        {
            if (CheckConnection())
            {
                var cl = new RestClient(ConfigurationManager.AppSettings.Get("Domain"));
                var request = new RestRequest(ConfigurationManager.AppSettings.Get("RouteUpdate"), Method.GET);
                // execute the request
                IRestResponse response = cl.Execute(request);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var content = response.Content;
                    var result = JsonConvert.DeserializeObject<ResultModel>(content);
                    if (result != null)
                    {
                        if (result.Name != Assembly.GetExecutingAssembly().GetName().Version.ToString())
                        {
                            var messageBox = MessageBox.Show("Có bản cập nhật mới. Bạn có muốn cập nhật?", "Cập nhật", MessageBoxButton.YesNo);
                            if (messageBox == MessageBoxResult.Yes)
                            {
                                StartDownLoader(result.Path);
                            }
                        }
                    }
                }
            }
        }

        public static bool CheckConnection()
        {
            try
            {
                using (var client = new WebClient())
                using (var stream = client.OpenRead("http://www.google.com"))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Bắt đầu tải file
        /// </summary>
        /// <param name="path">Đường dẫn file</param>
        private bool StartDownLoader(string path)
        {
            _downloader = new WebClient();
            string folderTempFile = System.IO.Path.GetTempPath() + "embedWeb";
            if (!System.IO.Directory.Exists(folderTempFile))
            {
                System.IO.Directory.CreateDirectory(folderTempFile);
            }
            var extension = System.IO.Path.GetExtension(path);
            TempFile = System.IO.Path.Combine(folderTempFile, Guid.NewGuid() + extension);
            var totalPath = ConfigurationManager.AppSettings.Get("Domain") + "/" + path;
            try
            {
                _downloader.OpenRead(totalPath);
            }
            catch
            {
                return false;
            }
            long fileSize = Convert.ToInt64(_downloader.ResponseHeaders["Content-Length"]);

            _downloader.DownloadFileCompleted += OnDownloadFileCompleted;
            _downloader.DownloadProgressChanged += OnDownloadProgressChanged;
            _downloader.DownloadFileAsync(new Uri(totalPath), TempFile);
            ProgressBarWindow = new ProgressBarWindow();
            ProgressBarWindow.Owner = this;
            ProgressBarWindow.FileSize = fileSize;
            ProgressBarWindow.Show();
            return true;
        }


        private void OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (ProgressBarWindow != null)
                ProgressBarWindow.CurrentFileSize = e.BytesReceived;
        }

        private void OnDownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (System.IO.File.Exists(TempFile) && !e.Cancelled && e.Error == null)
            {
                try
                {
                    Process.Start(TempFile);
                    Application.Current.Shutdown();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Có lỗi xảy ra trong quá trình cập nhật. Vui lòng thử lại.");
                }
            }
        }

        private void backBtn_Click(object sender, RoutedEventArgs e)
        {
            webView.Address = ConfigurationManager.AppSettings.Get("Domain");
            webView.Visibility = Visibility.Visible;
            refresh.Visibility = Visibility.Hidden;
            error.Visibility = Visibility.Hidden;
            backBtn.Visibility = Visibility.Hidden;
        }

        private void checkUpdate_Click(object sender, RoutedEventArgs e)
        {
            InfoWindow infoWindow = new InfoWindow();
            infoWindow.Owner = this;
            infoWindow.ShowDialog();
        }
    }
}
