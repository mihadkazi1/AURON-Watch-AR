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

> **Image paths:** The gallery expects the eight images to be uploaded to `docs/screenshots/` with exactly the filenames shown above. The logo banner should be available at `docs/assets/auron-logo-banner.jpg`. If your files have different extensions or spelling, update the matching paths in this README.

---

## ✨ Key Features

### 🕰️ Interactive 3D Watch Exploration
- Rotate and inspect the watch using touch or mouse controls.
- Pinch to zoom on supported touch screens.
- Use the automatic turntable presentation mode.

### ⚙️ Exploded View & Mechanical Inspection
- Animate the watch into its mechanical layers for a closer look.
- Inspect the separated components, including the watch layers, crown, and strap.
- Tap supported components to highlight gears and view technical information.
- Reassemble the watch with a structured animation.

### 🎨 Premium Finish Customization
Choose from four finishes:
- **Obsidian Black**
- **18K Rose Gold**
- **Royal Yellow Gold**
- **Platinum 950**

### 🌙 Super-LumiNova™ Night Glow
Enable luminous presentation to showcase the watch in a low-light style.

### 🖐️ Live Virtual Wrist Try-On
- Uses MediaPipe hand landmarks to estimate hand and wrist position.
- Anchors a watch model to the tracked wrist in camera view.
- Includes wrist-size options: **36 mm, 40 mm, 42 mm, and 46 mm**.
- Includes hand-pose filtering designed to keep tracking stable for an open hand or a fist.

### 📸 Boutique Snapshot
Capture a wrist try-on image with an AURON-inspired dark-and-gold frame and save it to the device gallery, subject to Android permissions and device support.

### 🔊 Mechanical Audio Details
Procedural escapement-style clicks and synchronized sounds complement watch reassembly and interaction.

---

## 📲 How to Use AURON AR

### 1. Install and launch
1. Open the [AURON-Watch-AR Releases page](https://github.com/mihadkazi1/AURON-Watch-AR/releases).
2. Download the APK attached to the latest published release. If no APK asset is listed, a public app build has not yet been attached to a release.
3. Install the APK on a compatible Android phone. If prompted, allow installation from the source you used to download it.
4. Launch AURON AR and allow camera access when Android asks.

### 2. Scan a watch target displayed on the website
**Use two devices** for the easiest scan: your Android phone for the app and a computer or second phone to display the target.

1. Open the [AURON Watch website](https://mihadkazi1.github.io/auron-watch-website/) on the computer or second device.
2. Find and display one of the available AURON target images: **Classic**, **Chrono**, or **Skeleton**. Keep the image fully visible, bright, and steady on the screen.
3. Open AURON AR on your Android phone and enter the image-target scanning experience.
4. Point the phone camera at the target image displayed on the other screen.
5. Hold the camera steady until Vuforia recognizes the target and the 3D watch appears.

**Scanning requirements:** The displayed image must match a target configured in the Vuforia database used by the installed app. Website images are scan targets only if their matching target images have been included in that Vuforia database. Screen glare, reflections, low brightness, cropping, or a small target image can affect recognition. You can also use a printed copy of a registered target.

### 3. Explore the watch
- Rotate the model and zoom in to inspect its design.
- Enter **Inside** or exploded mode to reveal the mechanical layers.
- Tap supported components to inspect their highlights and specifications.
- Use the reassemble control to restore the watch.

### 4. Customize the appearance
- Select a finish: Obsidian Black, 18K Rose Gold, Royal Yellow Gold, or Platinum 950.
- Try Super-LumiNova night-glow mode where available.
- Adjust the wrist-size setting while using Try-On mode.

### 5. Try it on and capture a photo
1. Enter **Try-On** mode.
2. Keep your hand and wrist visible within the camera view and allow tracking to settle.
3. Move naturally while the watch follows the estimated wrist position.
4. Use **SNAP** to capture an image. If prompted, grant the required media or photo permissions.
5. Look for saved images in the device's Pictures/AURON location or Gallery/Photos app; exact behavior may vary by Android version and device.

### Single-phone note
If the phone is displaying AURON AR, it generally cannot simultaneously show the website target in front of its own camera. Use a second screen or a printed registered target for scanning.

---

## 🧰 Technology Stack

| Technology | Purpose |
|---|---|
| Unity 6.3 LTS | Real-time 3D app and Android build |
| C# | Interaction, UI, AR, customization, and capture logic |
| Vuforia Engine | Image-target recognition and AR camera pipeline |
| Google MediaPipe Unity Plugin | Hand landmark estimation for wrist try-on |
| TextMeshPro | In-app text and interface labels |
| Android native MediaStore integration | Saving captured images to the gallery |

## 🏗️ Core Scripts & Architecture

| Script | Responsibility |
|---|---|
| `AuronWatchExperience.cs` | 3D orbit/zoom, auto-turntable, exploded/reassemble sequence, gear inspection, finishes, and lume mode |
| `AuronTryOnController.cs` | Creates and positions the try-on watch, controls wrist attachment, and manages sizing |
| `AuronVuforiaHandLandmarker.cs` | Connects MediaPipe hand landmarks to the AR experience and applies hand-pose checks |
| `AURONUI.cs` | Luxury interface controls and contextual actions |
| `AuronLuxuryFeatures.cs` | Supporting premium audio-visual features and snapshot presentation |

## 🛠️ Developer Setup

The following steps are for developers who want to open and modify the Unity project.

1. Clone or download this repository.
2. Open the project using the intended Unity version (**Unity 6.3 LTS**).
3. Allow Unity to resolve packages and import assets.
4. Verify that the Vuforia license/configuration and image-target database used by the app are set up for your environment.
5. Verify that the MediaPipe model file `hand_landmarker.bytes` exists at the expected StreamingAssets path and is included in the project.
6. Open the intended startup scene and check that required scenes are included in Build Settings.
7. Connect an Android device and test the camera, image-target tracking, wrist try-on, and photo saving on-device.

### Build an Android APK
1. In Unity, open **File → Build Profiles** (or **File → Build Settings**, depending on the Unity UI).
2. Select Android and switch platform if needed.
3. Confirm the app's scenes, package identifier, permissions, and Vuforia configuration.
4. For a release build, disable **Development Build** and configure your own signing keystore under the Android Publishing Settings.
5. Build the APK and test it on a physical Android device before distributing it.

**Signing-key warning:** Keep the keystore and its passwords private and backed up securely. Do not commit them to GitHub.

---

## 🧩 Troubleshooting

- **The watch does not appear after scanning:** Check that the image is registered in the app's Vuforia target database, fully visible, adequately lit, and not cropped.
- **The camera is black or unavailable:** Check Android camera permission, camera configuration, and device compatibility.
- **The wrist try-on is unstable:** Improve lighting, keep the wrist in view, avoid strong motion blur, and confirm that the MediaPipe model asset is present.
- **The screenshot is not saved:** Check the app's permissions and the Android device's Gallery/Photos app. Behavior can vary across Android versions.
- **A screenshot does not render in this README:** Confirm the image is uploaded to `docs/screenshots/` and the filename, capitalization, and extension match the path written in this file.

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
