using OpenCvSharp;

namespace PictureTool
{
    public class SurfMatchResult
    {
        public int? MatchPointsCount { get; set; }

        public Point2f? MatchLocation { get; set; }

        public double? HessianThreshold { get; set; } = 400;

        public string Error { get; set; }

        public bool? Match { get; set; }
    }
}
