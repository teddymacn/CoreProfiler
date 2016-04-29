using System;

namespace EF.Diagnostics.Profiling
{
    /// <summary>
    /// Extension methods for profiling step.
    /// </summary>
    public static class ProfilingStepExtensions
    {
        /// <summary>
        /// Discards the current profiling step.
        /// </summary>
        /// <param name="step"></param>
        public static void Discard(this IDisposable step)
        {
            var profilingStep = step as IProfilingStep;
            if (profilingStep == null) return;

            profilingStep.Discard();
        }

        /// <summary>
        /// Add a tag to current profiling step.
        /// </summary>
        /// <param name="step"></param>
        /// <param name="tag"></param>
        public static void AddTag(this IDisposable step, string tag)
        {
            var profilingStep = step as IProfilingStep;
            if (profilingStep == null) return;

            profilingStep.AddTag(tag);
        }

        /// <summary>
        /// Add a custom data field to current profiling step.
        /// </summary>
        /// <param name="step"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void AddField(this IDisposable step, string key, string value)
        {
            var profilingStep = step as IProfilingStep;
            if (profilingStep == null) return;

            profilingStep.AddField(key, value);
        }
    }
}
