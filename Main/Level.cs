using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

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

    public class Level : GameScene
    {
        private TileType[,] _grid;
        private readonly LevelGenerator _levelGenerator;

        public Player Player;
        public Camera Camera;
        public ProjectileController ProjectileController;
        public ParticleController ParticleController;
        public CollectiblesController CollectiblesController;
        public EnemyController EnemyController;

        public Point Spawnpoint;
        public float Difficulty => 1f + (float)(Player?.Position.Y ?? 0f) / Game.TileSize / 100f;
        public float Modifier => Math.Clamp(Difficulty / 4f, 0.5f, 2f);

        public Level(DiamondHollowGame game, string filename) : base(game, null)
        {
            _levelGenerator = new LevelGenerator(game, this, filename);
            Spawnpoint = Point.Zero;
        }

        protected override void LoadContent()
        {
            _grid = new TileType[0, 0];
            _levelGenerator.LoadNext(ref _grid);

            base.LoadContent();
        }

        public override void Initialize()
        {
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
            if (Player.Position.Y + 2 * Game.WindowHeight > GetHeight())
            {
                _levelGenerator.LoadNext(ref _grid);
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            Game.SpriteBatch.Begin();

            for (int y = 0; y < _grid.GetLength(0); y++)
            {
                for (int x = 0; x < _grid.GetLength(1); x++)
                {
                    switch (_grid[y, x])
                    {
                        case TileType.Empty:
                            DrawRectangle(new Point(x, y).FromGrid().MakeTile(), Color.White);
                            break;
                        case TileType.Wall:
                            DrawRectangle(new Point(x, y).FromGrid().MakeTile(), Color.Black);
                            break;
                    }
                }
            }

            DrawRectangle(Spawnpoint.SnapToGrid().MakeTile(), Color.LightBlue);

            Game.SpriteBatch.End();

            base.Draw(gameTime);
        }

        public void DrawRectangle(Rectangle rect, Color color) => Game.SpriteBatch.Draw(Game.WhitePixel, rect.ToScreen(), color);
        public void DrawLine(Point start, Point end, Color color, int width) => Game.SpriteBatch.DrawLine(start.ToScreen().ToVector2(), end.ToScreen().ToVector2(), color, width);

        public int GetHeight() => _grid.GetLength(0) * Game.TileSize;
        public TileType GetTile(int x, int y) => _grid[y, x];

        public bool IsWall(Point p)
        {
            try
            {
                var coords = p.ToGrid();
                return _grid[coords.Y, coords.X] == TileType.Wall;
            }
            catch
            {
                return true;
            }
        }
        public bool IsWall(int x, int y) => IsWall(new Point(x, y));
        public bool IsOnGround(Rectangle box) => IsWall(box.Location.OffsetY(-1)) || IsWall(box.Location.Offset(box.Width - 1, -1));

        public IEnumerable<Point> Raycast(Vector2 origin, Vector2 direction, float distance = 1e6f)
        {
            Vector2 delta = new(Math.Abs(direction.X), Math.Abs(direction.Y));

            Point curr = origin.ToPoint();
            Point target = (origin + direction * distance).ToPoint();

            Point inc = new();
            double error;
            int n = 1;

            if (direction.X == 0)
            {
                inc.X = 0;
                error = double.PositiveInfinity;
            }
            else if (direction.X > 0)
            {
                inc.X = 1;
                n += target.X - curr.X;
                error = (Math.Ceiling(origin.X) - origin.X) * delta.Y;
            }
            else
            {
                inc.X = -1;
                n += curr.X - target.X;
                error = (origin.X - Math.Floor(origin.X)) * delta.Y;
            }

            if (direction.Y == 0)
            {
                inc.Y = 0;
                error -= double.PositiveInfinity;
            }
            else if (direction.Y > 0)
            {
                inc.Y = 1;
                n += target.Y - curr.Y;
                error -= (Math.Ceiling(origin.Y) - origin.Y) * delta.X;
            }
            else
            {
                inc.Y = -1;
                n += curr.Y - target.Y;
                error -= (origin.Y - Math.Floor(origin.Y)) * delta.X;
            }

            for (; n > 0 && !IsWall(curr.FromGrid()); n--)
            {
                yield return curr;

                if (error > 0)
                {
                    curr.Y += inc.Y;
                    error -= delta.X;
                }
                else
                {
                    curr.X += inc.X;
                    error += delta.Y;
                }
            }
        }
    }
}