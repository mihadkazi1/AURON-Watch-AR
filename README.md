<p align="center">
  <img src="docs/assets/auron-logo-banner.jpg" alt="AURON — Time in a New Dimension" width="420" />
</p>

<h1 align="center">AURON AR Watch ⌚🥽</h1>

<p align="center">
  <strong>Interactive Luxury Watch Experience &amp; Virtual Wrist Try-On</strong><br />
  Explore the mechanics. Choose your finish. Experience AURON on your wrist.
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
  <img alt="Vuforia Engine" src="https://img.shields.io/badge/AR-Vuforia-6D4AFF" />
  <img alt="MediaPipe" src="https://img.shields.io/badge/Hand%20Tracking-MediaPipe-129B50" />
  <img alt="Android" src="https://img.shields.io/badge/Platform-Android-3DDC84?logo=android&amp;logoColor=white" />
</p>

---

## 📖 About AURON AR

**AURON AR** is an Android augmented-reality watch experience designed to let people explore a luxury timepiece beyond a traditional product photo. Built with **Unity, C#, Vuforia Engine, and the Google MediaPipe Unity Plugin**, the app combines image-target AR, interactive 3D inspection, premium finish customization, luminous presentation, and camera-based wrist try-on.

The experience follows this flow:

**Launch → Scan → Explore → Inspect → Customize → Try On → Capture**

<p align="center">
  <a href="https://mihadkazi1.github.io/auron-watch-website/"><strong>🌐 Visit the AURON Website</strong></a>
  &nbsp; • &nbsp;
  <a href="https://github.com/mihadkazi1/AURON-Watch-AR/releases"><strong>📲 Android APK Releases</strong></a>
</p>

---

## 🖼️ App Screenshots

<p align="center"><em>Selected views from the AURON AR experience</em></p>

<table>
  <tr>
    <td align="center" width="50%">
      <strong>1. Main Watch View</strong><br />
      <img src="docs/screenshots/01-main-watch-view.jpg" alt="AURON main watch view" width="220" /><br />
      <sub>Explore the watch in the interactive 3D view.</sub>
    </td>
    <td align="center" width="50%">
      <strong>2. Exploded Mechanical View</strong><br />
      <img src="docs/screenshots/02-exploded-watch.jpg" alt="AURON exploded mechanical watch view" width="220" /><br />
      <sub>Separate the watch into its mechanical layers.</sub>
    </td>
  </tr>
  <tr>
    <td align="center" width="50%">
      <strong>3. Gear Inspection</strong><br />
      <img src="docs/screenshots/03-gear-inspection.jpg" alt="AURON gear inspection" width="220" /><br />
      <sub>Tap a component to highlight and inspect it.</sub>
    </td>
    <td align="center" width="50%">
      <strong>4. Watch Finishes</strong><br />
      <img src="docs/screenshots/04-watch-finishes.jpg" alt="AURON watch finish customization" width="220" /><br />
      <sub>Switch between premium case and bracelet finishes.</sub>
    </td>
  </tr>
  <tr>
    <td align="center" width="50%">
      <strong>5. Live Wrist Try-On</strong><br />
      <img src="docs/screenshots/05-live-wrist-try-on.jpg" alt="AURON live AR wrist try-on" width="220" /><br />
      <sub>Preview the watch on your wrist through the camera.</sub>
    </td>
    <td align="center" width="50%">
      <strong>6. Super-LumiNova Night Glow</strong><br />
      <img src="docs/screenshots/06-lume-mode.jpg" alt="AURON luminous night mode" width="220" /><br />
      <sub>See the luminous presentation in night-glow mode.</sub>
    </td>
  </tr>
  <tr>
    <td align="center" width="50%">
      <strong>7. Boutique Snapshot</strong><br />
      <img src="docs/screenshots/07-boutique-snapshot.png" alt="AURON boutique snapshot" width="220" /><br />
      <sub>Capture and save an AURON-framed image.</sub>
    </td>
    <td align="center" width="50%">
      <strong>8. AURON Website</strong><br />
      <img src="docs/screenshots/08-website-homepage.jpg" alt="AURON website homepage" width="220" /><br />
      <sub>Browse the companion watch website and scan targets.</sub>
    </td>
  </tr>
