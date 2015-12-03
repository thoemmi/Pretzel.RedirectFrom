## Pretzel.RedirectFrom

This is a plugin for [Pretzel](https://github.com/Code52/pretzel), a static site generation tool following (more or less) the same conventions as [Jekyll](https://github.com/mojombo/jekyll). It mimics the [JekyllRedirectFrom gem](https://github.com/jekyll/jekyll-redirect-from) for Jekyll. 

Sometimes when migrating a site to Pretzel, you may change the structure of your site. This plugin helps to redirect old URLs to new locations.

### Installation

Download the [latest `Pretzel.RedirectFrom.dll`](https://github.com/thoemmi/Pretzel.RedirectFrom/releases) and save it to the `_plugin` folder at the root of your site folder.

### Usage

Add the old URL to the front-matter of your post or page:

```
---
title: Awesome page
redirect_from:
  - /pages/old-awesome-url.html
---
...
```

This will generate a new file `index.html` in the folder `\pages\old-awesome-url.html\` with an HTTP-REFRESH meta tag which redirects to the new page URL.