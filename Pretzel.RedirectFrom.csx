using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO.Abstractions;
using System.Linq;
using Pretzel.Logic.Extensibility;
using Pretzel.Logic.Templating.Context;

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
                }
            }
        }
    }

    private void WriteRedirectFile(SiteContext siteContext, Page post, IEnumerable<string> sourceUrls) {
        var targetUrl = post.Url;
        var content = String.Format(RedirectHtmlTemplate, targetUrl);

        foreach (var sourceUrl in sourceUrls) {
            try {
                var directory = _fileSystem.Path.Combine(siteContext.OutputFolder, sourceUrl.TrimStart('/').Replace('/', '\\'));
                if (!_fileSystem.Directory.Exists(directory)) {
                    _fileSystem.Directory.CreateDirectory(directory);
                }
                _fileSystem.File.WriteAllText(_fileSystem.Path.Combine(directory, "index.html"), content);
            } catch (Exception ex) {
                Console.WriteLine("Generating redirect for {0} at {1} failed:{2}{3}", post.Id, sourceUrl, Environment.NewLine, ex);
            }
        }
    }
    
    private static string RedirectHtmlTemplate = @"<!DOCTYPE html>
<meta charset=""utf-8"" />
<title>Redirecting...</title>
<link rel =""canonical"" href=""{0}"" />
<meta http-equiv=""refresh"" content=""0; url={0}"" />
<h1>Redirecting...</h1>
<a href =""{0}"">Click here if you are not redirected.</a>
<script>
    location=""{0}""
</script>";}
