﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;

namespace NuGet.Versioning
{
    /// <summary>
    /// VersionRange formatter
    /// </summary>
    public class VersionRangeFormatter : IFormatProvider, ICustomFormatter
    {
        private const string LessThanOrEqualTo = "<=";
        private const string GreaterThanOrEqualTo = ">=";
        private const string ZeroN = "{0:N}";
        private readonly VersionFormatter _versionFormatter;

        /// <summary>
        /// Custom version range format provider.
        /// </summary>
        public VersionRangeFormatter()
        {
            _versionFormatter = VersionFormatter.Instance;
        }

        /// <summary>
        /// Format a version range string.
        /// </summary>
        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (arg == null)
            {
                throw new ArgumentNullException(nameof(arg));
            }

            string formatted = null;
            //var argType = arg.GetType();

            if (false)
            {
                //formatted = ((IFormattable)arg).ToString(format, formatProvider);
            }
            else if (!string.IsNullOrEmpty(format))
            {
                var range = arg as VersionRange;

                if (range != null)
                {
                    // single char identifiers
                    if (format.Length == 1)
                    {
                        formatted = Format(format[0], range);
                    }
                    else
                    {
                        var sb = new StringBuilder(format.Length);

                        for (var i = 0; i < format.Length; i++)
                        {
                            var s = Format(format[i], range);

                            if (s == null)
                            {
                                sb.Append(format[i]);
                            }
                            else
                            {
                                sb.Append(s);
                            }
                        }

                        formatted = sb.ToString();
                    }
                }
            }

            return formatted;
        }

        /// <summary>
        /// Format type.
        /// </summary>
        public object GetFormat(Type formatType)
        {
            if (formatType == typeof(ICustomFormatter)
                || formatType == typeof(VersionRange))
            {
                return this;
            }

            return null;
        }

        private string Format(char c, VersionRange range)
        {
            string s = null;

            switch (c)
            {
                case 'P':
                    s = PrettyPrint(range);
                    break;
                case 'L':
                    s = range.HasLowerBound ? string.Format(VersionFormatter.Instance, ZeroN, range.MinVersion) : string.Empty;
                    break;
                case 'U':
                    s = range.HasUpperBound ? string.Format(VersionFormatter.Instance, ZeroN, range.MaxVersion) : string.Empty;
                    break;
                case 'S':
                    s = GetToString(range);
                    break;
                case 'N':
                    s = GetNormalizedString(range);
                    break;
                case 'D':
                    s = GetLegacyString(range);
                    break;
                case 'T':
                    s = GetLegacyShortString(range);
                    break;
                case 'A':
                    s = GetShortString(range);
                    break;
            }

            return s;
        }

        private string GetShortString(VersionRange range)
        {
            string s = null;

            if (range.HasLowerBound
                && range.IsMinInclusive
                && !range.HasUpperBound)
            {
                s = range.IsFloating ?
                    range.Float.ToString() :
                    string.Format(_versionFormatter, ZeroN, range.MinVersion);
            }
            else if (range.HasLowerAndUpperBounds
                     && range.IsMinInclusive
                     && range.IsMaxInclusive
                     &&
                     range.MinVersion.Equals(range.MaxVersion))
            {
                // Floating should be ignored here.
                s = string.Format(_versionFormatter, "[{0:N}]", range.MinVersion);
            }
            else
            {
                s = GetNormalizedString(range);
            }

            return s;
        }

        /// <summary>
        /// Builds a normalized string with no short hand
        /// </summary>
        private string GetNormalizedString(VersionRange range)
        {
            // TODO: write out the float version
            var sb = new StringBuilder();

            sb.Append(range.HasLowerBound && range.IsMinInclusive ? '[' : '(');

            if (range.HasLowerBound)
            {
                if (range.IsFloating)
                {
                    sb.Append(range.Float.ToString());
                }
                else
                {
                    sb.AppendFormat(_versionFormatter, ZeroN, range.MinVersion);
                }
            }

            sb.Append(", ");

            if (range.HasUpperBound)
            {
                sb.AppendFormat(_versionFormatter, ZeroN, range.MaxVersion);
            }

            sb.Append(range.HasUpperBound && range.IsMaxInclusive ? ']' : ')');

            return sb.ToString();
        }

