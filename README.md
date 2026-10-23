# Boulder Dash (placeholder prototype)

See also HOW_IT_WORKS.md for how the code is organised.

Godot 4.7 + C#. Open the folder in Godot, press Build (hammer icon), then Play (F5).

## Controls
- Arrow keys or WASD: move and dig
- Push boulders sideways, collect enough diamonds to open the exit
- R: restart level, Enter: next level
- SPACE next to a Heaven statue: flips gravity (rocks and souls fall up). Each statue works once, the sheep charges up first
- E or right-click: Heaven Burst, smashes dirt, rocks, bushes and skeletons within one tile. Each level gives a limited number of burst charges
- Click: hides the hint box (H brings it back). F11: fullscreen. The window is 1280x720 and can be resized
- M (or Tab): switch between the zoomed view (follows the player, minimap in the corner) and the full map

## Levels
Tutorials with on-screen hints: Welcome to Hell, Falling Rocks, Skeletons, Hidden in the Brush, Heaven Power, Heaven Burst, Keys and Gates.
Then Ashfields, Walled Pits, and three non-rectangular levels (Crossroads, The Ring, Caverns), and The Abyss.
Each level has a Title, Hint, DiamondsNeeded, GravityCharges and TimeSeconds in `Code/Levels.cs`.

## Rules implemented
- Dirt is dug away when you walk into it
- Boulders and diamonds fall, and roll off other boulders/diamonds
- Standing still under a resting rock for 1 second makes it wobble, and 0.8 seconds later it drops on you
  (`WobbleStart` and `WobbleDuration` on the Main node)
- Rocks and souls roll off round objects to a free side (random if both are free)
- A falling boulder or diamond kills you (also when gravity is flipped)
- Skeletons (`x` in a level) patrol empty tunnels by following the wall on their left, they kill you on touch,
  and a falling rock turns a skeleton into a soul. Speed: `EnemyStepTime` on the Main node
- The gate opens when `DiamondsNeeded` souls are collected AND `KillsNeeded` skeletons are defeated (a rock must fall on them). Set either to 0 to ignore it
- `DriftSeconds` (per level): every N seconds one soul far from the player respawns buried somewhere else
- Bushes (`g`): skeletons hide inside (the bush shakes when one walks in), the sheep walks through them, a falling rock destroys them
- `RandomExit = true` (per level) puts the gate in a random reachable spot every time the level starts
- Levels do not have to be rectangles: use `_` (void) outside and `#` walls around the playable area
- Keys (`k`) open doors (`D`): walk into a door while holding a key, each key opens one door
- Heaven statues (`S`): gravity flips when you press SPACE next to an unused one (charge-up `FlipDelay`, default 0.8 s: the sheep cannot move, then a flash). Burst uses the same charge-up
- Exit opens after the required diamonds, time bonus is added to the score

## Fonts
`Fonts/title.otf` (Dystopian Canticle, HUD and titles) and `Fonts/body.ttf` (Poppins, hint text). Replace the files to change the fonts, see `Fonts/FONTS.txt`.

## Where to work (so nobody edits the same file)
- Art: put PNGs in `Art/` named `dirt.png`, `wall.png`, `boulder.png`, `diamond.png` (the soul gem), `skeleton.png`, `exit.png`, `player.png`
  (optional `exit_closed.png`, `skeleton.png`, `key.png`, `door.png` and `background.png`, 960x560). They replace the placeholder shapes automatically. Tiles are square, 64x64 looks best.
- Levels: `Code/Levels.cs` (text layouts, one character per tile)
- Rules and drawing: `Code/Game.cs`
- Theme: hell. Placeholders are red/black with embers, the player is a white cube (the sheep goes here)
- Levels can be any size (level 3 is 40x24), the zoomed view scrolls to follow the player

## Story, tower and effects
- The game opens with a short story dialog (`StoryPages` in `Code/Game.cs`). Click or SPACE continues, ESC skips. Edit the text there.
- `Art/floor.png` is the stone floor inside the tower (shows behind every non-void tile). Wall tiles on the open top edge get battlements.
- Heaven gravity: golden light, rays and a glowing frame fade in while gravity is flipped.
- Four or more boulders falling at once shake the screen. Tune it near the end of `PhysicsStep`.
- Death messages are in `DeathLines`. End screens fade in.
