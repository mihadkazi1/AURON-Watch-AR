<p align="center">
  <img src="docs/assets/auron-logo-banner.jpg" alt="AURON — Time in a New Dimension" width="420" />
</p>

<h1 align="center">AURON AR Watch ⌚🥽</h1>

<p align="center">
  <strong>Real-Time AI Wrist Try-On &amp; Interactive Luxury Horology AR Experience</strong><br />
  Wear AURON live on your wrist using markerless AI hand tracking. Explore the mechanical calibre, customize precious-metal finishes, and capture boutique-edition snapshots.
</p>

<p align="center">
  <a href="https://github.com/mihadkazi1/AURON-Watch-AR/releases">
    <img src="https://img.shields.io/badge/Android-APK%20Releases-3DDC84?style=for-the-badge&amp;logo=android&amp;logoColor=white" alt="Android APK Releases" />
  </a>
  <a href="https://mihadkazi1.github.io/auron-watch-website/">
    <img src="https://img.shields.io/badge/Visit-AURON%20Website-C8A96E?style=for-the-badge&amp;logo=googlechrome&amp;logoColor=black" alt="Visit AURON Website" />
  </a>
  <a href="https://github.com/mihadkazi1/AURON-Watch-AR">
    <img src="https://img.shields.io/badge/Unity-Source%20Code-black?style=for-the-badge&amp;logo=unity" alt="Unity Source Code" />
  </a>
</p>

<p align="center">
  <img alt="Unity 6.3 LTS" src="https://img.shields.io/badge/Engine-Unity%206.3%20LTS-black?logo=unity" />
  <img alt="C Sharp" src="https://img.shields.io/badge/Language-C%23-68217A?logo=csharp" />
  <img alt="MediaPipe" src="https://img.shields.io/badge/AI%20Wrist%20Tracking-MediaPipe-129B50" />
  <img alt="Vuforia Engine" src="https://img.shields.io/badge/AR-Vuforia-6D4AFF" />
  <img alt="Android" src="https://img.shields.io/badge/Platform-Android-3DDC84?logo=android&amp;logoColor=white" />
</p>

---

## 📖 About AURON AR

**AURON AR** is an Android augmented-reality luxury-watch experience built with **Unity 6.3 LTS, C#, Vuforia Engine, and the Google MediaPipe Unity Plugin**. It lets users explore a watch beyond a traditional product image: rotate the 3D model, inspect separated mechanical components, switch finishes, activate luminous presentation, and try the watch on their wrist with live camera-based hand tracking.

The experience follows this journey:

**Launch → Scan → Explore → Inspect → Customize → Try On Wrist → Capture**

<p align="center">
  <a href="https://mihadkazi1.github.io/auron-watch-website/"><strong>🌐 Visit the AURON Website</strong></a>
  &nbsp; • &nbsp;
  <a href="https://github.com/mihadkazi1/AURON-Watch-AR/releases"><strong>📲 Android APK Releases</strong></a>
</p>

