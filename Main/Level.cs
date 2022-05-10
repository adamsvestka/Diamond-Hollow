using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;

namespace DiamondHollow
{
    public enum TileType
    {
        Empty = '.',
        Wall = '#',
        Player = 'p',
        SingleGem = '1',
        DounbleGem = '2',
        TripleGem = '3',
        QuadrupleGem = '4',
        Slime = 's',
        WallShooter = 'w',
        CeilingShooter = 'c',
        Spider = 'z',
        Bird = 'b',
    }

    public class Level : GameScene
    {
        private TileType[,] _grid;
        readonly private string _filename;

        public Player Player;
        public Camera Camera;
        public ProjectileController ProjectileController;
        public ParticleController ParticleController;
        public DiamondController DiamondController;
        public EnemyController EnemyController;

        public Level(DiamondHollowGame game, string filename) : base(game, null)
        {
            _filename = filename;
        }

        public override void Initialize()
        {

            Camera = new Camera(Game.GraphicsDevice, this);

            Player = new Player(Game, this);
            ProjectileController = new ProjectileController(Game, this);
            ParticleController = new ParticleController(Game, this);
            DiamondController = new DiamondController(Game, this);
            EnemyController = new EnemyController(Game, this);

            AddComponent(Player);
            AddComponent(ParticleController);
            AddComponent(ProjectileController);
            AddComponent(DiamondController);
            AddComponent(EnemyController);

            base.Initialize();
        }

        private void SpawnDiamond(float x, float y) => DiamondController.Spawn(new Point((int)((x + 0.5f) * Game.TileSize), (int)((y + 0.5f) * Game.TileSize)));
        protected override void LoadContent()
        {
            var lines = File.ReadAllLines(Path.Combine(Game.Content.RootDirectory, _filename));
            _grid = new TileType[lines.Length, lines[0].Length];
            for (int y = 0; y < lines.Length; y++)
            {
                for (int x = 0; x < lines[0].Length; x++)
                {
                    var tile = (TileType)lines[lines.Length - y - 1][x];
                    _grid[y, x] = TileType.Empty;
                    switch (tile)
                    {
                        case TileType.Empty or TileType.Wall:
                            _grid[y, x] = tile;
                            break;
                        case TileType.Player:
                            Player.Position = new Point(x, y);
                            break;
                        case TileType.SingleGem:
                            SpawnDiamond(x, y);
                            break;
                        case TileType.DounbleGem:
                            SpawnDiamond(x - 0.5f, y);
                            SpawnDiamond(x + 0.5f, y);
                            break;
                        case TileType.TripleGem:
                            SpawnDiamond(x, y - 1);
                            SpawnDiamond(x, y);
                            SpawnDiamond(x, y + 1);
                            break;
                        case TileType.QuadrupleGem:
                            SpawnDiamond(x - 1, y - 1);
                            SpawnDiamond(x - 1, y + 1);
                            SpawnDiamond(x + 1, y - 1);
                            SpawnDiamond(x + 1, y + 1);
                            break;
                        case TileType.Slime:
                            EnemyController.SpawnSlime(new Point((int)((x + 0.5f) * Game.TileSize), (int)((y + 0.5f) * Game.TileSize)));
                            break;
                        case TileType.WallShooter:
                            var right = _grid[y, x - 1] == TileType.Wall;
                            int wx = right ? x * Game.TileSize + WallShooter.Size.X / 2 : (x + 1) * Game.TileSize - WallShooter.Size.X / 2;
                            EnemyController.SpawnWallShooter(new Point(wx, (int)((y + 0.5f) * Game.TileSize)), right);
                            break;
                        case TileType.CeilingShooter:
                            int cy = (y + 1) * Game.TileSize - CeilingShooter.Size.Y / 2;
                            EnemyController.SpawnCeilingShooter(new Point((int)((x + 0.5f) * Game.TileSize), cy));
                            break;
                        case TileType.Spider:
                            int zy = (y + 1) * Game.TileSize - Spider.Size.Y / 2;
                            EnemyController.SpawnSpider(new Point((int)((x + 0.5f) * Game.TileSize), zy));
                            break;
                        case TileType.Bird:
                            EnemyController.SpawnBird(new Point((int)((x + 0.5f) * Game.TileSize), (int)((y + 0.5f) * Game.TileSize)));
                            break;
                    }
                }
            }

            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            Camera.Track(Player.Position.Y, (int)Player.Velocity.Y);
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