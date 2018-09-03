using System;
using System.Collections.Generic;

namespace IPFilter.Cli
{
    class Options
    {
        public Options()
        {
            Inputs = new List<string>();
            Outputs = new List<string>();
        }

        public static Options Parse(params string[] args)
        {
            var options = new Options();

            if (args != null)
            {
                foreach (var arg in args)
                {
                    // Skip empty arguments
                    if (arg == null) continue;
                    var trimmed = arg.Trim();
                    if( trimmed.Length == 0) continue;

                    if (arg.StartsWith("o:", StringComparison.OrdinalIgnoreCase))
                    {
                        options.Outputs.Add(arg.Substring(2));
                    }
                    else
                    {
                        options.Inputs.Add(arg);
                    }
                }
            }

            return options;
        }

        public IList<string> Inputs { get; set; }

        public IList<string> Outputs { get; set; }
    }
}