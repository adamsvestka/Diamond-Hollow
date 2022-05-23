using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DiamondHollow
{
    public enum TileType
    {
        Empty = '.',
        Wall = '#',
        Spawnpoint = '*',
        SingleGem = '1',
        DounbleGem = '2',
        TripleGem = '3',
        QuadrupleGem = '4',
        Slime = 's', Slime2 = 'S',
        WallShooter = 'w', WallShooter2 = 'W',
        CeilingShooter = 'c', CeilingShooter2 = 'C',
        Spider = 'z', Spider2 = 'Z',
        Bird = 'b', Bird2 = 'B',
        LargeHeart = '+',
        Nuke = '@',
        Checkpoint = '$',
    }

    class Background : DHGameComponent
    {
        public Background(DiamondHollowGame game) : base(game, game.Level)
        {
            DrawOrder = (int)DrawingLayers.Foreground;
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            int bottom = Math.Max(Level.Camera.CameraY / Game.TileSize, 0);
            int top = Math.Min(bottom + Game.WindowHeight / Game.TileSize + 1, Level.MapHeight);

            Game.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap);

            for (int y = bottom; y < top; y++)
            {
                for (int x = 0; x < Level.MapWidth / Game.TileSize; x++)
                {
                    Rectangle tile = new Point(x, y).FromGrid().MakeTile().ToScreen();

                    if (Level.GetTile(x, y) == TileType.Wall)
                    {
                        Point fg = Level._platformTileCache[y, x];
                        Game.SpriteBatch.Draw(Level._platformTileset, tile, new Rectangle(fg.X * 32, fg.Y * 32, 32, 32), Color.White);
                    }
                }
            }

            Game.SpriteBatch.End();
        }
    }

    public class Level : GameScene
    {
        private TileType[,] _grid;
        private readonly LevelGenerator _levelGenerator;
        private Point[][] _platformTileMap;
        public Point[,] _platformTileCache, _backgroundCache;
        public Texture2D _platformTileset, _backgroundTileset;
        public Dictionary<Point, Point> _backgroundTileMap;
        private int[] _backgroundTileMapWeights;

        public Player Player;
        public Camera Camera;
        public ProjectileController ProjectileController;
        public ParticleController ParticleController;
        public CollectiblesController CollectiblesController;
        public EnemyController EnemyController;

        public Point Spawnpoint;
        public float Difficulty => 1f + (float)(Camera?.CameraY ?? 0f) / Game.TileSize / 100f;
        public float Modifier => Math.Clamp(Difficulty / 4f, 0.5f, 2f);

        public Level(DiamondHollowGame game, string filename) : base(game, null)
        {
            _levelGenerator = new LevelGenerator(game, this, filename);
            Spawnpoint = Point.Zero;
        }

        protected override void LoadContent()
        {
            _grid = new TileType[0, 0];
            _platformTileCache = new Point[0, 0];
            _backgroundCache = new Point[0, 0];

            GenerateTileMap();
            LoadNextMapSegment();

            _platformTileset = Game.Content.Load<Texture2D>("Sprites/Environment/Tileset");
            _backgroundTileset = Game.Content.Load<Texture2D>("Sprites/Environment/Background");

            base.LoadContent();
        }

        public override void Initialize()
        {
            DrawOrder = (int)DrawingLayers.Background;
            AddComponent(new Background(Game));

            Camera = new Camera(Game, this);
            Player = new Player(Game, this);
            ProjectileController = new ProjectileController(Game, this);
            ParticleController = new ParticleController(Game, this);
            CollectiblesController = new CollectiblesController(Game, this);
            EnemyController = new EnemyController(Game, this);

            AddComponents(Player, Camera);
            AddComponents(ParticleController, ProjectileController, CollectiblesController, EnemyController);

            Camera.Track(Player);

            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            if (Player.Position.Y + 2 * Game.WindowHeight > MapHeight)
            {
                LoadNextMapSegment();
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);

            int bottom = Math.Max(Camera.CameraY / Game.TileSize, 0);
            int top = Math.Min(bottom + Game.WindowHeight / Game.TileSize + 1, MapHeight);

            Game.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap);

            for (int y = bottom; y < top; y++)
            {
                for (int x = 0; x < _grid.GetLength(1); x++)
                {
                    Rectangle tile = new Point(x, y).FromGrid().MakeTile().ToScreen();

                    Point bg = _backgroundCache[y, x];
                    Point spawn = Spawnpoint.ToGrid();
                    Color color = Color.White;
                    Point pt = new(x, y);
                    int dist2 = (pt - spawn).LengthSquared();

                    if (pt == spawn) bg = _backgroundTileMap.Keys.ElementAt(6);
                    else if (pt == spawn.OffsetY(1)) bg = _backgroundTileMap.Keys.ElementAt(5);
                    if (dist2 < 70 && _backgroundTileMap.ContainsKey(bg))
                    {
                        bg = _backgroundTileMap[bg];
                        color *= 0.95f + dist2 / 35f * 0.05f;
                    }

                    Game.SpriteBatch.Draw(_backgroundTileset, tile, new Rectangle(bg.X * 16, bg.Y * 16, 16, 16), color);
                }
            }

            Game.SpriteBatch.End();

            base.Draw(gameTime);
        }

        private void LoadNextMapSegment()
        {
            _levelGenerator.LoadNext(ref _grid);
            int height = Math.Max(_platformTileCache.GetLength(0) - 1, 0);
            int height2 = _grid.GetLength(0);
            int width = _grid.GetLength(1);

            Helpers.ResizeArray(ref _backgroundCache, height2, width);
            Helpers.ResizeArray(ref _platformTileCache, height2, width);

            for (int y = height + 1; y < height2; y++)
                for (int x = 0; x < width; x++)
                    _backgroundCache[y, x] = Game.WeightedChoice(_backgroundTileMap.Keys, _backgroundTileMapWeights);

            for (int y = height; y < height2; y++)
                for (int x = 0; x < width; x++)
                    if (_grid[y, x] == TileType.Wall)
                        _platformTileCache[y, x] = Game.Choice(_platformTileMap[CollectSurroundingTiles(new Point(x, y))]);
        }

        private byte CollectSurroundingTiles(Point point)
        {
            byte data = 0, i = 7;
            for (int y = point.Y + 1; y >= point.Y - 1; y--)
            {
                for (int x = point.X - 1; x <= point.X + 1; x++)
                {
                    if (x == point.X && y == point.Y) continue;
                    if (IsWall(new Point(x, y).FromGrid())) data |= (byte)(1 << i);
                    i--;
                }
            }
            return data;
        }

        public void DrawRectangle(Rectangle rect, Color color) => Game.SpriteBatch.Draw(Game.WhitePixel, rect.ToScreen(), color);
        public void DrawLine(Point start, Point end, Color color, int width) => Game.SpriteBatch.DrawLine(start.ToScreen().ToVector2(), end.ToScreen().ToVector2(), color, width);

        public int MapHeight => _grid.GetLength(0) * Game.TileSize;
        public int MapWidth => _grid.GetLength(1) * Game.TileSize;
        public TileType GetTile(int x, int y) => _grid[y, x];

        public bool IsWall(Point p)
        {
            var c = p.ToGrid();
            return c.X < 0 || c.X >= _grid.GetLength(1)
                || c.Y < 0 || c.Y >= _grid.GetLength(0)
                || _grid[c.Y, c.X] == TileType.Wall;
        }
        public bool IsWall(int x, int y) => IsWall(new Point(x, y));
        public bool IsOnGround(Rectangle box) => IsWall(box.Location.OffsetY(-1)) || IsWall(box.Location.Offset(box.Width - 1, -1));

        private IEnumerable<string> GeneratePermutations(string repr)
        {
            int i = repr.IndexOf('.');
            if (i == -1) return new[] { repr };
            return GeneratePermutations(repr[0..i] + "x" + repr[(i + 1)..]).Concat(GeneratePermutations(repr[0..i] + " " + repr[(i + 1)..]));
        }
        private void AddTileVariant(string repr, params (int x, int y)[] c)
        {
            foreach (var k in GeneratePermutations(repr.Replace("|", "")))
            {
                int i = Convert.ToInt32(k.Remove(4, 1).Replace('x', '1').Replace(' ', '0'), 2);
                _platformTileMap[i] = c.Select(p => new Point(p.x, p.y)).ToArray();
            }
        }
        private void GenerateTileMap()
        {
            _backgroundTileMap = new() {
                { new Point(1, 2), new Point(1, 2) },
                { new Point(15, 1), new Point(15, 1) },
                { new Point(13, 1), new Point(21, 1) },
                { new Point(11, 1), new Point(19, 1) },
                { new Point(9, 1), new Point(17, 1) },
                { new Point(15, 7), new Point(20, 7) },
                { new Point(15, 8), new Point(20, 8) },
            };

            _backgroundTileMapWeights = new[] {
                5, 4, 3, 2, 1, 0, 0,
            };


            _platformTileMap = new Point[256][];
            for (int i = 0; i < 256; i++) _platformTileMap[i] = new[] { Point.Zero };

            // Single
            AddTileVariant(". .| x |. .", (2, 8));

            // Horizontal
            AddTileVariant(". .| x |.x.", (0, 8), (7, 0));
            AddTileVariant(".x.| x |.x.", (0, 9), (8, 4));
            AddTileVariant(".x.| x |. .", (0, 10), (7, 1));

            // Vertical
            AddTileVariant(". .| xx|. .", (0, 12), (9, 0));
            AddTileVariant(". .|xxx|. .", (1, 12));
            AddTileVariant(". .|xx |. .", (2, 12), (10, 0));

            // Box outer
            AddTileVariant(". .| xx|.xx", (0, 0));
            AddTileVariant(". .|xxx|xxx", (1, 0));
            AddTileVariant(". .|xx |xx.", (2, 0));
            AddTileVariant(".xx| xx|.xx", (0, 1), (4, 10), (4, 11), (4, 12), (13, 14));
            AddTileVariant("xxx|xxx|xxx", (1, 1));
            AddTileVariant("xx.|xx |xx.", (2, 1), (8, 10), (8, 11), (8, 12), (15, 14));
            AddTileVariant(".xx| xx|. .", (0, 2), (13, 15));
            AddTileVariant("xxx|xxx|. .", (1, 2));
            AddTileVariant("xx.|xx |. .", (2, 2), (15, 15));

            // Box inner
            AddTileVariant("xxx|xxx|xx ", (0, 4));
            AddTileVariant("xxx|xxx|x x", (1, 4));
            AddTileVariant("xxx|xxx| xx", (2, 4));
            AddTileVariant("xxx|xx |xxx", (0, 5));
            AddTileVariant("xxx| xx|xxx", (2, 5));
            AddTileVariant("xx |xxx|xxx", (0, 6));
            AddTileVariant("x x|xxx|xxx", (1, 6));
            AddTileVariant(" xx|xxx|xxx", (2, 6));

            // +
            AddTileVariant(" x |xxx| x ", (10, 13));

            // Extruded Ts
            AddTileVariant(" x |xxx|xxx", (6, 9));
            AddTileVariant("xx |xxx|xx ", (8, 13));
            AddTileVariant("xxx|xxx| x ", (9, 11));
            AddTileVariant(" xx|xxx| xx", (10, 5));

            // Thin Ts
            AddTileVariant(" x |xxx|. .", (10, 15));
            AddTileVariant(".x | xx|.x ", (4, 7));
            AddTileVariant(". .|xxx| x ", (8, 3));
            AddTileVariant(" x.|xx | x.", (15, 9));

            // Corner Ls
            AddTileVariant(" x |xx |  .", (11, 11));
            AddTileVariant(" x | xx|.  ", (4, 15));
            AddTileVariant(".  | xx| x ", (10, 11));
            AddTileVariant("  .|xx | x ", (15, 3));

            // Notched slab
            AddTileVariant(" xx|xxx|. .", (6, 15));
            AddTileVariant("xx |xxx|. .", (8, 15));
            AddTileVariant(".x | xx|.xx", (4, 9));
            AddTileVariant(".xx| xx|.x ", (4, 13));
            AddTileVariant(". .|xxx|xx ", (10, 3));
            AddTileVariant(". .|xxx| xx", (13, 3));
            AddTileVariant(" x.|xx |xx.", (15, 5));
            AddTileVariant("xx.|xx | x.", (15, 7));

            // {
            //     Console.WriteLine($"WARNING: Missing tile mappings ({_platformTileMap.Count(t => t.Contains(Point.Zero))})");
            //     int i = 0;
            //     _platformTileMap.Select(e => (i++, e)).Where(e => e.e.Contains(Point.Zero)).Select(e => Convert.ToString(e.Item1, 2).PadLeft(8, '0').Replace('0', ' ').Replace('1', 'x').Insert(4, "x").Insert(6, "|").Insert(3, "|")).ToList().ForEach(Console.WriteLine);
            // }
        }
    }
}