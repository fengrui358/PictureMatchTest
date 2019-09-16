using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using OpenCvSharp.XFeatures2D;
using Window = System.Windows.Window;

namespace PictureTool
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
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

        private List<SiftMatchResult> _siftMatchResults;

        public List<SiftMatchResult> SiftMatchResults
        {
            get => _siftMatchResults;
            set
            {
                _siftMatchResults = value;
                OnPropertyChanged();
            }
        }

        private List<SurfMatchResult> _surfMatchResults;

        public List<SurfMatchResult> SurfMatchResults
        {
            get => _surfMatchResults;
            set
            {
                _surfMatchResults = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<BatchMatchResult> _batchMatchResults = new ObservableCollection<BatchMatchResult>();

        private long _errorCount;

        public long ErrorCount => _errorCount;

        public ObservableCollection<BatchMatchResult> BatchMatchResults
        {
            get => _batchMatchResults;
            set
            {
                _batchMatchResults = value;
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
            var src1 = new Bitmap("Pictures\\Src1.bmp");
            var sub1 = new Bitmap("Pictures\\Sub1.bmp");
            var notMatch1 = new Bitmap("Pictures\\NotMatch.bmp");

            var templateMatchResults = new List<TemplateMatchResult>();

            foreach (TemplateMatchModes value in Enum.GetValues(typeof(TemplateMatchModes)))
            {
                var matchResult = TemplateMatchLocation(sub1, src1, value);
                templateMatchResults.Add(matchResult);
            }

            templateMatchResults.Add(new TemplateMatchResult());

            foreach (TemplateMatchModes value in Enum.GetValues(typeof(TemplateMatchModes)))
            {
                var matchResult = TemplateMatchLocation(notMatch1, src1, value);
                templateMatchResults.Add(matchResult);
            }

            TemplateMatchResults = templateMatchResults;
        }

        private void SiftMatchBtn_OnClick(object sender, RoutedEventArgs e)
        {
            var src1 = new Bitmap("Pictures\\Src1.bmp");
            var sub1 = new Bitmap("Pictures\\Sub1.bmp");
            var notMatch1 = new Bitmap("Pictures\\NotMatch.bmp");

            var siftMatchResults = new List<SiftMatchResult>();

            siftMatchResults.Add(SiftMatchLocation(sub1, src1));

            siftMatchResults.Add(new SiftMatchResult());

            siftMatchResults.Add(SiftMatchLocation(notMatch1, src1));

            SiftMatchResults = siftMatchResults;
        }

        private void SurfMatchBtn_OnClick(object sender, RoutedEventArgs e)
        {
            var src1 = new Bitmap("Pictures\\Src1.bmp");
            var sub1 = new Bitmap("Pictures\\Sub1.bmp");
            var notMatch1 = new Bitmap("Pictures\\NotMatch.bmp");

            var surfMatchResults = new List<SurfMatchResult>();

            surfMatchResults.Add(SurfMatchLocation(sub1, src1));

            surfMatchResults.Add(new SurfMatchResult
            {
                HessianThreshold = null
            });

            surfMatchResults.Add(SurfMatchLocation(notMatch1, src1));

            SurfMatchResults = surfMatchResults;
        }

        private void BatchMatchBtn_OnClick(object sender, RoutedEventArgs e)
        {
            _errorCount = 0;
            OnPropertyChanged(nameof(ErrorCount));

            BatchMatchResults.Clear();

            var pictureDir = new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Pictures", "Others"));
            var allPics = pictureDir.GetFiles("*", SearchOption.AllDirectories)
                .Select(s => new BatchMatchResult(s.FullName)).ToList();

            var taskFactory = new TaskFactory(CancellationToken.None, TaskCreationOptions.LongRunning, TaskContinuationOptions.LongRunning, TaskScheduler.Default);
            taskFactory.StartNew(() =>
            {
                Parallel.ForEach(allPics, batchMatchResult =>
                {
                    var sw = new Stopwatch();

                    foreach (TemplateMatchModes value in Enum.GetValues(typeof(TemplateMatchModes)))
                    {
                        using (var bitMap = batchMatchResult.GetBitmap())
                        using (var subBitMap = batchMatchResult.GetSubBitmap())
                        using (var notMatch1 = new Bitmap("Pictures\\Src1.bmp"))
                        using (var notMatch2 = new Bitmap("Pictures\\Sub1.bmp"))
                        using (var notMatch3 = new Bitmap("Pictures\\NotMatch.bmp"))
                        {
                            //判断有效
                            sw.Start();
                            var matchResult = TemplateMatchLocation(subBitMap, bitMap, value);
                            sw.Stop();
                            var matchMillionSeconds = sw.ElapsedMilliseconds;
                            if (!string.IsNullOrEmpty(matchResult.Error))
                            {
                                Interlocked.Increment(ref _errorCount);
                                OnPropertyChanged(nameof(ErrorCount));
                            }

                            if (matchResult.Match == false || !string.IsNullOrEmpty(matchResult.Error))
                            {
                                continue;
                            }

                            if (!(matchResult.MaxLocation.HasValue && batchMatchResult.SubRectangle.HasValue &&
                                  matchResult.MaxLocation.Value.X == batchMatchResult.SubRectangle.Value.X &&
                                  matchResult.MaxLocation.Value.Y == batchMatchResult.SubRectangle.Value.Y))
                            {
                                continue;
                            }

                            //判断无效
                            sw.Restart();
                            var notMatchResult1 = TemplateMatchLocation(notMatch1, bitMap, value);
                            sw.Stop();
                            var notMatchMillionSeconds1 = sw.ElapsedMilliseconds;
                            if (!string.IsNullOrEmpty(notMatchResult1.Error))
                            {
                                Interlocked.Increment(ref _errorCount);
                                OnPropertyChanged(nameof(ErrorCount));
                            }

                            if (notMatchResult1.Match == true)
                            {
                                continue;
                            }

                            sw.Restart();
                            var notMatchResult2 = TemplateMatchLocation(notMatch2, bitMap, value);
                            sw.Stop();
                            var notMatchMillionSeconds2 = sw.ElapsedMilliseconds;
                            if (!string.IsNullOrEmpty(notMatchResult2.Error))
                            {
                                Interlocked.Increment(ref _errorCount);
                                OnPropertyChanged(nameof(ErrorCount));
                            }

                            if (notMatchResult2.Match == true)
                            {
                                continue;
                            }

                            sw.Restart();
                            var notMatchResult3 = TemplateMatchLocation(notMatch3, bitMap, value);
                            sw.Stop();
                            var notMatchMillionSeconds3 = sw.ElapsedMilliseconds;
                            if (!string.IsNullOrEmpty(notMatchResult3.Error))
                            {
                                Interlocked.Increment(ref _errorCount);
                                OnPropertyChanged(nameof(ErrorCount));
                            }

                            if (notMatchResult3.Match == true)
                            {
                                continue;
                            }

                            batchMatchResult.AlgorithmDescription +=
                                $"  {value} Y{matchMillionSeconds}:N{(int) ((notMatchMillionSeconds1 + notMatchMillionSeconds2 + notMatchMillionSeconds3) / 3)}";
                        }
                    }

                    using (var bitMap = batchMatchResult.GetBitmap())
                    using (var subBitMap = batchMatchResult.GetSubBitmap())
                    using (var notMatch1 = new Bitmap("Pictures\\Src1.bmp"))
                    using (var notMatch2 = new Bitmap("Pictures\\Sub1.bmp"))
                    using (var notMatch3 = new Bitmap("Pictures\\NotMatch.bmp"))
                    {
                        var match = true;
                        var notMatch = true;

                        //判断有效
                        sw.Restart();
                        var matchResult = SiftMatchLocation(subBitMap, bitMap);
                        sw.Stop();
                        var matchMillionSeconds = sw.ElapsedMilliseconds;
                        if (!string.IsNullOrEmpty(matchResult.Error))
                        {
                            Interlocked.Increment(ref _errorCount);
                            OnPropertyChanged(nameof(ErrorCount));
                        }

                        if (matchResult.Match == false || !string.IsNullOrEmpty(matchResult.Error))
                        {
                            match = false;
                        }

                        if (!(matchResult.MatchLocation.HasValue && batchMatchResult.SubRectangle.HasValue &&
                              Math.Abs(matchResult.MatchLocation.Value.X - batchMatchResult.SubRectangle.Value.X) <
                              float.Epsilon &&
                              Math.Abs(matchResult.MatchLocation.Value.Y - batchMatchResult.SubRectangle.Value.Y) <
                              float.Epsilon))
                        {
                            match = false;
                        }

                        //判断无效
                        sw.Restart();
                        var notMatchResult1 = SiftMatchLocation(notMatch1, bitMap);
                        sw.Stop();
                        var notMatchMillionSeconds1 = sw.ElapsedMilliseconds;
                        if (!string.IsNullOrEmpty(notMatchResult1.Error))
                        {
                            Interlocked.Increment(ref _errorCount);
                            OnPropertyChanged(nameof(ErrorCount));
                        }

                        if (notMatchResult1.Match == true)
                        {
                            notMatch = false;
                        }

                        sw.Restart();
                        var notMatchResult2 = SiftMatchLocation(notMatch2, bitMap);
                        sw.Stop();
                        var notMatchMillionSeconds2 = sw.ElapsedMilliseconds;
                        if (!string.IsNullOrEmpty(notMatchResult2.Error))
                        {
                            Interlocked.Increment(ref _errorCount);
                            OnPropertyChanged(nameof(ErrorCount));
                        }

                        if (notMatchResult2.Match == true)
                        {
                            notMatch = false;
                        }

                        sw.Restart();
                        var notMatchResult3 = SiftMatchLocation(notMatch3, bitMap);
                        sw.Stop();
                        var notMatchMillionSeconds3 = sw.ElapsedMilliseconds;
                        if (!string.IsNullOrEmpty(notMatchResult3.Error))
                        {
                            Interlocked.Increment(ref _errorCount);
                            OnPropertyChanged(nameof(ErrorCount));
                        }

                        if (notMatchResult3.Match == true)
                        {
                            notMatch = false;
                        }

                        if (match && notMatch)
                        {
                            batchMatchResult.AlgorithmDescription +=
                                $"  Sift Y{matchMillionSeconds}:N{(int) ((notMatchMillionSeconds1 + notMatchMillionSeconds2 + notMatchMillionSeconds3) / 3)}";
                        }
                    }

                    using (var bitMap = batchMatchResult.GetBitmap())
                    using (var subBitMap = batchMatchResult.GetSubBitmap())
                    using (var notMatch1 = new Bitmap("Pictures\\Src1.bmp"))
                    using (var notMatch2 = new Bitmap("Pictures\\Sub1.bmp"))
                    using (var notMatch3 = new Bitmap("Pictures\\NotMatch.bmp"))
                    {
                        var match = true;
                        var notMatch = true;

                        //判断有效
                        sw.Restart();
                        var matchResult = SurfMatchLocation(subBitMap, bitMap);
                        sw.Stop();
                        var matchMillionSeconds = sw.ElapsedMilliseconds;
                        if (!string.IsNullOrEmpty(matchResult.Error))
                        {
                            Interlocked.Increment(ref _errorCount);
                            OnPropertyChanged(nameof(ErrorCount));
                        }

                        if (matchResult.Match == false || !string.IsNullOrEmpty(matchResult.Error))
                        {
                            match = false;
                        }

                        if (!(matchResult.MatchLocation.HasValue && batchMatchResult.SubRectangle.HasValue &&
                              Math.Abs(matchResult.MatchLocation.Value.X - batchMatchResult.SubRectangle.Value.X) <
                              float.Epsilon &&
                              Math.Abs(matchResult.MatchLocation.Value.Y - batchMatchResult.SubRectangle.Value.Y) <
                              float.Epsilon))
                        {
                            match = false;
                        }

                        //判断无效
                        sw.Restart();
                        var notMatchResult1 = SurfMatchLocation(notMatch1, bitMap);
                        sw.Stop();
                        var notMatchMillionSeconds1 = sw.ElapsedMilliseconds;
                        if (!string.IsNullOrEmpty(notMatchResult1.Error))
                        {
                            Interlocked.Increment(ref _errorCount);
                            OnPropertyChanged(nameof(ErrorCount));
                        }

                        if (notMatchResult1.Match == true)
                        {
                            notMatch = false;
                        }

                        sw.Restart();
                        var notMatchResult2 = SurfMatchLocation(notMatch2, bitMap);
                        sw.Stop();
                        var notMatchMillionSeconds2 = sw.ElapsedMilliseconds;
                        if (!string.IsNullOrEmpty(notMatchResult2.Error))
                        {
                            Interlocked.Increment(ref _errorCount);
                            OnPropertyChanged(nameof(ErrorCount));
                        }

                        if (notMatchResult2.Match == true)
                        {
                            notMatch = false;
                        }

                        sw.Restart();
                        var notMatchResult3 = SurfMatchLocation(notMatch3, bitMap);
                        sw.Stop();
                        var notMatchMillionSeconds3 = sw.ElapsedMilliseconds;
                        if (!string.IsNullOrEmpty(notMatchResult3.Error))
                        {
                            Interlocked.Increment(ref _errorCount);
                            OnPropertyChanged(nameof(ErrorCount));
                        }

                        if (notMatchResult3.Match == true)
                        {
                            notMatch = false;
                        }

                        if (match && notMatch)
                        {
                            batchMatchResult.AlgorithmDescription +=
                                $"  Surf Y{matchMillionSeconds}:N{(int) ((notMatchMillionSeconds1 + notMatchMillionSeconds2 + notMatchMillionSeconds3) / 3)}";
                        }
                    }

                    Application.Current.Dispatcher?.Invoke(() => { BatchMatchResults.Add(batchMatchResult); });
                });

                Application.Current.Dispatcher?.Invoke(() =>
                {
                    MessageBox.Show(
                        $"共检查{allPics.Count}个文件，失效{allPics.Count(s => string.IsNullOrEmpty(s.AlgorithmDescription))}个");
                });
            });
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
            var result = new TemplateMatchResult
            {
                Match = false
            };

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
                result.Error = ex.Message;
                Debug.WriteLine(ex);
            }

            return result;
        }

        /// <summary>
        /// Sift match
        /// </summary>
        /// <param name="wantBitmap">Want match bitmap</param>
        /// <param name="bitmap">target bitmap</param>
        /// <returns>Target bitmap location</returns>
        private SiftMatchResult SiftMatchLocation(Bitmap wantBitmap, Bitmap bitmap)
        {
            var result = new SiftMatchResult
            {
                Match = false
            };

            try
            {
                using (var matSrc = bitmap.ToMat())
                using (var matTo = wantBitmap.ToMat())
                using (var matSrcRet = new Mat())
                using (var matToRet = new Mat())
                {
                    KeyPoint[] keyPointsSrc, keyPointsTo;
                    using (var sift = SIFT.Create())
                    {
                        sift.DetectAndCompute(matSrc, null, out keyPointsSrc, matSrcRet);
                        sift.DetectAndCompute(matTo, null, out keyPointsTo, matToRet);
                    }

                    using (var bfMatcher = new BFMatcher())
                    {
                        var matches = bfMatcher.KnnMatch(matSrcRet, matToRet, k: 2);

                        var pointsSrc = new List<Point2f>();
                        var pointsDst = new List<Point2f>();
                        foreach (var items in matches.Where(x => x.Length > 1))
                        {
                            if (items[0].Distance < 0.5 * items[1].Distance)
                            {
                                pointsSrc.Add(keyPointsSrc[items[0].QueryIdx].Pt);
                                pointsDst.Add(keyPointsTo[items[0].TrainIdx].Pt);
                            }
                        }

                        if (pointsSrc.Count > 0 && pointsDst.Count > 0)
                        {
                            var location = pointsSrc[0] - pointsDst[0];

                            result.MatchLocation = location;
                            result.Match = true;
                            result.MatchPointsCount = pointsDst.Count;

                            return result;
                        }
                        else
                        {
                            result.MatchPointsCount = pointsDst.Count;
                            result.Match = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.Error = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// Surf match
        /// </summary>
        /// <param name="wantBitmap">Want match bitmap</param>
        /// <param name="bitmap">target bitmap</param>
        /// <returns>Target bitmap location</returns>
        private SurfMatchResult SurfMatchLocation(Bitmap wantBitmap, Bitmap bitmap)
        {
            var result = new SurfMatchResult
            {
                Match = false
            };

            try
            {
                using (var matSrc = bitmap.ToMat())
                using (var matTo = wantBitmap.ToMat())
                using (var matSrcRet = new Mat())
                using (var matToRet = new Mat())
                {
                    KeyPoint[] keyPointsSrc, keyPointsTo;
                    using (var surf = SURF.Create(result.HessianThreshold.Value, 4, 3, true, true))
                    {
                        surf.DetectAndCompute(matSrc, null, out keyPointsSrc, matSrcRet);
                        surf.DetectAndCompute(matTo, null, out keyPointsTo, matToRet);
                    }

                    using (var flnMatcher = new FlannBasedMatcher())
                    {
                        var matches = flnMatcher.Match(matSrcRet, matToRet);

                        //求最小最大距离
                        var minDistance = 1000d; //反向逼近
                        var maxDistance = 0d;
                        for (int i = 0; i < matSrcRet.Rows; i++)
                        {
                            var distance = matches[i].Distance;
                            if (distance > maxDistance)
                            {
                                maxDistance = distance;
                            }

                            if (distance < minDistance)
                            {
                                minDistance = distance;
                            }
                        }

                        var pointsSrc = new List<Point2f>();
                        var pointsDst = new List<Point2f>();

                        for (int i = 0; i < matSrcRet.Rows; i++)
                        {
                            double distance = matches[i].Distance;
                            if (distance < Math.Max(minDistance * 2, 0.02))
                            {
                                pointsSrc.Add(keyPointsSrc[matches[i].QueryIdx].Pt);
                                pointsDst.Add(keyPointsTo[matches[i].TrainIdx].Pt);
                            }
                        }

                        if (pointsSrc.Count > 0 && pointsDst.Count > 0)
                        {
                            var location = pointsSrc[0] - pointsDst[0];

                            result.MatchLocation = location;
                            result.Match = true;
                            result.MatchPointsCount = pointsDst.Count;
                        }
                        else
                        {
                            result.MatchPointsCount = pointsDst.Count;
                            result.Match = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.Error = ex.Message;
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