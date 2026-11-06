# Stage 1: Reliable Tagging Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make auto-tagging trustworthy: no silent overwrites, no bogus conflicts from empty values, correct Last.fm/Genius lookups, new tag and cover sources, and a "ready / needs review" results screen.

**Architecture:** Pure, unit-testable helpers (value policy, title matching, readiness, advisor, JSON parsers) sit under `Music/Metadata` and `Services`; providers and view models stay thin and call them. New sources are `TrackDetailsProvider` subclasses discovered by the existing reflection-based registration. The results screen replaces the conflict list with a per-track list (variant B); the existing per-conflict window becomes the detail view.

**Tech Stack:** C# / .NET 8, Avalonia 11, CommunityToolkit.Mvvm, TagLibSharp, xunit + Moq. Source root: `source/`.

**Spec:** `docs/superpowers/specs/2026-11-06-musicdownloader-overhaul-design.md` (stage 1 only; stage 2 download statuses and stage 4 sources are separate plans).

## Global Constraints

- UI language is Russian; strings go through `Localization/Resources` (`lang:Resources.*`), never hardcoded in axaml.
- Never overwrite a meaningful existing tag without the owner's explicit choice. An existing meaningful value is one more source in a conflict, selected by default.
- "Meaningful" = not null/empty/whitespace, not `0`, not starting with "Unknown", not `TrackDetails.UnknownTitle`, and a title is not meaningful if it equals the file name without extension.
- Readiness: required tags = `Title`, `Performers`; recommended = `Album`, `Genres`, `Year`, `Track` + cover. "Ready" = no conflicts and required filled; recommended completeness is shown separately and never blocks "ready". All other tags in `Tags.AllTags` are "extras", counted separately.
- API keys are entered by the owner in settings (`TokenStorage`); never embed new keys. Every external source fails independently (its own try/catch, logged, no effect on other sources).
- Tests must run offline; fixtures are hand-written JSON strings from public API docs. `CoverTag` has a static initializer that needs `App.Services`; tests must not instantiate it.
- Test commands (the user's global NuGet config has a broken source): from `source/` run `dotnet restore SUSUProgramming.Tests --source https://api.nuget.org/v3/index.json` once, then `dotnet test SUSUProgramming.Tests --no-restore`. Baseline: 21 pass, 2 pre-existing failures in `AppConfigTests`; do not fix them here.
- `SUSUProgramming.Tests.csproj` excludes `TagServiceTests.cs` from compilation; leave that exclusion, put new tests in new files.

## Review Focus

- A source returns `0`/empty for year, track number or album: must neither create a conflict option nor erase an existing value (Task 1, 2).
- Track already has a meaningful year and sources agree with it: nothing changes, no conflict (Task 2).
- Track title differs only by case, punctuation or a `(feat. X)` suffix between sources: treated as the same track (Task 3).
- Genius search returns only unrelated tracks: no tags taken from it (Task 4).
- MusicBrainz returns no match, malformed JSON, or HTTP failure: provider returns `null`, other providers still run (Task 5).
- Track with no album (single): cover/year still filled from sources that search by artist + title (Task 5, 6).

---

### Task 1: Value policy and title matching

**Files:**
- Create: `SUSUProgramming.MusicDownloader/Music/Metadata/TagValuePolicy.cs`
- Create: `SUSUProgramming.MusicDownloader/Music/Metadata/TrackTitleMatcher.cs`
- Test: `SUSUProgramming.Tests/TagValuePolicyTests.cs`, `SUSUProgramming.Tests/TrackTitleMatcherTests.cs`

**Interfaces:**
- Produces:
  - `internal static class TagValuePolicy { public static bool IsMeaningful(ITag tag, string? fileNameWithoutExtension = null); }`
  - `internal static class TrackTitleMatcher { public static string Normalize(string s); public static bool IsSameTrack(string titleA, string artistA, string titleB, string artistB); public static int IndexOfTitle(IEnumerable<string> titles, string title); }` (`IndexOfTitle` returns -1 when absent, otherwise a zero-based index.)

- [ ] **Step 1: Write failing tests.** `TagValuePolicyTests`: `Year0_NotMeaningful`, `Year2019_Meaningful`, `TitleEqualsFileName_NotMeaningful` (`Tags.Title + "Song"`, file name `"Song"`), `TitleUnknown_NotMeaningful` (`"Untitled"`), `PerformersUnknownArtist_NotMeaningful` (`["Unknown Artist"]`), `EmptyStringArray_NotMeaningful`, `RealAlbum_Meaningful`. `TrackTitleMatcherTests`: `Normalize("Hello (feat. B)!")` equals `Normalize("hello")`; `IsSameTrack("Song","A","SONG ","a")` true; `IsSameTrack("Song","A","Other","A")` false; `IsSameTrack("Song","A, B","Song","A")` true (first-artist overlap); `IndexOfTitle(["One","Two (Live)"], "two")` is 1; absent title gives -1.
- [ ] **Step 2: Run** `dotnet test SUSUProgramming.Tests --no-restore --filter "FullyQualifiedName~TagValuePolicy|FullyQualifiedName~TrackTitleMatcher"`. Expected: build FAIL (types missing).
- [ ] **Step 3: Implement both classes** per the signatures. `Normalize`: lower-invariant, drop bracketed `(...)`/`[...]` groups containing feat/ft/with/remaster/live only when matching suffixes, strip punctuation, collapse spaces. `IsSameTrack`: normalized titles equal and at least one normalized artist token shared.
- [ ] **Step 4: Run the same command.** Expected: PASS.
- [ ] **Step 5: Commit** `git add -A && git commit -m "feat: add tag value policy and title matcher"` (with the co-author trailer).

### Task 2: Existing values as a source; advice; no overwrite

**Files:**
- Modify: `SUSUProgramming.MusicDownloader/Services/TaggingConflictInfo.cs` (Accumulate, new members)
- Modify: `SUSUProgramming.MusicDownloader/Services/TagService.cs` (`TagAsync`, `CombineResults`)
- Modify: `SUSUProgramming.MusicDownloader/Services/ConflictsCollection.cs` (`AutoResolve`)
- Test: `SUSUProgramming.Tests/TaggingConflictTests.cs`

**Interfaces:**
- Consumes: `TagValuePolicy.IsMeaningful`.
- Produces on `TaggingConflictInfo`:
  - `public void Accumulate(ITag tag, string source)` — ignores non-meaningful tags; records `source` as a vote for that value; keeps the old one-argument overload delegating with source `"?"`.
  - `public int? CurrentCase { get; }` — index in `FoundData` of the value already on the track, or null.
  - `public int? RecommendedCase { get; }` — index of the value with strictly the most distinct sources, null on a tie.
  - `public IReadOnlyList<string> SourcesOf(int index)`.
- `TagService.CombineResults(AppConfig, TrackDetails, TaggingResult)` keeps its signature but never replaces a meaningful original tag; `AppConfig.RewriteMetadata` is no longer consulted.

- [ ] **Step 1: Write failing tests** in `TaggingConflictTests` (use `Tags.Year`, `Tags.Title`; no covers): `Accumulate_ZeroYear_IsIgnored`; `Accumulate_SameValueTwoSources_OneCaseTwoSources`; `Recommended_TwoSourcesAgreeOneDiffers_PicksMajority` (X from LastFm and MusicBrainz, Y from Genius → `RecommendedCase` points to X); `Recommended_Tie_IsNull`; `Current_ExistingValueMarked` (accumulate with source `"File"` → `CurrentCase` set); `AutoResolve_SingleMeaningfulValue_Fills`; `AutoResolve_ExistingPlusDifferentValue_StaysConflict`; `AutoResolve_ExistingEqualsFound_NoChange`; `CombineResults_RewriteMetadataTrue_DoesNotOverwriteMeaningfulTag` (original Title "A", result Title "B", settings `new AppConfig(".") { RewriteMetadata = true }` → "A"); `CombineResults_OriginalPlaceholder_IsFilled` (original Year 0 → absent; result Year 2019 → 2019).
- [ ] **Step 2: Run** `--filter FullyQualifiedName~TaggingConflictTests`. Expected: FAIL.
- [ ] **Step 3: Implement.** In `TagService.TagAsync`, before provider loops, seed each conflict lazily with the track's own meaningful tag using source `"File"` (only for tags a provider returns). Pass `provider.GetType().Name` as source. `AutoResolve` adds a conflict's single value only if `CurrentCase` is null or equal to it.
- [ ] **Step 4: Run the filter.** Expected: PASS; then run the whole suite — still 21+new pass, same 2 failures.
- [ ] **Step 5: Commit** `fix: stop bogus conflicts, never overwrite meaningful tags, add advice`.

### Task 3: Readiness evaluation

**Files:**
- Create: `SUSUProgramming.MusicDownloader/Music/Metadata/TagCompleteness.cs`
- Modify: `SUSUProgramming.MusicDownloader/ViewModels/TrackViewModel.cs:401-446` (`ResetState` calls the new class; `RequiredTags`/`RecommendedTags` move there)
- Test: `SUSUProgramming.Tests/TagCompletenessTests.cs`

**Interfaces:**
- Produces: `internal static class TagCompleteness { public static readonly string[] Required, Recommended; public static TrackProcessingState Evaluate(TrackDetails track); public static bool IsReady(TrackDetails track, bool hasConflicts); public static int ExtrasFilled(TrackDetails track); }`. `Evaluate` returns `Untagged`/`Incomplete`/`Complete` exactly as `ResetState` does today.

- [ ] **Step 1: Failing tests:** title+performers only → `Incomplete`; none → `Untagged`; all recommended but no cover → `Incomplete` (cover counted through `HasCover`, set via `SetTag(nameof(VirtualTags.HasCover), true)`); `IsReady(titleAndPerformers, hasConflicts:false)` true; `IsReady(..., true)` false; `ExtrasFilled` counts `Lyrics`, `Description`, `Comment`, `Subtitle`, `Disc`, `DiscCount`, `AlbumArtists`, `PerformersRole`, `TrackCount` only.
- [ ] **Step 2: Run** `--filter FullyQualifiedName~TagCompletenessTests`. Expected: FAIL.
- [ ] **Step 3: Implement** and switch `ResetState` to `State = TagCompleteness.Evaluate(track)`.
- [ ] **Step 4: Run the filter.** Expected: PASS.
- [ ] **Step 5: Commit** `refactor: extract tag completeness rules`.

### Task 4: Fix Last.fm and verify Genius matches

**Files:**
- Modify: `Music/Metadata/DetailProviders/LastFmProvider.cs:45-90`
- Modify: `Music/Metadata/DetailProviders/GeniusProvider.cs:28-92`
- Test: `SUSUProgramming.Tests/GeniusHitSelectionTests.cs`

**Interfaces:**
- Consumes: `TrackTitleMatcher`.
- Produces: `internal readonly record struct GeniusHit(int Id, string Title, string ArtistNames, string Url); internal static GeniusHit? GeniusProvider.PickMatchingHit(IEnumerable<GeniusHit> hits, string title, string artist);` (checks the first 5 hits with `IsSameTrack`).

- [ ] **Step 1: Failing tests:** `PickMatchingHit` returns the 2nd hit when the 1st is another song; returns null when none match; matches ignoring case and `(feat. …)`.
- [ ] **Step 2: Run** `--filter FullyQualifiedName~GeniusHitSelection`. Expected: FAIL.
- [ ] **Step 3: Implement `PickMatchingHit`** and use it in `SearchTrackDetailsAsync` instead of `hits[0]`; map the `dynamic` hits into `GeniusHit` first. In `LastFmProvider`: call `Album.GetInfoAsync(track.ArtistName, track.AlbumName, true)` (Last.fm's own names), compute the track number with `TrackTitleMatcher.IndexOfTitle(album.Tracks.Select(x => x.Name), track.Name)` and emit `Tags.Track` only when the index is >= 0 (index + 1), emit `Tags.Year` only when `ReleaseDateUtc` year is >= 1900. Do not log API tokens.
- [ ] **Step 4: Run the filter and the whole suite.** Expected: PASS. Last.fm network behavior is verified manually in Task 9.
- [ ] **Step 5: Commit** `fix: stricter Last.fm track number/year and Genius hit matching`.

### Task 5: MusicBrainz + Cover Art Archive provider

**Files:**
- Create: `Music/Metadata/DetailProviders/MusicBrainzParser.cs`, `Music/Metadata/DetailProviders/MusicBrainzProvider.cs`
- Test: `SUSUProgramming.Tests/MusicBrainzParserTests.cs`

**Interfaces:**
- Produces: `internal record MusicBrainzMatch(string Title, string[] Artists, string? Album, uint Year, uint Track, uint TrackCount, string? ReleaseId);` `internal static MusicBrainzMatch? MusicBrainzParser.Parse(string json, string title, string artist);` `MusicBrainzProvider : TrackDetailsProvider` (base url `https://musicbrainz.org/ws/2/`).

- [ ] **Step 1: Failing tests** with a hand-written recording-search JSON (`recordings[]` with `title`, `artist-credit[].name`, `releases[]` having `id`, `title`, `date`, `media[0].track-count`, `media[0].track[0].number`): picks the recording matching title+artist via `IsSameTrack`; year parsed from `date` `"2019-05-03"` and `"2019"`; missing `date` gives year 0; empty `recordings` gives null; malformed JSON gives null (no exception).
- [ ] **Step 2: Run** `--filter FullyQualifiedName~MusicBrainzParser`. Expected: FAIL.
- [ ] **Step 3: Implement the parser** with `System.Text.Json` and the provider: query `recording?query=recording:"{title}" AND artist:"{artist}"&fmt=json&limit=5`, a descriptive `User-Agent` header, at most one request per second (shared static limiter), cover URL `https://coverartarchive.org/release/{ReleaseId}/front-1200` via `CoverTag.DownloadCoverAsync`. Return `Title`, `Performers`, `Album`, `Year`, `Track`, `TrackCount`, cover; skip zero values (Task 2 also filters). Confirm registration happens by reflection in `ServiceRegistration.cs`; if not, register it there.
- [ ] **Step 4: Run the filter.** Expected: PASS.
- [ ] **Step 5: Commit** `feat: add MusicBrainz and Cover Art Archive tag source`.

### Task 6: iTunes and Deezer cover sources

**Files:**
- Create: `Music/Metadata/DetailProviders/CoverSearchParsers.cs`, `ItunesCoverProvider.cs`, `DeezerCoverProvider.cs`
- Test: `SUSUProgramming.Tests/CoverSearchParsersTests.cs`

**Interfaces:**
- Produces: `internal static Uri? CoverSearchParsers.ParseItunes(string json, string title, string artist)` (matching result's `artworkUrl100` with `100x100bb` replaced by `1000x1000bb`); `internal static Uri? CoverSearchParsers.ParseDeezer(string json, string title, string artist)` (matching `data[]` item's `album.cover_xl`). Both providers return a `TrackDetails` holding only the cover.

- [ ] **Step 1: Failing tests:** each parser picks the matching item (not first), returns the enlarged URL, and returns null for no match and for malformed JSON.
- [ ] **Step 2: Run** `--filter FullyQualifiedName~CoverSearchParsers`. Expected: FAIL.
- [ ] **Step 3: Implement** parsers and providers (iTunes: `https://itunes.apple.com/search?term={artist} {title}&entity=song&limit=5`; Deezer: `https://api.deezer.com/search?q=artist:"{artist}" track:"{title}"&limit=5`). Verify both endpoints are currently reachable before relying on them; if one is unusable, drop it and note it in the commit message.
- [ ] **Step 4: Run the filter.** Expected: PASS.
- [ ] **Step 5: Commit** `feat: add iTunes and Deezer cover sources`.

### Task 7: Results list (variant B) with recommendations

**Files:**
- Modify: `ViewModels/AutoTaggingViewModel.cs` (per-track result items), `ViewModels/ConflictViewModel.cs`, `ViewModels/ConflictValueViewModel.cs` (show sources, recommended/current markers, cover pixel size)
- Create: `ViewModels/TaggingResultItemViewModel.cs`
- Modify: `Views/ConflictsResolveWindow.axaml` and `.axaml.cs`, `Views/Library/UnsortedTracksView.axaml(.cs)` and `MyTracksView.axaml(.cs)` (open the results list)
- Modify: `Localization/Resources.resx` (+ designer) with new Russian strings
- Test: `SUSUProgramming.Tests/TaggingResultItemTests.cs`

**Interfaces:**
- Consumes: `TagCompleteness`, `TaggingConflictInfo.RecommendedCase/CurrentCase/SourcesOf`.
- Produces: `TaggingResultItemViewModel(TrackViewModel track, IReadOnlyList<TaggingConflictInfo> conflicts)` with `bool IsReady`, `string Reason` (e.g. conflicting field names, "нет номера трека и года"), `IReadOnlyList<ConflictViewModel> Conflicts`; `AutoTaggingViewModel.Items : ObservableCollection<TaggingResultItemViewModel>`, `int ReadyCount`, `void ApplyReady()` (saves every ready track once).

- [ ] **Step 1: Failing tests** for the pure parts: `IsReady` false when conflicts exist; `Reason` lists conflicted tag names; `ApplyReady` on an `AutoTaggingViewModel` with fake items saves only ready ones (use a small `ISaveTrack` seam so the test needs no UI).
- [ ] **Step 2: Run** `--filter FullyQualifiedName~TaggingResultItem`. Expected: FAIL.
- [ ] **Step 3: Implement** view models, then the window: header "найдено N · готово M · проверить K", a row per track (✅ / ⚠️ / ❌ + reason + action button), button "Применить готовые (M)", and the existing per-field table opened from a row's button. In the table, mark the recommended value and the current value, show source names, and for covers show pixel size. Conflict resolution stays manual; never auto-select a recommendation.
- [ ] **Step 4: Run the filter and the whole suite**, then `dotnet build` the app. Expected: tests PASS, build succeeds.
- [ ] **Step 5: Commit** `feat: results list with ready/needs-review tracks and recommendations`.

### Task 8: Cover input helpers and browser search buttons

**Files:**
- Modify: `ViewModels/TrackViewModel.cs` (new commands), the track editing view `Views/Library/*` where the existing "cover from clipboard link" button lives (find it with a search for the clipboard handler), `Localization/Resources.resx`
- Create: `Services/WebSearchLauncher.cs`
- Test: `SUSUProgramming.Tests/WebSearchLauncherTests.cs`

**Interfaces:**
- Produces: `internal static class WebSearchLauncher { public static Uri LyricsQuery(string artist, string title); public static Uri CoverQuery(string artist, string title); public static void Open(Uri uri); }` — queries are `https://www.google.com/search?q={escaped "artist title текст песни"}` and an image search (`&tbm=isch`) for covers.

- [ ] **Step 1: Failing tests:** `LyricsQuery("A B","C&D")` contains an escaped `q=` with both words and no raw `&D`; `CoverQuery` contains `tbm=isch`.
- [ ] **Step 2: Run** `--filter FullyQualifiedName~WebSearchLauncher`. Expected: FAIL.
- [ ] **Step 3: Implement** the launcher (`Process.Start` with `UseShellExecute = true` only for http/https URIs). Add buttons "Найти текст песни в браузере" and "Найти обложку в браузере" next to the existing cover-by-link button (keep it); add cover from a clipboard image and from a file picker/drag-and-drop to the same area.
- [ ] **Step 4: Run the filter.** Expected: PASS.
- [ ] **Step 5: Commit** `feat: browser search buttons and more ways to set a cover`.

### Task 9: Manual end-to-end check

- [ ] **Step 1:** Run the app (`dotnet run --project SUSUProgramming.MusicDownloader.Desktop`; use the `run` skill if available), put 5 tracks into Unsorted — one with all tags, one single without album, one with wrong casing in the title, one unknown to Last.fm, one with a year already filled.
- [ ] **Step 2:** Select all, "Найти теги", and confirm: the pre-filled year is never changed without a click; singles get a cover from iTunes/Deezer/MusicBrainz; the results list shows ready vs. needs-review counts; "Применить готовые" saves only ready tracks.
- [ ] **Step 3:** Write the observed results in the commit message of the last fix commit, or in a short note to the owner; do not claim success for anything not seen.
