using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
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
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Rectangle = System.Drawing.Rectangle;
using Window = System.Windows.Window;

namespace PictureTool
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private string _templateMatchMsg;

        public string TemplateMatchMsg
        {
            get { return _templateMatchMsg; }
            set
            {
                _templateMatchMsg = value;
                OnPropertyChanged();
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var src1 = new Bitmap("Pictures\\Src1.bmp");
            var sub1 = new Bitmap("Pictures\\Sub1.bmp");
            var notMatch1 = new Bitmap("Pictures\\NotMatch.bmp");

            var sb = new StringBuilder();

            sb.AppendLine("匹配存在图片");

            double maxValue;

            foreach (TemplateMatchModes value in Enum.GetValues(typeof(TemplateMatchModes)))
            {
                var rectangle = TemplateMatchLocation(sub1, src1, out maxValue, value);
                if (rectangle.HasValue)
                {
                    sb.AppendLine($"{nameof(TemplateMatchModes)}:{value} {nameof(maxValue)}:{maxValue} {rectangle}");
                }
                else
                {
                    sb.AppendLine($"{nameof(TemplateMatchModes)}:{value} {nameof(maxValue)}:{maxValue} NotFound");
                }
            }

            sb.AppendLine();
            sb.AppendLine("匹配不存在图片");

            foreach (TemplateMatchModes value in Enum.GetValues(typeof(TemplateMatchModes)))
            {
                var rectangle = TemplateMatchLocation(notMatch1, src1, out maxValue, value);
                if (rectangle.HasValue)
                {
                    sb.AppendLine($"{nameof(TemplateMatchModes)}:{value} {nameof(maxValue)}:{maxValue} {rectangle}");
                }
                else
                {
                    sb.AppendLine($"{nameof(TemplateMatchModes)}:{value} {nameof(maxValue)}:{maxValue} NotFound");
                }
            }

            TemplateMatchMsg = sb.ToString();
        }

        /// <summary>
        /// Template match
        /// </summary>
        /// <param name="wantBitmap">Want match bitmap</param>
        /// <param name="bitmap">target bitmap</param>
        /// <param name="templateMatch">template match option</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>Target bitmap location</returns>
        private Rectangle? TemplateMatchLocation(Bitmap wantBitmap, Bitmap bitmap, out double maxValue,
            TemplateMatchModes templateMatch, CancellationToken cancellationToken = default)
        {
            maxValue = -1;

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                using (var srcMat = bitmap.ToMat())
                using (var dstMat = wantBitmap.ToMat())
                using (var outArray = OutputArray.Create(srcMat))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    Cv2.MatchTemplate(srcMat, dstMat, outArray, templateMatch);

                    cancellationToken.ThrowIfCancellationRequested();

                    Cv2.MinMaxLoc(InputArray.Create(outArray.GetMat()), out _,
                        out maxValue, out _, out var point);

                    if (maxValue >= 0.9)
                    {
                        var rectangle =
                            new Rectangle?(new Rectangle(point.X, point.Y, wantBitmap.Width, wantBitmap.Height));

                        return rectangle;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return null;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
