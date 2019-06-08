using System;
using System.Collections.Generic;
using System.Text;

namespace PhotoTagging.Processor.Models
{
    public class PhotoAnalysisResult
    {
        public PhotoAnalysisRequest PhotoAnalysisRequest { get; set; }
        public bool Success { get; set; }
        public PhotoAnalysisResultData Data { get; set; }
    }

    public class PhotoAnalysisResultData
    {
        public List<NameWithConfidence> Tags { get; set; }
        public Description Description { get; set; }
    }

    public class Description
    {
        public List<string> Tags { get; set; }
        public List<TextWithConfidence> Captions { get; set; }
    }

    public class NameWithConfidence
    {
        public string Name { get; set; }
        public double Confidence { get; set; }
    }

    public class TextWithConfidence
    {
        public string Text { get; set; }
        public double Confidence { get; set; }
    }
}
