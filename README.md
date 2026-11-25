# RTools 2.0.0 — Windows Forms / .NET Framework

## Release launch diagnostics

The desktop entry point now catches startup and UI-thread exceptions, displays the error, and writes the full exception and startup stage to `%LOCALAPPDATA%\RTools\startup-error.log` (fallback: `%TEMP%\RTools\startup-error.log`). The optional title-bar icon is extracted from the executable without depending on decoding the embedded ICO at startup. The Windows publish script also starts the actual Release executable with `--startup-test`, disables the updater for that test, verifies the window appears and closes it automatically. This test must pass before publishing. Launch the regular executable without arguments for normal use, and keep its `.exe.config` beside it. If the executable still exits before the CLR starts, inspect Windows Event Viewer > Windows Logs > Application for .NET Runtime or Application Error entries.

## 2.0.0 release — version and branding

- Release version is 2.0.0 (assembly/file/manifest version 2.0.0.0). All page designers and tools from the previous development build are retained.
- The supplied Polyvox logo is embedded in the About page resources and shown in a 24 × 24 designer-owned PictureBox immediately before the developer credit. No extra branding card or network request is needed.
- The GitHub updater still compares numeric versions; earlier development builds numbered above 2.0.0 will not automatically downgrade. Install this release manually if using one of those builds. Publish the new version file only when the release is available.

## Designer update

Every screen now has an editable Visual Studio design surface: the main window and all twelve tool pages, plus About & Help. The pages are public WinForms `UserControl` classes in `RTools/UI/Pages`. Each has a `.cs` constructor, `.Designer.cs`, `.resx` and separate `.Runtime.cs` controller. The original `ToolPages.cs`, `WorkbenchPages.cs` and `HelpPage.cs` factories have been removed. They no longer generate the visual tree.

The designer uses ordinary Windows Forms controls for reliable loading. At runtime, `DesignerPageRuntime` reads the actual designer control tree, row/column order, margins, padding, text, sizes and default values and adapts it to the existing rounded Windows 11-inspired theme. The 12-pixel bottom inset and bounded content measurement remain. Buttons, text editors, labels and grids retain their original designer instances. Other stock inputs are replaced by their themed equivalents. Fixed grid columns are designer-owned; CSV columns are still created from the imported CSV headers. The shell, tools, core logic, CLI, update checker, developer credit, original icon and red palette are retained.

### Editing any page

1. Open `RTools.sln` in Visual Studio (2022 17.8 or newer, .NET desktop development workload and .NET Framework 4.7.2 developer pack).
2. Expand `RTools > UI > Pages`. Select a page's main `.cs` file (for example, `ColorPickerPage.cs`), then right-click **View Designer** or press **Shift+F7**. Expand the file node to see its Designer, resources and Runtime code. The main window is still `MainForm.cs`.
3. Use the Properties window and **View > Other Windows > Document Outline** to select the nested cards and controls. Change text, font, default values, table cell order, column widths, margins, padding and fixed editor/list/grid sizes. Add controls inside the page's `pageLayout` table or its cards. Table rows stack vertically; side-by-side tables use the column widths; flow panels wrap action buttons. Use margins/padding for intentional blank space. Runtime labels remeasure to their actual text, and empty trailing rows are not reserved.
4. Keep existing control `Name` values stable because the corresponding `.Runtime.cs` controller binds by name. Wire new tool actions in that controller (or edit existing action logic there). A stock button's Click event is also retained. The themed combo boxes, checkboxes, number spinners, sliders and lists are replaced at runtime, so bind their runtime events using `ui.Get<T>(name)` instead of adding native input event handlers in the designer. The runtime layout adapts table cells and flow ordering rather than preserving arbitrary overlapping/free-form coordinates or cell spans in tables. Use an ordinary Panel within a cell for a free-form group.
5. Set a card's `Tag` to `surface` for its outline/background, `inherit` for a nested layout, or `swatch` for a panel that retains its color. Set a button's `Tag` to `primary` for a red action and `action` for a secondary action. Labels with `Tag=muted` use the subdued text color. Runtime light/dark palette overrides design-time colors. The design surface intentionally shows stock WinForms controls; custom rounded rendering is attached only when running.
6. About tool guide content is in `AboutHelpPage.Runtime.cs`; the console syntax and examples are synchronized from `ConsoleHelp.cs`. The version text reflects the actual assembly version at runtime.

