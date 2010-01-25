using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace IPFilter
{
    public class SourceForgeMirrorParser
    {
        readonly Regex regex = new Regex(@"<input type=""radio"" name=""mirror"" id=""mirror_[^""]+"" value=""([^""]+)"" [^\/]*\/>\s*<label for=""mirror_[^""]+"">([^<]+)<\/label>([^<]*)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

        public IEnumerable<FileMirror> ParseMirrors(string mirrorsHtml)
        {
            foreach (Match match in regex.Matches(mirrorsHtml))
            {
                var name = match.Groups[2].Value.Trim() + " " + match.Groups[3].Value.Trim();
                yield return new FileMirror(match.Groups[1].Value, name);
            }
        }
    }
}