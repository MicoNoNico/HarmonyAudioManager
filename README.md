<p align="center">
  <img src="https://github.com/user-attachments/assets/52153a1d-3363-45b6-a054-58d557aec622" alt="Harmony Banner" />
</p>

# Harmony - A Simple Audio Manager

Harmony is a simple and efficient audio manager for Unity that enables easy playback of music and sound effects, with features like volume control, fading, and saving values.

### Quick Jump:

- [Getting Started](#getting-started)
- [Documentation](#documentation)
- [Upcoming Features](#upcoming-features)

## Features

- **Easy Playback**: Play and control background music, sound effects, and voice clips.


- **Volume Control**: Adjust master, music, sound, and voice volumes.


- **Fading**: Fade in/out music and voice tracks smoothly.


- **Audio Library**: Manage audio clips efficiently using `AudioAsset` and `AudioLibrary`.


- **Enum References**: Automatic conversion of audio assets to enum references for type-safe access.


- **Randomization**: Play random clips from a set of audio clips.


- **Voice Extension**: Optional support for voice clips with pooling and volume control.


- **Pooling**: Efficiently manage multiple audio sources for simultaneous playback.


- **Settings Persistence**: Save and load volume settings using `PlayerPrefs`.

## Installation

1. Download the latest `.unitypackage` file from the [Releases](https://github.com/MicoNoNico/HarmonyAudioManager/releases) section.
2. In Unity, go to **Assets > Import Package > Custom Package...**.
3. Select the downloaded `.unitypackage` file and click **"Open"**.
4. Import all files.


## Getting Started

### Table of Contents
- [1. Creating the Audio Manager](#1-creating-the-audio-manager)
- [2. Creating the Audio Library](#2-creating-the-audio-library)
- [3. Creating Audio Assets](#3-creating-audio-assets)
- [4. Assigning Audio Assets to the Library](#4-assigning-audio-assets-to-the-library)
- [5. Regenerating Library](#5-regenerating-library)
- [6. Using the Audio Manager](#6-using-the-audio-manager)

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


### [3] Creating Audio Assets

1. **Create an Audio Asset**:
   - Right-click in the Project window and navigate to **Create > HarmonyAudio > Audio Asset**.
   - Name the asset according to its purpose (e.g., `Footsteps`, `MainTheme`, `ExplosionSound`).

2. **Configure the Audio Asset**:
   - **Single Clip**:
      - Set **Allow Multiple Clips** to **False**.
      - Assign an `AudioClip` to the **Single Clip** field.
   - **Multiple Clips**:
      - Set **Allow Multiple Clips** to **True**.
      - Add `AudioClip`s to the **Multiple Clips** list.

### [4] Assigning Audio Assets to the Library

1. **Open the Audio Library**:
   - Select the `AudioLibrary` asset in your Project window.

2. **Add Audio Assets to the Appropriate Lists**:
   - **Music Assets**:
      - Click the **+** button in the **Music Assets** list.
      - Assign your music `AudioAsset`s.
   - **Sound Assets**:
      - Click the **+** button in the **Sound Assets** list.
      - Assign your sound effect `AudioAsset`s.
   - **Voice Assets** (if Voice Extension is enabled):
      - Click the **+** button in the **Voice Assets** list.
      - Assign your voice `AudioAsset`s.

### [5] Regenerating Library

**Important: You must regenerate the enums at least once for the `AudioManager` to recognize your audio clips.**

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

### [6] Using the Audio Manager

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

## Documentation

For more detailed information on the `AudioManager` and its capabilities, refer to the sections below.

### AudioManager API Reference

#### Music Methods

- `PlayMusic(MusicClips musicClip, bool loop = true)`
    - Plays a music clip.
- `PlayRandomMusic(MusicClips musicClip, bool loop = true)`
    - Plays a random music clip from an `AudioAsset` that allows multiple clips.
- `StopMusic()`
    - Stops the currently playing music.
- `PauseMusic()`
    - Pauses the currently playing music.
- `ResumeMusic()`
    - Resumes paused music.
- `SetMusicVolume(float volume)`
    - Sets the music volume.
- `GetMusicVolume()`
    - Gets the current music volume.
- `FadeMusicVolume(float targetVolume, float duration)`
    - Fades the music volume to a target volume over a duration.

#### Sound Methods

- `PlaySound(SoundClips soundClip)`
    - Plays a sound effect.
- `PlayRandomSound(SoundClips soundClip)`
    - Plays a random sound effect from an `AudioAsset` that allows multiple clips.
- `SetSoundVolume(float volume)`
    - Sets the sound effects volume.
- `GetSoundVolume()`
    - Gets the current sound effects volume.

#### Voice Methods (if enabled)

- `PlayVoice(VoiceClips voiceClip, bool loop = false)`
    - Plays a voice clip.
- `PlayRandomVoice(VoiceClips voiceClip, bool loop = false)`
    - Plays a random voice clip from an `AudioAsset` that allows multiple clips.
- `StopVoice()`
    - Stops all currently playing voice clips.
- `PauseVoice()`
    - Pauses all currently playing voice clips.
- `ResumeVoice()`
    - Resumes paused voice clips.
- `SetVoiceVolume(float volume)`
    - Sets the voice volume.
- `GetVoiceVolume()`
    - Gets the current voice volume.
- `FadeVoiceVolume(float targetVolume, float duration)`
    - Fades the voice volume to a target volume over a duration.

#### General Methods

- `SetMasterVolume(float volume)`
    - Sets the master volume.
- `GetMasterVolume()`
    - Gets the current master volume.
- `SaveVolumeSettings()`
    - Saves the current volume settings.
- `LoadVolumeSettings()`
    - Loads the saved volume settings.

### Creating Custom Audio Assets

You can create custom `AudioAsset`s to organize your audio clips efficiently.

1. **Create an Audio Asset**:
    - Right-click in the Project window and select **Create > HarmonyAudio > Audio Asset**.
    - Name the asset appropriately.

2. **Configure the Audio Asset**:
    - **Allow Multiple Clips**:
        - Set to **True** if you want to assign multiple clips (for randomization).
        - Set to **False** if you only need a single clip.
    - **Assign Clips**:
        - If **Allow Multiple Clips** is **True**, add clips to the **Multiple Clips** list.
        - If **Allow Multiple Clips** is **False**, assign a clip to the **Single Clip** field.

3. **Use Descriptive Names**:
    - Name your `AudioAsset` clearly, as it will be used in the generated enums.

### Advanced Features

#### Fading Audio

You can fade music and voice volumes over time:

```csharp
// Fade music volume to 0 over 2 seconds
AudioManager.FadeMusicVolume(0f, 2f);

// Fade voice volume to 0.5 over 3 seconds
AudioManager.FadeVoiceVolume(0.5f, 3f); // Only if Voice Extension is enabled
```

#### Pooling Audio Sources

The `AudioManager` uses pooling for sound effects and voice clips to allow multiple audio clips to play simultaneously.

- **Adjust Pool Sizes**:
    - **Sound Source Pool Size**: Adjust the `Sound Source Pool Size` in the `AudioManager` component.
    - **Voice Source Pool Size**: Adjust the `Voice Source Pool Size` when Voice Extension is enabled.

#### Volume Controls in Inspector

- **Volume Sliders**:
    - Adjust the **Master Volume**, **Music Volume**, **Sounds Volume**, and **Voice Volume** directly in the `AudioManager` Inspector.
- **Voice Extension Controls**:
    - Enable or disable the Voice Extension from the `AudioManager` Inspector under **Extensions**.


## Upcoming Features

*Note: This is a list of upcoming features that I plan to add in due time. This list is also subject to change.*

- Custom path for Enums script generation.
