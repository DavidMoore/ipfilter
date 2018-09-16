using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace IPFilter.Core
{
    public class FormatDetector
    {
        public async static Task<DataFormat> GetFormat(byte[] header, MediaTypeHeaderValue contentType = null)
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
                    return DataFormat.GZip;

                case "application/zip":
                case "application/x-zip":
                case "application/x-zip-compressed":
                case "multipart/x-zip":
                    return DataFormat.Zip;

//                case "application/x-compressed":
//                case "application/octet-stream":
//                case "text/plain":
                default:
                {
                    // Look for the GZip header bytes
                    if (header[0] == 31 && header[1] == 139)
                    {
                        return DataFormat.GZip;
                    }

                    // Look for the ZIP header bytes.
                    var zipHeaderNumber = BitConverter.ToInt32(header, 0);
                    if (zipHeaderNumber == 0x4034b50)
                    {
                        return DataFormat.Zip;
                    }

                    // Try to parse json
//                    var serializer = new JsonSerializer();
//                    stream.Seek(0, SeekOrigin.Begin);
//                    using (var streamReader = StreamHelper.CreateStreamReader(stream))
//                    using (var reader = new JsonTextReader(streamReader))
//                    {
//                        try
//                        {
//                            // Try to strongly de-serialize
//                            stream.Seek(0, SeekOrigin.Begin);
//                            var list = serializer.Deserialize<BlocklistBundle>(reader);
//                            if (list?.Lists?.Count > 0) return DataFormat.Json;
//                        }
//                        catch (Exception ex)
//                        {
//                            Trace.TraceWarning(ex.ToString());
//                        }
//                    }
                }
                    break;
            }

            return DataFormat.Text;
        }

    }
}