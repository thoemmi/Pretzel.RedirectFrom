using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using NUnit.Framework;
using Pretzel.Logic.Templating.Context;

namespace Pretzel.RedirectFrom.Tests {
    [TestFixture]
    public class RedirectFromTransformTests {
        private SiteContextGenerator _generator;
        private MockFileSystem _fileSystem;

        private const string _postContent1 = @"---
title: Title
redirect_from:
  - /old/url/post.aspx
---
Content";
        private const string _postContent2 = @"---
title: Title
redirect_from: [/old/url/post.aspx]
---
Content";
        private const string _postContent3 = @"---
title: Title
redirect_from: /old/url/post.aspx
---
Content";

        private const string _expectedRedirectingContent = @"<!DOCTYPE html>
<meta charset=""utf-8"" />
<title>Redirecting...</title>
<link rel=""canonical"" href=""/_site/2010/01/04/test.html"" />
<meta http-equiv=""refresh"" content=""0; url=/_site/2010/01/04/test.html"" />
<h1>Redirecting...</h1>
<a href=""/_site/2010/01/04/test.html"">Click here if you are not redirected.</a>
<script>
    location='/_site/2010/01/04/test.html'
</script>";

        [SetUp]
        public void SetUp() {
            _fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>());
            _generator = new SiteContextGenerator(_fileSystem, new LinkHelper());
        }

        [Test]
        [TestCase(_postContent1, TestName = "with multi-line array of redirects")]
        [TestCase(_postContent2, TestName = "with single-line array of redirects")]
        [TestCase(_postContent3, TestName = "with redirect scalar")]
        public void RedirectFrom_creates_redirecting_file(string input) {
            // arrange
            _fileSystem.AddFile(@"C:\TestSite\_site\_posts\test.md", new MockFileData(input));
            var siteContext = _generator.BuildContext(@"C:\TestSite", @"C:\TestSite\_site", false);

            // act
            var redirectFromTransform = new RedirectFromTransform(_fileSystem);
            redirectFromTransform.Transform(siteContext);

            // assert
            Assert.True(_fileSystem.File.Exists(@"C:\TestSite\_site\old\url\post.aspx\index.html"));

            var content = _fileSystem.File.ReadAllText(@"C:\TestSite\_site\old\url\post.aspx\index.html");
            Assert.AreEqual(_expectedRedirectingContent, content);
        }
    }
}