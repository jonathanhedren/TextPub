﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TextPub.Models;

namespace TextPub.Collections
{
    internal class PageCollection : CachedModelCollection<Page>
    {
        private Regex _htmlHeadingRegex = new Regex(@"^\s*<h(?<HeadingLevel>\d)[^>]*?>(?<Title>.*)(?<SortOrder>\[[0-9]+\])?</h\k<HeadingLevel>>");

        public PageCollection(string path) : base(path) { }

        protected override Page CreateModel(FileInfo fileInfo, string relativePath)
        {

            var id = GenerateId(fileInfo, relativePath);
            var path = GenerateLocalPath(relativePath, fileInfo.Name);
            int? sortOrder = null;
            string title = null;

            byte[] fileContents = File.ReadAllBytes(fileInfo.FullName);

            string fileContentsString = System.Text.Encoding.UTF8.GetString(fileContents).TrimStart();

            var html = MarkdownHelper.Transform(fileContentsString);
            var match = _htmlHeadingRegex.Match(html);
            if (match.Success)
            {
                title = match.Groups["Title"].Value;

                string sortOrderString = match.Groups["SortOrder"].Value;
                if (!string.IsNullOrWhiteSpace(sortOrderString))
                {
                    try
                    {
                        sortOrder = Convert.ToInt32(sortOrderString);
                    }
                    catch { };
                }

                html = html.Substring(match.Index + match.Length);
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                title = fileInfo.Name.Substring(0, fileInfo.Name.Length - fileInfo.Extension.Length);
            }
            
            return new Page(
                id: id, 
                path: path, 
                title: title, 
                body: html, 
                level: path.Count(p => p == '/'), 
                sortOrder: sortOrder
            );
        }

        protected override void RefreshCollection()
        {
            var list = ReadFilesRecursively(_relativeFilesPath).OrderBy(p => p.Path.Length).ToList();

            // Set page parents
            foreach (Page page in list)
            {
                if (page.Level > 0)
                {
                    string parentId = page.Id.Substring(0, page.Id.LastIndexOf('/'));

                    page.Parent = list.FirstOrDefault(p => p.Id == parentId);
                }
            }

            // Set page children
            foreach (Page page in list)
            {
                page.Children = list.Where(p => p.Parent != null && p.Parent.Id == page.Id);
            }

            PutList(list.OrderBy(p => p.SortOrder).ToList());
        }
    }
}