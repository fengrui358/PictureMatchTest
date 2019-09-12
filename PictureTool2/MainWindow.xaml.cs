using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FrHello.NetLib.Core.Windows.Windows;
using OpenCvSharp;
using Window = System.Windows.Window;

namespace PictureTool2
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private string _msg;

        public string Msg
        {
            get { return _msg; }
            set
            {
                _msg = value;
                OnPropertyChanged();
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            WindowsApi.ReceiveApiOperateLogEvent += WindowsApiOnReceiveApiOperateLogEvent;

            DataContext = this;
        }

        private void WindowsApiOnReceiveApiOperateLogEvent(object sender, string e)
        {
            Msg = e;
        }

        private async void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var src1 = new Bitmap("Pictures\\Src1.bmp");
            //var sub1 = new Bitmap("Pictures\\Sub1.bmp");
            var sub1 = new Bitmap("Pictures\\NotMatch.bmp");

            var rectangle = await WindowsApi.ScreenApi.ScanBitmapLocation(sub1, src1);
            if (rectangle.HasValue)
            {
                Msg = rectangle.ToString();
            }
            else
            {
                Msg = "NotFound";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
