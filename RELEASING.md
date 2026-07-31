# Release process

The library's version lives in three properties in
[QrCodeGenerator/QrCodeGenerator.csproj](QrCodeGenerator/QrCodeGenerator.csproj):
`Version`, `PackageVersion`, `FileVersion`. Between releases it
carries a `-dev` prerelease suffix (e.g. `3.1.0-dev`) so it's always ordered above the
last release but never resolved by a floating `3.*` `PackageReference` (NuGet excludes
prereleases from floating ranges by default). `README.md`, `QrCodeGenerator/docs/README.md`,
and the demo projects' `PackageReference` versions stay pinned to the last published
release at all times — they're only updated as part of a release, never in between.

## Steps

1. Update `QrCodeGenerator/QrCodeGenerator.csproj`: set `Version`, `PackageVersion`,
   `FileVersion` (but not `AssemblyVersion`) to the release version `X.Y.Z` (drop the `-dev`
   suffix), and update `PackageReleaseNotes`.
2. Update `README.md`: install command and any prose version references → `X.Y.Z`.
3. Update `QrCodeGenerator/docs/README.md` the same way, including its
   `.../blob/vX.Y.Z/...` tag-pinned links.
5. Commit as `Release vX.Y.Z`, tag the commit `vX.Y.Z`, push commit and tag.
6. `dotnet pack -c Release` and publish the resulting nupkg to nuget.org (manual today).
7. Bump `QrCodeGenerator/QrCodeGenerator.csproj` to the next planned version with a
   `-dev` suffix (e.g. `3.2.0-dev`), commit as `Bump version to 3.2.0-dev for development`.
   Leave `README.md`, `QrCodeGenerator/docs/README.md`, and the demo projects untouched —
   they keep pointing at `X.Y.Z` until the next release.

## Why examples still build against HEAD

`.github/workflows/demos.yaml` packs the current source into a local NuGet feed and,
before building each demo, overrides that demo's resolved package version for the CI
run only (`dotnet add package ... --source Local`, which rewrites the checked-out
`.csproj` in the runner's workspace — nothing is committed). This means CI always
validates the examples against the in-progress library code, even though the
`PackageReference` version committed to the repo stays pinned to the last release.
