# 3D Stickman

**CSS 451 Final Project**

3D Stickman is a simple animation program that allows users to create and animate a stickman figure (and other simple shapes) in a 3D space, based on the classic [Pivot](https://pivotanimator.net/) animator. This project was developed as a final project for the CSS 451 course at the University of Washington.

## Features

- Spawn objects including stickman figures, simple shapes, light sources, and particles.
- Manage a timeline for simple keyframe-based animation.
- Manipulate objects using gizmos or via direct keyframe edit.
- Undo/redo system for correcting simple mistakes.
- Save, load, and share animations as JSON files.
- Render animations to MP4 video files.

## Key Bindings
| Key(s)                 | Action                                      |
|------------------------|---------------------------------------------|
| **Alt + Right Click**  | Drag camera                                 |
| **Alt + Left Click**   | Rotate camera around look-at point          |
| **Alt + Scroll**       | Zoom camera                                 |
| **Shift + Left Click** | Select object                               |
| **Delete / Backspace** | Delete selected object                      |
| **Backspace**          | Delete selected object                      |
| **Ctrl + Z**           | Undo                                        |
| **Ctrl + Y**           | Redo                                        |
| **Ctrl + S**           | Save animation                              |
| **Ctrl + O**           | Load animation                              |
| **Ctrl + R**           | Render animation to video                   |
| **Spacebar**           | Play/Pause animation                        |
| **Left Arrow**         | Move one frame backward                     |
| **Right Arrow**        | Move one frame forward                      |
| **Left Sq. Bracket**   | Jump to start of animation                  |
| **Right Sq. Bracket**  | Jump to end of animation                    |
| **G**                  | Position gizmo tool                         |
| **R**                  | Rotation gizmo tool                         |
| **S**                  | Scale gizmo tool                            |

## Build
3D Stickman was created in Unity **6000.2.7f1** in C#.

### Dependencies
#### Included Packages
- [FFMpegOut](https://github.com/keijiro/FFmpegOut) - Video rendering using an FFMpeg backend.
- [Newtonsoft.Json](https://www.newtonsoft.com/json) - JSON serialization and deserialization.
- [StandaloneFileBrowser](https://github.com/Netherlands3D/FileBrowser) - Cross-platform file browser dialog.
- [UniTask](https://github.com/Cysharp/UniTask) - Better asynchronous programming for Unity.

#### NOT INCLUDED
- [FFMpegOutBinaries](https://github.com/keijiro/FFmpegOutBinaries) - Precompiled FFMpeg binaries for FFMpegOut.

Due to license restrictions, FFMpeg binaries are not included in this repository. To build 3D Stickman with animation rendering support, please download the latest [FFMpegOutBinaries release](https://github.com/keijiro/FFmpegOutBinaries/releases/latest) and import the Unity package before building.

3D Stickman will still function without FFMpeg binaries, but animation rendering will be disabled.

### Contributing

3D Stickman will not be actively maintained since it was made as a final project. However, contributions or forks of the project are welcome. 3D Stickman is open source for a reason!