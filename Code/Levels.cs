namespace BoulderDash
{
    // Level layouts as text. Levels can be any size, but every row of one level must have the same length.
    // The game view scrolls to follow the player, so bigger levels are fine (press M for the full map).
    //   #  wall (cannot be dug)
    //   .  dirt (the player digs through it)
    //   (space) empty tunnel
    //   o  boulder
    //   *  soul gem (diamond)
    //   x  skeleton (enemy, walks through empty tunnels)
    //   g  hell bush (skeletons can hide inside, it shakes when one walks in; the sheep can walk through; a falling rock destroys it)
    //   _  void (nothing there: use it around the edge to make levels that are not rectangles; surround the playable area with # walls)
    //   S  Heaven statue (stand next to it and press SPACE to flip gravity, one use each)
    //   k  key (opens one door)
    //   D  door (blocks the way until you walk into it holding a key)
    //   P  player start
    //   E  exit gate (opens when enough souls are collected)
    //
    // Title and Hint are shown on screen. A Hint is great for tutorial levels (leave it "" to hide it).
    // The gate opens when DiamondsNeeded souls are collected AND KillsNeeded skeletons are defeated (a skeleton is defeated by dropping a rock on it). Set either to 0 to ignore it.
    // BurstCharges is how many times the sheep may use Heaven Burst (E or right-click) in that level.
    // Gravity is flipped at Heaven statues ('S'): stand next to one and press SPACE. Each statue works once.
    public static class Levels
    {
        public class LevelData
        {
            public string Title;
            public string Hint;
            public string[] Rows;
            public int DiamondsNeeded;      // souls to collect before the gate opens
            public int KillsNeeded;         // skeletons to defeat before the gate opens (0 = none)
            public float DriftSeconds;      // every this many seconds a soul respawns elsewhere (0 = never)
            public int BurstCharges;      // Heaven Burst charges (E or right-click)
            public bool Tutorial;           // tutorial hints stay up until the player clicks them away
            public bool RandomExit;         // move the gate (E) to a random reachable spot on every start
            public int TimeSeconds;
        }

        public static readonly LevelData[] All =
        {
            new LevelData
            {
                Title = "Welcome to Hell",
                Tutorial = true,
                Hint = "Move with the arrow keys or WASD. You dig through dirt just by walking into it. Collect 3 souls (the glowing gems) to open the gate, then walk into the gate.",
                DiamondsNeeded = 3,
                BurstCharges = 0,
                TimeSeconds = 150,
                Rows = new[]
                {
                    "##############",
                    "#P...........#",
                    "#.....*......#",
                    "#............#",
                    "#..*.........#",
                    "#.........*..#",
                    "#...........E#",
                    "##############",
                }
            },
            new LevelData
            {
                Title = "Falling Rocks",
                Tutorial = true,
                Hint = "Rocks fall when nothing is under them, and a falling rock crushes you. A resting rock is safe only while you keep moving. Stand still under one and it wobbles, then drops. You can push a rock sideways. Dig out the souls and reach the gate.",
                DiamondsNeeded = 3,
                BurstCharges = 0,
                TimeSeconds = 150,
                Rows = new[]
                {
                    "##############",
                    "#P...........#",
                    "#.....o......#",
                    "#.....*......#",
                    "#............#",
                    "#..o.....*...#",
                    "#..*.......E.#",
                    "##############",
                }
            },
            new LevelData
            {
                Title = "Skeletons",
                Tutorial = true,
                Hint = "Skeletons patrol open tunnels and kill you on touch. They cannot dig through dirt. Drop a rock on one to defeat it. The gate needs 2 souls AND 1 defeated skeleton.",
                DiamondsNeeded = 2,
                KillsNeeded = 1,
                BurstCharges = 0,
                TimeSeconds = 150,
                Rows = new[]
                {
                    "##############",
                    "#P...........#",
                    "#...........*#",
                    "#.....o..o...#",
                    "#............#",
                    "#.  *   x   .#",
                    "#.........*..#",
                    "#...........E#",
                    "##############",
                }
            },
            new LevelData
            {
                Title = "Hidden in the Brush",
                Tutorial = true,
                Hint = "Skeletons can hide in the hell bushes. A bush shakes whenever something walks into it, so watch for movement. You can walk through bushes too, and a falling rock tears them down. Collect 3 souls.",
                DiamondsNeeded = 3,
                BurstCharges = 0,
                TimeSeconds = 150,
                Rows = new[]
                {
                    "##############",
                    "#P...........#",
                    "#...........*#",
                    "#............#",
                    "#............#",
                    "#.  *ggg x  .#",
                    "#.........*..#",
                    "#...........E#",
                    "##############",
                }
            },
            new LevelData
            {
                Title = "Heaven Power",
                Tutorial = true,
                Hint = "Heaven statues flip gravity. Stand next to the statue and press SPACE: the sheep charges up for a moment, then rocks and souls fall UP. Each statue works once. The rocks in the shaft block the gate, so flip gravity, then go down to the gate.",
                DiamondsNeeded = 3,
                BurstCharges = 0,
                TimeSeconds = 150,
                Rows = new[]
                {
                    "##############",
                    "#P..*.. .....#",
                    "#S..... ....*#",
                    "#.*.... .....#",
                    "#.....#o#....#",
                    "#.....#o#....#",
                    "#.....#E#....#",
                    "##############",
                }
            },
            new LevelData
            {
                Title = "Heaven Burst",
                Tutorial = true,
                Hint = "Press E or right-click to spend a charge and smash everything breakable around you: dirt, rocks, bushes and skeletons within one tile. Walls and gates stay. The rocks block the way down, so stand above them and burst. Careful: rocks above the hole can fall.",
                DiamondsNeeded = 3,
                BurstCharges = 1,
                TimeSeconds = 150,
                Rows = new[]
                {
                    "##############",
                    "#P...........#",
                    "#.....*......#",
                    "#............#",
                    "#..*.........#",
                    "######ooo#####",
                    "#.........*..#",
                    "#...........E#",
                    "##############",
                }
            },
            new LevelData
            {
                Title = "Keys and Gates",
                Tutorial = true,
                Hint = "Pick up the key and walk into the gold door to open it. Each key opens one door. Collect 3 souls, open the door, then enter the gate.",
                DiamondsNeeded = 3,
                BurstCharges = 0,
                TimeSeconds = 150,
                Rows = new[]
                {
                    "##############",
                    "#P...........#",
                    "#.....*......#",
                    "#..k.........#",
                    "#.........*..#",
                    "#.....#D#....#",
                    "#.....#E#..*.#",
                    "##############",
                }
            },
            new LevelData
            {
                Title = "Ashfields",
                RandomExit = true,
                Hint = "Skeletons wander the tunnels, and every 30 seconds a soul drifts away and respawns somewhere else. Keep moving. Press M for the full map.",
                DiamondsNeeded = 8,
                DriftSeconds = 30,
                BurstCharges = 1,
                TimeSeconds = 200,
                Rows = new[]
                {
                    "####################",
                    "#P.....o..*....o...#",
                    "#.oo.......o......*#",
                    "#....*..oo.....*...#",
                    "#..o.....*...o.....#",
                    "#.......o..... x ..#",
                    "#*..oo......S*..o..#",
                    "#.....*...o........#",
                    "#..o.........o...*.#",
                    "#....*..o.......o..#",
                    "#..*.........*....E#",
                    "####################",
                }
            },
            new LevelData
            {
                Title = "Walled Pits",
                RandomExit = true,
                Hint = "Two skeletons this time, and the walls trap rocks. Souls drift every 25 seconds. Plan your route.",
                DiamondsNeeded = 8,
                DriftSeconds = 25,
                BurstCharges = 1,
                TimeSeconds = 200,
                Rows = new[]
                {
                    "####################",
                    "#P..#......*.......#",
                    "#...#..oo..........#",
                    "#.o.#......#####.*.#",
                    "#...#.*....# x #...#",
                    "#.*...o..S.#.o.#.o.#",
                    "#######.....#.*..S.#",
                    "#..*...o..o.#####..#",
                    "#.....oo......*....#",
                    "#.o..*...o. x  .o*.#",
                    "#....*...*.......E.#",
                    "####################",
                }
            },
            new LevelData
            {
                Title = "Crossroads",
                RandomExit = true,
                Hint = "The cave is shaped like a cross, and the gate is in a different place every time. Souls drift every 30 seconds.",
                DiamondsNeeded = 10,
                DriftSeconds = 30,
                BurstCharges = 2,
                TimeSeconds = 420,
                Rows = new[]
                {
                    "____________############____________",
                    "____________#......*...#____________",
                    "____________#gg   o....#____________",
                    "____________#*o.o..o...#____________",
                    "____________#..........#____________",
                    "____________#...o......#____________",
                    "____________#o.....o...#____________",
                    "____________#o...o.oo.*#____________",
                    "#############....*    .#############",
                    "#........o...gg   .....gg    ...*..#",
                    "#.....oo.......*.o.......o..ogg   .#",
                    "#....*oo...o.....ogg   x  .........#",
                    "#............o.o........o...*......#",
                    "#P.......*......*.......      g x E#",
                    "#...o..*o     ....*o..*.........o..#",
                    "#o........... x ...........o....o.o#",
                    "#.*..**.......o....................#",
                    "#############So.......o#############",
                    "____________#......o...#____________",
                    "____________#....So.o..#____________",
                    "____________#....o.o...#____________",
                    "____________#.....*....#____________",
                    "____________#..........#____________",
                    "____________#...*...o..#____________",
                    "____________#.o..o....o#____________",
                    "____________############____________",
                }
            },
            new LevelData
            {
                Title = "The Ring",
                RandomExit = true,
                Hint = "Circle the pit to find the gate. It moves every time you start. Watch the bushes.",
                DiamondsNeeded = 12,
                DriftSeconds = 25,
                BurstCharges = 2,
                TimeSeconds = 450,
                Rows = new[]
                {
                    "____________________________________",
                    "____________############____________",
                    "_________####.*o.*  x  ####_________",
                    "_______###.......o........###_______",
                    "______##......S...*..*o.o   ##______",
                    "____###........S...o........o###____",
                    "___##.*.....o.########....o....##___",
                    "___#........###______###......o.#___",
                    "__##..*...###__________###o..o..##__",
                    "__#gg    ##______________##....o.#__",
                    "_##.o....#________________#.gg   ##_",
                    "_#.*..o..#________________#o.o.o..#_",
                    "_#...*...#________________#o..*...#_",
                    "_#P..  *.#________________#......E#_",
                    "_#....*o.#________________#...*...#_",
                    "_##..o...#________________#.oo*..##_",
                    "__#......##______________##......#__",
                    "__##gg   o###__________###.o. x ##__",
                    "___#........###______###o...o..*#___",
                    "___##o..o...o.########.....o...##___",
                    "____###..oo*......o.........*###____",
                    "______##.....     ..........##______",
                    "_______###.o.o...o x *...o###_______",
                    "_________####..gg   ..*####_________",
                    "____________############____________",
                    "____________________________________",
                }
            },
            new LevelData
            {
                Title = "Caverns",
                RandomExit = true,
                Hint = "A twisting cave with hidden skeletons and a hidden gate. Use the minimap.",
                DiamondsNeeded = 12,
                DriftSeconds = 25,
                BurstCharges = 2,
                TimeSeconds = 450,
                Rows = new[]
                {
                    "______________________________________",
                    "______________________________________",
                    "___####________________###____________",
                    "__##..##______###_____##o##___________",
                    "__#.o..####__##.#_____#...#___________",
                    "__#      .####..##___##.o.##__________",
                    "__#.o.........*o.#####....##__________",
                    "__#.....*.o......*..o...###___________",
                    "__#..o......*.*.....o..##_____________",
                    "__#*.......ooo..S...oo*#______________",
                    "__#.....o...o....*..o..#______________",
                    "__#.*o..o.........o...o######_________",
                    "__#.....     ..  x .........####______",
                    "__#P..####....    .....gg   x  ####___",
                    "__#..##__#...o....................##__",
                    "__#..#___#...*..o....o..####..o....#__",
                    "_##..#___#.      x  *..##__##      #__",
                    "_##.o#___#o............#____##*..*.#__",
                    "__#o.#___#.............#_____##   o#__",
                    "__#..#___#.........o.o.#______#....##_",
                    "__####___#*.o*.o....o.o##_____#.S..E#_",
                    "___##___##..*.....########____##..o##_",
                    "________##*..######____#.##____#####__",
                    "_________#####_________####___________",
                    "________________________##____________",
                    "______________________________________",
                }
            },
            new LevelData
            {
                Title = "The Abyss",
                RandomExit = true,
                Hint = "A big level. Souls drift every 20 seconds. Watch the minimap in the corner, and press M for the whole map.",
                DiamondsNeeded = 20,
                DriftSeconds = 20,
                BurstCharges = 2,
                TimeSeconds = 500,
                Rows = new[]
                {
                    "########################################",
                    "#P........o.....o......oo.o............#",
                    "#....*..o*.o............o*....*....*...#",
                    "#....S....o#...*.....*......o*...o.....#",
                    "#....o.....#.....  x   ..o...........oo#",
                    "#.......o...*.......o*.....*o....o*..o.#",
                    "#...#.......o.........*.##o...*......o.#",
                    "#o..#...o..........*..####..o*..o..*...#",
                    "#*.##....o.o.....*o...    x    *....*..#",
                    "#o...........*........o*.....*..o.....o#",
                    "#....*........o....o..............o....#",
                    "#o........**o......*.oo..o.........o...#",
                    "#.......##......o.........oo x  .o...*.#",
                    "#.......o....o.o.....*...*o....o.......#",
                    "#.S.......S.....o....o.*..o..#.....o*..#",
                    "#......oo...  x   .........o.#..####...#",
                    "#....####......ooo.     .....#..       #",
                    "#..................###*.**.#.#*o....o..#",
                    "#...o....    ...o*.#...*o..#...........#",
                    "#.........oo.......#o...........    ..*#",
                    "#o.#...............#.........###o......#",
                    "#o.#...o...........#o...*...o..*..o....#",
                    "#..#......o.o*.*.o...o................E#",
                    "########################################",
                }
            },
        };
    }
}
