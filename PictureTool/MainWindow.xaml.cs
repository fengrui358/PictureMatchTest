using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Point = OpenCvSharp.Point;
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
            get => _templateMatchMsg;
            set
            {
                _templateMatchMsg = value;
                OnPropertyChanged();
            }
        }

        private List<TemplateMatchResult> _templateMatchResults;

        public List<TemplateMatchResult> TemplateMatchResults
        {
            get => _templateMatchResults;
            set
            {
                _templateMatchResults = value;
                OnPropertyChanged();
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;
        }

        private void TemplateMatchBtn_OnClick(object sender, RoutedEventArgs e)
        {
            //todo:使用格式化数据来展示
            var src1 = new Bitmap("Pictures\\Src1.bmp");
            var sub1 = new Bitmap("Pictures\\Sub1.bmp");
            var notMatch1 = new Bitmap("Pictures\\NotMatch.bmp");

            var sb = new StringBuilder();

            sb.AppendLine("匹配存在图片");

            double minValue;
            double maxValue;
            Point p;

            foreach (TemplateMatchModes value in Enum.GetValues(typeof(TemplateMatchModes)))
            {
                var rectangle = TemplateMatchLocation(sub1, src1, out minValue, out maxValue, out p, value);
                if (rectangle.HasValue)
                {
                    sb.AppendLine($"{value} {nameof(minValue)}:{minValue} {nameof(maxValue)}:{maxValue} Point:{p} {rectangle}");
                }
                else
                {
                    sb.AppendLine($"{value} {nameof(minValue)}:{minValue} {nameof(maxValue)}:{maxValue} Point:{p} NotFound");
                }
            }

            sb.AppendLine();
            sb.AppendLine("匹配不存在图片");

            foreach (TemplateMatchModes value in Enum.GetValues(typeof(TemplateMatchModes)))
            {
                var rectangle = TemplateMatchLocation(notMatch1, src1, out minValue, out maxValue, out p, value);
                if (rectangle.HasValue)
                {
                    sb.AppendLine($"{value} {nameof(minValue)}:{minValue} {nameof(maxValue)}:{maxValue} Point:{p} {rectangle}");
                }
                else
                {
                    sb.AppendLine($"{value} {nameof(minValue)}:{minValue} {nameof(maxValue)}:{maxValue} Point:{p} NotFound");
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
        /// <returns>Target bitmap location</returns>
        private TemplateMatchResult TemplateMatchLocation(Bitmap wantBitmap, Bitmap bitmap, TemplateMatchModes templateMatch)
        {
            var result = new TemplateMatchResult();

            try
            {
                using (var srcMat = bitmap.ToMat())
                using (var dstMat = wantBitmap.ToMat())
                using (var outArray = OutputArray.Create(srcMat))
                {
                    Cv2.MatchTemplate(srcMat, dstMat, outArray, templateMatch);

                    Cv2.MinMaxLoc(InputArray.Create(outArray.GetMat()), out var minValue,
                        out var maxValue, out var minLoc, out var point);

                    result.TemplateMatch = templateMatch;
                    result.MinValue = minValue;
                    result.MaxValue = maxValue;
                    result.MinLocation = minLoc;
                    result.MaxLocation = point;

                    if (maxValue >= 0.9 && maxValue <= 1d)
                    {
                        result.Match = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return result;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}