namespace KDCLGD
{
    using System.Windows;

    /// <summary>
    /// Interaction logic for ProgressBarWindow.xaml
    /// </summary>
    public partial class ProgressBarWindow : Window
    {
        public long FileSize
        {
            get { return (long)GetValue(FileSizeProperty); }
            set { SetValue(FileSizeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FileSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FileSizeProperty =
            DependencyProperty.Register("FileSize", typeof(long), typeof(ProgressBarWindow), new PropertyMetadata((long)0));



        public decimal CurrentFileSize
        {
            get { return (decimal)GetValue(CurrentFileSizeProperty); }
            set { SetValue(CurrentFileSizeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CurrentFileSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentFileSizeProperty =
            DependencyProperty.Register("CurrentFileSize", typeof(decimal), typeof(ProgressBarWindow), new PropertyMetadata((decimal)0));


        public ProgressBarWindow()
        {
            InitializeComponent();
        }
    }
}
