# Visual Block Studio 2G

**A dedicated visual designer for XAML.**

Visual Block Studio 2G (VBS 2G) is a Windows desktop application designed to make creating and editing XAML interfaces more visual and accessible.

Instead of manually writing every layout element and property, developers can build interfaces using a visual design surface, configure controls through a property inspector, preview their work, and generate clean XAML ready to use in Visual Studio.

VBS 2G is designed to complement existing development tools rather than replace them.

## Features

### Visual XAML Designer
- Add controls directly from the toolbox.
- Build layouts using supported container controls.
- Select elements directly on the design surface.
- Configure control properties through the property inspector.
- Edit layout dimensions, alignment, colors, and content.

### XAML Import
- Load existing XAML documents.
- Parse XAML into an internal syntax representation.
- Reconstruct visual elements from the parsed document.
- Edit imported layouts through the visual designer.

### Live Preview
- Preview layouts in a separate window.
- Fullscreen preview support.
- Interactive preview mode for testing control interactions.

### XAML Generation
- Generate XAML from the internal syntax tree.
- Serialize elements and their properties.
- Preserve nested control structures.
- Copy generated XAML directly to the system clipboard.

### Source Generation
VBS 2G uses Roslyn source generation to automate control metadata registration.

Controls marked with the appropriate metadata attribute can participate in the generated serialization system without requiring manual registration in the main XAML generator.

## Architecture

VBS 2G uses a syntax-based architecture to separate the visual representation from the generated XAML.

```text
       Existing XAML
             |
             v
        XAML Parser
             |
             v
      XAML Syntax Tree
             |
             v
      Visual Designer
             |
       Property Editing
             |
             v
      Updated Syntax Tree
             |
             v
      XAML Code Generator
             |
             v
        Generated XAML
             |
             v
          Clipboard
             |
             v
       Visual Studio
```

The internal syntax tree acts as the source of truth for the designer. Visual controls are associated with their corresponding syntax nodes, allowing property changes to update the underlying representation.

## Technology

- **C#** — Main application and compiler implementation.
- **WinUI 3** — Native Windows user interface.
- **.NET** — Application runtime.
- **System.Xml** — XML parsing and serialization.
- **Roslyn Source Generators** — Automatic control metadata generation.

## Intended Workflow

VBS 2G is designed around a simple workflow:

1. Open an existing XAML document or start a new design.
2. Add and configure controls visually.
3. Preview the resulting interface.
4. Generate the XAML.
5. Copy the generated code.
6. Paste it into your Visual Studio project.

No project management, solution explorer, build system, or debugging environment is intended to be part of VBS 2G.

It focuses on one job: **visual XAML editing.**

## Development Status

VBS 2G is currently under development.

| Feature | Status |
|---|---|
| Visual designer | Implemented |
| Toolbox | Implemented |
| Property inspector | Implemented |
| XAML import | Implemented |
| Fullscreen preview | Implemented |
| Interactive preview | Implemented |
| XAML generation | Implemented |
| Clipboard integration | Implemented |
| Document saving | Implemented |
| Adding new files | Implemented |
| Opening files | Partially Implemented |

## Philosophy

VBS 2G isn't intended to compete with Visual Studio.

Visual Studio already provides a complete development environment. VBS 2G instead focuses on improving one particular part of that workflow: designing XAML interfaces.

**Design visually. Generate XAML. Continue developing in Visual Studio.**