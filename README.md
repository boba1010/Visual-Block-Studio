# Visual Block Studio

**Visual Block Studio (VBS)** is a hybrid IDE, built with WinUI 3, for building WinUI 3 apps. It combines three ways of working in one environment:

1. **A visual XAML editor:** drag controls onto a design surface, select them and edit their properties in an inspector.
2. **Scratch-inspired block editing for C# logic (planned):** visual blocks for application behavior, stored as `.csvblock` files. The UI itself is authored in the visual XAML editor, not with blocks.
3. **A normal code editor:** a custom text editor for the real C# and XAML files, so you always keep full control.

Under the visual layer there is no proprietary runtime. VBS keeps a typed XAML syntax tree, generates ordinary XAML from it, and emits a standard `.csproj`, so what you build is a regular WinUI app.

> 🚧 **VBS is in active development.** Architecture, file formats and features are still changing, and many parts are partly implemented. See [Current status](#current-status), [Known limitations](#known-limitations) and [Release Channels](RELEASE_CHANNELS.md).

---

## Table of contents

- [Overview](#overview)
- [Why VBS?](#why-vbs)
- [Features](#features)
- [How it works](#how-it-works)
- [Project and file formats](#project-and-file-formats)
- [Repository layout](#repository-layout)
- [Architecture](#architecture)
- [Getting started](#getting-started)
- [Using VBS](#using-vbs)
- [Using the compiler library directly](#using-the-compiler-library-directly)
- [Current status](#current-status)
- [Known limitations](#known-limitations)
- [Roadmap](#roadmap)
- [Release channels](#release-channels)
- [Tech stack](#tech-stack)
- [Contributing](#contributing)
- [License](#license)

---

## Overview

Visual designers for XAML tend to be tied to a specific IDE or hide the underlying code. Block-based tools make programming approachable but usually produce something that isn't real source code. VBS tries to do both: a visual designer for the UI, Scratch-style blocks for behavior (planned), and output that is plain C# and XAML.

**Where the project is heading:** the UI is authored in the **visual XAML editor** (design surface plus property inspector). It edits a typed XAML syntax tree that is saved as a `.xamlvblock` document and turned into real XAML. Scratch-style blocks are reserved for **C# logic** (`.csvblock`) and are planned, not built yet.

**Goals**

- Design XAML interfaces visually, backed by a typed XAML syntax tree.
- Represent C# logic as visual blocks, in a Scratch-like workflow (planned).
- Generate XAML from the syntax tree and C# from logic blocks, and convert existing XAML and C# back into the visual representations.
- Provide live previews without compiling the target application.
- Integrate with .NET compiler and code-analysis tooling (Roslyn).
- Keep a normal source-code workflow available alongside visual development.

---

## Why VBS?

WinUI 3 has historically lacked a good visual designer. Most workflows rely on writing XAML by hand and rebuilding or hot-reloading the app to see the result.

VBS takes a different approach:

- **No compile step for previews.** The previewer builds real WinUI controls straight from the typed XAML syntax tree. The preview is the actual WinUI renderer, not an approximation, so you see changes without building or running your app.
- **Real code underneath.** What you design is saved as a syntax tree and turned into plain XAML. The project VBS creates is a standard WinUI project that builds with the normal .NET toolchain, so nothing locks you in.
- **Standalone and native.** VBS is itself a WinUI 3 app and doesn't depend on a particular IDE or on a running target app.
- **Designer and code editor in one place.** Switch between the visual editor and a text editor for the same project when you need full control.

> **Note:** Previews that don't need compilation work for built-in controls. Custom controls, code-behind, bindings to your own types and app-level resources need your project's code. Supporting them is part of the planned runtime execution work (see the [Roadmap](#roadmap)).

## Features

### Workspace and project management

- **Start window** that opens before the main IDE (Visual Studio style). It can also open a project or solution passed on the command line, for example by double-clicking a file.
- **Dashboard** with:
  - A searchable list of recent solutions, projects and folders, with the last-updated date.
  - **Create a new project**, **Open a project or solution** and **Open folder** actions.
- **Create project page** with a *WinUI Blank App* template:
  - Project name, location and solution name fields.
  - A *place solution and project in the same directory* option.
  - A built-in folder picker.
- **Solutions and projects** with their own formats (`.vbsslnx`, `.vbsproj`). VBS also generates a standard `.slnx` solution and `.csproj` project, so the output opens in other .NET tooling.
- **Solution Explorer** in the left navigation pane. It shows folders, C# files, XAML files and block files, with the `.xamlvblock` document and generated `.xaml` for a window grouped together.
- **Tabbed documents** using WinUI `TabView`, with tabs opened through a messenger (`CommunityToolkit.Mvvm`).
- **Modern shell:** Mica backdrop, custom title bar with a command bar (file, undo/redo and run buttons), and teaching-tip error messages for failed loads.
- **Persistent recents** stored per user in `%LOCALAPPDATA%\Visual Block Studio\recents.json`.

### Visual XAML editor

- **Two modes:** *Edit* mode, where the design surface handles hit testing and selection, and *Interactive* mode, where the live preview controls receive input.
- **Controls palette** for adding controls (currently `Button` and `Grid`) to the design surface. Controls added while a panel is selected are nested inside it.
- **Selection** with a dashed highlight overlay that follows the selected control as it resizes, plus hit testing on the design surface.
- **Property inspector** that is built from the control's syntax model and edits it directly:
  - **Content** (string)
  - **Width** and **Height** (number boxes)
  - **Background** (color picker with alpha)
  - **Vertical** and **horizontal alignment** (`Start`, `Center`, `End`, `Stretch`)
  - Toggle button to show or hide the inspector panel.
- **Native preview:** VBS builds real WinUI controls from the syntax tree, so the preview is the actual WinUI rendering rather than an approximation drawn by VBS.
- **Full-screen preview** that opens the generated window in its own native window.
- **Synchronized edits:** inspector edits update both the syntax model (used for XAML generation) and the live preview control.

### Code editor

A custom text editor written for VBS, rendered with **Win2D** (`CanvasControl`). It is organized as an engine plus separate renderers, not a wrapper around a stock text box:

- **Document model:** line-based `TextDocument` with change notifications (`DocumentChanged`, with a change kind).
- **Editing:** insert, new line, backspace, delete, tab insert and un-indent, replace-selection.
- **Caret and selection:** caret movement by character, line, Home/End and document start/end, with selection extension (Shift), select all, and pointer-based caret placement.
- **Clipboard:** copy, cut and paste.
- **Undo/redo:** command-pattern history (`IUndoableEdit`) with specific edit types for insert, delete, delete line break, new line, paste and replace, and `CanUndo`/`CanRedo` state.
- **Rendering pipeline:** separate text, caret, selection and editor renderers, with text-style metrics.
- **Input handling:** dedicated keyboard handler (Ctrl+A/C/X/V/Z/Y, arrows, Home/End, Tab, Backspace, Enter, Delete).

### XAML syntax and compiler library (`XamlBlocksCompiler`)

- **Typed XAML syntax tree** with elements, properties, attributes and XML namespaces. Built-in control syntax types:

  | Syntax type | XAML element | Notes |
  | --- | --- | --- |
  | `XamlApplicationSyntax` | `Application` | Application root |
  | `XamlWindowSyntax` | `Window` | `Title`, single `Content` |
  | `XamlGridSyntax` | `Grid` | Panel with `Children` |
  | `XamlCanvasSyntax` | `Canvas` | Panel with `Children` |
  | `XamlButtonSyntax` | `Button` | Content control |

  Controls share `Width`, `Height`, `Background`, `VerticalAlignment` and `HorizontalAlignment`.
- **Parser** for the `.xamlvblock` JSON format, with validation and descriptive `JsonException` messages. Unknown element names fall back to a generic element.
- **XAML code generator** that writes indented, readable XAML through `XmlWriter`, with namespaces, attributes, properties and children.
- **Compiler facade** (`XamlCompiler`) that reads a `.xamlvblock` file, parses it and writes the generated `.xaml` file.
- **Roslyn incremental source generator** (`XamlBlocksCompiler.SourceGeneration`). Classes marked `[XamlControl]` get generated control metadata (their public settable properties, walking the base-class hierarchy), so controls don't need hand-written metadata tables.
- **Type registry and symbols** (`XamlTypeRegistry`, `XamlTypeSymbol`, `XamlPropertySymbol`) for resolving XAML types by namespace and name.
- **Color and alignment models** that don't depend on WinUI. `Color` reads and writes `#AARRGGBB`.

### Persistence

- DTO-based serialization for solutions, projects, project items, files, recents and `.xamlvblock` documents.
- Explicit mapping between runtime models and DTOs, so persistence stays independent of UI state.
- Pretty-printed JSON project and solution files.
- Read-only generated mirror of the project file (`*.vbsproj.g.cs`) for readability. It has no effect on the build.
- Generated `.csproj` that lists the project's C# files as `Compile` items and XAML files as `Page` items.

---

## How it works

```text
┌───────────────────────────┐   ┌──────────────────────┐   ┌──────────────────────┐
│ Visual XAML editor        │   │ Code editor          │   │ Solution Explorer    │
│ design surface + inspector│   │ Win2D text editor    │   │ solutions & projects │
└────────────┬──────────────┘   └──────────┬───────────┘   └──────────┬───────────┘
             │ edits                       │ edits                    │ opens / creates
             ▼                             ▼                          ▼
   ┌───────────────────┐          real .cs / .xaml files       ┌─────────────────┐
   │ Typed XAML syntax │                                       │ VBS project     │
   │ tree (compiler)   │                                       │ (.vbsproj JSON) │
   └─────────┬─────────┘                                       └────────┬────────┘
             │                                                          │ save
     ┌───────┴────────┐                                                 ▼
     ▼                ▼                                       generated .csproj + .slnx
┌──────────┐   ┌──────────────┐                                (standard WinUI project)
│ XAML     │   │ XamlPreviewer│
│ generator│   │ native WinUI │
│ → .xaml  │   │ controls     │
└──────────┘   └──────────────┘
```

1. The editor works on a **typed syntax tree**, not on text. Each control in the visual editor corresponds to a node (`XamlGridSyntax`, `XamlButtonSyntax`, and so on).
2. **Property edits** in the inspector change that node.
3. The tree has two consumers:
   - `XamlCodeGenerator` turns it into XAML text for the real project.
   - `XamlPreviewer` turns it into live WinUI controls for the preview, without compiling your app.
4. Saving a project writes the `.vbsproj`, regenerates the read-only mirror and regenerates the `.csproj`, so the project builds with the normal .NET toolchain.

---

## Project and file formats

> These formats are still evolving. Early Access and Experimental builds may change them without migration.

| Extension | Purpose |
| --- | --- |
| `.vbsslnx` | VBS solution file (JSON). Lists the projects and the generated `.slnx` path. |
| `.vbsproj` | VBS project file (JSON). The single source of truth for the project: name, architecture, output path, layout of files. |
| `.slnx` | Standard .NET solution generated from the `.vbsslnx`. Project paths are written relative to the solution. |
| `.csproj` | Standard MSBuild project generated from the `.vbsproj` (WinUI, `net10.0-windows10.0.19041.0`). |
| `.xamlvblock` | Source document for a window's XAML: the typed syntax tree the visual editor edits (JSON). |
| `.csvblock` | Reserved for Scratch-style C# logic blocks (planned, not yet implemented). |
| `.xaml` | Generated XAML. Regenerated from the `.xamlvblock` file. |
| `*.vbsproj.g.cs` | Read-only, human-readable mirror of the project file. Editing it has no effect. |

### `.vbsproj` structure

The project file stores these fields (property names are the DTO's):

```jsonc
{
  "Name": "MyApp",
  "TargetArchitecture": "x64",
  "OutputPath": "<project dir>/bin",
  "FileName": "MyApp.vbsproj",
  "FilePath": "<full path>",
  "GeneratedFileName": "MyApp.vbsproj.g.cs",
  "GeneratedFilePath": "<project dir>/obj/MyApp.vbsproj.g.cs",
  "CsProjFileName": "MyApp.csproj",
  "CsProjFilePath": "<full path>",
  "LayoutFiles": [
    {
      "Name": "MainWindow",
      "Type": "FilesParent",          // Folder | FilesParent | SingleCsFile | SingleXamlFile | SingleBlockFile
      "Files": [
        { "FileName": "MainWindow.xamlvblock", "FilePath": "…", "Type": "Block" },   // Cs | Xaml | Block
        { "FileName": "MainWindow.xaml",       "FilePath": "…", "Type": "Xaml"  }
      ]
    }
  ]
}
```

### `.xamlvblock` structure

The parser currently reads version `1` documents with a single root element:

```jsonc
{
  "version": 1,
  "root": {
    "type": "element",
    "name": "Window",
    "namespaces": [
      { "prefix": "",  "uri": "http://schemas.microsoft.com/winfx/2006/xaml/presentation" },
      { "prefix": "x", "uri": "http://schemas.microsoft.com/winfx/2006/xaml" }
    ],
    "attributes": [ { "name": "Title", "value": "My App" } ],
    "content": {
      "type": "element",
      "name": "Grid",
      "children": [ { "type": "element", "name": "Button" } ]
    }
  }
}
```

- Node `type` is `element` or `property`.
- Panels (`Grid`, `Canvas`) take `children`; content controls (`Window`, `Button`) take `content`.
- Element `name` values that aren't built in are parsed as generic elements.

---

## Repository layout

```text
Visual Block Studio/
├── Visual Block Studio.slnx             Solution
├── Visual Block Studio/                 The IDE (WinUI 3 desktop app)
├── XamlBlocksCompiler/                  Syntax tree, parser, generator, type registry
├── XamlBlocksCompiler.SourceGeneration/ Roslyn source generator for control metadata
├── XamlPreviewer/                       Builds native WinUI controls from the syntax tree
├── VBS.Test/                            Console project for exercising the compiler
├── PROGRESS.md                          Detailed progress checklist and roadmap
├── RELEASE_CHANNELS.md                  Release channel definitions
└── LICENSE.txt                          GPL-3.0
```

### IDE project (`Visual Block Studio/`)

| Folder | Contents |
| --- | --- |
| `Windows/` | `StartWorkspaceWindow`, the launcher shown before the workspace. |
| `Views/` | Main pages: `XamlVisualEditor`, `TextEditor`, `Dashboard/DashboardPage`, `Dashboard/CreateProjectPage`. |
| `Views/Controls/` | Reusable controls: `CodeEditor`, file and folder picker dialogs, `Badge`. |
| `Views/Controls/CodeEditor/` | `Core/` (document, caret, selection, engine), `Input/`, `Rendering/`, `UndoRedo/`. |
| `ViewModels/` | `ShellViewModel`, `TabsViewModel`, `SolutionViewModel`, `TextEditorViewModel`, `XamlVisualEditorViewModel`. |
| `Services/` | `VBSProjectManager`, `SolutionManager`, `TemplateProjectService`, `MSBuildProjectGenerator`, `SlnxWriter`, `RecentsService`, `TextEditorService`, `Preferences`. |
| `Models/` | Runtime models for solutions, projects, project items and recents. |
| `DTOs/` | Serialization shapes for the on-disk formats. |
| `Contracts/` | Request objects (`CreateProjectRequest`, `CreateSolutionRequest`). |
| `Enums/` | `ProjectItemType`, `ProjectFileType`, `RecentType`, `ToolbarItemType`. |
| `Helpers/` | Mappers between models and DTOs, clipboard and list helpers. |
| `Messages/` | Messenger messages, for example `OpenTabRequestMessage`. |
| `Properties/PublishProfiles/` | Publish profiles for `win-x64`, `win-x86` and `win-arm64`. |

### Compiler project (`XamlBlocksCompiler/`)

| Folder | Contents |
| --- | --- |
| `Syntax/` | Syntax node types (`XamlElementSyntax`, `XamlPropertySyntax`, `XamlAttributeSyntax`, `XamlNamespaceSyntax`, `[XamlControl]`). |
| `Syntax/Controls/` | Control syntax types: `Application`, `Window`, `Grid`, `Canvas`, `Button` and their base classes. |
| `Parsing/` | `XamlBlockDocumentReader`, `XamlBlockParser`. |
| `Generation/` | `XamlCodeGenerator`. |
| `Compilation/` | `XamlCompiler` (read → parse → generate → write). |
| `Types/` | `XamlTypeRegistry`, `XamlTypeSymbol`, `XamlPropertySymbol`. |
| `Semantics/` | Placeholders for the semantic model and diagnostics. |
| `Models/` | `Color`, `Alignment`. |

---

## Architecture

- **MVVM with dependency injection.** `CommunityToolkit.Mvvm` provides observable objects and messaging; `Microsoft.Extensions.DependencyInjection` wires the shell, solution and tab view models and the services as singletons, with the text editor view model transient.
- **Separation of runtime models, DTOs and mappers.** The on-disk shape is independent of the objects the UI binds to, which avoids recursive graphs such as `Parent → Children → Parent` during serialization.
- **The `.vbsproj` is the source of truth.** Everything else (the `.csproj`, the `.slnx` and the `.g.cs` mirror) is generated from it and overwritten on save.
- **Compiler independent of the UI.** `XamlBlocksCompiler` targets plain `net10.0` and has no WinUI dependency, so it is usable from console apps (see `VBS.Test`). Only `XamlPreviewer` depends on WinUI.
- **Source-generated metadata.** The source generator targets `netstandard2.0` (as Roslyn components require) and is referenced as an analyzer, so control metadata stays in sync with the syntax classes automatically.
- **Unpackaged, self-contained app.** `WindowsPackageType` is `None` and the Windows App SDK is self-contained, so no MSIX packaging is required to run it.
- **Release builds** use ReadyToRun and trimming; the code and type registry are written to stay AOT/trimming friendly (string-based type identification, no runtime type scanning).

---

## Getting started

### Requirements

| Requirement | Details |
| --- | --- |
| OS | Windows 10 version 2004 (build 19041) or later. x64, x86 and ARM64 builds are configured. |
| SDK | [.NET 10 SDK](https://dotnet.microsoft.com/download) |
| IDE | Visual Studio with the **.NET desktop development** workload and Windows App SDK / WinUI tooling, and support for the `.slnx` solution format. |

NuGet dependencies are restored on build (see [Tech stack](#tech-stack)).

### Build and run

1. Clone the repository.
2. Open `Visual Block Studio.slnx` in Visual Studio.
3. Pick a platform (`x64`, `x86` or `ARM64`) in the toolbar.
4. Set **Visual Block Studio** as the startup project and press **F5**.

Or from a terminal:

```powershell
dotnet build "Visual Block Studio\Visual Block Studio.csproj" -p:Platform=x64
```

### Publish

Publish profiles for each architecture are in `Visual Block Studio/Properties/PublishProfiles/`:

```powershell
dotnet publish "Visual Block Studio\Visual Block Studio.csproj" -p:PublishProfile=win-x64
```

Replace `win-x64` with `win-x86` or `win-arm64` as needed. Non-debug configurations publish ReadyToRun and trimmed.

---

## Using VBS

1. **Launch VBS.** The start window shows your recent solutions, projects and folders.
2. **Create a project:** choose *Create a new project* → *WinUI Blank App*. Enter a project name, a location and a solution name, then choose *Create*.
   VBS creates the solution, the project and a `MainWindow` with both a `.xamlvblock` document and a generated XAML file.
3. **Open the workspace.** Solution Explorer shows the project on the left. Open `MainWindow` to get the visual editor in a new tab.
4. **Design the UI.**
   - Add controls from the *Controls* panel. Select a `Grid` first to nest new controls in it.
   - Select a control on the design surface to edit its properties in the inspector.
   - Use *Open Props* to show or hide the inspector.
5. **Preview.** Switch to *Interactive Mode* to use the live controls, or choose *FullScreen Preview* to see the window as a native WinUI window.
6. **Edit code.** Open C# or XAML files in the text editor tab for full manual control.
7. **Open an existing project.** Use *Open a project or solution*, *Open folder* or pass a file path on the command line.

---

## Using the compiler library directly

`XamlBlocksCompiler` can be used on its own to build XAML from code, or to convert `.xamlvblock` documents to XAML:

```csharp
using XamlBlocksCompiler.Generation;
using XamlBlocksCompiler.Syntax.Controls;

var window = new XamlWindowSyntax();
window.Namespaces.Add(new("", "http://schemas.microsoft.com/winfx/2006/xaml/presentation"));
window.Namespaces.Add(new("x", "http://schemas.microsoft.com/winfx/2006/xaml"));

var grid = new XamlGridSyntax();
grid.Children.Add(new XamlButtonSyntax { Content = "Button", Width = 200, Height = 40 });
window.Content = grid;

string xaml = new XamlCodeGenerator().Generate(window);
Console.WriteLine(xaml);
```

To convert a `.xamlvblock` file on disk to XAML:

```csharp
using XamlBlocksCompiler.Compilation;

new XamlCompiler().Compile("MainWindow.xamlvblock", "MainWindow.xaml");
```

The `VBS.Test` project is a ready-made console app that runs the first example.

---

## Current status

VBS is at the **visual editor foundation stage**: a working shell, project system, XAML syntax model, generator, native preview and basic inspector, with the surrounding workflow still being filled in.

| Area | State |
| --- | --- |
| Shell, start window, dashboard, recents | ✅ Working |
| Project / solution creation and generated `.csproj` / `.slnx` | ✅ Working |
| Typed XAML syntax tree and XAML generation | ✅ Working |
| Source-generated control metadata | ✅ Working |
| Native previewer and full-screen preview | ✅ Basic controls |
| Property inspector (string, number, color, alignment) | ✅ Basic |
| Custom Win2D code editor (editing, selection, clipboard, undo/redo) | ✅ Core editing |
| `.xamlvblock` parser | ✅ Version 1, limited element set |
| Visual XAML editor (design surface, inspector) | 🔄 Current focus |
| Wider control and property coverage | 🔄 In progress |
| Save/load visual editor documents, Save/Open menu actions | 🔄 In progress |
| Semantic model and diagnostics | ⏳ Planned (placeholders exist) |
| XAML parser and round-tripping | ⏳ Planned |
| Scratch-style blocks for C# logic (`.csvblock`) | ⏳ Planned, after the XAML editor |
| Running generated apps without a full rebuild | ⏳ Planned |

See [PROGRESS.md](PROGRESS.md) for the full checklist.

---

## Known limitations

- Only a small set of controls is supported in the editor (currently `Button` and `Grid`; `TextBlock` is in the palette but not yet functional).
- Several toolbar actions (Save, Open, Undo, Redo and Run) are present in the UI but not yet wired up.
- The code editor has no syntax highlighting, IntelliSense or find/replace yet.
- No XAML parsing yet: editing the generated `.xaml` by hand doesn't update the visual editor.
- Attached properties, resources, styles, bindings and events aren't represented yet.
- The visual editor doesn't yet support multi-selection, deletion, duplication, resizing handles or undo/redo.
- Windows only (WinUI 3).
- File formats may change between builds.

---

## Roadmap

The long-term goal is to combine a reliable visual WinUI designer with a Scratch-inspired visual programming system, so you can build real WinUI apps with interactive behavior, not only static UIs. The current priority is:

> **Build a reliable visual WinUI development workflow first, then expand into visual application logic.**

Planned phases (see [PROGRESS.md](PROGRESS.md) for the detailed checklist):

1. **Core designer:** control creation and nesting, expanded property editing, delete/duplicate/resize, undo/redo, improved selection and drag-and-drop.
2. **XAML support:** broader control and property coverage, parsing, type conversion and validation, attached properties and collections, round-trip generation.
3. **Preview and debug mode:** better layout fidelity, diagnostics, resources and styles, stable preview state, live property updates.
4. **Visual programming foundation (C# logic blocks):** logic block model and editor, event-handler blocks, variables, expressions and conditions, linking logic to controls, persisted logic graphs.
5. **Runtime execution:** initializing generated apps at runtime, connecting event handlers, executing supported logic, investigating Roslyn-based runtime compilation.
6. **Project workflow and synchronization:** complete save/load, versioning and recovery, keeping visual edits and generated XAML in sync.
7. **Advanced IDE features:** search, shortcuts, minimap, extension API, plugin system, performance work and virtualization for large documents.

---

## Release channels

VBS builds are published to channels that describe their expected stability:

| Channel | Stability | Development |
| --- | --- | --- |
| 🚧 Early Access | Very Low | Very High |
| 🧪 Experimental | Low | Very High |
| 🔬 Alpha | Low | High |
| 🧩 Beta | Medium | Medium |
| 👀 Preview | High | Low |
| ✅ Stable | Highest | Maintenance |

Details are in [RELEASE_CHANNELS.md](RELEASE_CHANNELS.md). Keep backups of important projects when using Early Access, Experimental, Alpha or Beta builds.

---

## Tech stack

| Component | Technology |
| --- | --- |
| Language / runtime | C#, .NET 10 |
| UI | WinUI 3, Windows App SDK 2.5 |
| Text rendering | Win2D (`Microsoft.Graphics.Win2D` 1.4) |
| MVVM and messaging | `CommunityToolkit.Mvvm` 8.4 |
| Dependency injection | `Microsoft.Extensions.DependencyInjection` 10.0 |
| Compiler tooling | Roslyn (`Microsoft.CodeAnalysis` 5.6 in the IDE; 4.14 in the source generator) |
| Project formats | JSON (`System.Text.Json`), XML (`slnx`, `csproj`, XAML) |
| Publishing | Self-contained, unpackaged; ReadyToRun and trimming for release builds |

The IDE already references Roslyn and MSBuild locator packages in preparation for compiler and code-analysis integration. Roslyn-driven features are on the roadmap and aren't implemented yet.

---

## Contributing

VBS is evolving quickly and its architecture may change significantly, so large changes may conflict with ongoing work. You're welcome to fork the project and experiment. If you'd like to contribute, please open an issue first to discuss what you'd like to change.

When reporting a bug, it helps to include:

- The release channel and build you used.
- Your Windows version and CPU architecture.
- Steps to reproduce, and the `.vbsproj` / `.xamlvblock` involved if relevant.

---

## License

Visual Block Studio is licensed under the **GNU General Public License v3.0 (GPL-3.0)**. See [LICENSE.txt](LICENSE.txt) for the full text.

---

*Build WinUI apps visually. Keep the code real.*