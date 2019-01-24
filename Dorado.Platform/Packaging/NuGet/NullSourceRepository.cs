﻿using NuGet;
using System.Linq;

namespace Dorado.Platform.Packaging
{
    /// <summary>
    /// This repository implementation fakes a source (remote) repository
    /// </summary>
    internal class NullSourceRepository : PackageRepositoryBase
    {
        public override IQueryable<IPackage> GetPackages()
        {
            return Enumerable.Empty<IPackage>().AsQueryable();
        }

        public override string Source
        {
            get { return string.Empty; }
        }

        public override bool SupportsPrereleasePackages
        {
            get { return true; }
        }

        public override void AddPackage(IPackage package)
        {
        }

        public override void RemovePackage(IPackage package)
        {
        }
    }
}