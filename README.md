<p align="center">
  <img src="docs/assets/auron-logo-banner.jpg" alt="AURON — Time in a New Dimension" width="100%" />
</p>

<h1 align="center">AURON AR — Luxury Watch Experience</h1>

<p align="center">
  <strong>Time in a New Dimension.</strong><br />
  Explore the mechanics. Customize the finish. Try the watch on your wrist with augmented reality.
</p>

<p align="center">
  <a href="https://mihadkazi1.github.io/auron-watch-website/"><strong>Visit the AURON Website</strong></a>
  &nbsp; • &nbsp;
  <a href="https://github.com/mihadkazi1/AURON-Watch-AR/releases"><strong>Android App / Releases</strong></a>
  &nbsp; • &nbsp;
  <a href="https://github.com/mihadkazi1/AURON-Watch-AR"><strong>Unity Source Repository</strong></a>
</p>

<p align="center">
  <img alt="Unity 6.3 LTS" src="https://img.shields.io/badge/Unity-6.3%20LTS-black?logo=unity" />
  <img alt="C Sharp" src="https://img.shields.io/badge/Language-C%23-68217A?logo=csharp" />
  <img alt="Vuforia Engine" src="https://img.shields.io/badge/AR-Vuforia-6D4AFF" />
  <img alt="MediaPipe" src="https://img.shields.io/badge/Tracking-MediaPipe-129B50" />
  <img alt="Platform Android" src="https://img.shields.io/badge/Platform-Android-3DDC84?logo=android&logoColor=white" />
</p>

---

## Overview

**AURON AR** is a mobile augmented-reality luxury watch experience built with Unity. It brings a detailed 3D timepiece into an interactive presentation, lets users inspect its mechanical construction, choose premium finishes, explore a lume effect, and try the watch on their wrist using the device camera and real-time hand/wrist tracking.

The project combines **Unity**, **Vuforia Engine**, and the **Google MediaPipe Unity Plugin** with custom C# controllers and a boutique-inspired interface. A companion AURON website presents the brand and watch experience online.

