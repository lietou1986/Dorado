﻿using Dorado.Core;
using Dorado.Extensions;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Dorado.Platform.Infrastructure
{
    public class UrlRoutingModule : IHttpModule
    {
        private static readonly object ContextKey = new object();
        private static readonly ConcurrentBag<RoutablePath> Routes = new ConcurrentBag<RoutablePath>();

        public void Init(HttpApplication application)
        {
            if (application.Context.Items[ContextKey] == null)
            {
                application.Context.Items[ContextKey] = ContextKey;
                application.PostResolveRequestCache += OnApplicationPostResolveRequestCache;
            }
        }

        private void OnApplicationPostResolveRequestCache(object sender, EventArgs e)
        {
            var application = (HttpApplication)sender;
            var context = new HttpContextWrapper(application.Context);
            this.PostResolveRequestCache(context);
        }

        /// <summary>
        /// Registers a path pattern which should be handled by the <see cref="UrlRoutingModule"/>
        /// </summary>
        /// <param name="path">The app relative path to handle (regular expression, e.g. <c>/sitemap.xml</c> or <c>/mini-profiler-resources/(.*)</c>)</param>
        /// <param name="verb">The http method constraint (regular expression, e.g. <c>GET|POST</c>)</param>
        /// <remarks>
        /// For performance reasons Dorado.Platform.NET is configured to serve static files (files with extensions other than those mapped to managed handlers)
        /// through the native <c>StaticFileModule</c>. This method lets you define exceptions to this default rule: every path registered as routable gets
        /// handled by the <see cref="UrlRoutingModule"/> and therefore enables dynamic processing of (physically non-existant) static files.
        /// </remarks>
        public static void RegisterRoutablePath(string path, string verb = ".*")
        {
            Guard.ArgumentNotEmpty(() => path);

            if (RegularExpressions.IsWebUrl.IsMatch(path))
            {
                throw new ArgumentException("Only relative paths are allowed.", "path");
            }

            var rgPath = new Regex(path, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var rgVerb = new Regex(verb, RegexOptions.CultureInvariant | RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);

            Routes.Add(new RoutablePath { PathPattern = rgPath, HttpMethodPattern = rgVerb });
        }

        public static bool HasMatchingPathHandler(string path, string method = "GET")
        {
            Guard.ArgumentNotEmpty(() => path);
            Guard.ArgumentNotEmpty(() => method);

            if (Routes.Count > 0)
            {
                return Routes.Any(route => route.PathPattern.IsMatch(path) && route.HttpMethodPattern.IsMatch(method));
            }

            return false;
        }

        public virtual void PostResolveRequestCache(HttpContextBase context)
        {
            var request = context.Request;
            if (request == null || Routes.Count == 0)
                return;

            var path = request.AppRelativeCurrentExecutionFilePath.TrimStart('~').TrimEnd('/');
            var method = request.HttpMethod.EmptyNull();

            if (Routes.Any(route => route.PathPattern.IsMatch(path) && route.HttpMethodPattern.IsMatch(method)))
            {
                var module = new UrlRoutingModule();
                module.PostResolveRequestCache(context);
            }
        }

        public void Dispose()
        {
            // nothing to dispose
        }

        private class RoutablePath
        {
            public Regex PathPattern { get; set; }
            public Regex HttpMethodPattern { get; set; }
        }
    }
}