# RopuchAdventure

RopuchAdventure is a 2D Unity platformer made for a videogame programming exam project. The player controls Ropuch, moves through a single stage, collects coins, defeats enemies, and reaches the end scene with a final score summary.

The project is inspired by the YouTube series "Kuce z bronksu". That influence is visible in the character choices, the scene presentation, and the audio direction.

## Game Flow

The project uses three scenes in this order:

1. `StartScene` - main menu with the start button and quit option.
2. `GameScene` - the playable level with enemies, pickups, HUD, and combat.
3. `EndScene` - end screen with the final score, win/lose status, and exit button.

## Main Characters

- Ropuch - the player character. He can move, jump, attack, shoot bullets, collect coins, and lose lives.
- Dziunia - an enemy character with automatic shooting behavior and a limited number of lives.
- Gryfica Gnilda - an enemy that can take damage from bullets and disappear after enough hits.

## Gameplay Systems

- Movement and jumping are handled by the player controller.
- Combat uses bullets and hit effects for both the player and enemies.
- Coins increase the score state and are counted in the HUD.
- Enemy kills are tracked separately and also contribute to the score state.
- The game stores the current score and final statistics across scenes so the end screen can display them.
- The end screen shows the final score and a win/lose message based on the remaining lives.
- The start and exit buttons reset the score state before loading the next scene.

## UI And Screens

- Start screen: contains the start button and a quit button.
- In-game HUD: shows collected coins and defeated enemies.
- Enemy life display: shows remaining lives for supported enemies.
- End screen: shows the final score summary and an exit button back to the menu.
- Audio mute support is managed through a global mute flag used by the project scripts.

## Controls

- `A` / `D` or horizontal input - move left and right.
- `Space` - jump.
- `Return` - attack / shoot, depending on the active character logic.

## Character Images

| Character | Preview |
|---|---|
| Ropuch | ![Ropuch](Assets/Resources/Images/Ropuch-spriteSheets/Ropuch-idle.png) |
| Dziunia | ![Dziunia](Assets/Resources/Images/dziunia.png) |
| Gryfica Gnilda | ![Gryfica Gnilda](Assets/Resources/Images/GryficaGnilda/GryficaGnilda-yeesh.png) |

Character spritesheets for the main cast were AI-generated and then adapted for this project.

## Project Notes

- Built in Unity.
- Uses custom scripts for player movement, enemy behavior, score tracking, buttons, bullets, and hit effects.
- The scene order and score state are designed so the game can restart cleanly from the menu and show a final summary at the end.

## Imported Assets, Sources, and Licenses

### Art, VFX, and Audio Packs

| Imported content | Repository location | Source | License |
|---|---|---|---|
| Main background music | Project audio setup | [Fairy Lands - Fantasy Music in a Magical Forest Fantasy Ambience](https://audio.com/logyne-emad/audio/fairy-lands-fantasy-music-in-a-magical-forest-fantasy-ambience) | Used under the source platform terms |
| Enchanted World Music Pack | `Assets/Enchanted World Music Pack` | Third-party imported music pack (original download source should be the asset page used during import) | Original pack license terms (not bundled as a separate license file in this repository) |
| Hits Effects FREE (Matthew Guz) | `Assets/Matthew Guz/Hits Effects FREE` | Matthew Guz (contact listed in included readme: `mattvg923@gmail.com`) | Follow the original distribution terms for this pack (see `Assets/Matthew Guz/Hits Effects FREE/Documentation/Readme IMPORTANT.txt`) |
| 2D Pixel Art Platformer Biome - American Forest | `Assets/Resources/Images/2D Pixel Art Platformer Biome - American Forest` | Third-party imported biome pack | See bundled `ReadMe.pdf` in this folder for source/license terms |
| 2D Pixel Art Platformer Biome - Plains | `Assets/Resources/Images/2D Pixel Art Platformer Biome - Plains` | Third-party imported biome pack | See bundled `ReadMe.pdf` in this folder for source/license terms |
| PlatformerTileset | `Assets/PlatformerTileset` | Third-party imported tileset pack | Use the original pack license terms from the source where it was downloaded |

### Imported .NET / NuGet Packages

Packages listed in `Assets/packages.config` are imported from NuGet and include license metadata in their `.nuspec` files:

| Package | Version | Source | License |
|---|---|---|---|
| Microsoft.Bcl.AsyncInterfaces | 10.0.8 | https://dot.net/ | MIT |
| Microsoft.Extensions.DependencyInjection | 10.0.8 | https://dot.net/ | MIT |
| Microsoft.Extensions.DependencyInjection.Abstractions | 10.0.8 | https://dot.net/ | MIT |
| System.IO.Pipelines | 10.0.8 | https://dot.net/ | MIT |
| System.Runtime.CompilerServices.Unsafe | 6.1.2 | https://github.com/dotnet/maintenance-packages | MIT |
| System.Text.Encodings.Web | 10.0.8 | https://dot.net/ | MIT |
| System.Text.Json | 10.0.8 | https://dot.net/ | MIT |

### Unity Packages

Unity packages are listed in `Packages/manifest.json` and are imported through the Unity Package Manager (Unity Registry). Their usage is governed by Unity package and editor licensing terms.

## Credits

- Game concept and implementation: RopuchAdventure project by Zuzanna Mysłek.
- Character, screen, and general inspiration: "Kuce z bronksu" YouTube series.
- Other audio used in the project: sourced from YouTube.
