using System.Windows.Forms;

namespace IPFilter.Logging
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Text;

    /// <devdoc>
    ///    <para>Directs tracing or debugging output to
    ///       a <see cref='T:System.IO.TextWriter'/> or to a <see cref='T:System.IO.Stream'/>,
    ///       such as <see cref='F:System.Console.Out'/> or <see cref='T:System.IO.FileStream'/>.</para>
    /// </devdoc>
    public class FileTraceListener : TraceListener
    {
        internal TextWriter writer;
        string fileName;

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Diagnostics.TextWriterTraceListener'/> class with
        /// <see cref='System.IO.TextWriter'/> 
        /// as the output recipient.</para>
        /// </devdoc>
        public FileTraceListener()
        {
            var name = Path.GetFileNameWithoutExtension(Application.ExecutablePath) + ".log";
            var path = Path.GetDirectoryName(Application.ExecutablePath);
            fileName = path == null ? name : Path.Combine(path, name);
        }

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Diagnostics.TextWriterTraceListener'/> class, using the 
        ///    stream as the recipient of the debugging and tracing output.</para>
        /// </devdoc>
        public FileTraceListener(Stream stream) : this(stream, string.Empty) {}

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Diagnostics.TextWriterTraceListener'/> class with the 
        ///    specified name and using the stream as the recipient of the debugging and tracing output.</para>
        /// </devdoc>
        public FileTraceListener(Stream stream, string name) : base(name)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            writer = new StreamWriter(stream);
        }

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Diagnostics.TextWriterTraceListener'/> class using the 
        ///    specified writer as recipient of the tracing or debugging output.</para>
        /// </devdoc>
        public FileTraceListener(TextWriter writer) : this(writer, string.Empty) {}

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Diagnostics.TextWriterTraceListener'/> class with the 
        ///    specified name and using the specified writer as recipient of the tracing or
        ///    debugging
        ///    output.</para>
        /// </devdoc>
        public FileTraceListener(TextWriter writer, string name) : base(name)
        {
            this.writer = writer ?? throw new ArgumentNullException(nameof(writer));
        }

        /// <devdoc>
        ///    <para>Initializes a new instance of the <see cref='System.Diagnostics.TextWriterTraceListener'/> class with the 
        ///    specified file name.</para>
        /// </devdoc>
        public FileTraceListener(string fileName)
        {
            this.fileName = fileName;
        }

        /// <devdoc>
        ///    <para>Initializes a new instance of the <see cref='System.Diagnostics.TextWriterTraceListener'/> class with the 
        ///    specified name and the specified file name.</para>
        /// </devdoc>
        public FileTraceListener(string fileName, string name) : base(name)
        {
            this.fileName = fileName;
        }

        /// <devdoc>
        ///    <para> Indicates the text writer that receives the tracing
        ///       or debugging output.</para>
        /// </devdoc>
        public TextWriter Writer
        {
            get
            {
                EnsureWriter();
                return writer;
            }

            set => writer = value;
        }

        /// <devdoc>
        /// <para>Closes the <see cref='System.Diagnostics.TextWriterTraceListener.Writer'/> so that it no longer
        ///    receives tracing or debugging output.</para>
        /// </devdoc>
        public override void Close()
        {
            if (writer != null)
            {
                try
                {
                    writer.Close();
                }
                catch (ObjectDisposedException) { }
                writer = null;
            }

            // We need to set the fileName to null so that we stop tracing output, if we don't set it
            // EnsureWriter will create the stream writer again if someone writes or traces output after closing.
            fileName = null;
        }

        /// <internalonly/>
        /// <devdoc>        
        /// </devdoc>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing) writer?.Dispose();
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        /// <devdoc>
        /// <para>Flushes the output buffer for the <see cref='System.Diagnostics.TextWriterTraceListener.Writer'/>.</para>
        /// </devdoc>
        public override void Flush()
        {
            EnsureWriter();
            try
            {
                writer?.Flush();
            }
            catch (ObjectDisposedException)
            {

            }
        }

        /// <devdoc>
        ///    <para>Writes a message 
        ///       to this instance's <see cref='System.Diagnostics.TextWriterTraceListener.Writer'/>.</para>
        /// </devdoc>
        public override void Write(string message)
        {
            EnsureWriter();
            if (writer == null) return;
            if (NeedIndent) WriteIndent();
            try
            {
                writer.Write(message);
            }
            catch (ObjectDisposedException) { }
        }

        /// <devdoc>
        ///    <para>Writes a message 
        ///       to this instance's <see cref='System.Diagnostics.TextWriterTraceListener.Writer'/> followed by a line terminator. The
        ///       default line terminator is a carriage return followed by a line feed (\r\n).</para>
        /// </devdoc>
        public override void WriteLine(string message)
        {
            EnsureWriter();
            if (writer == null) return;
            if (NeedIndent) WriteIndent();
            try
            {
                writer.WriteLine(message);
                NeedIndent = true;
            }
            catch (ObjectDisposedException) { }
        }

        static Encoding GetEncodingWithFallback(Encoding encoding)
        {
            // Clone it and set the "?" replacement fallback
            Encoding fallbackEncoding = (Encoding)encoding.Clone();
            fallbackEncoding.EncoderFallback = EncoderFallback.ReplacementFallback;
            fallbackEncoding.DecoderFallback = DecoderFallback.ReplacementFallback;

            return fallbackEncoding;
        }

        internal void EnsureWriter()
        {
            if (writer != null) return;
            if (fileName == null) return;

            // StreamWriter by default uses UTF8Encoding which will throw on invalid encoding errors.
            // This can cause the internal StreamWriter's state to be irrecoverable. It is bad for tracing 
            // APIs to throw on encoding errors. Instead, we should provide a "?" replacement fallback  
            // encoding to substitute illegal chars. For ex, In case of high surrogate character 
            // D800-DBFF without a following low surrogate character DC00-DFFF
            // NOTE: We also need to use an encoding that does't emit BOM which is StreamWriter's default
            Encoding encoding = GetEncodingWithFallback(new UTF8Encoding(false));

            var fullPath = Path.GetFullPath(fileName);
            var dirPath = Path.GetDirectoryName(fullPath);
            if (dirPath == null)
            {
                fileName = null;
                return;
            }

            try
            {
                if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);
                writer = new StreamWriter(File.Open(fullPath, FileMode.Truncate, FileAccess.Write, FileShare.ReadWrite), encoding, 4096);
                return;
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
            catch (Exception)
            {
            }

            fileName = null;
        }

        internal bool IsEnabled(TraceOptions opts)
        {
            return (opts & TraceOutputOptions) != 0;
        }
    }
}