> **APK download:** Open the [AURON AR Releases page](https://github.com/mihadkazi1/AURON-Watch-AR/releases) and download the APK attached to the latest published release. If no APK asset appears there, a release with the installer has not yet been published.

## 🌟 Flagship Feature: Markerless AI Wrist Try-On

Unlike marker-based wrist placement, **AURON's wrist try-on mode uses markerless hand tracking** to position the watch using the phone camera and MediaPipe landmarks. The app can also use a Vuforia image target to begin the watch AR experience; the image target is for launching/anchoring the AR presentation, while wrist tracking in Try-On mode is driven by hand landmarks.

- **21-point hand landmark tracking:** Uses camera frames from the Vuforia pipeline (`AuronVuforiaCameraFrame.cs`) and MediaPipe's `hand_landmarker.bytes` model to estimate hand and wrist landmarks.
- **Wrist pose and forearm alignment:** Combines wrist, knuckle-span, and hand-surface direction estimates to orient the watch on the wrist.
- **Open-hand and closed-fist continuity:** `ScorePlausibleHandOrFist` helps reject implausible detections while maintaining tracking across common hand poses.
- **Live case-size selection:** Compare **36 mm, 40 mm, 42 mm, and 46 mm** case sizes on your wrist.
- **Optional wrist occlusion:** `AuronWristOccluder.shader` can mask rear clasp geometry to help the watch appear more naturally fitted.

## 🖥️ How to Scan from the AURON Website

You can use the AURON website to display the image target that the mobile app recognizes. **Use a second screen or print the target**—one phone cannot normally scan an image that is displayed on that same phone's screen with its own camera.

1. **Open the website** on a computer, tablet, or second phone: [mihadkazi1.github.io/auron-watch-website](https://mihadkazi1.github.io/auron-watch-website/).
2. **Find and display an AURON target image** on the website. The project includes three starter targets: **Classic**, **Chrono**, and **Skeleton**.
3. Make the target large, clear, bright, and fully visible on the screen. Avoid glare, reflections, and cropping.
4. **Launch AURON AR** on your Android phone and allow camera access when prompted.
5. Point the phone camera at the target displayed on the other screen or printed page. Hold the phone steady while Vuforia recognizes the image.
6. Once the watch appears, explore the 3D experience. Open **Try-On** to use the camera-based markerless wrist tracking.

**Troubleshooting scan recognition:** Make sure the displayed image matches a target included in the Vuforia database packaged with the app. Keep the whole image in view, improve the lighting, and move the phone slowly closer or farther away if the target is not detected.

> **Note:** The website provides a way to display target images; successful recognition also depends on those exact target images being configured in the app's Vuforia target database.

## 🖼️ App Screenshots

<p align="center"><em>A look at the AURON AR experience</em></p>

<table>
  <tr>
    <td align="center" width="50%">
      <strong>1. Main Watch View</strong><br />
      <img src="docs/screenshots/01-main-watch-view.jpg" alt="AURON main watch view" width="240" /><br />
      <sub>Explore and rotate the watch in the interactive 3D view.</sub>
    </td>
    <td align="center" width="50%">
      <strong>2. Exploded Mechanical View</strong><br />
      <img src="docs/screenshots/02-exploded-watch.jpg" alt="AURON exploded mechanical watch view" width="240" /><br />
      <sub>Separate the watch into its ordered mechanical layers.</sub>
    </td>
  </tr>
  <tr>
    <td align="center" width="50%">
      <strong>3. Gear Inspection</strong><br />
      <img src="docs/screenshots/03-gear-inspection.jpg" alt="AURON gear inspection" width="240" /><br />
      <sub>Select a component to highlight and inspect its details.</sub>
    </td>
    <td align="center" width="50%">
      <strong>4. Watch Finishes</strong><br />
      <img src="docs/screenshots/04-watch-finishes.jpg" alt="AURON watch finish customization" width="240" /><br />
      <sub>Compare the available case and bracelet finishes.</sub>
    </td>
  </tr>
  <tr>
    <td align="center" width="50%">
      <strong>5. Live AI Wrist Try-On</strong><br />
      <img src="docs/screenshots/05-live-wrist-try-on.jpg" alt="AURON live AI wrist try-on" width="240" /><br />
      <sub>Preview the watch on your wrist using live hand tracking.</sub>
    </td>
    <td align="center" width="50%">
      <strong>6. Super-LumiNova™ Night Glow</strong><br />
      <img src="docs/screenshots/06-lume-mode.jpg" alt="AURON luminous night mode" width="240" /><br />
      <sub>View the luminous dial presentation in low-light mode.</sub>
    </td>
  </tr>
  <tr>
    <td align="center" width="50%">
      <strong>7. Boutique Snapshot</strong><br />
      <img src="docs/screenshots/07-boutique-snapshot.png" alt="AURON boutique snapshot" width="240" /><br />
      <sub>Capture a wrist image with the AURON boutique frame.</sub>
    </td>
    <td align="center" width="50%">
      <strong>8. AURON Website</strong><br />
      <img src="docs/screenshots/08-website-homepage.jpg" alt="AURON website homepage" width="240" /><br />
      <sub>Explore the companion website and available target images.</sub>
    </td>
  </tr>
</table>

---

## ✨ Key Features

### 🖐️ 1. Real-Time AI Wrist Try-On

- Camera-space wrist alignment using MediaPipe hand landmarks.
- Four case-size options: **36 mm, 40 mm, 42 mm, and 46 mm**.
- Smoothed position and rotation updates for natural wrist following.
- Open-hand and closed-fist pose support.
- Optional depth-mask wrist occlusion for the rear clasp.

### ⚙️ 2. Exploded Calibre and Part Inspection — `INSIDE` Mode

- Separates the watch into **13 ordered mechanical layers**, plus the crown and bracelet components.
- Tap-to-inspect callouts highlight selected components in gold and display their technical details.
- Staggered reassembly restores the watch in a controlled order, accompanied by synchronized mechanical sound effects.

### 🕰️ 3. Interactive 3D Exploration and Crown Winding — `WIND`

- Touch/mouse orbit controls and pinch-to-zoom inspection.
- Automatic 360° showcase rotation after a period of inactivity.
- Interactive crown pull/winding presentation with ratchet-style sound effects.
- User interaction returns the model to an easy-to-inspect pose and scale.

### 🎨 4. Precious-Metal Customization and Night Glow — `LUME`

Switch case and bracelet materials in the 3D view and wrist try-on experience:

- **Obsidian Black** (`BLK`)
- **18K Rose Gold** (`ROSE`)
- **Royal Yellow Gold** (`GOLD`)
- **Platinum 950** (`PLAT`)

The **Super-LumiNova™ Night Glow** mode dims the studio lighting and activates luminous dial effects for a low-light presentation.

### 📸 5. Boutique Snapshot — `SNAP` — and Mechanical Audio — `SFX`

- Captures the current AR wrist view and frames it in an obsidian-and-gold **AURON Genève** boutique design.
- Saves snapshots to the Android device's `Pictures/AURON` gallery location using native `MediaStore` integration.
- Generates mechanical escapement clicks, crown ratchet ticks, and reassembly sounds procedurally, without requiring separate recorded effects for these sounds.

---

## 🧭 How to Use the App

1. **Install and launch:** Download the Android APK from the [Releases page](https://github.com/mihadkazi1/AURON-Watch-AR/releases), install it, then open AURON AR.
2. **Allow camera access:** Camera permission is required for image-target recognition and wrist try-on.
3. **Scan a target:** Display an AURON target from the [AURON website](https://mihadkazi1.github.io/auron-watch-website/) on another screen, or use a printed target. Point the phone camera at the target and hold steady until the watch appears.
4. **Explore the 3D watch:** Rotate and zoom to inspect the case, dial, bracelet, and details.
5. **Inspect the mechanism:** Enter `INSIDE` mode to separate the mechanical layers and tap a component to inspect it. Use reassemble mode to return the watch to its complete form.
6. **Customize the look:** Choose Obsidian Black, Rose Gold, Yellow Gold, or Platinum. Enable `LUME` to view the luminous night effect.
7. **Try it on:** Enter wrist Try-On mode, keep your hand and wrist visible to the phone camera, and use the size selector to compare 36–46 mm case sizes.
8. **Capture a photo:** Use `SNAP` to capture a framed AURON image. Check the phone's Gallery/Photos app for the saved image.

## 🧰 Technology Stack

| Technology | Purpose |
|---|---|
| Unity 6.3 LTS | Real-time 3D rendering and Android app development |
| C# | AR controllers, interaction logic, customization, and UI behaviour |
| Vuforia Engine | Camera pipeline and image-target recognition |
| Google MediaPipe Unity Plugin | Hand landmark detection for markerless wrist try-on |
| TextMeshPro | Text rendering for in-app interface and inspection callouts |
| Android `MediaStore` | Saving captured snapshots to the device gallery |
| Custom Unity shader | Optional wrist/clasp depth-mask occlusion |

## 🧩 Core Scripts and Responsibilities

| Script / Component | Responsibility |
|---|---|
| `AuronWatchExperience.cs` | 3D orbit/zoom, showcase rotation, exploded layers, reassembly, and crown interactions |
| `AuronTryOnController.cs` | Watch clone, camera-space placement, wrist pose alignment, and case-size scaling |
| `AuronVuforiaHandLandmarker.cs` | MediaPipe hand landmark inference and hand/fist plausibility checks |
| `AuronVuforiaCameraFrame.cs` | Extracts camera frames from the Vuforia pipeline for MediaPipe processing |
| `AURONUI.cs` | Luxury interface, mode controls, and status UI |
| `AuronLuxuryFeatures.cs` | Material swatches, `LUME`, `SNAP`, `SFX`, `WIND`, and component callouts |
| `AuronWristOccluder.shader` | Optional depth-mask shader for wrist/clasp occlusion |

## 🛠️ Development Setup

### Requirements

- Unity **6.3 LTS** with Android Build Support installed.
- Vuforia Engine configured for the project.
- MediaPipe Unity Plugin and its required runtime/native files.
- The `hand_landmarker.bytes` model in the expected StreamingAssets location.
- The Vuforia target database containing the AURON target images used by the app.
- An Android device with a working camera for real-device testing.

### Open the Unity Project

1. Clone or download this repository. If you clone it using Git, ensure **Git LFS** is installed and enabled so LFS-managed assets can be retrieved.
2. Open Unity Hub and select **Add project from disk**.
3. Choose the project root containing `Assets`, `Packages`, and `ProjectSettings`.
4. Open the project with the compatible Unity editor version.
5. Allow Unity to import assets and resolve packages. Check the Console for missing package, model, or Vuforia configuration errors.
6. Open the main AURON scene and verify the target database, MediaPipe model asset, camera permission, and script references before building.

### Build for Android

1. Open **File → Build Profiles** (or **File → Build Settings**, depending on editor UI).
2. Select **Android** and switch platform if needed.
3. Confirm the correct startup and AR scenes are included in the build.
4. Check Player Settings, app identifier, camera permission, icon, and signing configuration.
5. Build an APK for direct installation, or an AAB if preparing a Google Play submission.
6. Test on a physical Android device; Editor play mode alone does not validate all camera and hand-tracking behaviour.

## 📂 Project Structure

```text
AURON-Watch-AR/
├── Assets/
│   ├── scripts/
│   │   ├── AuronTryOnController.cs
│   │   ├── AuronVuforiaHandLandmarker.cs
│   │   ├── AuronVuforiaCameraFrame.cs
│   │   ├── AuronWatchExperience.cs
│   │   ├── AuronLuxuryFeatures.cs
│   │   ├── AURONUI.cs
│   │   └── AuronWristOccluder.shader
│   ├── StreamingAssets/
│   │   ├── hand_landmarker.bytes
│   │   └── Vuforia/          # Vuforia image-target database files
│   ├── MediaPipeUnity/       # Plugin runtime and native bindings
│   ├── Resources/            # Runtime assets and configuration
│   └── Scenes/               # Main AURON AR Unity scene(s)
├── Packages/
│   └── manifest.json
├── ProjectSettings/
├── docs/
│   ├── assets/
│   │   └── auron-logo-banner.jpg
│   └── screenshots/
│       ├── 01-main-watch-view.jpg
│       ├── 02-exploded-watch.jpg
│       ├── 03-gear-inspection.jpg
│       ├── 04-watch-finishes.jpg
│       ├── 05-live-wrist-try-on.jpg
│       ├── 06-lume-mode.jpg
│       ├── 07-boutique-snapshot.png
│       └── 08-website-homepage.jpg
└── README.md
```

> Folder and file names can differ slightly in your local Unity project. Keep the README paths in sync with the actual files committed to GitHub.

## 🐛 Troubleshooting

| Issue | What to check |
|---|---|
| Image target is not detected | Confirm the image matches a target in the bundled Vuforia database. Improve screen brightness, remove glare, and keep the whole target visible. |
| Camera is black or unavailable | Grant camera permission, verify Vuforia's camera configuration, and test on a supported physical device. |
| Watch is invisible in Try-On mode | Confirm camera-frame delivery to MediaPipe, model scale, camera clipping planes, wrist-pose references, and scene object assignments. |
| Watch jumps or rotates incorrectly | Check the camera-frame orientation and coordinate mapping between the Vuforia image and MediaPipe landmarks. |
| Closed fist causes tracking loss | Check hand/fist plausibility thresholds and ensure the hand remains adequately visible to the camera. |
| Snapshot does not appear in Gallery | Check Android permissions/`MediaStore` insertion, then look in `Pictures/AURON` or refresh the gallery. |
| 3D model or assets are missing after cloning | Check Git LFS installation and confirm that LFS-managed files were downloaded rather than only pointer files. |

## 📄 License

This project is developed to showcase augmented reality, interactive 3D visualization, and virtual watch try-on technology.

## 👨‍💻 Developer

<p align="center">
  <strong>Kazi Saqlain Mihad</strong><br />
  Game &amp; AR Developer · Software Developer · CSE Graduate
</p>

<p align="center">
  <a href="https://github.com/mihadkazi1">GitHub: @mihadkazi1</a>
  &nbsp; • &nbsp;
  <a href="https://mihadkazi.me">Portfolio: mihadkazi.me</a>
</p>

---

<p align="center"><strong>AURON — Time in a New Dimension</strong><br /><em>Explore the mechanics. Choose your finish. Experience AURON on your wrist.</em></p>