</table>

---

## ✨ Key Features

### 🕰️ Interactive 3D Watch Exploration
- Rotate and inspect the watch using touch or mouse controls.
- Pinch to zoom on supported touch screens.
- Automatic 360° turntable showcase mode after idle timeout (snaps back to normal size and pose on touch).
- Pull out the crown (`WIND`) in 3D View and swipe vertically to manually wind the mainspring and accelerate the internal gear train.

### ⚙️ Exploded View & Mechanical Inspection
- Animate the watch into its 13 mechanical layers (`Cube.003` to `Empty.012`), crown (`Empty.013`), and strap (`Empty.014`).
- Tap individual floating components in Inside mode to highlight gears in gold and view horological specifications.
- Reassemble the watch with a structured layer-by-layer animation and synchronized mechanical locking audio.

### 🎨 Premium Finish Customization
Choose from four finishes live in 3D View or Wrist Try-On:
- **Obsidian Black** (`BLK`)
- **18K Rose Gold** (`ROSE`)
- **Royal Yellow Gold** (`GOLD`)
- **Platinum 950** (`PLAT`)

### 🌙 Super-LumiNova™ Night Glow
Toggle `LUME` to dim ambient studio lighting and illuminate the geometric skeleton dial and movement with luminescent night-glow emission.

### 🖐️ Live Virtual Wrist Try-On
- Uses MediaPipe 21-point hand landmarks to estimate 3D wrist position and orientation in real time.
- Includes four live case-diameter options: **36 mm, 40 mm, 42 mm, and 46 mm**.
- Uses palm-and-knuckle geometry validation (`ScorePlausibleHandOrFist`) to reject background false positives while keeping tracking locked whether your hand is open or closed into a fist.

### 📸 Boutique Snapshot
Tap `SNAP` to capture a clean AR wrist photo framed in an obsidian-and-gold **AURON Genève** border and save it directly to your device's Gallery (`Pictures/AURON`).

### 🔊 Mechanical Audio Details
Toggle `SFX` for procedural 28,800 vph Swiss lever escapement clicks, crown-winding ratchet audio, and multi-layer mechanical reassembly sounds.

---

## 📲 How to Use AURON AR

