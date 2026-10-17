<p align="center">
  <img src="docs/assets/auron-logo-banner.jpg" alt="AURON — Time in a New Dimension" width="100%" />
</p>

<h1 align="center">AURON AR Watch ⌚🥽</h1>

<p align="center">
  <strong>Interactive Luxury Watch Experience & Virtual Wrist Try-On</strong><br />
  Explore the mechanics. Choose your finish. Experience AURON on your wrist.
</p>

<p align="center">
  <a href="https://github.com/mihadkazi1/AURON-Watch-AR/releases">
    <img src="https://img.shields.io/badge/Android-APK%20Releases-3DDC84?style=for-the-badge&logo=android&logoColor=white" alt="Android APK Releases" />
  </a>
  <a href="https://mihadkazi1.github.io/auron-watch-website/">
    <img src="https://img.shields.io/badge/Visit-AURON%20Website-C8A96E?style=for-the-badge&logo=googlechrome&logoColor=black" alt="Visit AURON Website" />
  </a>
  <a href="https://github.com/mihadkazi1/AURON-Watch-AR">
    <img src="https://img.shields.io/badge/Unity-Source%20Code-black?style=for-the-badge&logo=unity" alt="Unity Source Code" />
  </a>
</p>

<p align="center">
  <img alt="Unity 6.3 LTS" src="https://img.shields.io/badge/Engine-Unity%206.3%20LTS-black?logo=unity" />
  <img alt="C Sharp" src="https://img.shields.io/badge/Language-C%23-68217A?logo=csharp" />
  <img alt="Vuforia Engine" src="https://img.shields.io/badge/AR-Vuforia-6D4AFF" />
  <img alt="MediaPipe" src="https://img.shields.io/badge/Hand%20Tracking-MediaPipe-129B50" />
  <img alt="Android" src="https://img.shields.io/badge/Platform-Android-3DDC84?logo=android&logoColor=white" />
</p>

---

## 📖 About AURON AR

**AURON AR** is an Android augmented-reality watch experience designed to let people explore a luxury timepiece beyond a traditional product photo. Built with **Unity, C#, Vuforia Engine, and the Google MediaPipe Unity Plugin**, the app combines image-target AR, interactive 3D inspection, premium finish customization, luminous presentation, and camera-based wrist try-on.

The experience is designed around this flow:

**Launch → Scan → Explore → Inspect → Customize → Try On → Capture**

Visit the companion website to explore the AURON brand and watch presentation:

**🌐 Website:** https://mihadkazi1.github.io/auron-watch-website/

