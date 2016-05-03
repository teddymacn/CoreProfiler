using System.Text;
using System.Text.RegularExpressions;

namespace EF.Diagnostics.Profiling.ProfilingFilters
{
    /// <summary>
    /// An <see cref="IProfilingFilter"/> implement for ignoring file requests by extension.
    /// </summary>
    public sealed class FileExtensionProfilingFilter : RegexProfilingFilter
    {
        #region Constructors

        /// <summary>
        /// Initializes a <see cref="FileExtensionProfilingFilter"/>.
        /// </summary>
        /// <param name="extensions">One or many file extensions</param>
        public FileExtensionProfilingFilter(params string[] extensions)
            : base(CreateRegex(extensions))
        {
        }

        /// <summary>
        /// Initializes a <see cref="FileExtensionProfilingFilter"/>.
        /// </summary>
        /// <param name="fileExts">Separated file extentions.</param>
        public FileExtensionProfilingFilter(string fileExts)
            : this(fileExts.Split("|,;".ToCharArray()))
        {
        }

        #endregion

        #region Private Methods

        private static Regex CreateRegex(string[] extensions)
        {
            if (extensions != null && extensions.Length > 0)
            {
                var sb = new StringBuilder();
                sb.Append("\\.(");
                var separator = "";
                foreach (var extension in extensions)
                {
                    sb.Append(separator);
                    sb.Append(extension.Trim(" .".ToCharArray()));
                    sb.Append("\\?|");
                    sb.Append(extension);
                    sb.Append("$");

                    separator = "|";
                }
                sb.Append(")");

                return new Regex(sb.ToString(), RegexOptions.IgnoreCase | RegexOptions.Compiled);
            }

            return null;
        }

        #endregion
    }
}