Shared theme, painting, scrollbar, layout, runtime controllers, and nonvisual utility classes are intentionally code-only, not form design surfaces. The designer constructors call only `InitializeComponent()`; they do not fetch GitHub versions, start tools, or use Windows interop.

## 2.8.0 interface updates

- Added shared rounded, red-accented RGB sliders, integer spinners with up/down chevrons, checkboxes, checked lists, file list selection and text-input frames. Sliders support drag, track clicks, keyboard arrows, Page Up/Down, Home/End and mouse wheel while focused. Number fields retain native editable text, enforce their configured range, support keyboard arrows, mouse-wheel stepping and press-and-hold arrow repetition. Color values remain synchronized with HEX and HSL.
- Text editors, list boxes and grids keep their existing native editing/selection behavior, with rounded frames, matching selection colors and the existing modern scrollbar overlays. Drop-downs and navigation retain their existing styling.
- Reintroduced a fixed 12-pixel bottom inset for the dashboard and all tool/help pages. The inset is included once in the measured content height, not in a recurring scroll margin or spacer control.
- Removed the standalone Polyvox developer card, favicon slot, favicon download/cache logic and optional favicon resources. The About version card retains a small developer credit; assembly company metadata is unchanged. GitHub update checks are unchanged.
- All four compiler-diagnostic fixes from 2.7.1 are retained where applicable. MainForm designer controls remain stock WinForms controls, with runtime-only styling.

RTools is a local-first collection of desktop utilities. This release returns the application to **classic Windows Forms on .NET Framework 4.7.2**, the exact runtime target of the original project. It replaces the previous WPF shell rather than embedding WPF controls. No third-party NuGet packages or .NET 10 runtime are required.

## Build

1. Install Visual Studio 2022 version 17.8 or newer (or a newer Visual Studio release), with the **.NET desktop development** workload and **.NET Framework 4.7.2 targeting pack/developer pack**. The source uses a modern C# 12 compiler; this does not change the Framework runtime target.
2. Open `RTools.sln`. Set `RTools` as the startup project. Select Release / Any CPU, then build or run.
3. The executable and configuration are in `RTools/bin/Release/`. Keep `RTools.exe.config` beside the executable. Windows 11 already includes a compatible .NET Framework 4.8 or 4.8.1 runtime.

Alternatively, from Windows PowerShell:

```powershell
.\publish-win-x64.ps1
```

This uses Visual Studio MSBuild (not `dotnet publish`), builds the solution, runs the 36 regression tests, and copies `RTools.exe` and `RTools.exe.config` to `publish/win-x64`. The application is Any CPU, with the 32-bit preference disabled; it runs as 64-bit on 64-bit Windows. It is not a self-contained .NET runtime distribution. The included GitHub Actions workflow uses the same build and publishes those two files as a Windows artifact.

## Interface

