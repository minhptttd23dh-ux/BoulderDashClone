# How the code is organised (and why)

## Files (all in `Code/`)
| File | Job |
|---|---|
| `Game.cs` | Fields, setup, level loading, random gate placement |
| `GameInput.cs` | Keyboard and mouse |
| `GameLoop.cs` | Per-frame loop, story intro, end-screen text, smooth movement |
| `GamePlayer.cs` | Player moves, pushing boulders, keys, doors, gate |
| `GamePhysics.cs` | Falling and rolling, Heaven gravity flip, Heaven Burst, screen shake trigger |
| `GameWorld.cs` | Gate rule, drifting souls, skeleton AI |
| `GameDraw*.cs` | Camera, HUD, background, tower floor, tiles, minimap, story screen |
| `Levels.cs` | Level data as text, one string per row |

`Game` is one `partial class`, so the files are one class split by topic, not separate systems.

## Key design choices
- **The grid is the game.** A 2D array of tile types holds all rules (falling, rolling, crushing). It is the classic Boulder Dash model and makes the rules easy to test and reason about.
- **Logic and visuals are separate.** The grid changes in discrete steps. Smooth movement is only a drawn offset that glides to the real position.
- **Levels are text.** Every level is a few lines of characters (`#` wall, `.` dirt, `o` boulder, `*` soul, `x` skeleton, `S` statue ...). Easy to edit by hand, easy to see in git diffs, and the team can add levels without touching the rules. The format is documented at the top of `Levels.cs`.
- **Placeholders by code, art by files.** Each tile is drawn with simple shapes unless a PNG with the right name exists in `Art/`. Artists drop files in and nothing else changes.
- **Drawing with `_Draw()`.** For a tile grid with hundreds of identical cells this avoids hundreds of nodes and keeps the camera, shake, fades and effects in one place.

## Honest limits (what we would do with more time)
- Tiles are not Godot nodes, so the level cannot be edited visually in the editor. Moving to `TileMapLayer` plus `Sprite2D` nodes is possible and would help artists and level designers.
- Balance and difficulty need playtesting.
- Not yet built: shooting demon, magic wall, heavy 2x2 rock, lava.

## Likely questions
- *Why not scenes and nodes?* Chosen for a grid rule set; see above. A node-based version is the natural next step.
- *Why is everything in `Game`?* It is one partial class split by topic so each file has one job.
- *How do artists replace graphics?* File names in `Art/`, listed in `README.md`.
