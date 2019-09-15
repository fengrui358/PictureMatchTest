using OpenCvSharp;

namespace PictureTool
{
    public class SiftMatchResult
    {
        public int? MatchPointsCount { get; set; }

        public Point2f? MatchLocation { get; set; }

        public string Error { get; set; }

        public bool? Match { get; set; }
    }
}
