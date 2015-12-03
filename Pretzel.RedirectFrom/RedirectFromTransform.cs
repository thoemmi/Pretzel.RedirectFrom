using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO.Abstractions;
using System.Linq;
using Pretzel.Logic.Extensibility;
using Pretzel.Logic.Templating.Context;
using Pretzel.RedirectFrom.Properties;

namespace Pretzel.RedirectFrom
{
    public class RedirectFromTransform : ITransform
    {
        private readonly IFileSystem _fileSystem;

        [ImportingConstructor]
        public RedirectFromTransform(IFileSystem fileSystem) {
            _fileSystem = fileSystem;
        }

        public void Transform(SiteContext siteContext) {
            foreach (var post in siteContext.Posts.Concat(siteContext.Pages).Where(p => !(p is NonProcessedPage))) {
                object obj;
                if (post.Bag.TryGetValue("redirect_from", out obj)) {
                    var sourceUrls = obj as IEnumerable<string>;
                    if (sourceUrls != null) {
                        WriteRedirectFile(siteContext, post, sourceUrls);
                    } else {
                        var sourceUrl = obj as string;
                        if (sourceUrl != null) {
                            WriteRedirectFile(siteContext, post, new[] { sourceUrl });
                        }
                    }
                }
            }
        }

        private void WriteRedirectFile(SiteContext siteContext, Page post, IEnumerable<string> sourceUrls) {
            var targetUrl = post.Url;
            var content = String.Format(Templates.Redirect, targetUrl);

            foreach (var sourceUrl in sourceUrls) {
                try {
                    var directory = _fileSystem.Path.Combine(siteContext.OutputFolder, sourceUrl.TrimStart('/').Replace('/', _fileSystem.Path.DirectorySeparatorChar));
                    if (!_fileSystem.Directory.Exists(directory)) {
                        _fileSystem.Directory.CreateDirectory(directory);
                    }
                    _fileSystem.File.WriteAllText(_fileSystem.Path.Combine(directory, "index.html"), content);
                } catch (Exception ex) {
                    Console.WriteLine("Generating redirect for {0} at {1} failed:{2}{3}", post.Id, sourceUrl, Environment.NewLine, ex);
                }
            }
        }
    }
}