> **Try it:** [Open the live AURON website](https://mihadkazi1.github.io/auron-watch-website/). The Android link below opens the repository's Releases page; download the APK there once a release asset has been published.

## Quick Links

| Resource | Link |
|---|---|
| 🌐 Live website | [mihadkazi1.github.io/auron-watch-website](https://mihadkazi1.github.io/auron-watch-website/) |
| 📱 Android APK / release notes | [AURON-Watch-AR Releases](https://github.com/mihadkazi1/AURON-Watch-AR/releases) |
| 💻 Unity source code | [AURON-Watch-AR repository](https://github.com/mihadkazi1/AURON-Watch-AR) |
| 🐞 Issues and feedback | [Open an issue](https://github.com/mihadkazi1/AURON-Watch-AR/issues) |

## Features

### 1. Interactive 3D Watch Showcase

- Orbit the watch with touch or mouse controls.
- Pinch to zoom on compatible touch screens.
- Use the automatic turntable presentation mode.
- Inspect the watch from different angles in the 3D view.

### 2. Exploded Mechanical View

- Switch between the assembled watch and an exploded presentation.
- View the watch's **13 mechanical layers** (`Cube.003` through `Empty.012`), plus the crown (`Empty.013`) and strap (`Empty.014`).
- Follow a deterministic layer sequence during the inside/reassemble animation.
- Tap supported components in inspection mode to highlight gears in gold and view their on-screen technical information.

### 3. Finish Customization

Explore four case/bracelet finish options:

- **Obsidian Black**
- **18K Rose Gold**
- **Royal Yellow Gold**
- **Platinum 950**

### 4. Super-LumiNova™ Night Glow

Enable the lume presentation to see the watch's luminous elements in a dark-mode visual treatment.

### 5. Live Wrist Try-On

- Enter try-on mode from the AR experience.
- Clone the active watch model into AR-camera space and hide the original target-tracked presentation model while try-on is active.
- Use MediaPipe hand landmark estimation to locate the hand and wrist in the camera feed.
- Track **21 hand landmarks** and apply plausibility checks so the watch can remain attached when the hand is open or closed into a fist.
- Choose a watch case size: **36 mm, 40 mm, 42 mm, or 46 mm**.
- Optionally use a depth-mask occluder for the back clasp where configured.

### 6. Vuforia Image-Target Experience

- Use Vuforia's AR camera and configured image targets to start the target-based 3D watch experience.
- The companion website includes starter target images for Classic, Chrono, and Skeleton models; verify target quality and database configuration before production use.

### 7. Luxury UI and Sound

- Rounded glass-style controls and contextual tools, including `LUME`, `SNAP`, `SFX`, and `WIND`.
- Champagne-gold accents (`#C8A96E`) and wide-tracked uppercase typography.
- Procedural mechanical escapement clicks and synchronized gear-locking sounds during reassembly.

### 8. Boutique Snapshot Capture

- Capture high-resolution images of the AR wrist experience.
- Frame captures in an obsidian-and-gold boutique-style AURON design.
- Save images to the device's public gallery through Android `MediaStore`, under `Pictures/AURON`.

## Screenshots & Demo Gallery

Add **real captures from the running Unity app or an Android device** to `docs/screenshots/` using the filenames below. The README is structured for the gallery, but actual app screenshots were not included with the project summary used to prepare this file, so no mock screenshots are presented as real app captures.

| Screenshot | File to add | What to capture |
|---|---|---|
| Main experience | `docs/screenshots/01-main-watch-view.png` | The assembled 3D watch and main controls |
| Exploded view | `docs/screenshots/02-exploded-watch.png` | Separated mechanical layers |
| Component inspection | `docs/screenshots/03-gear-inspection.png` | A highlighted gear and its technical details |
| Finish customization | `docs/screenshots/04-watch-finishes.png` | The material/finish selector |
| Live wrist try-on | `docs/screenshots/05-live-wrist-try-on.png` | Watch positioned on a real wrist through the camera |
| Lume mode | `docs/screenshots/06-lume-mode.png` | The Super-LumiNova™ night presentation |
| Snapshot capture | `docs/screenshots/07-boutique-snapshot.png` | The branded snapshot frame or saved photo |
| Website | `docs/screenshots/08-website-homepage.png` | The live website homepage |

Once those files are committed, replace this table with an embedded gallery. Example Markdown:

```md
![AURON 3D watch view](docs/screenshots/01-main-watch-view.png)
![AURON live wrist try-on](docs/screenshots/05-live-wrist-try-on.png)
```

## Technology Stack

| Technology | Role |
|---|---|
| **Unity 6.3 LTS** | Real-time 3D rendering, mobile application, interaction and build pipeline |
| **C#** | AR controllers, model interactions, UI behaviour and snapshot workflow |
| **Vuforia Engine** | AR camera and image-target tracking |
| **Google MediaPipe Unity Plugin** | Hand landmark estimation for live wrist try-on |
| **TextMeshPro** | Crisp in-app typography and information panels |
| **Android MediaStore** | Saves snapshot images to the device's public photo gallery |
| **Git LFS** | Versioning for large model/archive assets stored through Git LFS |

## Core Architecture

| Component | Responsibility |
|---|---|
| `AuronWatchExperience.cs` | 3D orbit/zoom, turntable mode, exploded/reassembly presentation, gear inspection and finish/lume controls |
| `AuronTryOnController.cs` | Try-on mode lifecycle, watch clone, wrist placement, size controls and optional occlusion |
| `AuronVuforiaHandLandmarker.cs` | MediaPipe hand landmark processing and hand/fist plausibility validation |
| `AURONUI.cs` | Luxury interface, contextual controls and on-screen presentation |
| `AuronLuxuryFeatures.cs` | Premium interaction features, mechanical-style audio and boutique snapshot capture |

## How the AR Experience Works

1. **Launch:** The app opens into its configured starting scene.
2. **Scan:** The user points the camera at a configured AURON image target.
3. **Explore:** The tracked watch can be viewed, rotated, zoomed and presented in turntable mode.
4. **Inspect:** The user opens the exploded view and taps supported mechanical components.
5. **Customize:** The user selects a finish or enables the lume treatment.
6. **Try on:** The app activates the try-on controller, tracks the hand/wrist and positions the watch relative to the detected wrist.
7. **Capture:** The user takes a branded snapshot, which is saved to the Android gallery when permissions and device configuration allow it.

## Requirements

- Unity **6.3 LTS** (or the compatible version used by this project).
- Android Build Support and the required Android SDK/NDK and OpenJDK modules for Android builds.
- Vuforia Engine configured for the project, including a valid Vuforia license key and the relevant image-target database.
- Google MediaPipe Unity Plugin and its required model assets.
- A compatible Android device with a working camera for mobile AR and try-on testing.

## Setup the Unity Project

### 1. Clone the repository

Install Git and Git LFS, then run:

```bash
git lfs install
git clone https://github.com/mihadkazi1/AURON-Watch-AR.git
cd AURON-Watch-AR
git lfs pull
```

Git LFS is needed to retrieve large assets tracked through LFS. Keep the `.gitattributes` file committed so Git knows which files use LFS.

### 2. Open in Unity

1. Open Unity Hub.
2. Select **Add project from disk** and choose the cloned `AURON-Watch-AR` folder.
3. Open it with Unity 6.3 LTS and allow Unity to import assets and resolve packages.
4. Confirm the expected scenes are included in Build Profiles/Build Settings.

### 3. Configure Vuforia

1. Open the project's Vuforia configuration.
2. Add your valid Vuforia license key locally.
3. Confirm the correct target database and image targets are enabled.
4. Do not commit private credentials or keys to the repository.

### 4. Configure MediaPipe

Confirm that the expected hand-landmarker model file, `hand_landmarker.bytes`, is present in the configured StreamingAssets location and is included in the app build. Use the matching model and plugin versions required by the project.

### 5. Build for Android

1. Select **File → Build Profiles** (or **File → Build Settings** in older Unity versions).
2. Select **Android** and switch platform if necessary.
3. Check that the correct startup and AR scenes are included.
4. Configure the app identifier, version and release signing key in Player/Publishing Settings.
5. Build an APK for direct testing or release.

**Signing-key safety:** Back up the release keystore and passwords securely. Never commit the keystore, passwords, Vuforia credentials, or other private secrets to GitHub.

## Download the Android App

The APK download is hosted through GitHub Releases rather than stored in the source tree:

**[Open AURON-Watch-AR Releases](https://github.com/mihadkazi1/AURON-Watch-AR/releases)**

When publishing a version, attach the built APK (for example, `AURON-AR-v1.0.0.apk`) to the release. If no release asset is listed yet, the APK has not been made available through that page.

To install a release APK on Android, download the file, open it on the device, allow installation from that source if Android requests it, and grant camera permission for AR features.

## Companion Website

The AURON website is deployed using GitHub Pages:

**[https://mihadkazi1.github.io/auron-watch-website/](https://mihadkazi1.github.io/auron-watch-website/)**

The website is a companion brand/product experience and is maintained in its own repository: [auron-watch-website](https://github.com/mihadkazi1/auron-watch-website).

## Development Notes

- Keep Unity `.meta` files alongside their assets to preserve stable asset references.
- Do not commit Unity-generated folders such as `Library/`, `Temp/`, `Obj/`, or local build output.
- Use Git LFS for files configured in `.gitattributes`; confirm LFS uploads succeeded before treating a fresh clone as complete.
- Test tracking, rendering, camera permission, try-on alignment, and photo saving on a physical Android device before publishing a new APK.
- Check third-party asset and plugin licenses before redistributing included packages or content publicly.

## Status

The project summary describes the main AR, interaction, customization, tracking and gallery-capture workflows as implemented, with Android rendering and hand-pose issues addressed during development. The companion website is live. The Android download becomes available to visitors once the APK is attached to a public GitHub Release.

## Acknowledgements

- Unity
- Vuforia Engine
- Google MediaPipe
- TextMeshPro

## License & Third-Party Assets

No project-wide open-source license has been specified in this README. Unless a license is added by the project owner, do not assume the source code, brand artwork, 3D models, textures, watch design or other bundled project assets are free to reuse. Third-party tools, plugins and assets remain subject to their own licenses and terms.

---

<p align="center">
  <strong>AURON</strong><br />
  <em>Time in a New Dimension.</em>
</p>
