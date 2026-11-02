# URP (Universal Render Pipeline)

The `ai-urp` branch switches the game from the built-in render pipeline to URP 17.6.

## How the conversion works

`Assets/Scripts/Editor/UrpConversion.cs` runs Unity's **Render Pipeline Converter** in batch mode
before every build. It does nothing once a URP asset is assigned. The converter:
1. creates URP pipeline and renderer assets from the quality levels and assigns them (*RenderSettings*),
2. upgrades Standard, Legacy and particle materials to URP shaders (*Material*, *ReadonlyMaterial*,
   *AnimationClip*).

CI does this on a fresh checkout, so the repository itself is not converted yet. To make it
permanent:
1. Open the project in the editor.
2. Run **The Nightmare → Convert project to URP**.
3. Commit the generated URP assets and the upgraded materials.

## What still needs manual work

These use custom shaders, which the converter skips. They render pink until rewritten (for example
in Shader Graph):

| Effect | Where |
|---|---|
| Enemy dissolve on death | `DissolveEnemyBody` materials |
| Particle dissolve | `Particle Dissolve Shader by Moonflower Carnivore` |
| Item outline | `QuickOutline` (built-in multi-pass outline) |
| Magic effects | `KY_effects` |
| Starfield skybox | skybox material |

The game still has two other gaps under URP:
- **Post-processing:** Post Processing Stack v1 does not run under URP. Recreate its look with a
  global **Volume** (bloom, vignette, color grading).
- **Lighting:** rebake it, since URP lights differently than the built-in pipeline.
