using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DiamondHollow
{
    /// <summary>
    /// The textual representation of the game map. Level files use a grid of characters to represent the map.
    /// </summary>
    public enum TileType
    {
        /// <summary>A blank tile, just the background.</summary>
        Empty = '.',
        /// <summary>A solid tile, cannot be moved through.</summary>
        Wall = '#',
        /// <summary>Where the player starts.</summary>
        Spawnpoint = '*',
        /// <summary>A single diamond.</summary>
        SingleGem = '1',
        /// <summary>Two diamonds next to each other.</summary>
        DoubleGem = '2',
        /// <summary>Three diamonds above each other.</summary>
        TripleGem = '3',
        /// <summary>Four diamonds in a square.</summary>
        QuadrupleGem = '4',
        /// <summary>A slime spawn location.</summary>
        Slime = 's',
        /// <summary>An extra slime spawn location, used at higher difficulties.</summary>
        Slime2 = 'S',
        /// <summary>A wall shooter spawn location.</summary>
        WallShooter = 'w',
        /// <summary>An extra wall shooter spawn location, used at higher difficulties.</summary>
        WallShooter2 = 'W',
        /// <summary>A ceiling shooter spawn location.</summary>
        CeilingShooter = 'c',
        /// <summary>An extra ceiling shooter spawn location, used at higher difficulties.</summary>
        CeilingShooter2 = 'C',
        /// <summary>A spider spawn location.</summary>
        Spider = 'z',
        /// <summary>An extra spider spawn location, used at higher difficulties.</summary>
        Spider2 = 'Z',
        /// <summary>A bird spawn location.</summary>
        Bird = 'b',
        /// <summary>An extra bird spawn location, used at higher difficulties.</summary>
        Bird2 = 'B',
        /// <summary>A large heart.</summary>
        LargeHeart = '+',
        /// <summary>A nuke.</summary>
        Nuke = '@',
        /// <summary>An activatable checkpoint.</summary>
        Checkpoint = '$',
    }

    /// <summary>
    /// A class just for rendering the foreground overlay, seperated because each component can only draw at one z-index.
    /// </summary>
    /// <seealso cref="DiamondHollow.Level"/>
    class Foreground : DHGameComponent
    {
        /// <summary>
        /// Creates a new foreground overlay.
        /// </summary>
        /// <param name="game">The game this component is attached to.</param>
        /// <returns>The new foreground overlay.</returns>
        /// <seealso cref="DiamondHollow.DrawingLayers.Foreground"/>
        public Foreground(DiamondHollowGame game) : base(game, game.Level)
        {
            DrawOrder = (int)DrawingLayers.Foreground;  // Setting the z-index
        }

        // <inheritdoc cref="Microsoft.Xna.Framework.DrawableGameComponent.Draw"/>
        /// <summary>
        /// Draw the platforms. Actual data is stored in <see cref="DiamondHollow.Level"/>.
        /// </summary>
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
                        // Doesn't store any data of its own, instead references it from Level
                        Point fg = Level._platformTileCache[y, x];
                        Game.SpriteBatch.Draw(Level._platformTileset, tile, new Rectangle(fg.X * 32, fg.Y * 32, 32, 32), Color.White);
                    }
                }
            }

            Game.SpriteBatch.End();
        }
    }

    /// <summary>
    /// The actual game scene.
    /// 
    /// #### Platform textures
    /// 
    /// Platform (foreground) textures are handled in the following way:
    /// - Platform tiles have connected textures meaning their texture is selected based on the surrounding tiles.
    /// - There are 8 surrounding tiles and we only need to know if they are there or not, so we can encode this into an 8-bit integer.
    /// - For each combination of surrounding tiles, we want an offset within a tileset pointing to the correct texture (its actualy an array of offsets/textures).
    /// - Since all combinations of are possible, we can store this in an array mapping the encoded tile to a list of variations of its texture.
    /// 
    /// #### Background textures
    /// 
    /// If a checkpoint is activated, background tiles within a radius are illuminated.
    /// This is done by swapping some textures with ore in them for ones where the ore is shining and also by mixing the texture with white.
    /// </summary>
    public class Level : GameScene
    {
        /// <summary>
        /// The map of the level.
        /// </summary>
        private TileType[,] _grid;
        /// <summary>
        /// Handles extending the map upwards.
        /// </summary>
        private readonly LevelGenerator _levelGenerator;
        /// <summary>
        /// The mapping of tiles to lists of texture variations
        /// </summary>
        private Point[][] _platformTileMap;
        /// <summary>
        /// Texture offsets are cached to avoid having to recalculate them every frame and so they don't flicker.
        /// </summary>
        public Point[,] _platformTileCache;
        /// <summary>
        /// Texture offsets are cached to avoid having to recalculate them every frame and so they don't flicker.
        /// </summary>
        public Point[,] _backgroundCache;
        /// <summary>
        /// The tilesets containing the platform (foreground) textures.
        /// </summary>
        public Texture2D _platformTileset;
        /// <summary>
        /// The tileset containing the background textures.
        /// </summary>
        public Texture2D _backgroundTileset;
        /// <summary>
        /// A map of unlit background texture offsets to their lit counterparts, also doubles as a list of background textures.
        /// </summary>
        public Dictionary<Point, Point> _backgroundTileMap;
        /// <summary>
        /// Background textures are selected randomly from a weighted pool of textures.
        /// </summary>
        private int[] _backgroundTileMapWeights;

        /// <summary>
        /// A reference to the player.
        /// </summary>
        public Player Player;
        /// <summary>
        /// A reference to the camera.
        /// </summary>
        public Camera Camera;
        /// <summary>
        /// Handles spawning, updating and despawning of projectiles.
        /// </summary>
        public ProjectileController ProjectileController;
        /// <summary>
        /// Handles spawning, updating and despawning of projectiles.
        /// </summary>
        public ParticleController ParticleController;
        /// <summary>
        /// Handles spawning, updating and despawning of collectible items.
        /// </summary>
        public CollectiblesController CollectiblesController;
        /// <summary>
        /// Handles spawning, updating and despawning of enemies.
        /// </summary>
        public EnemyController EnemyController;

        /// <summary>
        /// Where the player will spawn after dying. Gets updated to the last activated checkpoint.
        /// </summary>
        public Point Spawnpoint;
        /// <summary>
        /// As the player progresses upwards, the difficulty increases. This increases the number of enemies, the number of collectibles and enemy difficulty.
        /// </summary>
        public float Difficulty => 1f + (float)(Camera?.CameraY ?? 0f) / Game.TileSize / 100f;
        /// <summary>
        /// Ranges from 0.5 to 2, is used to modify the toughness of enemies. It's a scaled value of <see cref="DiamondHollow.Level.Difficulty"/>.
        /// </summary>
        public float Modifier => Math.Clamp(Difficulty / 4f, 0.5f, 2f);

        /// <summary>
        /// Creates a new level.
        /// </summary>
        /// <param name="game">The game this component is attached to.</param>
        /// <param name="filename">The filename of the level to load.</param>
        /// <returns>The new level.</returns>
        public Level(DiamondHollowGame game, string filename) : base(game, null)
        {
            _levelGenerator = new LevelGenerator(game, this, filename);
            Spawnpoint = Point.Zero;    // Should be overriden by the LevelGenerator
        }

        // <inheritdoc cref="Microsoft.Xna.Framework.DrawableGameComponent.Initialize"/>
        /// <summary>
        /// - Allocates memory for the map.
        /// - Generates a tile to texture mapping for the platform (foreground) textures. See: <see cref="DiamondHollow.Level.GenerateTileMap"/>.
        /// - Load the first map segment, See: <see cref="DiamondHollow.Level.LoadNextMapSegment"/>.
        /// - Loads textures.
        /// </summary>
        protected override void LoadContent()
        {
            _grid = new TileType[0, 0];
            _platformTileCache = new Point[0, 0];
            _backgroundCache = new Point[0, 0];

            GenerateTileMap();      // Called only once, prepares the tile to texture mapping
            LoadNextMapSegment();   // Loads the first map segment

            _platformTileset = Game.GetTexture("Sprites/Environment/Tileset");
            _backgroundTileset = Game.GetTexture("Sprites/Environment/Background");

            base.LoadContent();
        }

        // <inheritdoc cref="Microsoft.Xna.Framework.DrawableGameComponent.Initialize"/>
        /// <summary>
        /// - Sets the draw order.
        /// - Creates the <see cref="DiamondHollow.Foreground"/>.
        /// - Creates the camera, player and controllers.
        /// - Sets the camera to follow the player.
        /// </summary>
        public override void Initialize()
        {
            DrawOrder = (int)DrawingLayers.Background;
            AddComponent(new Foreground(Game));

            Camera = new Camera(Game, this);
            Player = new Player(Game, this);
            ProjectileController = new ProjectileController(Game, this);
            ParticleController = new ParticleController(Game, this);
            CollectiblesController = new CollectiblesController(Game, this);
            EnemyController = new EnemyController(Game, this);

            AddComponents(Player, Camera);
            AddComponents(ParticleController, ProjectileController, CollectiblesController, EnemyController);

            Camera.Track(Player);   // Camera locks on to player

            base.Initialize();
        }

        // <inheritdoc cref="Microsoft.Xna.Framework.GameComponent.Update"/>
        /// <summary>
        /// If the player nears the end of the map, the next map segment is loaded.
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            // If the player nears the end of the map, load the next segment
            if (Player.Position.Y + 2 * Game.WindowHeight > MapHeight)
            {
                LoadNextMapSegment();
            }

            base.Update(gameTime);
        }

        // <inheritdoc cref="Microsoft.Xna.Framework.DrawableGameComponent.Draw"/>
        /// <summary>
        /// Clears the screen and draws the background.
        /// A lamppost is drawn at the spawnpoint and surrounding tiles are brightened.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Yellow);     // Clear color becomes the checkpoint light tint

            // No need to render tiles offscreen
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

                    // Render a lamppost as the checkpoint and brighten surrounding tiles
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

        /// <summary>
        /// Load the next map segment. The segment itself is loaded by the <see cref="DiamondHollow.LevelGenerator"/>.
        /// This function handles merging the new segment with the map and selecting the correct textures for platforms/background.
        /// </summary>
        private void LoadNextMapSegment()
        {
            _levelGenerator.LoadNext(ref _grid);
            int height = Math.Max(_platformTileCache.GetLength(0) - 1, 0);
            int height2 = _grid.GetLength(0);
            int width = _grid.GetLength(1);

            // Fast resize & copy
            Helpers.ResizeArray(ref _backgroundCache, height2, width);
            Helpers.ResizeArray(ref _platformTileCache, height2, width);

            // Generate background textures
            for (int y = height + 1; y < height2; y++)
                for (int x = 0; x < width; x++)
                    _backgroundCache[y, x] = Game.WeightedChoice(_backgroundTileMap.Keys, _backgroundTileMapWeights);

            // Generate platform textures
            for (int y = height; y < height2; y++)
                for (int x = 0; x < width; x++)
                    if (_grid[y, x] == TileType.Wall)
                        _platformTileCache[y, x] = Game.Choice(_platformTileMap[CollectSurroundingTiles(new Point(x, y))]);
        }

        /// <summary>
        /// Encodes the surrounding tiles into an 8-bit integer.
        /// 
        /// In the following diagram, the surrounding tiles are represented by the numbers:
        /// ```text
        /// 0 1 2
        /// 3 x 4
        /// 5 6 7
        /// ```
        /// The nth bit corresponds to the nth tile.
        /// 
        /// Tiles can have the following values:
        /// - <c>0</c> - empty
        /// - <c>1</c> - platform (wall)
        /// </summary>
        /// <param name="point">The tile to encode.</param>
        /// <returns>The encoded tile.</returns>
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

        /// <summary>
        /// A helper function to draw a colored rectangle.
        /// </summary>
        /// <param name="rect">The rectangle to draw.</param>
        /// <param name="color">The color of the rectangle.</param>
        public void DrawRectangle(Rectangle rect, Color color) => Game.SpriteBatch.Draw(Game.WhitePixel, rect.ToScreen(), color);
        /// <summary>
        /// A helper function to draw a styled line.
        /// </summary>
        /// <param name="start">The start point of the line.</param>
        /// <param name="end">The end point of the line.</param>
        /// <param name="color">The color of the line.</param>
        /// <param name="width">The width of the line.</param>
        public void DrawLine(Point start, Point end, Color color, int width) => Game.SpriteBatch.DrawLine(start.ToScreen().ToVector2(), end.ToScreen().ToVector2(), color, width);

        /// <summary>
        /// The height of the map, in pixels.
        /// </summary>
        public int MapHeight => _grid.GetLength(0) * Game.TileSize;
        /// <summary>
        /// The width of the map, in pixels.
        /// </summary>
        public int MapWidth => _grid.GetLength(1) * Game.TileSize;
        /// <summary>
        /// Get the tile at a position.
        /// </summary>
        /// <param name="x">The x-coordinate of the tile.</param>
        /// <param name="y">The y-coordinate of the tile.</param>
        /// <returns>The tile at the given position.</returns>
        public TileType GetTile(int x, int y) => _grid[y, x];

        /// <summary>
        /// Check if a position is outside the map or in a platform.
        /// </summary>
        /// <param name="p">The position to check.</param>
        /// <returns>True if the position is outside the map or in a platform.</returns>
        public bool IsWall(Point p)
        {
            var c = p.ToGrid();
            return c.X < 0 || c.X >= _grid.GetLength(1)
                || c.Y < 0 || c.Y >= _grid.GetLength(0)
                || _grid[c.Y, c.X] == TileType.Wall;
        }
        /// <summary>
        /// Check if a position is outside the map or in a platform.
        /// </summary>
        /// <param name="x">The x-coordinate of the position to check.</param>
        /// <param name="y">The y-coordinate of the position to check.</param>
        /// <returns>True if the position is outside the map or in a platform.</returns>
        /// <remarks>This is a convenience function for <see cref="IsWall(Point)"/>.</remarks>
        public bool IsWall(int x, int y) => IsWall(new Point(x, y));
        /// <summary>
        /// Check if a rectangle is directly above a platform.
        /// </summary>
        /// <param name="box">The rectangle to check.</param>
        /// <returns>True if the rectangle is directly above a platform.</returns>
        public bool IsOnGround(Rectangle box) => IsWall(box.Location.OffsetY(-1)) || IsWall(box.Location.Offset(box.Width - 1, -1));

        /// The following set of functions define the tile to texture mapping

        /// <summary>
        /// Expands occurences of <c>'.'</c> to <c>'x'</c> and <c>' '</c>
        /// 
        /// eg. <c>".x."</c> -> <c>{ "xxx", "xx ", " xx", " x " }</c>
        /// </summary>
        /// <param name="repr">The string to expand.</param>
        /// <returns>An enumerable of the expanded strings.</returns>
        private IEnumerable<string> GeneratePermutations(string repr)
        {
            int i = repr.IndexOf('.');
            if (i == -1) return new[] { repr };
            return GeneratePermutations(repr[0..i] + "x" + repr[(i + 1)..]).Concat(GeneratePermutations(repr[0..i] + " " + repr[(i + 1)..]));
        }
        /// <summary>
        /// Registers a set of texture variations for a set of tile variations.
        /// </summary>
        /// <param name="repr">The string representation of the tile variations.</param>
        /// <param name="c">A list of texture offset for the tile representation.</param>
        private void AddTileVariant(string repr, params (int x, int y)[] c)
        {
            foreach (var k in GeneratePermutations(repr.Replace("|", "")))
            {
                int i = Convert.ToInt32(k.Remove(4, 1).Replace('x', '1').Replace(' ', '0'), 2);
                _platformTileMap[i] = c.Select(p => new Point(p.x, p.y)).ToArray();
            }
        }

        /// <summary>
        /// Defines a list of tile representations and their corresponding texture offsets.
        /// 
        /// For ease of use, the surrounding tiles are written as a string, so the following layout:
        /// ```text
        /// 0 1 2
        /// 3 x 4
        /// 5 6 7
        /// ```
        /// Would be written as: <c>"012|3x4|567"</c>.
        /// 
        /// Positions can have the following values:
        /// - <c>'x'</c> - tile
        /// - <c>' '</c> - empty
        /// - <c>'.'</c> - any (will be expanded to <c>'x'</c> and <c>' '</c>)
        /// </summary>
        private void GenerateTileMap()
        {
            // Mapping of background unlit textures to lit textures
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