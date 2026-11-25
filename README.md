<div align="center">
  <img src="RTools-2/RTools/Assets/hammericon.svg" alt="RTools logo" width="112" />
  <h1>RTools</h1>
  <p><strong>Small tools. Big time savings.</strong></p>
  <p>A Windows utility toolbox for everyday tasks, IT professionals, and developers.</p>
  <p>
    <img src="https://img.shields.io/badge/version-2.0.0-861B2D?style=flat-square" alt="Version 2.0.0" />
    <img src="https://img.shields.io/badge/platform-Windows-0078D4?style=flat-square" alt="Windows" />
    <img src="https://img.shields.io/badge/.NET_Framework-4.7.2-512BD4?style=flat-square" alt=".NET Framework 4.7.2" />
    <img src="https://img.shields.io/badge/tools-12-F83357?style=flat-square" alt="12 tools" />
  </p>
  <p>
    <a href="https://github.com/RShupe/RTools/releases/latest"><strong>Download</strong></a>
    &nbsp;·&nbsp;
    <a href="#tools">Explore Tools</a>
    &nbsp;·&nbsp;
    <a href="#command-line">Command Line</a>
    &nbsp;·&nbsp;
    <a href="#building-from-source">Build from Source</a>
  </p>
</div>

---

## About

RTools brings together a collection of practical utilities in one lightweight Windows desktop application. Instead of switching between separate programs or uploading files to online tools, you can perform common file, data, networking, and development tasks from a single workspace.

Version 2.0.0 introduces a redesigned Windows 11-inspired interface, an expanded collection of tools, and a more consistent experience throughout the application.

## Features

- **12 built-in utilities** for file management, data processing, diagnostics, and development.
- **Modern Windows interface** with searchable navigation, rounded controls, and the original RTools red accent colors.
- **Light and dark themes** that remember your preference.
- **Local processing**  your files and tool inputs are processed on your computer, not uploaded to an external service.
- **Command-line support** for automating text file joining and search-and-append operations.
- **Built-in help** with instructions for every utility and console examples.
- **GitHub update checks** with a link to download newer releases when available.
- **No third-party NuGet dependencies.**

## Tools

### Files & Data

| Tool | Description |
| :--- | :--- |
| **Text File Joiner** | Combine multiple text files in a custom order, configure blank-line separators, and optionally remove the original files after a successful save. |
| **Search & Append** | Find lines containing keywords and copy or move matching lines to another file. Preview matches before making changes. |
| **Report to CSV** | Extract records from supported fixed-width transaction reports and export them as CSV. Includes previews and identifier masking. |
| **File Comparison** | Compare files side by side with highlighted text differences, case and whitespace options, and binary comparison. |
| **Bulk Renamer** | Rename multiple files using find-and-replace, regular expressions, prefixes, suffixes, numbering, or case changes. Includes previews, collision detection, and undo for the last batch in the current session. |
| **CSV Workbench** | Open, edit, filter, sort, merge, and export CSV data using a spreadsheet-style interface. Remove duplicate rows and choose which columns to export. |

### IT & Diagnostics

| Tool | Description |
| :--- | :--- |
| **Subnet Calculator** | Calculate IPv4 networks, CIDR prefixes, subnet masks, wildcard masks, broadcast addresses, and usable host ranges. |
| **Log Viewer** | Read and filter log files, highlight severity levels, and follow changes as new entries are written. |
| **System Information** | View Windows, CPU, memory, uptime, storage, and network adapter information. Copy or save the resulting report. |
| **Folder Analyzer** | Analyze folder contents, calculate logical file sizes, and identify the largest files and subfolders. |

### Developer Tools

| Tool | Description |
| :--- | :--- |
| **Regex Tester** | Test .NET regular expressions with highlighted matches, capture groups, matching options, and replacement previews. |
| **Color Picker** | Explore colors using RGB sliders and numeric inputs. Convert between HEX, RGB, and HSL and copy the resulting values. |

