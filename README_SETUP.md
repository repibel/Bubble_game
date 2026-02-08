# Bubble Touch 60 - Setup Guide

## 1. Scene Setup
1. **GameManager Object**
   - Create an Empty GameObject named `GameManager`.
   - Attach `GameManager.cs`.
   - Attach `BubbleManager.cs`.
   - Attach `UIManager.cs`.
   - **[New] Attach `SoundManager.cs`**.

2. **Sound Manager Configuration**
   - On the `SoundManager` component:
     - Assign `Bgm Source`: Drag an AudioSource (create one on GameManager if needed).
     - Assign `Sfx Source`: Drag another AudioSource.
     - Assign `Game Bgm`: Your background music clip.
     - Assign `Bubble Pop Sfx`: Sound for correct touch.
     - Assign `Wrong Sfx`: Sound for wrong touch.

3. **UI Setup**
   - Create a Canvas (Scale with Screen Size).
   - Add TextMeshProUGUI elements for:
     - Time
     - Score (Target Number)
     - Combo
   - Add a Panel for Game Over with:
     - Final Score Text
     - Max Combo Text
     - Retry Button
   - Assign these references to the `UIManager` component.

4. **Bubble Prefab**
   - Create a 2D Sprite object.
   - Add `CircleCollider2D`.
   - Add `Bubble.cs` script.
   - Add a child TextMeshPro object for the number.
   - Create a Prefab from this object.
   - **Important:** Assign this Prefab to the `BubbleManager`'s `Bubble Prefab` field.

5. **BubbleManager Configuration**
   - Assign `Bubble Container` (create an empty object to hold bubbles).
   - Adjust `Grid Size` and `Grid Origin` to fit your camera view. 
     - *Tip: Set Grid Origin to bottom-left of the playable area.*

## 2. Dependencies
- Ensure **TextMeshPro** is installed via Package Manager.
- Ensure **Universal RP** is set up if using URP assets.

## 3. Play
- Press Play. The `GameManager` will initialize the game loop.