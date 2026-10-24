# Visual Block Studio — Progress & Roadmap

> **Project:** Visual Block Studio (VBS)  
> **Platform:** WinUI 3 / C# / .NET  
> **Vision:** A Scratch-inspired visual programming IDE for building real WinUI applications, combining visual UI design, XAML generation, and visual application logic.

## 🚀 Current Milestones

- [x] Native WinUI application and visual editor
- [x] Custom typed XAML syntax tree
- [x] XAML code generation from the syntax tree
- [x] Source-generated control metadata and code generation support
- [x] Native WinUI previewer
- [x] Preview basic controls without compiling the target application
- [x] Full-screen preview window
- [x] Property inspector integration with the syntax model
- [x] Update supported preview control properties through the inspector
- [x] Basic XAML content editing
- [ ] Complete control and property coverage
- [ ] Visual programming for application behavior
- [ ] Runtime execution of complete generated applications

---

## 🧱 Canvas & Visual Editing

### Navigation
- [x] Zoom
- [x] Zoom-at-cursor
- [x] Canvas panning
- [ ] Zoom limits and zoom indicator
- [ ] Reset view
- [ ] Fit canvas to content

### Selection & Interaction
- [x] Block hit testing
- [x] Nested block hit testing
- [x] Block dragging
- [x] Parent constraints
- [x] Selection rendering
- [ ] Multi-selection
- [ ] Selection rectangle
- [ ] Move multiple blocks
- [ ] Delete blocks
- [ ] Duplicate blocks
- [ ] Copy and paste
- [ ] Undo and redo

---

## 🎨 Rendering & Designer Experience

- [x] Basic Win2D block rendering
- [x] Selection rendering
- [x] Property flyout rendering
- [x] Property flyout hit testing
- [ ] Hover states
- [ ] Improved block visuals
- [ ] Connection and nesting indicators
- [ ] Selection and resize handles
- [ ] Canvas grid
- [ ] Snap guides
- [ ] Minimap
- [ ] Keyboard navigation and shortcuts
- [ ] Context menus
- [ ] Themes

---

## ⚙️ Property Inspector

- [x] Property flyout
- [x] Basic property editing
- [x] String property editor
- [x] Update syntax model from property edits
- [x] Synchronize supported edits with native preview controls
- [x] Numeric property editors
- [ ] Boolean and enumeration editors
- [x] Color picker
- [ ] Property-type conversion
- [ ] Property validation
- [ ] Validation diagnostics
- [ ] Edit position and dimensions
- [ ] Support more XAML properties
- [ ] Handle read-only and unsupported properties
- [ ] Dependency property metadata
- [ ] Property change notifications

---

## 📄 XAML Language Support

### Syntax & Generation
- [x] Custom typed XAML syntax model
- [x] Represent controls and nested elements
- [x] Represent content
- [x] Generate readable XAML
- [x] Generate control metadata using source generation
- [ ] Expand supported control types
- [ ] Expand property-type conversions
- [ ] Attached properties
- [ ] Collections and collection properties
- [ ] Events and event declarations
- [ ] Namespaces and namespace mapping
- [ ] XAML parser
- [ ] XAML-to-syntax-tree conversion
- [ ] Round-trip parsing and generation
- [ ] Preserve unsupported XAML safely

### Styles & Resources
- [ ] Resource dictionaries
- [ ] Static and theme resources
- [ ] Styles and setters
- [ ] Control templates
- [ ] Data templates
- [ ] Bindings and converters
- [ ] Design-time data
- [ ] Custom controls
- [ ] Application-level resources

---

## 🖥️ Native Preview & Debug Mode

### Current Capabilities
- [x] Construct actual WinUI controls from the syntax tree
- [x] Preview basic controls without compiling the target application
- [x] Display the preview in a separate native window
- [x] Update supported control properties through the inspector

### Preview Improvements
- [ ] Expand supported controls
- [ ] Improve nested content handling
- [ ] Handle unsupported controls and invalid values
- [ ] Improve layout fidelity
- [ ] Preview diagnostics and error reporting
- [ ] Resource and theme support
- [ ] Preview selection synchronization
- [ ] Keep the preview window open across updates
- [ ] Live property updates
- [ ] Incremental visual-tree updates
- [ ] Structural updates without recreating the entire preview

### Runtime Execution
- [ ] Runtime application initialization
- [ ] Runtime event-handler registration
- [ ] Execute supported event handlers
- [ ] Integrate user-written C# handlers
- [ ] Investigate Roslyn-based runtime compilation
- [ ] Support application resources and navigation
- [ ] Handle runtime errors and exceptions
- [ ] Run generated applications without a full project rebuild

---

## 🧩 Visual Programming System

**Goal:** Provide a Scratch-inspired visual programming experience for building real WinUI application behavior.

### Visual Logic Editor
- [ ] Define a visual logic block model
- [ ] Create a dedicated logic workspace
- [ ] Design block shapes and visual categories
- [ ] Drag and connect logic blocks
- [ ] Support nested statements and expressions
- [ ] Validate block connections
- [ ] Add variables and constants
- [ ] Add operators and expressions
- [ ] Add conditional logic
- [ ] Add loops
- [ ] Add functions and reusable logic
- [ ] Add error and type validation

