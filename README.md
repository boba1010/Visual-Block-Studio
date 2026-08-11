# Visual Block Studio

**Visual Block Studio (VBS)** is a visual development environment for building **.NET/XAML applications using visual blocks**.

VBS provides a visual representation of application structure while working with real C# and XAML underneath.

> 🚧 **VBS is currently in active development.**
> The architecture and features are still evolving.

## ✨ Features

### 🎨 Visual Block Canvas

* 🧱 Nested XAML block hierarchy
* 🎯 Block hit testing
* 🖱️ Block dragging
* 📦 Parent/child constraints
* 🌳 Recursive child rendering
* 🎨 Win2D-based rendering
* ✨ Block selection
* 📋 Property flyouts
* 🎯 Property flyout hit testing

### 🗺️ Canvas Navigation

* 🔍 Zoom
* 🎯 Zoom around cursor
* 🖱️ Canvas panning
* 🔄 Screen ↔ world coordinate conversion

### 💾 Data & Persistence

* DTO-based serialization
* Explicit DTO ↔ runtime block conversion
* Parent metadata without recursive serialization
* String-based block type identification
* AOT-friendly type registry

## 🏗️ Architecture

VBS separates the visual editor into several major components:

```text
┌──────────────────────────────┐
│          BlocksPage          │
│         WinUI 3 View         │
└──────────────┬───────────────┘
               │
               ▼
┌──────────────────────────────┐
│         VBlockEditor         │
│                              │
│ • Selection                  │
│ • Hit testing                │
│ • Dragging                   │
│ • Parent constraints         │
│ • Zoom                       │
│ • Panning                    │
└──────────────┬───────────────┘
               │
        ┌──────┴──────┐
        ▼             ▼
┌──────────────┐ ┌──────────────┐
│  XamlBlock   │ │   Block DTO  │
│ Runtime Model│ │  Persistence │
└──────┬───────┘ └──────────────┘
       │
       ▼
┌──────────────────────────────┐
│       VBlockRenderer         │
│            Win2D             │
└──────────────────────────────┘
```

The runtime block hierarchy and DTO representation are intentionally separated.

Parent information is represented as lightweight metadata when serialized, preventing recursive `Parent → Children → Parent` object graphs.

## 🎯 Goals

VBS is designed to provide a visual development workflow for **.NET/XAML applications**, including:

* Designing XAML interfaces visually
* Representing XAML structures as blocks
* Representing C# logic through visual blocks
* Generating C# and XAML from blocks
* Converting C# and XAML into visual representations
* Providing live previews
* Integrating with .NET compiler and code-analysis tooling
* Maintaining a normal source-code workflow alongside visual development

## 🧰 Technology

VBS is currently built around:

* **C#**
* **.NET**
* **XAML**
* **WinUI 3**
* **Windows App SDK**
* **Win2D**
* **Roslyn**

## 📂 Project Status

VBS is currently in the **visual editor foundation stage**.

The canvas interaction system is being developed as the foundation for the rest of the visual development environment.

Expect breaking changes while the project is under active development.

## 🤝 Contributing

VBS is currently under active development and its architecture may change significantly.

If you want to experiment with the project, feel free to fork it and explore the code.

## 📜 License

Visual Block Studio is licensed under the **GNU General Public License v3.0 (GPL-3.0)**.

See the [`LICENSE`](LICENSE.txt) file for the full license text.

---

**Visual Block Studio**

*Build .NET applications visually. Keep the code real.*

