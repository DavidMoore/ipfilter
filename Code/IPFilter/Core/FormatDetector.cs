using System;
using System.Globalization;
using System.IO;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace IPFilter.Core
{
    public class FormatDetector
    {
        static internal async Task<DataFormat> DetectFormat(Stream stream)
        {
            var buffer = new byte[4];
            var bytesRead = await stream.ReadAsync(buffer, 0, 4);
            if (bytesRead == 4)
            {
                // Look for the GZip header bytes
                if (buffer[0] == 31 && buffer[1] == 139) return DataFormat.GZip;

                // Look for the ZIP header bytes.
                var zipHeaderNumber = BitConverter.ToInt32(buffer, 0);
                if (zipHeaderNumber == 0x4034b50) return DataFormat.Zip;
            }

            stream.Seek(0, SeekOrigin.Begin);

            // Read the first line
            using (var reader = new StreamReader(stream))
            {
                var lineBuffer = new char[1000];
                var charsRead = await reader.ReadBlockAsync(lineBuffer, 0, lineBuffer.Length);
                
                var sb = new StringBuilder(lineBuffer.Length);
                //sb.Append(lineBuffer, 0, charsRead);

                for (var i = 0; i < charsRead; i++)
                {
                    var character = lineBuffer[i];
                    
                    // If we see non-text characters, it's not a text file
                    if (!char.IsLetterOrDigit(character) || !char.IsWhiteSpace(character))
                    {
                        switch (char.GetUnicodeCategory(character))
                        {
                            case UnicodeCategory.UppercaseLetter:
                                break;
                            case UnicodeCategory.LowercaseLetter:
                                break;
                            case UnicodeCategory.TitlecaseLetter:
                                break;
                            case UnicodeCategory.ModifierLetter:
                                break;
                            case UnicodeCategory.OtherLetter:
                                break;
                            case UnicodeCategory.NonSpacingMark:
                                break;
                            case UnicodeCategory.SpacingCombiningMark:
                                break;
                            case UnicodeCategory.EnclosingMark:
                                break;
                            case UnicodeCategory.DecimalDigitNumber:
                                break;
                            case UnicodeCategory.LetterNumber:
                                break;
                            case UnicodeCategory.OtherNumber:
                                break;
                            case UnicodeCategory.SpaceSeparator:
                                break;
                            case UnicodeCategory.LineSeparator:
                                break;
                            case UnicodeCategory.ParagraphSeparator:
                                break;
                            case UnicodeCategory.Control:
                                break;
                            case UnicodeCategory.Format:
                                break;
                            case UnicodeCategory.Surrogate:
                                break;
                            case UnicodeCategory.PrivateUse:
                                break;
                            case UnicodeCategory.ConnectorPunctuation:
                                break;
                            case UnicodeCategory.DashPunctuation:
                                break;
                            case UnicodeCategory.OpenPunctuation:
                                break;
                            case UnicodeCategory.ClosePunctuation:
                                break;
                            case UnicodeCategory.InitialQuotePunctuation:
                                break;
                            case UnicodeCategory.FinalQuotePunctuation:
                                break;
                            case UnicodeCategory.OtherPunctuation:
                                break;
                            case UnicodeCategory.MathSymbol:
                                break;
                            case UnicodeCategory.CurrencySymbol:
                                break;
                            case UnicodeCategory.ModifierSymbol:
                                break;
                            case UnicodeCategory.OtherSymbol:
                                break;
                            case UnicodeCategory.OtherNotAssigned:
                                return DataFormat.Binary;
                            default:
                                return DataFormat.Binary;
                        }
                    }

                    sb.Append(character);
                }

                // This looks like a text file, but is it maybe JSON?
                if (sb.ToString().TrimStart().StartsWith("{")) return DataFormat.Json;

                return DataFormat.Text;
            }
        }
        
        public static Task<DataFormat> GetFormat(Stream stream, MediaTypeHeaderValue contentType = null)
        {
            var mediaType = contentType?.MediaType;
            
            switch (mediaType)
            {
                case "application/gzip":
                case "application/x-gzip":
                case "application/x-gunzip":
                case "application/gzipped":
                case "application/gzip-compressed":
                case "gzip/document":
                    return Task.FromResult(DataFormat.GZip);

                case "application/zip":
                case "application/x-zip":
                case "application/x-zip-compressed":
                case "multipart/x-zip":
                    return Task.FromResult(DataFormat.Zip);

                case "application/x-compressed":
                case "application/octet-stream":
                case "text/plain":
                default:
                    return DetectFormat(stream);
            }
        }
    }
}