### WinUI Events & Controls
- [ ] Associate logic with WinUI controls
- [ ] Represent events such as button clicks
- [ ] Create event-handler blocks
- [ ] Access control properties from logic blocks
- [ ] Modify control properties through visual logic
- [ ] Support arguments and event parameters
- [ ] Reference controls through stable identifiers
- [ ] Handle missing controls and invalid event references

### Application Behavior
- [ ] Create message and dialog actions
- [ ] Support navigation actions
- [ ] Support variables and application state
- [ ] Allow logic to interact with the visual tree
- [ ] Define reusable behavior blocks
- [ ] Support custom logic blocks
- [ ] Provide visual diagnostics for invalid logic

### Execution & Code Generation
- [ ] Define an intermediate representation for visual logic
- [ ] Interpret supported logic blocks
- [ ] Connect interpreted logic to WinUI events
- [ ] Investigate generating C# from visual logic
- [ ] Investigate runtime compilation where needed
- [ ] Handle runtime exceptions
- [ ] Debug visual logic execution
- [ ] Preserve separation between UI syntax and behavior logic

---

## 💾 Persistence & Project System

- [x] DTO models
- [x] DTO-to-block conversion
- [x] Block-to-DTO conversion
- [x] Parent serialization strategy
- [x] Type-to-string mapping
- [ ] Save project files
- [ ] Load project files
- [ ] Persist visual logic graphs
- [ ] Project structure and metadata
- [ ] File format versioning
- [ ] Graceful handling of unknown block types
- [ ] Error recovery
- [ ] Document dirty-state tracking
- [ ] Autosave and recovery

---

## 🏗️ Architecture & Compiler Infrastructure

- [x] Typed syntax representation
- [x] Source-generator-based metadata generation
- [x] Separate XAML generation and native preview paths
- [ ] Central control registry
- [ ] Control factory and extensible control mapping
- [ ] Separate model, editor, and rendering responsibilities
- [ ] Command system
- [ ] Undo/redo command history
- [ ] Keep persistence DTOs independent of editor state
- [ ] Consistent property metadata
- [ ] Incremental syntax-model updates
- [ ] AOT compatibility improvements
- [ ] Reduce duplicated control-specific logic
- [ ] Define an independent visual logic model
- [ ] Define the interface between UI controls and visual logic
- [ ] Extension API

---

## 🔄 Code & Model Synchronization

- [ ] Synchronize visual UI edits with the XAML syntax tree
- [ ] Synchronize supported syntax changes with the visual editor
- [ ] Detect external XAML changes
- [ ] Preserve manual code edits
- [ ] Conflict handling
- [ ] Incremental updates
- [ ] Diagnostics for unsupported syntax
- [ ] Synchronize control identifiers with visual logic references
- [ ] Detect invalid event-handler references

---

## 📈 Suggested Development Order

### Phase 1 — Complete the Core Designer
- [ ] Finish basic control creation and nesting
- [ ] Expand property editing
- [ ] Add delete, duplicate, and resize
- [ ] Add undo and redo
- [ ] Improve selection and drag-and-drop

### Phase 2 — Strengthen XAML Support
- [ ] Expand control and property coverage
- [ ] Implement parsing
- [ ] Add type conversion and validation
- [ ] Support attached properties and collections
- [ ] Improve round-trip generation

### Phase 3 — Stabilize Preview & Debug Mode
- [ ] Improve control coverage and layout fidelity
- [ ] Add diagnostics and error handling
- [ ] Support resources and styles
- [ ] Keep preview state stable
- [ ] Add live property updates when ready

### Phase 4 — Build the Visual Programming Foundation
- [ ] Define the visual logic model
- [ ] Implement logic block editing
- [ ] Add event-handler blocks
- [ ] Add variables, expressions, and conditions
- [ ] Associate logic with UI controls
- [ ] Persist logic graphs

### Phase 5 — Runtime Execution
- [ ] Initialize generated application UI at runtime
- [ ] Connect event handlers
- [ ] Execute supported visual logic
- [ ] Investigate runtime C# execution
- [ ] Support navigation and application resources
- [ ] Handle runtime errors
- [ ] Reduce dependence on full rebuilds during development

### Phase 6 — Project Workflow & Synchronization
- [ ] Save and load complete projects
- [ ] Establish project structure
- [ ] Add versioning and recovery
- [ ] Synchronize visual edits and generated XAML
- [ ] Synchronize UI controls with visual logic references

### Phase 7 — Advanced IDE Features
- [ ] Search and keyboard shortcuts
- [ ] Minimap and alignment tools
- [ ] Extension API
- [ ] Plugin system
- [ ] Performance optimization
- [ ] Large-document virtualization

---

## 🎯 Current Priority

**Build a reliable visual WinUI development workflow first, then expand into visual application logic.**

The immediate goal is to make VBS reliably design, represent, preview, edit, and generate XAML for basic WinUI applications.

The long-term goal is to combine that designer with a Scratch-inspired visual programming system, allowing developers to build real WinUI applications with interactive behavior—not merely static interfaces.

Live preview, runtime event execution, advanced resources, and additional languages should be introduced incrementally as the foundation becomes stable.