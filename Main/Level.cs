using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;

namespace DiamondHollow
{
    public enum TileType
    {
        Empty = '.',
        Wall = '#'
    }

    public class Level : DrawableGameComponent
    {
        private new DiamondHollowGame Game { get => (DiamondHollowGame)base.Game; }

        private TileType[,] _grid;
        readonly private string _filename;

        public Level(DiamondHollowGame game, string filename) : base(game)
        {
            _filename = filename;
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            var lines = File.ReadAllLines(Path.Combine(Game.Content.RootDirectory, _filename));
            _grid = new TileType[lines.Length, lines[0].Length];
            for (int y = 0; y < lines.Length; y++)
                for (int x = 0; x < lines[0].Length; x++)
                    _grid[y, x] = (TileType)lines[lines.Length - y - 1][x];
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            Game.Camera.Track(Game.Player.Position.Y, (int)Game.Player.Velocity.Y);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            Game.SpriteBatch.Begin();

            for (int y = 0; y < _grid.GetLength(0); y++)
            {
                for (int x = 0; x < _grid.GetLength(1); x++)
                {
                    Game.SpriteBatch.Draw(Game.WhitePixel, new Point(x, y).FromGrid().MakeTile().ToScreen(), _grid[y, x] == TileType.Wall ? Color.Black : Color.White);
                }
            }

            Game.SpriteBatch.End();
        }

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