- Windows 11-inspired WinForms shell: Segoe UI typography, navigation glyphs, rounded red action buttons, categorized tool cards, searchable sidebar, native window chrome and Windows 11 DWM corner preference.
- Original palette: `#861B2D` deep red, `#F83357` bright red, `#1F1F1F` charcoal. Persistent dark/light mode. Theme file: `%LOCALAPPDATA%\RTools\theme.txt`.
- A 24-DPI-independent-pixel separator row with a subtle theme-colored divider separates the bottom three buttons from the scrolling tool list. Runtime-only rounded scrollbar overlays cover both vertical and horizontal bars in the sidebar, dashboard, tool pages (including About & Help), multiline text boxes, rich-text editors, file lists, checked CSV column lists, data grids and drop-down option lists. They retain the native control scroll ranges, use a thin thumb at rest, expand on hover/focus, support dragging, page clicks, mouse wheel and keyboard keys, and hide the thumb when no overflow exists. The custom selection-only drop-downs have rounded red-accented fields, theme-aware popup lists, keyboard selection, type-ahead and Escape to dismiss without changing the current selection. Windows-owned file/color dialogs and system menus retain native OS styling; the remaining application-owned text input frames, numeric spinners, sliders, checkboxes and list selection indicators use the 2.8 theme.
- Ctrl+K focuses tool search. Esc cancels a running operation when supported. Long-running work executes in the background, with a status bar and cancellation. Navigation is disabled while an operation runs. A committed destructive operation finishes or attempts rollback rather than being interrupted halfway.
- Each page is created lazily and remains cached while the app is open, retaining inputs. The main window has a conventional `MainForm.cs`, `MainForm.Designer.cs`, and `MainForm.resx` structure. Its actual navigation, dashboard, header, footer, and settings buttons are designer-owned controls. `MainFormRuntime.cs` is a separate controller (not a partial form) and connects events and initializes the theme, tools, and updater only when the executable runs. Every control in `MainForm.Designer.cs` is a built-in Windows Forms control; the custom styling of the shell is attached after the executable starts. All tool pages and About & Help now have their own stock WinForms designer files under `UI/Pages`. The UI is Windows 11-inspired, not WinUI 3 and not a native Mica implementation.

## Page sizing and scrolling (2.7)

The dashboard and all twelve tools plus About & Help now measure their actual content at the available page width. Runtime page groups adapt the designer tables and flow panels to explicit content-sized rows and columns rather than relying on nested auto-sizing tables. Fixed-height editors, grids and file lists keep their own independent scrollbars and intended heights. The last visible card plus a fixed 12-pixel page inset determines the end of the page. Resizing or changing a help topic remeasures the content; shorter content clamps the existing scroll position. Pages that fit do not have a vertical scroll range. The intentional spacing inside cards and between cards remains, as does unused viewport space when a page is shorter than the window. The main form remains stock designer controls.

Two Windows regression tests cover the page endpoints, repeated bottom scrolling, window resize, help-topic changes, and shrinking text content. They are included but were not executed in the non-Windows authoring environment.

## Visual Studio designer

1. Extract this ZIP into a fresh folder. Open `RTools.sln` in Visual Studio 2022 17.8 or later with the .NET desktop development workload and .NET Framework 4.7.2 developer pack installed.
2. Build the RTools project (Build > Build Solution) to catch project/reference errors. The main form now has no custom designer controls or runtime initialization dependencies. Its designer is intended to load even without first compiling a custom control.
3. In Solution Explorer, select **RTools > MainForm.cs** (not `MainForm.Designer.cs` or `MainFormRuntime.cs`). Right-click **View Designer**, or press **Shift+F7**.
4. Use Properties and Document Outline to select nested controls. Each `home...` card is now one native Button, placed directly in `homeGrid` with Dock=Fill. Edit its `Text` property (first line is the title, remaining lines are its description). Do not change the control Name unless the matching runtime lookup is also updated. Runtime themes, navigation and rounded painting are attached in `MainFormRuntime.cs` and `UI/ShellButtonStyle.cs`.
5. If **View Designer** is missing, confirm you opened the solution (not the ZIP or Open Folder), the project is loaded, and the .NET desktop development workload and Framework 4.7.2 developer pack are installed. In Solution Explorer select `MainForm.cs` itself, not the nested designer file.
6. If the designer still shows an error, close Visual Studio, delete only the extracted solution's `.vs` directory and the two projects' `bin` and `obj` directories, then reopen and rebuild. Preserve source files. If it still fails, capture the exact designer error, any build error, and the Visual Studio version for troubleshooting.