### 1. Install and launch
1. Open the [AURON-Watch-AR Releases page](https://github.com/mihadkazi1/AURON-Watch-AR/releases).
2. Download the APK attached to the latest published release.
3. Install the APK on a compatible Android phone. If prompted, allow installation from your browser or file manager.
4. Launch AURON AR and grant camera access when prompted.

### 2. Scan a watch target displayed on the website
**Use two devices** for the easiest scan: your Android phone for the app and a computer or second phone to display the target.

1. Open the [AURON Watch website](https://mihadkazi1.github.io/auron-watch-website/) on your computer or second device.
2. Display one of the available AURON target images (**Classic**, **Chrono**, or **Skeleton**). Keep the image bright, flat, and fully visible on screen.
3. Open AURON AR on your Android phone and tap **Explore AR Watch**.
4. Point the phone camera at the target image until Vuforia recognizes the target and locks the 3D watch into view.

### 3. Explore the watch
- Drag to rotate the model, pinch to zoom, or tap `WIND` to pull out the crown and wind the movement.
- Tap **INSIDE** to separate the watch into its mechanical layers and tap any gear to inspect its specifications.
- Tap **REASSEMBLE** to lock the movement layers back into the case.

### 4. Customize the appearance
- Tap `BLK`, `ROSE`, `GOLD`, or `PLAT` to switch case and bracelet finishes.
- Tap `LUME` to toggle Super-LumiNova™ night-glow optics.
- Tap `SFX` to toggle the mechanical escapement sound on or off.

### 5. Try it on and capture a photo
1. Tap **TRY ON** and hold your wrist in front of the camera.
2. Compare **36MM**, **40MM**, **42MM**, and **46MM** case sizes directly on your wrist.
3. Tap **SNAP** to capture and save a boutique-framed photo directly to your phone's Gallery (`Pictures/AURON`).

---

## 🧰 Technology Stack

| Technology | Purpose |
|---|---|
| Unity 6.3 LTS | Real-time 3D rendering, physics, and Android build pipeline |
| C# | Interaction, procedural UI, AR pose math, material customization, and procedural audio |
| Vuforia Engine | Image-target recognition and live AR camera frame provider |
| Google MediaPipe Unity Plugin | 21-point 3D hand and wrist landmark estimation |
| TextMeshPro | High-resolution in-app typography and horology callouts |
| Android MediaStore API | Native gallery image insertion on Android 10+ |

## 🏗️ Core Scripts & Architecture

| Script / Asset | Responsibility |
|---|---|
| `AuronWatchExperience.cs` | 3D orbit/zoom controls, exploded/reassemble layer animation, and crown pull mechanics |
| `AuronLuxuryFeatures.cs` | Finish customizer (`BLK`/`ROSE`/`GOLD`/`PLAT`), `LUME` night glow, gear tap inspection, crown winding (`WIND`), auto-turntable, procedural escapement audio (`SFX`), and boutique snapshot (`SNAP`) |
| `AuronTryOnController.cs` | Clones and anchors the watch to the tracked wrist in camera space, manages 36–46 mm case sizing, and handles pose smoothing |
| `AuronVuforiaHandLandmarker.cs` | Runs MediaPipe hand landmark detection on Vuforia camera frames and validates open-hand/fist geometry |
| `AuronVuforiaCameraFrame.cs` | Bridges live Vuforia camera textures to MediaPipe |
| `AURONUI.cs` | Core luxury interface layout, mode switching (`3D VIEW`, `INSIDE`, `TRY ON`), and status header |

---

## 🛠️ Developer Setup

1. Clone or download this repository.
2. Open the project in **Unity 6.3 LTS**.
3. Allow Unity Package Manager to resolve dependencies.
4. Verify that your Vuforia license key and target database are configured in `VuforiaConfiguration`.
5. Confirm that `hand_landmarker.bytes` is present in `Assets/StreamingAssets/`.
6. Open the main scene, switch platform to **Android** under **File → Build Profiles**, and build/run on an Android device.

---

## 🧩 Troubleshooting

- **The watch does not appear after scanning:** Ensure the target image is bright, uncropped, and free of screen glare.
- **The camera is black or unavailable:** Verify that Android camera permissions are granted in your device settings.
- **The wrist try-on is unstable:** Ensure your wrist is well-lit and clearly visible inside the camera frame.
- **The screenshot is not saved:** Check your device's **Gallery / Photos → Albums → AURON** (or `Pictures/AURON` in file storage).

---

## 🔗 Project Links

- **Live Website:** https://mihadkazi1.github.io/auron-watch-website/
- **GitHub Source Repository:** https://github.com/mihadkazi1/AURON-Watch-AR
- **Android Release Page:** https://github.com/mihadkazi1/AURON-Watch-AR/releases
- **Developer Portfolio:** https://mihadkazi.me
- **Developer GitHub:** https://github.com/mihadkazi1

---

## 👨‍💻 Developer

<p align="center">
  <strong>Kazi Saqlain Mihad</strong><br />
  Game &amp; AR Developer | Software Developer | CSE Graduate
  <br /><br />
  <a href="https://github.com/mihadkazi1">GitHub: @mihadkazi1</a>
  &nbsp; • &nbsp;
  <a href="https://mihadkazi.me">Portfolio: mihadkazi.me</a>
</p>

---

<p align="center"><strong>AURON — Time in a New Dimension.</strong></p>
