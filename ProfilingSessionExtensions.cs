using System;

namespace EF.Diagnostics.Profiling
{
    /// <summary>
    /// Extension methods for <see cref="ProfilingSession"/> class.
    /// </summary>
    public static class ProfilingSessionExtensions
    {
        /// <summary>
        /// Creates an <see cref="IProfilingStep"/> that will time the code between its creation and disposal.
        /// </summary>
        /// <param name="profilingSession">The profiling session.</param>
        /// <param name="name">The name of the step.</param>
        /// <param name="tags">The tags of the step.</param>
        /// <returns></returns>
        public static IDisposable Step(this ProfilingSession profilingSession, string name, params string[] tags)
        {
            if (profilingSession == null || string.IsNullOrEmpty(name)) return null;

            return profilingSession.StepImpl(name, tags);
        }

        /// <summary>
        /// Creates an <see cref="IProfilingStep"/> that will time the code between its creation and disposal.
        /// </summary>
        /// <param name="profilingSession">The profiling session.</param>
        /// <param name="getName">The delegate to get the name of the step.</param>
        /// <param name="tags">The tags of the step.</param>
        /// <returns></returns>
        public static IDisposable Step(this ProfilingSession profilingSession, Func<string> getName, params string[] tags)
        {
            if (getName == null) return null;

            return profilingSession.Step(getName(), tags);
        }

        /// <summary>
        /// Returns an <see cref="System.IDisposable"/> that will ignore the profiling between its creation and disposal.
        /// </summary>
        /// <param name="profilingSession">The profiling session.</param>
        /// <returns>Returns the created <see cref="System.IDisposable"/> as the ignored step.</returns>
        public static IDisposable Ignore(this ProfilingSession profilingSession)
        {
            if (profilingSession == null) return null;

            return profilingSession.IgnoreImpl();
        }

        /// <summary>
        /// Add a tag to current profiling session.
        /// </summary>
        /// <param name="profilingSession"></param>
        /// <param name="tag"></param>
        public static void AddTag(this ProfilingSession profilingSession, string tag)
        {
            if (profilingSession == null) return;

            profilingSession.AddTagImpl(tag);
        }

        /// <summary>
        /// Add a custom data field to current profiling session.
        /// </summary>
        /// <param name="profilingSession"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void AddField(this ProfilingSession profilingSession, string key, string value)
        {
            if (profilingSession == null) return;

            profilingSession.AddFieldImpl(key, value);
        }
    }
}