The design-time layout uses stock flat rectangular buttons and the default dark palette. The running application draws rounded red-accent cards, glyphs and hover/focus states over the same controls. The form constructor calls only `InitializeComponent()`. `Program.Main` explicitly attaches `StartRuntime(true)` before `Application.Run`. The designer does not call that method, read theme files, create tool pages, use native interop, or contact GitHub. The designer file uses fully qualified standard WinForms types and conventional statements. The `.resx` uses the standard schema.

## High-resolution icon

The original icon contained just one 32×32 frame. The new `RTools/Assets/hammericon.ico` is a vector-based redraw of the same red hammer and brown handle, not a new logo. It embeds ten 32-bit alpha frames: 16, 20, 24, 32, 40, 48, 64, 96, 128 and 256 pixels. The original proportions and colors were used as the reference; the exact outlines and shading have been retraced. The editable source is `RTools/Assets/hammericon.svg`. The ICO is used for the executable resource and runtime window icon. Rebuild the executable and recreate or repin old shortcuts if Windows still displays its cached 32px icon.

## Tools

| Tool | Capabilities |
|---|---|
| Text File Joiner | Multiple files, drag/drop, reorder, separators, optional deletion after successful save |
| Search & Append | Literal keywords, match counts, copy or move, exact backup of source in move mode |
| Report to CSV | Legacy fixed-width parser, configurable prefixes and header count, privacy masking, preview, export |
| File Comparison | Aligned line differences, red/green highlights, ignore case/whitespace, changes-only view, binary comparison |
| Bulk Renamer | Literal/regex replacement, prefix/suffix, numbering, case conversion, collision checks, reviewed preview, undo last successful batch |
| CSV Workbench | Quoted multiline CSV parsing, multiple delimiters, editable grid, filtering, sorting, merging matching headers, add/delete/dedup rows, selected-column export |
| Subnet Calculator | IPv4 CIDR/mask, wildcard, network/broadcast, host range, /31 and /32 handling |
| Log Viewer | Bounded recent-window reads, keyword/severity filter, live follow while the page is visible, shared log file access |
| System Information | Windows, CPU, RAM, local storage, uptime, network adapters; copy or save report |
| Regex Tester | Match and capture groups, highlighted results, options, replacement preview with limits and timeouts |
| Color Picker | RGB sliders/numbers, HEX, HSL, preview, native color dialog, copy CSS values |
| Folder Analyzer | Recursive logical file sizes, direct-child totals, largest files, inaccessible-entry count; skip junctions/symlinks |

All twelve tools from 2.1 are retained; the five utilities removed in that release have not been reintroduced. CSV workbench limits are 25,000 rows, 200 columns, 250,000 cells and 20 MB input. Text comparison is limited to 20,000 lines and 2 million characters per file, with a bounded diff matrix. Log following checks the size and timestamps every two seconds and reads only the last 1 MiB when changed (large logs assumed UTF-8). Folder size is logical file size, not allocated disk space. Bulk rename undo is in-memory only, and the subnet tool is IPv4 only. Color picker does not include a screen eyedropper.

## Restored GitHub updater

The original source checked this exact URL:

```
https://github.com/RShupe/RTools/raw/main/currentreleaseversion.txt
```

The app again checks the version file on startup, asynchronously after the window is shown. A manual **Check for updates** button is available in the sidebar and About page. It trims whitespace and BOM, accepts an optional `v` prefix, and compares numeric `System.Version` values with missing build/revision components treated as zero. It only offers an update when the published version is strictly newer than the installed assembly version. Timeout is ten seconds; failed automatic checks are silent and failed manual checks show an explanation. Update prompts open the original GitHub releases page, `https://github.com/RShupe/RTools/releases`, in the default browser. The app remains open, and no executable is downloaded or installed automatically.

### Publishing a new version

- Set the new version in `RTools/Properties/AssemblyInfo.cs` (`AssemblyVersion`, `AssemblyFileVersion`, `AssemblyInformationalVersion`). The updater reads `AssemblyVersion` automatically; there is no duplicate hardcoded local version string in the check logic.
- Build and test, then publish the corresponding Windows binary/configuration to a GitHub release in `RShupe/RTools`.
- Only **after the release is available**, update `currentreleaseversion.txt` at the repository root on the `main` branch with the new numeric version (for example `2.0.0`). The included file is a template for this release; do not publish it prematurely.
- You can test the comparison locally with `RTools.Tests.exe`. The actual GitHub version-file contents and reachability are not verified in this package.

