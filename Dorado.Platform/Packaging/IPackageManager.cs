﻿using System.IO;

namespace Dorado.Platform.Packaging
{
    public interface IPackageManager
    {
        PackageInfo Install(Stream packageStream, string location, string applicationPath);

        void Uninstall(string packageId, string applicationPath);

        PackagingResult BuildPluginPackage(string pluginName);

        PackagingResult BuildThemePackage(string themeName);
    }
}