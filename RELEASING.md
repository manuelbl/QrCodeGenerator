# Release process

The library's version lives in three properties in
[QrCodeGenerator/QrCodeGenerator.csproj](QrCodeGenerator/QrCodeGenerator.csproj):
`Version`, `PackageVersion`, `FileVersion`. Between releases `Version` and `PackageVersion`
carry a `-dev` prerelease suffix (e.g. `3.1.0-dev`) so they're always ordered above the
last release but never resolved by a floating `3.*` `PackageReference` (NuGet excludes
prereleases from floating ranges by default). `FileVersion` stays a plain four-part number
(e.g. `3.1.0.0`) — it cannot hold a prerelease suffix. `AssemblyVersion` is only bumped on
breaking changes and is not touched by a release.

`README.md`, `QrCodeGenerator/docs/README.md` and `QrCodeGenerator/docfx/api/index.md` stay
pinned to the last published release at all times — they're only updated as part of a
release, never in between. This way everyone cloning the repository reads instructions for a
package actually available on nuget.org. The demo projects need no such updating: they use a
floating `Version="3.*"` `PackageReference`, which the `-dev` suffix keeps resolving to the
last published release.

## Steps

1. Update `QrCodeGenerator/QrCodeGenerator.csproj`: set `Version` and `PackageVersion` to the
   release version `X.Y.Z` (drop the `-dev` suffix), set `FileVersion` to `X.Y.Z.0` (but leave
   `AssemblyVersion` alone), and update `PackageReleaseNotes`.
2. Update `README.md`: install command and any prose version references → `X.Y.Z`.
3. Update `QrCodeGenerator/docs/README.md` the same way, including its
   `.../blob/vX.Y.Z/...` tag-pinned links.
4. Update `QrCodeGenerator/docfx/api/index.md`, which carries the same tag-pinned
   `.../blob/vX.Y.Z/...` links.
5. Commit as `Release vX.Y.Z` and push.
6. Run the [*Publish Release to NuGet*](https://github.com/manuelbl/QrCodeGenerator/actions/workflows/release.yml)
   workflow. It reads the version from `QrCodeGenerator/QrCodeGenerator.csproj`, runs the
   tests, packs and publishes the package, generates the API documentation with docfx, and —
   after a successful publish — creates and pushes the `vX.Y.Z` tag and creates a *draft*
   GitHub release with the nupkg and `API.Documentation.zip` attached. The job only runs if
   the deployment review is approved.
7. Review the draft release on GitHub — edit the auto-generated notes, mark it as a
   pre-release if applicable — and publish it.
8. Bump `QrCodeGenerator/QrCodeGenerator.csproj` to the next planned version with a
   `-dev` suffix (e.g. `3.2.0-dev`), commit as `Bump version to 3.2.0-dev for development`.
   Leave `README.md`, `QrCodeGenerator/docs/README.md` and `QrCodeGenerator/docfx/api/index.md`
   untouched — they keep pointing at `X.Y.Z` until the next release.

The tag and draft-release steps are idempotent: re-running the workflow for a version that
was already tagged skips the tag, and an existing release has its assets re-uploaded rather
than being recreated.

## Why examples still build against HEAD

`.github/workflows/demos.yaml` packs the current source into a local NuGet feed and,
before building each demo, overrides that demo's resolved package version for the CI
run only (`dotnet add package ... --source Local`, which rewrites the checked-out
`.csproj` in the runner's workspace — nothing is committed). This means CI always
validates the examples against the in-progress library code, even though the
`PackageReference` version committed to the repo resolves to the last release.
