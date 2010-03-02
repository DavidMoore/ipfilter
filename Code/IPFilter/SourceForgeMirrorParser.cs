using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace IPFilter
{
    /// <summary>
    /// Parses SourceForge mirror list HTML into a collection of <see cref="FileMirror"/> objects
    /// </summary>
    public class SourceForgeMirrorParser : ISourceForgeMirrorParser
    {
        const string MirrorRegexSource = @"<input type=""radio"" name=""mirror"" id=""mirror_[^""]+"" value=""([^""]+)"" [^\/]*\/>\s*<label for=""mirror_[^""]+"">([^<]+)<\/label>([^<]*)";

        readonly Regex regex = new Regex(MirrorRegexSource, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);
        
        /// <summary>
        /// Parses some passed HTML received from SourceForge for
        /// available file mirrors
        /// </summary>
        /// <param name="mirrorsHtml"></param>
        /// <returns></returns>
        public IEnumerable<FileMirror> ParseMirrors(string mirrorsHtml)
        {
            return from Match match in regex.Matches(mirrorsHtml)
                   let name = match.Groups[2].Value.Trim() + " " + match.Groups[3].Value.Trim()
                   select new FileMirror(match.Groups[1].Value, name);
        }
    }
}