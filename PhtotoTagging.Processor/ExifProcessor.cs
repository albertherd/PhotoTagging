using PhotoTagging.Processor.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.MetaData.Profiles.Exif;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PhtotoTagging.Processor
{
    public class ExifProcessor
    {
        private const double highConfidenceThreshold = 0.7;

        public void ReadExifMetaData(string imagePath)
        {
            using (Image<Rgba32> image = Image.Load(imagePath))
            {
                foreach (var value in image.MetaData.ExifProfile.Values)
                {
                    string valueAsString = value.Value as string;

                    Type valueType = value.Value.GetType();
                    if (valueType == typeof(string))
                    {
                        valueAsString = (string)value.Value;
                    }
                    else if (valueType == typeof(byte[]))
                    {
                        valueAsString = Encoding.Unicode.GetString((byte[])value.Value);
                    }

                    Console.WriteLine($"{value.Tag} : {valueAsString}");
                }
            }
        }

        public void WriteExifMetaData(string imagePath, PhotoAnalysisResult photoAnalysisResult)
        {
            var data = photoAnalysisResult.Data;

            if (data.Description != null)
            {
                TextWithConfidence captionWithConfidence = data.Description.Captions.FirstOrDefault();
            }

            using (Image<Rgba32> image = Image.Load(imagePath))
            {
                if (image.MetaData.ExifProfile == null)
                    image.MetaData.ExifProfile = new ExifProfile();

                TextWithConfidence captionWithConfidence = data.Description.Captions.FirstOrDefault();
                if (captionWithConfidence != null)
                {
                    if (captionWithConfidence.Confidence >= highConfidenceThreshold)
                    {
                        image.MetaData.ExifProfile.SetValue(ExifTag.ImageDescription, captionWithConfidence.Text);
                        image.MetaData.ExifProfile.SetValue(ExifTag.XPTitle, ToExifByteArray(captionWithConfidence.Text));
                    }
                    else
                    {
                        // Add them for manual review.
                    }
                }

                List<string> highConfidenceTags = new List<string>();
                List<NameWithConfidence> lowConfidenceTags = new List<NameWithConfidence>();
                foreach (NameWithConfidence tag in data.Tags)
                {
                    if (tag.Confidence >= highConfidenceThreshold)
                        highConfidenceTags.Add(tag.Name);
                    else
                        lowConfidenceTags.Add(tag);
                }

                if (highConfidenceTags.Any())
                {
                    image.MetaData.ExifProfile.SetValue(ExifTag.XPKeywords,
                        ToExifByteArray(string.Join(',', highConfidenceTags)));
                }
                else
                {
                    // Add them for manual review
                }

                image.Save(imagePath);
            }
        }

        private object ToExifByteArray(string value) => Encoding.Unicode.GetBytes(value);
    }
}
