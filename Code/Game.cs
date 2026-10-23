using Godot;
using System.Collections.Generic;

namespace BoulderDash
{
    public enum Tile { Empty, Dirt, Wall, Boulder, Diamond, Exit, Key, Door, Bush, Void, Statue, StatueUsed }
    public partial class Game : Node2D
    {
        public const int HudHeight = 40;
        public const int ViewW = 1280;
        public const int ViewH = 680;
        public const float ZoomTile = 72f;

        [Export] public float PlayerStepTime = 0.13f;
        [Export] public float PhysicsStepTime = 0.15f;
        [Export] public float EnemyStepTime = 0.35f; 
        [Export] public float FlipDelay = 0.8f;
        [Export] public float WobbleStart = 1.0f;
        [Export] public float WobbleDuration = 0.8f; 
        private enum State { Playing, Dead, LevelComplete, AllClear, Story }
        private enum Power { Flip, Burst }

        private class Enemy
        {
            public int X;
            public int Y;
            public int Dir;
            public Vector2 Visual; 
        }

        private static readonly Vector2I[] Dirs = { Vector2I.Up, Vector2I.Right, Vector2I.Down, Vector2I.Left };

        private Tile[,] _grid;
        private bool[,] _falling;
        private bool[,] _moved;
        private bool[,] _wobbling;
        private int _width;
        private int _height;
        private Vector2I _player;

        private Vector2 _playerVisual;
        private Vector2[,] _offset; 

        private class DriftFx
        {
            public Vector2I Cell;
            public float Age;
        }

        private readonly List<DriftFx> _fx = new List<DriftFx>();
        private int _kills;
        private int _killsNeeded;
        private float _driftSeconds; 
        private float _driftTimer;

        private readonly List<Enemy> _enemies = new List<Enemy>();
        private int _gravity = 1;
        private int _charges; 
        private float _heaven;
        private string _title = "";
        private string _hint = "";
        private float _enemyTimer;
        private float _levelClock;
        private float _idle; 
        private int _keys; 
        private float _flipTimer;
        private float _flash;
        private Power _pending = Power.Flip;
        private bool _hintHidden;
        private bool _tutorial;
        private Vector2I _pendingStatue;
        private string _toast = "";
        private float _toastTime;
        private float[,] _bushShake;

        private int _levelIndex;
        private int _diamonds;
        private int _needed;
        private int _score;
        private float _timeLeft;
        private float _playerTimer;
        private float _physicsTimer;
        private State _state = State.Playing;
        private float _stateTime;
        private string _endLine = "";
        private float _shake;
        private int _storyPage;
        private float _storyChars;

        private static readonly string[][] StoryPages =
        {
            new[] { "V", "Little lamb. Do you know why you were summoned?" },
            new[] { "S", "Baa...?" },
            new[] { "V", "You ate the golden grass of the Garden. All of it. Every sacred blade." },
            new[] { "V", "For that, you will be sent below. Hell is full of souls that were lost and forgotten." },
            new[] { "V", "Dig through the pits and gather every soul you find. Open the gates so they can rise to Heaven." },
            new[] { "V", "The dead will not let them go, and the rocks above are not kind. At my statues I will lend you a little of my power." },
            new[] { "S", "Baa." },
            new[] { "V", "Go now. And try not to eat anything down there." },
        };

        private static readonly string[] DeathLines =
        {
            "Your journey ends here, little lamb.",
            "The souls will wait a little longer.",
            "Your penance is not yet paid.",
            "Hell keeps one more soul.",
            "The flock will not find you here.",
        };

        private bool _showFullMap;
        private float _ts;
        private Vector2 _origin;
        private float _time;

        private const int EmberCount = 50;
        private readonly Vector2[] _embers = new Vector2[EmberCount];
        private readonly float[] _emberSpeed = new float[EmberCount];
        private RandomNumberGenerator _rng;

        private Font _font;
        private Font _titleFont;

        private readonly Dictionary<string, Texture2D> _textures = new Dictionary<string, Texture2D>();

        public override void _Ready()
        {
            foreach (string name in new[] { "dirt", "wall", "boulder", "diamond", "exit", "exit_closed", "player", "skeleton", "key", "door", "bush", "statue", "statue_used", "floor", "background" })
            {
                string path = "res://Art/" + name + ".png";
                if (ResourceLoader.Exists(path))
                {
                    _textures[name] = GD.Load<Texture2D>(path);
                }
            }

            _font = LoadFont("body");
            _titleFont = LoadFont("title");
            if (_titleFont != _font)
            {
                _titleFont.Fallbacks = new Godot.Collections.Array<Font> { _font };
            }

            _rng = new RandomNumberGenerator();
            _rng.Randomize();
            for (int i = 0; i < EmberCount; i++)
            {
                _embers[i] = new Vector2(_rng.RandfRange(0, ViewW), _rng.RandfRange(HudHeight, HudHeight + ViewH));
                _emberSpeed[i] = _rng.RandfRange(15f, 55f);
            }

            LoadLevel(0);
            _state = State.Story;
            _storyPage = 0;
            _storyChars = 0;
        }

