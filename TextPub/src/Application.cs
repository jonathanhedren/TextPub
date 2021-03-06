﻿using System;
using System.Web.Hosting;
using TextPub.Collections;
using TextPub.Models;

namespace TextPub
{
    public sealed class Application
    {
        private static Lazy<PostCollection> _posts = new Lazy<PostCollection>(() => new PostCollection(GetAbsolutePath(Configuration.PostsPath), Configuration.MarkdownOptions, Configuration.PostDecoratorProvider));
        private static Lazy<PageCollection> _pages = new Lazy<PageCollection>(() => new PageCollection(GetAbsolutePath(Configuration.PagesPath), Configuration.MarkdownOptions, Configuration.PageDecoratorProvider));
        private static Lazy<SnippetCollection> _snippets = new Lazy<SnippetCollection>(() => new SnippetCollection(GetAbsolutePath(Configuration.SnippetsPath), Configuration.MarkdownOptions, Configuration.SnippetDecoratorProvider));

        private static IConfiguration _configuration;

        //private static Application _instance = new Application();

        //public static Application Instance 
        //{ 
        //    get {
        //        return _instance;
        //    }
        //}

        public static IConfiguration Configuration
        {
            get
            {
                if (_configuration == null)
                {
                    _configuration = new WebConfiguration();
                }

                return _configuration;
            }
            set
            {
                _configuration = value;
            }
        }

        /// <summary>
        /// Returns a collection of posts. By default the posts is sorted by publish date (descending).
        /// </summary>
        /// <returns></returns>
        public static IPostCollection Posts
        {
            get
            {
                return _posts.Value;
            }
        }
                
        /// <summary>
        /// Returns all pages.
        /// </summary>
        /// <returns></returns>
        public static IModelCollection<IPage> Pages
        {
            get
            {
                return _pages.Value;
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        public static IModelCollection<ISnippet> Snippets
        {
            get
            {
                return _snippets.Value;
            }
        }

        public static void ClearCaches()
        {
            if (_posts.IsValueCreated)
                _posts.Value.ClearCache();

            if (_pages.IsValueCreated)
                _pages.Value.ClearCache();

            if (_snippets.IsValueCreated)
                _snippets.Value.ClearCache();
        }

        private static string GetAbsolutePath(string path)
        {
            var basePath = Configuration.BasePath;
            if (basePath.StartsWith("~"))
            {
                basePath = HostingEnvironment.MapPath(basePath);
            }

            return basePath + "/" + path;
        }
    }
}
