using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ulang
{
    public class CaseInsensitiveEqualityComparer : IEqualityComparer<char>
    {
        public static readonly CaseInsensitiveEqualityComparer Default = new CaseInsensitiveEqualityComparer();

        public bool Equals(char x, char y)
        {
            return char.ToLowerInvariant(x) == char.ToLowerInvariant(y);
        }

        public int GetHashCode([DisallowNull] char obj)
        {
            return obj.GetHashCode();
        }
    }

    public struct ProjectFile
    {
        public static readonly Edition CURRENT_LANG_EDITION = new Edition(2024, 1, 0);
        public static readonly Edition CURRENT_STD_EDITION = new Edition(2024, 1, 0);

        // Project Meta
        public string Path;

        public string Id;
        public string Name;
        public Version Version;

        // Compiler Settings
        public bool OverrideAlloc;
        public bool OverrideDealloc;
        public bool HasEntry;

        public string Allocator;
        public string Deallocator;
        public string Entry;

        public bool NoDeprecated;
        public bool NoWarnings;

        // Language Versioning
        public Edition LangEdition;
        public bool UseStd;
        public Edition StdEdition;

        private static unsafe bool GetString(ReadOnlySpan<char> value, ref string str)
        {
            if (value.IsEmpty)
                return false;

            str = new string((char*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(value)));
            return true;
        }
        private static unsafe bool GetBool(ReadOnlySpan<char> value, ref bool bl)
        {
            bool t = value.SequenceEqual("true", CaseInsensitiveEqualityComparer.Default);
            bl = !value.SequenceEqual("false", CaseInsensitiveEqualityComparer.Default);

            if (!t)
                return !bl;

            bl = t;
            return t;
        }
        private const NumberStyles VERSION_STYLE = NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite;
        private static unsafe bool GetVersionNum(ReadOnlySpan<char> value, ref uint maj, ref uint min, ref uint rev)
        {
            int firstSep = value.IndexOf('.');
            if (firstSep == -1)
                return uint.TryParse(value, VERSION_STYLE, NumberFormatInfo.InvariantInfo, out maj);

            int lastSep = value.LastIndexOf('.');
            if (lastSep == -1)
                return uint.TryParse(value[..firstSep++], VERSION_STYLE, NumberFormatInfo.InvariantInfo, out maj) &&
                       uint.TryParse(value[firstSep..], VERSION_STYLE, NumberFormatInfo.InvariantInfo, out min);

            return uint.TryParse(value[..firstSep++], VERSION_STYLE, NumberFormatInfo.InvariantInfo, out maj) &&
                   uint.TryParse(value[firstSep..lastSep++], VERSION_STYLE, NumberFormatInfo.InvariantInfo, out min) &&
                   uint.TryParse(value[lastSep..], VERSION_STYLE, NumberFormatInfo.InvariantInfo, out rev);
        }
        private static unsafe bool GetVersion(ReadOnlySpan<char> value, ref Version ver)
        {
            Version nver = ver;

            if (GetVersionNum(value, ref ver.Major, ref ver.Minor, ref ver.Revision))
            {
                ver = nver;
                return true;
            }

            return false;
        }
        private static unsafe bool GetEdition(ReadOnlySpan<char> value, ref Edition ver)
        {
            Edition nver = ver;

            if (GetVersionNum(value, ref ver.Year, ref ver.Month, ref ver.Revision))
            {
                ver = nver;
                return true;
            }

            return false;
        }

        public static unsafe bool Read(string path, out ProjectFile proj)
        {
            proj = new ProjectFile {
                Path = path,
                Version = new Version { Major = 1, Minor = 0, Revision = 0 },
                OverrideAlloc = false,
                OverrideDealloc = false,
                HasEntry = false,
                NoDeprecated = false,
                NoWarnings = false,
                LangEdition = CURRENT_LANG_EDITION,
                UseStd = true,
                StdEdition = CURRENT_STD_EDITION,
            };
            if (!File.Exists(path))
                return false;

            using FileStream fs = File.OpenRead(path);
            using StreamReader r = new StreamReader(fs);

            bool success = false;

            Span<char> line = stackalloc char[1024];
            line.Clear();

            nint llen;
            while ((llen = r.ReadLine(line)) >= 0)
            {
                int i = line.IndexOf('=');
                if (i < 0 || i == llen - 1)
                    return false;
                Span<char> name = line[..i++];
                Span<char> value = line[i..];

                i = 0;
                while (i < name.Length)
                    name[i] = char.ToLowerInvariant(name[i++]);
                
                switch (name)
                {
                    case "id":
                        success = GetString(value, ref proj.Id);
                        break;
                    case "name":
                        if (GetString(value, ref proj.Name) && !success)
                        {
                            success = true;
                            proj.Id = proj.Name;
                        }
                        break;
                    case "version":
                        GetVersion(value, ref proj.Version);
                        break;

                    case "allocator":
                        proj.OverrideAlloc = GetString(value, ref proj.Allocator);
                        break;
                    case "deallocator":
                        proj.OverrideDealloc = GetString(value, ref proj.Deallocator);
                        break;
                    case "entry":
                        proj.HasEntry = GetString(value, ref proj.Entry);
                        break;

                    case "disallow_deprecated":
                        if (!GetBool(value, ref proj.NoDeprecated))
                            proj.NoDeprecated = false;
                        break;
                    case "disallow_warnings":
                        if (!GetBool(value, ref proj.NoWarnings))
                            proj.NoWarnings = false;
                        break;

                    case "lang_edition":
                        GetEdition(value, ref proj.LangEdition);
                        break;
                    case "use_std":
                        if (!GetBool(value, ref proj.UseStd))
                            proj.UseStd = true;
                        break;
                    case "std_edition":
                        GetEdition(value, ref proj.StdEdition);
                        break;
                }
            }

            return proj.Id != null;
        }
    }
}