## About & Help / command line

Open **About & Help** from the sidebar or press F1. The page includes all twelve step-by-step tool guides, a shortcut to open the selected tool, version and repository links, update controls, and copyable console syntax and examples. The GUI and `--help` share the same console instructions.

### Developer credit

About & Help retains the **Developed by Polyvox Softworks** credit inside the existing RTools version card, with a small PictureBox named `polyvoxLogo` between the `developedByLabel` (Developed by) and `polyvoxCredit` (Polyvox Softworks) labels. The supplied image is stored in `RTools/Assets/polyvox-logo.png` and embedded through `AboutHelpPage.resx`. Use the AboutHelpPage designer to edit the image, size or position. Windows file properties retain Polyvox Softworks as the assembly company. No separate developer box or online favicon request is used. The application hammer icon remains unchanged.

The original two console commands are still supported. The other ten tools are GUI-only. From an interactive Command Prompt in the executable directory, use `start /wait` to wait for the Windows GUI executable:

```bat
start "" /wait RTools.exe --help
start "" /wait RTools.exe tfj "C:\output.txt" 1 N "C:\input1.txt" "C:\input2.txt"
start "" /wait RTools.exe sa "C:\input.txt" "C:\output.txt" "ERROR" "WARNING"
```

`tfj` takes the output path, blank-line count (0–1000), Y/N delete flag, and ordered input paths. N keeps the originals; Y deletes after successful output. Existing output is replaced without a CLI confirmation. `sa` takes input, output and one or more case-sensitive literal keywords; it moves matching lines and creates a source backup when matches are moved. The GUI defaults to copy mode for safety. The program is a WinExe and attaches to the parent console for CLI output. Running it without arguments opens the GUI. Exit codes: 0 success/help, 1 operation error or failed cleanup, 2 invalid command/argument count.

## Safety and test notes

- Existing file outputs use same-directory staging and Framework `File.Replace` to avoid deleting an existing destination first. If the filesystem does not support file replacement, the operation reports an error rather than using an unsafe delete-and-replace fallback. Path identity checks use Windows file handles and reject the same file (including aliases/hardlinks) when the identities can be verified. Avoid concurrent writers to inputs/destinations during destructive operations.
- The legacy transaction report parser uses fixed offsets. Card/account identifiers are masked by default; unmasked export requires confirmation in the GUI. Validate against an authorized, representative report before production use. CSV export guards formula-like content by default; disabling the guard may allow spreadsheet formulas to execute when opened in Excel.
- The included regression suite contains 36 cases, including stock designer-control and full-card structure checks, a Windows runtime navigation smoke test with automatic network requests disabled, scrollbar thumb geometry, coverage checks for tool controls, selection-collection and scrollbar/drop-down Windows smoke tests, numeric/slider value, theme and paint smoke tests and an ICO frame/load test. It does not launch the Visual Studio designer.
- **Verification status:** source and project-file structural checks were performed, including all twelve navigation registrations, dashboard controls, all twelve help topics, project references, form resource metadata, all icon frames and ZIP integrity. The redrawn icon was visually checked on light and dark backgrounds. The authoring environment did not have Windows, Visual Studio/MSBuild or a C# compiler, so the actual build, tests, WinForms rendering, DPI behavior, custom control rendering and interaction, designer behavior and live update check have not been verified. Build on Windows before deploying or publishing the version file.


### Homepage simplification

The introductory dashboard card, duplicate version row and Home subtitle have been removed. The tool categories start immediately below the compact Home heading. Other pages retain their subtitles. Dashboard descriptions wrap rather than adding ellipses, and each tile has a single drawn chevron. The footer and About version remain 2.0.0. The Release startup diagnostics are retained.
