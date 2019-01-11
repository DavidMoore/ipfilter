using System;

namespace IPFilter.Models
{
    public static class ProgressModelExtensions
    {
        public static void Report(this IProgress<ProgressModel> progress, UpdateState state, string caption)
        {
            progress.Report(state, caption, -1);
        }

        public static void Report(this IProgress<ProgressModel> progress, UpdateState state, string caption, int value)
        {
            progress.Report(new ProgressModel(state, caption, value));
        }
    }
}