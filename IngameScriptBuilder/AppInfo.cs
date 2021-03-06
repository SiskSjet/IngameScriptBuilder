﻿using System;
using System.IO;
using System.Reflection;

namespace IngameScriptBuilder {
    public static class AppInfo {
        private static readonly Assembly Assembly = Assembly.GetEntryAssembly();
        private static Version _assemblyVersion;
        private static string _company;
        private static string _copyright;
        private static string _description;
        private static string _fileName;
        private static Version _fileVersion;
        private static string _product;
        private static string _productVersion;
        private static string _title;

        public static Version AssemblyVersion => _assemblyVersion ?? (_assemblyVersion = Assembly.GetName().Version);
        public static string Company => _company ?? (_company = GetCustomAttributes<AssemblyCompanyAttribute>().Company);
        public static string Copyright => _copyright ?? (_copyright = GetCustomAttributes<AssemblyCopyrightAttribute>().Copyright);
        public static string Description => _description ?? (_description = GetCustomAttributes<AssemblyDescriptionAttribute>().Description);
        public static string FileName => _fileName ?? (_fileName = Path.GetFileName(Location));

        public static Version FileVersion {
            get {
                if (_fileVersion == null) {
                    Version result;
                    Version.TryParse(GetCustomAttributes<AssemblyFileVersionAttribute>().Version, out result);
                    _fileVersion = result;
                }
                return _fileVersion;
            }
        }

        public static string Location => Assembly.Location;
        public static string Product => _product ?? (_product = GetCustomAttributes<AssemblyProductAttribute>().Product);
        public static string ProductVersion => _productVersion ?? (_productVersion = GetCustomAttributes<AssemblyInformationalVersionAttribute>().InformationalVersion);

        public static string Title => _title ?? (_title = GetCustomAttributes<AssemblyTitleAttribute>().Title);

        private static T GetCustomAttributes<T>() where T : Attribute {
            var attributes = Assembly.GetCustomAttributes(typeof(T), true);
            if (attributes.Length == 0) {
                return null;
            }
            return (T) attributes[0];
        }
    }
}