        private Font LoadFont(string name)
        {
            foreach (string ext in new[] { ".ttf", ".otf", ".woff2", ".woff" })
            {
                string path = "res://Fonts/" + name + ext;
                if (ResourceLoader.Exists(path))
                {
                    Font f = GD.Load<Font>(path);
                    if (f != null)
                    {
                        return f;
                    }
                }
            }
            return ThemeDB.FallbackFont;
        }


        private void LoadLevel(int index)
        {
            _levelIndex = index;
            Levels.LevelData data = Levels.All[index];
            _height = data.Rows.Length;
            _width = data.Rows[0].Length;
            _grid = new Tile[_width, _height];
            _falling = new bool[_width, _height];
            _moved = new bool[_width, _height];
            _wobbling = new bool[_width, _height];
            _idle = 0;
            _keys = 0;
            _flipTimer = 0;
            _flash = 0;
            _offset = new Vector2[_width, _height];
            _bushShake = new float[_width, _height];
            _hintHidden = false;
            _tutorial = data.Tutorial;
            _pending = Power.Flip;
            _needed = data.DiamondsNeeded;
            _killsNeeded = data.KillsNeeded;
            _kills = 0;
            _driftSeconds = data.DriftSeconds;
            _driftTimer = 0;
            _fx.Clear();
            _timeLeft = data.TimeSeconds;
            _title = data.Title ?? "";
            _hint = data.Hint ?? "";
            _charges = data.BurstCharges;
            _toast = "";
            _toastTime = 0;
            _gravity = 1;
            _heaven = 0;
            _enemyTimer = 0;
            _levelClock = 0;
            _enemies.Clear();
            _diamonds = 0;
            _playerTimer = 0;
            _physicsTimer = 0;
            _state = State.Playing;

            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    switch (data.Rows[y][x])
                    {
                        case '#': _grid[x, y] = Tile.Wall; break;
                        case '.': _grid[x, y] = Tile.Dirt; break;
                        case 'o': _grid[x, y] = Tile.Boulder; break;
                        case '*': _grid[x, y] = Tile.Diamond; break;
                        case 'E': _grid[x, y] = Tile.Exit; break;
                        case 'k': _grid[x, y] = Tile.Key; break;
                        case 'D': _grid[x, y] = Tile.Door; break;
                        case 'g': _grid[x, y] = Tile.Bush; break;
                        case 'S': _grid[x, y] = Tile.Statue; break;
                        case '_': _grid[x, y] = Tile.Void; break;
                        case 'x':
                            _grid[x, y] = Tile.Empty;
                            _enemies.Add(new Enemy { X = x, Y = y, Dir = 3, Visual = new Vector2(x, y) });
                            break;
                        case 'P':
                            _grid[x, y] = Tile.Empty;
                            _player = new Vector2I(x, y);
                            break;
                        default: _grid[x, y] = Tile.Empty; break;
                    }
                }
            }
            if (data.RandomExit)
            {
                PlaceRandomExit();
            }

            _playerVisual = new Vector2(_player.X, _player.Y);
            QueueRedraw();
        }

        // Moves the gate to a random reachable spot far from the start, so the player cannot always head for the same corner.
        private void PlaceRandomExit()
        {
            Vector2I oldExit = new Vector2I(-1, -1);
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    if (_grid[x, y] == Tile.Exit)
                    {
                        oldExit = new Vector2I(x, y);
                    }
                }
            }
            if (oldExit.X < 0)
            {
                return;
            }

            bool[,] seen = new bool[_width, _height];
            Queue<Vector2I> queue = new Queue<Vector2I>();
            queue.Enqueue(_player);
            seen[_player.X, _player.Y] = true;
            while (queue.Count > 0)
            {
                Vector2I p = queue.Dequeue();
                foreach (Vector2I d in Dirs)
                {
                    Vector2I n = p + d;
                    if (InBounds(n.X, n.Y) && !seen[n.X, n.Y] && _grid[n.X, n.Y] != Tile.Wall && _grid[n.X, n.Y] != Tile.Void
                        && _grid[n.X, n.Y] != Tile.Statue && _grid[n.X, n.Y] != Tile.StatueUsed)
                    {
                        seen[n.X, n.Y] = true;
                        queue.Enqueue(n);
                    }
                }
            }

            List<Vector2I> spots = new List<Vector2I>();
            int minDist = (_width + _height) / 3;
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    int dist = Mathf.Abs(x - _player.X) + Mathf.Abs(y - _player.Y);
                    if (seen[x, y] && dist >= minDist && (_grid[x, y] == Tile.Dirt || _grid[x, y] == Tile.Empty) && EnemyAt(x, y) == null)
                    {
                        spots.Add(new Vector2I(x, y));
                    }
                }
            }
            if (spots.Count == 0)
            {
                return;
            }

            Vector2I chosen = spots[(int)(_rng.Randi() % (uint)spots.Count)];
            _grid[oldExit.X, oldExit.Y] = Tile.Dirt;
            _grid[chosen.X, chosen.Y] = Tile.Exit;
        }
    }
}
