using System.Linq;

#pragma warning disable CS8632

namespace System.Reflection
{
    public static class AssemblyExtensions
    {
        private static string s_copyright;

        private static string s_product;

        private static string s_company;

        private static string s_title;

        private static string s_description;

        private static Version? s_version;

        private static Version? s_fileVersion;

        private static string s_informationalVersion;

        public static string Copyright(this Assembly @this)
        {
            if (string.IsNullOrWhiteSpace(s_copyright))
            {
                s_copyright = @this.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), true).OfType<AssemblyCopyrightAttribute>().FirstOrDefault()?.Copyright;
            }

            return s_copyright;
        }

        public static string Product(this Assembly @this)
        {
            if (string.IsNullOrWhiteSpace(s_product))
            {
                s_product = @this.GetCustomAttributes(typeof(AssemblyProductAttribute), true).OfType<AssemblyProductAttribute>().FirstOrDefault()?.Product;
            }

            return s_product;
        }

        public static string Company(this Assembly @this)
        {
            if (string.IsNullOrWhiteSpace(s_company))
            {
                s_company = @this.GetCustomAttributes(typeof(AssemblyCompanyAttribute), true).OfType<AssemblyCompanyAttribute>().FirstOrDefault()?.Company;
            }

            return s_company;
        }

        public static string Title(this Assembly @this)
        {
            if (string.IsNullOrWhiteSpace(s_title))
            {
                s_title = @this.GetCustomAttributes(typeof(AssemblyTitleAttribute), true).OfType<AssemblyTitleAttribute>().FirstOrDefault()?.Title;
            }

            return s_title;
        }

        public static string Description(this Assembly @this)
        {
            if (string.IsNullOrWhiteSpace(s_description))
            {
                s_description = @this.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), true).OfType<AssemblyDescriptionAttribute>().FirstOrDefault()?.Description;
            }

            return s_description;
        }

        public static Version? Version(this Assembly @this)
        {
            if (s_version == null)
            {
                var versionString = @this.GetCustomAttributes(typeof(AssemblyVersionAttribute), true).OfType<AssemblyVersionAttribute>().FirstOrDefault()?.Version;
                _ = System.Version.TryParse(versionString, out s_version);
            }

            return s_version;
        }

        public static Version? FileVersion(this Assembly @this)
        {
            if (s_fileVersion == null)
            {
                var fileVersionString = @this.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), true).OfType<AssemblyFileVersionAttribute>().FirstOrDefault()?.Version;
                _ = System.Version.TryParse(fileVersionString, out s_fileVersion);
            }

            return s_version;
        }

        public static string InformationalVersion(this Assembly @this)
        {
            if (string.IsNullOrWhiteSpace(s_informationalVersion))
            {
                s_informationalVersion = @this.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), true).OfType<AssemblyInformationalVersionAttribute>().FirstOrDefault()?.InformationalVersion;
            }

            return s_informationalVersion;
        }
    }
}
