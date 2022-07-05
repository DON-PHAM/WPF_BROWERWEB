namespace KDCLGD
{
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
    /// Interaction logic for InfoWindow.xaml
    /// </summary>
    public partial class InfoWindow : Window
    {
        public string TempFile { get; set; }
        public ProgressBarWindow ProgressBarWindow { get; set; }
        private WebClient _downloader;

        public Version AppVersion
        {
            get { return (Version)GetValue(AppVersionProperty); }
            set { SetValue(AppVersionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AppVersion.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AppVersionProperty =
            DependencyProperty.Register("AppVersion", typeof(Version), typeof(InfoWindow), new PropertyMetadata(Assembly.GetExecutingAssembly().GetName().Version));


        public InfoWindow()
        {
            InitializeComponent();
            Loaded += InfoWindow_Loaded;
            Closed += InfoWindow_Closed;
        }

        private void InfoWindow_Loaded(object sender, RoutedEventArgs e)
        {
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
            var company = ConfigurationManager.AppSettings.Get("Company");
            if (company == "SSG")
            {
                txtCompany.Text = "Phát triển bởi: Công ty TNHH Phần Mềm Sao Sài Gòn";
                txtPhone.Text = "Điện thoại: (028) 3715 4593";
                txtEmail.Text = "Email: ssg.cskh.hcm@gmail.com";
            }
            else if (company == "HV")
            {
                txtCompany.Text = "Phát triển bởi: Công ty TNHH Phát triển Hương Việt";
                txtPhone.Text = "Điện thoại: (024) 6.269.2438";
                txtEmail.Text = "Email: hn.cskh@huongvietgroup.com";
            }

            txtName.Text = "Phần mềm " + ConfigurationManager.AppSettings.Get("Name") + " - " + role;
        }

        private void InfoWindow_Closed(object sender, EventArgs e)
        {
            if (_downloader != null && _downloader.IsBusy)
            {
                _downloader.CancelAsync();
                _downloader.Dispose();
            }
        }

        /// <summary>
        /// Sự kiện nhấn nút kiểm tra cập nhật
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckConnection())
            {
                MessageBox.Show("Không có kết nối đến mạng internet. Vui lòng thử lại sau");
            }
            else
            {
                var cl = new RestClient(ConfigurationManager.AppSettings.Get("Domain"));
                var request = new RestRequest(ConfigurationManager.AppSettings.Get("RouteUpdate"), Method.GET);
                // execute the request
                IRestResponse response = cl.Execute(request);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    MessageBox.Show("Bạn đang sử dụng phiên bản mới nhất.");
                }
                else
                {
                    var content = response.Content;
                    var result = JsonConvert.DeserializeObject<ResultModel>(content);
                    if (result != null)
                    {
                        if (result.Name == AppVersion.ToString())
                        {
                            MessageBox.Show("Bạn đang sử dụng phiên bản mới nhất.");
                        }
                        else
                        {
                            if (!StartDownLoader(result.Path))
                            {
                                MessageBox.Show("Có lỗi xảy ra trong quá trình cập nhật. Vui lòng thử lại.");
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Bạn đang sử dụng phiên bản mới nhất.");
                    }
                }
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
    }
}