> **Android download:** Open the [AURON-Watch-AR Releases page](https://github.com/mihadkazi1/AURON-Watch-AR/releases) and download the APK attached to the latest published release. If no APK asset is listed, a public app release has not yet been attached.

---

## 🚀 Project Highlights

- 📱 Android AR application built with Unity.
- 🎯 Image-target tracking powered by Vuforia Engine.
- ⌚ Interactive 3D watch showcase with touch/mouse orbit, pinch-to-zoom, and auto-turntable mode.
- ⚙️ Exploded mechanical view and reassembly animation for the watch's internal layers.
- 🔎 Tap-to-inspect supported mechanical parts, with gold highlighting and on-screen technical information.
- 🎨 Four premium finishes: Obsidian Black, 18K Rose Gold, Royal Yellow Gold, and Platinum 950.
- 🌙 Super-LumiNova™ night-glow presentation.
- 🤚 Camera-based wrist try-on using MediaPipe hand-landmark estimation.
- 📏 Four selectable watch case sizes: 36 mm, 40 mm, 42 mm, and 46 mm.
- 📸 Boutique-style snapshot framing and saving to the Android gallery under `Pictures/AURON`.
- 🔊 Mechanical-inspired escapement clicks and contextual `LUME`, `SNAP`, `SFX`, and `WIND` controls.

---

## 🕹️ How to Use the App

### 1. Install and launch

1. Open the [GitHub Releases page](https://github.com/mihadkazi1/AURON-Watch-AR/releases).
2. Download the APK from the latest release.
3. Open the downloaded APK on your Android phone and confirm installation. Android may ask you to allow installation from the app you used to open the APK.
4. Launch **AURON AR** and grant camera access when prompted. Camera access is required for the AR and wrist try-on features.

### 2. Scan directly from the AURON website

You can use the AURON website as the image-target source—there is no need to print the target if you can display it on another screen.

**Recommended setup: use two devices**

1. On a computer, tablet, or second phone, open the [AURON Watch website](https://mihadkazi1.github.io/auron-watch-website/).
2. Open one of the target images below and display it clearly on that screen. Use full-screen view where possible and increase the screen brightness.
   - [Classic — AURON Target 01](https://mihadkazi1.github.io/auron-watch-website/ar-targets/AURON_TARGET_01.jpg)
   - [Chrono — AURON Target 02](https://mihadkazi1.github.io/auron-watch-website/ar-targets/AURON_TARGET_02.jpg)
   - [Skeleton — AURON Target 03](https://mihadkazi1.github.io/auron-watch-website/ar-targets/AURON_TARGET_03.jpg)
3. On your Android phone, install and launch **AURON AR**, then grant camera permission.
4. Enter the app's AR/scanning experience and point the phone camera at the target image displayed on the other screen.
5. Keep the whole image inside the camera view. Hold both devices steady and avoid glare or reflections.
6. When Vuforia recognizes the target, the configured 3D watch presentation should appear.

**Only have one phone?** A phone cannot scan an image displayed on its own screen using its rear camera at the same time. Open the website target on a computer/second device, or print the target and scan the printed image.

**Important target compatibility note:** Vuforia only recognizes target images that are included in the target database configured for the installed APK. The three website links above are the project's starter targets; successful recognition depends on the matching images being registered in the Vuforia database used to build the app. If a target is not recognized, try another registered target and check the app's Vuforia database configuration.

### 3. Explore the 3D watch

- **Rotate:** Drag across the watch view using touch or mouse input supported by the current build.
- **Zoom:** Pinch on a touch screen to zoom in or out where pinch controls are enabled.
- **Auto showcase:** Use the turntable/showcase control, if shown, to let the watch rotate automatically.
- **Inspect details:** Open the inside/exploded view to see the internal mechanical layers.

### 4. Explore the inside and inspect components

1. Select the **Inside** or exploded-view control.
2. Watch the mechanical layers separate in the configured sequence.
3. Tap a supported component in inspection mode to highlight it and view its technical information.
4. Use the reassemble control to return the components to the assembled watch.

The exploded presentation includes the configured mechanical layers (`Cube.003` through `Empty.012`), plus the crown (`Empty.013`) and strap (`Empty.014`).

### 5. Customize the watch

Select a finish in the customization controls:

| Finish | Look |
|---|---|
| **Obsidian Black** | Dark, understated finish |
| **18K Rose Gold** | Warm rose-gold finish |
| **Royal Yellow Gold** | Classic yellow-gold finish |
| **Platinum 950** | Cool platinum finish |

Use **LUME** to activate the Super-LumiNova™ night-glow presentation when available in the current view.

### 6. Try the watch on your wrist

1. Open **Try On** mode.
2. Allow the camera view and hand-tracking system to initialize.
3. Move your hand into the camera view so the hand and wrist are visible; keep the wrist reasonably steady while tracking locks on.
4. Choose the case size that you want to preview: **36 mm, 40 mm, 42 mm, or 46 mm**.
5. Move your wrist slowly to preview the watch placement from different angles.

**Tips for better tracking:** use even lighting, keep your hand and wrist in the camera frame, avoid heavy motion blur, and use a background that contrasts with your hand. The hand tracker is designed to handle both an open hand and a fist, but tracking quality can still vary with device, lighting, and camera conditions.

### 7. Capture and save a snapshot

1. Position the watch as you want it to appear in the photo.
2. Select **SNAP**.
3. Wait for the capture/save operation to finish.
4. Open your phone's Gallery or Photos app and look for the AURON image under `Pictures/AURON` where supported by the Android build.

Use **SFX** and **WIND** for the corresponding sound and watch-interaction controls when they are available in the active screen.

---

## 🖼️ Screenshots & Demo

Add real screenshots from the app to `docs/screenshots/`. Recommended captures are listed below. These filenames are suggestions; use the same names if you want the gallery paths to match the README.

| Screenshot | Suggested filename | What to show |
|---|---|---|
| Main watch view | `01-main-watch-view.png` | Assembled watch and main interface |
| Exploded view | `02-exploded-watch.png` | Separated mechanical components |
| Gear inspection | `03-gear-inspection.png` | Highlighted gear and specifications |
| Finish selector | `04-watch-finishes.png` | The four case/bracelet finishes |
| Wrist try-on | `05-live-wrist-try-on.png` | Watch placed on a wrist through the camera |
| Night glow | `06-lume-mode.png` | Luminous watch presentation |
| Snapshot | `07-boutique-snapshot.png` | Framed photo saved by SNAP |
| Website | `08-website-homepage.png` | A screenshot of the live website |

After adding the screenshots, you can embed them in this section like this:

```md
<p align="center">
  <img src="docs/screenshots/01-main-watch-view.png" width="48%" alt="AURON main watch view" />
  <img src="docs/screenshots/02-exploded-watch.png" width="48%" alt="AURON exploded mechanical view" />
</p>
```

*The package includes the brand banner, but not real in-app screenshots. Add your own captures so the README gallery shows the actual app.*

---

## 🧰 Technology Stack

| Technology | Purpose |
|---|---|
| **Unity 6.3 LTS** | Real-time 3D rendering, user interaction, Android application build |
| **C#** | Watch interaction, AR state, UI, try-on logic and snapshot features |
| **Vuforia Engine** | AR camera integration and image-target tracking |
| **Google MediaPipe Unity Plugin** | Hand-landmark estimation for live wrist try-on |
| **TextMeshPro** | In-app text and technical information panels |
| **Android MediaStore** | Saving snapshots to the device photo gallery |
| **Git LFS** | Versioning larger project assets tracked via LFS |

---

## 🏗️ Main Scripts & Responsibilities

| Script | Responsibility |
|---|---|
| `AuronWatchExperience.cs` | 3D orbit/zoom, turntable mode, exploded/reassemble animation, component inspection, finish and lume controls |
| `AuronTryOnController.cs` | Try-on flow, watch clone, wrist positioning, case-size controls and optional occlusion |
| `AuronVuforiaHandLandmarker.cs` | MediaPipe hand-landmark processing and open-hand/fist plausibility checks |
| `AURONUI.cs` | Luxury UI, contextual buttons and on-screen interaction flow |
| `AuronLuxuryFeatures.cs` | Premium interactions, mechanical-inspired audio and boutique snapshot capture |

---

## 💻 Open the Unity Project

### Requirements

- Unity **6.3 LTS**, or the compatible Unity version used to create the project.
- Unity Android Build Support, Android SDK/NDK Tools, and OpenJDK for Android builds.
- Vuforia Engine configured with a valid license key and the relevant image-target database.
- Google MediaPipe Unity Plugin and the required hand-landmarker model asset.
- An Android phone with a working camera for on-device AR and wrist try-on testing.

### Clone the repository

Install Git and Git LFS, then run:

```bash
git lfs install
git clone https://github.com/mihadkazi1/AURON-Watch-AR.git
cd AURON-Watch-AR
git lfs pull
```

### Open and configure

1. Open Unity Hub and choose **Add project from disk**.
2. Select the cloned `AURON-Watch-AR` folder and open it using the compatible Unity version.
3. Wait for Unity to import assets and resolve packages.
4. Confirm the intended startup and AR scenes are included in Build Profiles/Build Settings.
5. Verify Vuforia's license configuration and the target database before testing image tracking.
6. Confirm that `hand_landmarker.bytes` is present in the configured StreamingAssets location required by the project.

### Build an Android APK

1. Open **File → Build Profiles** (or **File → Build Settings** in older Unity versions).
2. Select **Android** and switch platform if required.
3. Confirm the app identifier, version, permissions, and included scenes.
4. Configure release signing through **Player Settings → Publishing Settings** if making a release build.
5. Build the APK and test it on a physical Android device, including camera permission, image-target recognition, wrist alignment, finish switching and snapshot saving.

**Security:** Do not commit a release keystore, its password, a private Vuforia key, or other credentials. Keep signing materials backed up privately so future app updates can be signed with the same key.

---

## 🌐 Links

- **Live AURON Website:** [mihadkazi1.github.io/auron-watch-website](https://mihadkazi1.github.io/auron-watch-website/)
- **Android APK and release notes:** [GitHub Releases](https://github.com/mihadkazi1/AURON-Watch-AR/releases)
- **Unity source repository:** [AURON-Watch-AR](https://github.com/mihadkazi1/AURON-Watch-AR)
- **Bug reports / feedback:** [Open an issue](https://github.com/mihadkazi1/AURON-Watch-AR/issues)

---

## 🛠️ Troubleshooting

- **The target is not detected:** Confirm it belongs to the target database built into the app, improve lighting, reduce glare, and keep the complete target in view.
- **The watch does not appear:** Ensure the target was detected, wait for AR initialization, and retry with the target fully visible.
- **Wrist tracking is unstable:** Improve lighting, keep the wrist within the camera frame, and move more slowly.
- **SNAP image is not in Gallery:** Wait for the save operation, then look under `Pictures/AURON`. Gallery indexing and behavior can vary by Android version and device.
- **The project opens with missing assets:** Run `git lfs pull` from the repository folder and allow Unity to finish importing.

---

## 📌 Project Status

The project summary describes the core watch showcase, exploded view, finish customization, MediaPipe wrist try-on, luxury UI/audio, and Android snapshot-saving workflow as implemented, with several mobile rendering and tracking issues addressed during development. The companion website is live. The APK is available to the public once it is attached to a published release on the Releases page.

## 📄 License & Asset Notice

No project-wide open-source license has been specified here. Unless the repository contains a license, don't assume the source code, brand artwork, watch model, textures, or bundled assets are free to reuse. Vuforia, MediaPipe, Unity, and other third-party components remain subject to their own licenses and terms. Review redistribution permissions before making the full project or its assets public.

---

<p align="center">
  <strong>AURON</strong><br />
  <em>Time in a New Dimension.</em>
</p>
