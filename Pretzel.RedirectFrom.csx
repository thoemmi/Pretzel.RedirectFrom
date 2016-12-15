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
    private bool _shouldGenerateAspx;

    [ImportingConstructor]
    public RedirectFromTransform(IFileSystem fileSystem) {
        _fileSystem = fileSystem;
    }

    public void Transform(SiteContext siteContext) {
        _shouldGenerateAspx = ShouldGenerateAspx(siteContext);

        foreach (var post in siteContext.Posts.Concat(siteContext.Pages).Where(p => !(p is NonProcessedPage))) {
            object obj;
            if (post.Bag.TryGetValue("redirect_from", out obj)) {
                var sourceUrls = obj as IEnumerable<string>;
                if (sourceUrls != null) {
                    WriteRedirectFile(siteContext, post, sourceUrls);
                } else {
                    var sourceUrl = obj as string;
                    if (sourceUrl != null) {
                        WriteRedirectFile(siteContext, post, new [] { sourceUrl });
                    }
                }
            }
        }
    }

    private void WriteRedirectFile(SiteContext siteContext, Page post, IEnumerable<string> sourceUrls) {
        var targetUrl = post.Url;
        var htmlContent = String.Format(RedirectHtmlTemplate, targetUrl);
        var aspxContent = _shouldGenerateAspx ? String.Format(RedirectAspxTemplate, targetUrl) : null;

        foreach (var sourceUrl in sourceUrls) {
            try {
                var filepath = _fileSystem.Path.Combine(siteContext.OutputFolder, sourceUrl.TrimStart('/').Replace('/', _fileSystem.Path.DirectorySeparatorChar));
                if (_shouldGenerateAspx && _fileSystem.Path.GetExtension(sourceUrl).ToLowerInvariant() == ".aspx") {
                    SaveContent(filepath, aspxContent);
                } else {
                    filepath = _fileSystem.Path.Combine(filepath, "index.html");
                    SaveContent(filepath, htmlContent);
                }
            } catch (Exception ex) {
                Console.WriteLine("Generating redirect for {0} at {1} failed:{2}{3}", post.Id, sourceUrl, Environment.NewLine, ex);
            }
        }
    }

    private void SaveContent(string filename, string content) {
        string directoryName = _fileSystem.Path.GetDirectoryName(filename);
        if (!_fileSystem.Directory.Exists(directoryName)) {
            _fileSystem.Directory.CreateDirectory(directoryName);
        }
        _fileSystem.File.WriteAllText(filename, content);
    }

    private static bool ShouldGenerateAspx(SiteContext context) {
        object obj;
        if (!context.Config.TryGetValue("redirect_generate_aspx", out obj) || !(obj is bool))
            return false;
        return (bool) obj;
    }

    private static string RedirectHtmlTemplate = @"<!DOCTYPE html>
<meta charset=""utf-8"" />
<title>Redirecting...</title>
<link rel=""canonical"" href=""{0}"" />
<meta http-equiv=""refresh"" content=""0; url={0}"" />
<h1>Redirecting...</h1>
<a href=""{0}"">Click here if you are not redirected.</a>
<script>
    location=""{0}""
</script>";
    private static string RedirectAspxTemplate = @"<%@ Page Language=""C#""%>
<script runat=""server"">
private void Page_Load(object sender, System.EventArgs e)
{{
    Response.StatusCode = 301;
    Response.Status = ""301 Moved Permanently"";
    Response.AddHeader(""Location"",""{0}"");
    Response.End();
}}
</script>";
}
