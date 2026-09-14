# Visual Block Studio Release Channels

Visual Block Studio uses release channels to communicate the stability and maturity of builds.

Each channel represents a different stage of development. Builds may move between channels as they become more stable and complete.

## 🚧 Early Access

**The earliest publicly available builds.**

Early Access builds are intended for users who want to experience VBS as it is being actively developed.

Expect:

* Major changes
* Incomplete features
* Broken or experimental functionality
* Frequent updates
* Possible data or project format changes
* Features being added or removed

**Recommended for:** Users who want to follow development from the beginning.

---

## 🧪 Experimental

**Features and systems that are actively being tested.**

Experimental builds contain functionality that may not yet be ready for regular development work.

Expect:

* Unstable features
* Significant changes
* Performance issues
* Experimental APIs or systems
* Features that may be removed or redesigned

**Recommended for:** Developers who want to test new ideas and provide feedback.

---

## 🔬 Alpha

**Early usable versions of VBS.**

Alpha builds contain the major foundations of a release but are still under heavy development.

Expect:

* Bugs
* Missing functionality
* Breaking changes
* Incomplete workflows
* Frequent improvements

**Recommended for:** Developers who want to use VBS while accepting instability.

---

## 🧩 Beta

**Feature-complete or close-to-complete builds undergoing testing and polishing.**

Beta builds are intended to be significantly more usable than Alpha builds.

Expect:

* Bug fixes
* Performance improvements
* UI refinements
* Occasional breaking changes
* Remaining known issues

**Recommended for:** Users who want to test upcoming functionality before release.

---

## 👀 Preview

**Near-stable builds of upcoming releases.**

Preview builds are intended to provide an early look at versions that are approaching stability.

Expect:

* Minor bugs
* Final polishing
* Performance improvements
* Limited breaking changes
* Features close to their final form

**Recommended for:** Users who want to try upcoming releases before they become Stable.

---

## ✅ Stable

**The recommended channel for normal use.**

Stable releases have passed the development and testing stages and are intended to provide a reliable VBS experience.

Stable does not mean that VBS will never contain bugs. It means the release is considered ready for general use.

Expect:

* Greater reliability
* Fewer breaking changes
* Tested features
* Improved performance
* Regular maintenance

**Recommended for:** Everyone using VBS for regular development.

---

## 📊 Channel Overview

| Channel         | Stability | Development |
| --------------- | --------- | ----------- |
| 🚧 Early Access | Very Low  | Very High   |
| 🧪 Experimental | Low       | Very High   |
| 🔬 Alpha        | Low       | High        |
| 🧩 Beta         | Medium    | Medium      |
| 👀 Preview      | High      | Low         |
| ✅ Stable        | Highest   | Maintenance |

## 🔄 Release Progression

A typical feature or release may progress through the channels like this:

```text
Early Access
     ↓
Experimental
     ↓
Alpha
     ↓
Beta
     ↓
Preview
     ↓
Stable
```

Not every feature or release is required to pass through every channel. A feature may remain in a channel for an extended period, skip a channel, or return to an earlier channel if significant problems are discovered.

## ⚠️ Important

Release channels describe the **expected stability** of a build, not a guarantee.

VBS is actively developed, and changes may occur at any stage of development.

Always keep backups of important projects when using **Early Access, Experimental, Alpha, or Beta** builds.