# MovieOCD

MovieOCD is a legacy movie-rating aggregator originally built around 2011-2014.

The app's goal is to search a movie title and display ratings from multiple providers in one place (historically IMDb, Rotten Tomatoes, and Netflix), along with recent searches.

## Current status

Several original provider integrations and API access models used by this project were deprecated years ago (including Netflix public API access and legacy social publish permissions).

To preserve the original product idea for portfolio/demo purposes, this repository now includes a mocked demo mode with curated sample results.

Mock titles currently supported:
- Pulp Fiction
- Inception
- The Matrix

## Branches

- `master`: Original ASP.NET MVC/Web API codebase with compatibility fixes and mock-data fallback support.
- `codex/gh-pages-demo`: Static GitHub Pages-friendly demo branch (no backend required) that showcases the MovieOCD experience with mocked data.

## Running locally (legacy app)

This is a legacy .NET Framework-era app. On macOS/Linux, it can be run with Mono/xsp in compatible environments.

```bash
msbuild MovieOCD.sln /p:Configuration=Debug
xsp4 --port 56357 --root .
```

Then open `http://127.0.0.1:56357`.

## GitHub Pages demo (same repo)

GitHub Pages cannot run the ASP.NET backend on `master`. Use the static demo branch:

1. In GitHub repo settings, open **Pages**.
2. Set source branch to `codex/gh-pages-demo` and folder to `/ (root)`.
3. Save and wait for deployment.

The published site uses mocked movie data and keeps recent searches in browser local storage.
