# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

- **Dev server**: `hugo server`
- **Build**: `hugo --minify` (outputs to `./public/`)
- **New blog post**: `hugo new blog/my-new-post.md`
- **New page**: `hugo new my-new-page.md`

Deployment is automatic via GitHub Actions (`.github/workflows/hugo.yml`) on push to `master`, deploying to GitHub Pages at `https://issac-lopez.github.io/`.

## Architecture

This is a minimal Hugo static blog using the [hugo-bearblog](https://github.com/janraasch/hugo-bearblog) theme, installed as a git submodule in `themes/hugo-bearblog/`.

**Content** lives in `content/`:
- `_index.md` — homepage
- `blog/` — blog posts (listed at `/blog/`)
- Other `.md` files become top-level pages linked in the main nav

**Front matter** uses TOML (` +++ ` delimiters). New content starts as `draft: true`.

**URL structure**: Blog posts resolve to `/:slug/` (no `/blog/` prefix in URLs). Tags use `/blog/:slug`. Taxonomies are otherwise disabled.

**Theme customization**: Override layouts by creating matching files under `layouts/`. Add custom CSS/JS via `layouts/partials/custom_head.html` or `layouts/partials/custom_body.html` — these are currently empty in the project but respected by the theme.

**Static assets** (`static/`) are copied as-is: favicons live at `static/favicon.ico` and `static/images/`.

The `public/` directory is the build output — it is deployed but should not be manually edited.