## Getting Started

1. Visit the [GitHub Releases page](https://github.com/RShupe/RTools/releases/latest).
2. Download the Windows release package and extract it to a folder of your choice.
3. Keep `RTools.exe` and `RTools.exe.config` together, then launch `RTools.exe`.
4. Select a utility from the homepage or the sidebar.

### System Requirements

- Windows with .NET Framework 4.7.2 or later (Windows 11 includes a compatible runtime).
- No separate .NET 10, WPF, or Microsoft Office installation is required.

For help using any utility, open **About & Help** in the application or press **F1**.

## Command Line

RTools also supports two of its utilities through the command line. These can be used in batch files and other automated workflows without opening the graphical interface.

Run the following examples from Command Prompt in the folder containing `RTools.exe`. Because RTools is a Windows GUI executable, `start "" /wait` makes Command Prompt wait for the operation to finish.

### Text File Joiner (`tfj`)

Combine two or more text files in the order provided.

**Syntax**

```text
RTools.exe tfj <output> <blank lines> <Y/N delete> <input1> [input2 ...]
```

**Example**

```bat
start "" /wait RTools.exe tfj "C:\output.txt" 1 N "C:\input1.txt" "C:\input2.txt"
```

| Argument | Description |
| :--- | :--- |
| `output` | Path of the combined output file. |
| `blank lines` | Number of blank lines between files, from 0 to 1000. |
| `Y/N delete` | `N` retains the source files; `Y` deletes them after successful output. |
| `input1` ... | Input files in the desired joining order. |

> **Note:** An existing output file is replaced without a command-line confirmation. Use `N` unless you intend to delete the original files.

### Search & Append (`sa`)

Move entire lines containing one or more literal keywords from a source file to a destination file.

**Syntax**

```text
RTools.exe sa <input> <output> <keyword1> [keyword2 ...]
```

**Example**

```bat
start "" /wait RTools.exe sa "C:\input.txt" "C:\output.txt" "ERROR" "WARNING"
```

This example moves lines containing `ERROR` or `WARNING` into the output file. Matching is case-sensitive, and existing destination content is preserved. A backup of the source is created when lines are moved.

> **Note:** The console command modifies the source file. Use the graphical interface for copy-only operations or case-insensitive matching.

### Help

```bat
start "" /wait RTools.exe --help
```

Running `RTools.exe` without arguments opens the graphical interface. All other tools are currently available through the GUI only.

| Exit code | Meaning |
| :--- | :--- |
| `0` | Success or help displayed |
| `1` | Operation failed or cleanup could not be completed |
| `2` | Invalid command or arguments |

## Building from Source

### Prerequisites

- Visual Studio 2022 (17.8 or newer) or compatible Visual Studio Build Tools.
- .NET desktop development workload.
- .NET Framework 4.7.2 Developer Pack.

### Build with Visual Studio

```bash
git clone https://github.com/RShupe/RTools.git
cd RTools
```

1. Open `RTools.sln` in Visual Studio.
2. Set `RTools` as the startup project.
3. Select **Release** and **Any CPU**.
4. Build the solution.

The executable is generated in `RTools/bin/Release/`.

### Build with PowerShell

```powershell
.\publish-win-x64.ps1
```

The included script uses MSBuild, runs the regression tests, performs a Release startup check, and copies the executable and configuration file to `publish/win-x64/` when the checks succeed.

## Updates

RTools checks the GitHub version file in the background when it starts. You can also select **Check for updates** from the sidebar or About & Help page. When a newer version is detected, RTools offers to open the Releases page; updates are not installed automatically.

## Feedback

Have a suggestion for a new tool or found something that needs fixing? Open an [issue](https://github.com/RShupe/RTools/issues) with your idea or a description of the problem.

---

<div align="center">
  <strong>Developed by <a href="https://polyvox.dev/">Polyvox Softworks</a></strong>
  <br />
  <sub>Practical software for everyday problems.</sub>
</div>
