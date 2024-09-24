# Harmony - A Simple Audio Manager

Harmony is a simple and efficient audio manager for Unity that enables easy playback of music and sound effects, with features like volume control, fading, and saving values.

## Features

- Play and control background music.
- Play multiple sound effects simultaneously.
- Fade in/out music tracks.
- Library for easy management of audio clips.
- Automatic conversion of clips to enum references.
- Save and load volume settings.

## Installation

### Via Unity Package Manager (Git URL)

1. Open your Unity project.
2. Navigate to **Window > Package Manager**.
3. Click the **"+"** button and select **"Add package from git URL..."**.
4. Enter the following URL:
```https://github.com/MicoNoNico/HarmonyAudioManager.git#1.0.1```
5. Click **"Add"**.

### Via Unity Package (`.unitypackage`)

1. Download the latest `.unitypackage` file from the [Releases](https://github.com/MicoNoNico/HarmonyAudioManager/releases) section.
2. In Unity, go to **Assets > Import Package > Custom Package...**.
3. Select the downloaded `.unitypackage` file and click **"Open"**.
4. Import all files.

---

## Getting Started

### Table of Contents
- [1. Creating the Audio Manager](#1-creating-the-audio-manager)
- [2. Creating the Audio Library](#2-creating-the-audio-library)
- [3. Assigning Audio Clips](#3-assigning-audio-clips)
- [4. Regenerating Enums](#4-regenerating-enums)
- [5. Using the Audio Manager](#5-using-the-audio-manager)

### [1] Creating the Audio Manager

1. **Create an Empty GameObject**:
    - In your Unity scene, create an empty GameObject and name it `AudioManager`.


2. **Attach the AudioManager Script**:
    - Add the `AudioManager` script to the `AudioManager` GameObject.


3. **Add Audio Sources**:
    - **Music Source**:
        - Add an `AudioSource` component to the `AudioManager` GameObject.
        - Uncheck **Play On Awake**.
        - Set **Output** to your preferred Audio Mixer Group (optional).
    - **Sound Sources**:
        - The `AudioManager` will automatically create a pool of `AudioSource` components for sound effects. You can edit the pool size in the `AudioManager` script.


4. **Assign the Music Source**:
    - In the `AudioManager` component, assign the `AudioSource` you just added to the **Music Source** field.


### [2] Creating the Audio Library

1. **Create an Audio Library Asset**:
   - Right-click in the Project window and navigate to **Create > HarmonyAudio > New Audio Library**.
   - Name the asset `AudioLibrary`.


2. **Assign the Audio Library to the Audio Manager**:
   - Select the `AudioManager` GameObject.
   - In the `AudioManager` component, assign the `AudioLibrary` asset to the **Audio Library** field.


### [3] Assigning Audio Clips

1. **Open the Audio Library**:
   - Select the `AudioLibrary` asset in your Project window.


2. **Add Music Clips**:
   - In the **Music Clips** list, click the **+** button to add a new entry.
   - Assign an `AudioClip` to the **Clip** field.
   - Repeat for all your music clips.


3. **Add Sound Effect Clips**:
   - In the **Sound Clips** list, click the **+** button to add a new entry.
   - Assign an `AudioClip` to the **Clip** field.
   - Repeat for all your sound effect clips.

***Note***: *Ensure that your `AudioClip` asset names don't have any space, special characters or leading digits as they will be used by the system to create enums.*

### [4] Regenerating Enums

**Important**: You must regenerate the enums at least once for the `AudioManager` to recognize your audio clips.

1. **Regenerate Enums**:
   - Select your `AudioLibrary`.
   - In the  Inspector, click the **Regenerate Library** button.
   - This action will:
      - Generate or update the `MusicClips` and `SoundClips` enums based on your `AudioClip` names.
      - Create or update the enum files in `HarmonyAudio/Scripts/Enums/`.
   - Wait for Unity to recompile the scripts.


2. **[Optional] Verify the Enums**:
   - Navigate to `Assets/HarmonyAudio/Scripts/Enums/`.
   - Open `MusicClips.cs` and `SoundClips.cs` to verify that the enums have been generated correctly.

### [5] Using the Audio Manager

With the setup complete, you can now use the `AudioManager` to play music and sound effects in your scripts.

#### Sample: Playing Music

```csharp
using HarmonyAudio.Scripts;
using HarmonyAudio.Scripts.Enums;

public class PlayMusicOnStart : MonoBehaviour
{
    private void Start()
    {
        // Play music using the MusicClips enum
        AudioManager.PlayMusic(MusicClips.MainTheme);
    }

    private void StopMusic()
    {
        AudioManager.StopMusic();
    }
}
```