        /// <summary>
        /// Builds a string to represent the VersionRange. This string can include short hand notations.
        /// </summary>
        private string GetToString(VersionRange range)
        {
            string s = null;

            if (range.HasLowerBound
                && range.IsMinInclusive
                && !range.HasUpperBound)
            {
                s = string.Format(_versionFormatter, ZeroN, range.MinVersion);
            }
            else if (range.HasLowerAndUpperBounds
                     && range.IsMinInclusive
                     && range.IsMaxInclusive
                     &&
                     range.MinVersion.Equals(range.MaxVersion))
            {
                // TODO: Does this need a specific version comparision? Does metadata matter?

                s = string.Format(_versionFormatter, "[{0:N}]", range.MinVersion);
            }
            else
            {
                s = GetNormalizedString(range);
            }

            return s;
        }

        /// <summary>
        /// Creates a legacy short string that is compatible with NuGet 2.8.3
        /// </summary>
        private string GetLegacyShortString(VersionRangeBase range)
        {
            string s = null;

            if (range.HasLowerBound
                && range.IsMinInclusive
                && !range.HasUpperBound)
            {
                s = string.Format(_versionFormatter, ZeroN, range.MinVersion);
            }
            else if (range.HasLowerAndUpperBounds
                     && range.IsMinInclusive
                     && range.IsMaxInclusive
                     &&
                     range.MinVersion.Equals(range.MaxVersion))
            {
                s = string.Format(_versionFormatter, "[{0:N}]", range.MinVersion);
            }
            else
            {
                s = GetLegacyString(range);
            }

            return s;
        }

        /// <summary>
        /// Creates a legacy string that is compatible with NuGet 2.8.3
        /// </summary>
        private string GetLegacyString(VersionRangeBase range)
        {
            var sb = new StringBuilder();

            sb.Append(range.HasLowerBound && range.IsMinInclusive ? '[' : '(');

            if (range.HasLowerBound)
            {
                sb.AppendFormat(_versionFormatter, ZeroN, range.MinVersion);
            }

            sb.Append(", ");

            if (range.HasUpperBound)
            {
                sb.AppendFormat(_versionFormatter, ZeroN, range.MaxVersion);
            }

            sb.Append(range.HasUpperBound && range.IsMaxInclusive ? ']' : ')');

            return sb.ToString();
        }

        /// <summary>
        /// A pretty print representation of the VersionRange.
        /// </summary>
        private string PrettyPrint(VersionRange range)
        {
            // empty range
            if (!range.HasLowerBound
                 && !range.HasUpperBound)
            {
                return string.Empty;
            }

            // single version
            if (range.HasLowerAndUpperBounds
                     && range.MaxVersion.Equals(range.MinVersion)
                     && range.IsMinInclusive
                     && range.IsMaxInclusive)
            {
                return string.Format(_versionFormatter, "(= {0:N})", range.MinVersion);
            }

            // normal case with a lower, upper, or both.
            var sb = new StringBuilder("(");

            if (range.HasLowerBound)
            {
                PrettyPrintBound(sb, range.MinVersion, range.IsMinInclusive, ">");
            }

            if (range.HasLowerAndUpperBounds)
            {
                sb.Append(" && ");
            }

            if (range.HasUpperBound)
            {
                PrettyPrintBound(sb, range.MaxVersion, range.IsMaxInclusive, "<");
            }

            sb.Append(")");

            return sb.ToString();
        }

        private void PrettyPrintBound(StringBuilder sb, NuGetVersion version, bool inclusive, string boundChar)
        {
            sb.Append(boundChar);

            if (inclusive)
            {
                sb.Append("=");
            }

            sb.Append(" ");
            sb.AppendFormat(_versionFormatter, ZeroN, version);
        }
    }
}
