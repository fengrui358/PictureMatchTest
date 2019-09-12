using OpenCvSharp;

namespace PictureTool
{
    public class TemplateMatchResult
    {
        public TemplateMatchModes? TemplateMatch { get; set; }

        public double? MinValue { get; set; }

        public double? MaxValue { get; set; }

        public Point? MinLocation { get; set; }

        public Point? MaxLocation { get; set; }

        public bool Match { get; set; }
    }
}
