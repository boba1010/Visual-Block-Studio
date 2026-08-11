# Visual Block Studio — Roadmap

## 🧱 Canvas Navigation

* [x] Zoom
* [x] Zoom-at-cursor
* [x] Canvas panning
* [ ] Zoom limits / zoom indicator
* [ ] Reset view
* [ ] Fit canvas to content

## 🧩 Block Interaction

* [x] Block hit testing
* [x] Nested block hit testing
* [x] Block dragging
* [x] Parent constraints
* [ ] Multi-selection
* [ ] Selection rectangle
* [ ] Move multiple blocks
* [ ] Delete blocks
* [ ] Duplicate blocks
* [ ] Copy / paste blocks
* [ ] Undo / redo

## ➕ Block Creation

* [ ] Toolbox
* [ ] Drag blocks from toolbox to canvas
* [ ] Right-click → Add Block
* [ ] Insert blocks into other blocks
* [ ] Visual nesting/drop targets
* [ ] Reorder children
* [ ] Block templates

## 🎨 Rendering

* [x] Basic Win2D block rendering
* [x] Selection rendering
* [x] Property flyout rendering
* [x] Property flyout hit testing
* [ ] Hover states
* [ ] Better block visuals
* [ ] Connection/nesting indicators
* [ ] Selection handles
* [ ] Resize handles
* [ ] Canvas grid
* [ ] Snap to grid
* [ ] Minimap

## ⚙️ Properties

* [x] Property flyout
* [ ] Property controls
* [ ] Edit block position
* [ ] Edit block size
* [ ] Edit XAML properties
* [ ] Property validation
* [ ] Property change notifications

## 💾 Persistence

* [x] DTO models
* [x] DTO → block conversion
* [x] Block → DTO conversion
* [x] Parent serialization strategy
* [x] `Type` → string mapping
* [ ] Save `.vb` files
* [ ] Load `.vb` files
* [ ] Project file support
* [ ] File format versioning
* [ ] Graceful handling of unknown block types

## 🏗️ Architecture

* [ ] Central `BlockTypes` registry
* [ ] Block factory
* [ ] Separate model/editor/rendering responsibilities
* [ ] Command system
* [ ] Undo/redo command history
* [ ] Keep DTOs independent from editor state
* [ ] Improve AOT compatibility

## 🧰 Toolbox

* [ ] Toolbox UI
* [ ] Block categories
* [ ] Block search
* [ ] Block templates
* [ ] Recently used blocks
* [ ] Drag-and-drop block creation

## 📄 XAML Support

* [ ] XAML parser
* [ ] XAML → block hierarchy
* [ ] Block hierarchy → XAML
* [ ] XAML property mapping
* [ ] XAML event mapping
* [ ] XAML validation
* [ ] Preserve unsupported XAML safely

## 💻 C++ Support

* [ ] C++ parser
* [ ] C++ AST integration
* [ ] C++ → blocks
* [ ] Blocks → C++
* [ ] C++ property/event mapping
* [ ] C++ syntax validation

## 🔄 Code Synchronization

* [ ] AST → block synchronization
* [ ] Block → AST synchronization
* [ ] Detect external code changes
* [ ] Preserve manual code changes
* [ ] Conflict handling
* [ ] Incremental updates

## 👁️ Live Preview

* [ ] XAML live preview
* [ ] Preview updates when blocks change
* [ ] Preview selection synchronization
* [ ] Preview error reporting
* [ ] Preview refresh/reload

## 🚀 Later / Advanced

* [ ] Minimap
* [ ] Search blocks
* [ ] Keyboard shortcuts
* [ ] Context menus
* [ ] Block alignment tools
* [ ] Distribute/space blocks
* [ ] Snap guides
* [ ] Themes
* [ ] Performance optimization
* [ ] Large-document virtualization
* [ ] Plugin system
* [ ] Extension API

## 🎯 Suggested Order

### Phase 1 — Editor Foundation

* [x] Zoom
* [x] Zoom-at-cursor
* [x] Panning
* [x] Hit testing
* [x] Block dragging
* [x] Parent constraints
* [x] Property flyout

### Phase 2 — Editing

* [ ] Block creation
* [ ] Selection improvements
* [ ] Delete
* [ ] Duplicate
* [ ] Copy / paste
* [ ] Undo / redo
* [ ] Resize
* [ ] Multi-selection

### Phase 3 — Block System

* [ ] Toolbox
* [ ] Block templates
* [ ] Nesting/drop targets
* [ ] Property editing
* [ ] Block factory
* [ ] `BlockTypes` registry

### Phase 4 — Persistence

* [ ] Save
* [ ] Load
* [ ] Project structure
* [ ] File format versioning
* [ ] Error recovery

### Phase 5 — Language Integration

* [ ] XAML parser
* [ ] XAML generation
* [ ] C++ parser
* [ ] C++ generation
* [ ] AST synchronization

### Phase 6 — IDE Features

* [ ] Live preview
* [ ] Code synchronization
* [ ] Search
* [ ] Keyboard shortcuts
* [ ] Minimap
* [ ] Performance optimization
