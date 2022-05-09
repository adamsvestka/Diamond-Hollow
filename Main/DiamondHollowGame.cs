using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DiamondHollow
{
    public class DiamondHollowGame : Game
    {
        public GraphicsDeviceManager Graphics;
        public SpriteBatch SpriteBatch;

        public Texture2D WhitePixel;
        public SpriteFont Menlo;
        public int TileSize = 50;

        public Player Player;
        public Level Level;
        public Camera Camera;
        public ParticleController ParticleController;

        private KeyboardState _previousKeyboardState;
        private MouseState _previousMouseState;
        public KeyboardState KeyboardState;
        public MouseState MouseState;

        public DiamondHollowGame()
        {
            Graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            _previousKeyboardState = Keyboard.GetState();
            _previousMouseState = Mouse.GetState();
        }

        protected override void Initialize()
        {
            Graphics.PreferredBackBufferWidth = 1400;
            Graphics.PreferredBackBufferHeight = 800;
            Graphics.IsFullScreen = false;
            Graphics.ApplyChanges();

            CoordinateExtensions.Initialize(this, new Point(TileSize, TileSize));

            Level = new Level(this, "Levels/Level2.txt");
            Player = new Player(this);
            Camera = new Camera(GraphicsDevice, Level);
            ParticleController = new ParticleController(this);

            Components.Add(Level);
            Components.Add(Player);
            Components.Add(ParticleController);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            SpriteBatch = new SpriteBatch(GraphicsDevice);

            WhitePixel = new Texture2D(GraphicsDevice, 1, 1);
            WhitePixel.SetData(new[] { Color.White });

            Menlo = Content.Load<SpriteFont>("Fonts/Menlo");
        }

        protected override void UnloadContent()
        {
            base.UnloadContent();

            SpriteBatch.Dispose();
            WhitePixel.Dispose();
        }

        public bool KeyPressed(Keys key) => KeyboardState.IsKeyDown(key) && _previousKeyboardState.IsKeyUp(key);
        public bool KeyReleased(Keys key) => KeyboardState.IsKeyUp(key) && _previousKeyboardState.IsKeyDown(key);
        public bool ButtonPressed(MouseButton button) => MouseState.IsButtonDown(button) && _previousMouseState.IsButtonUp(button);

        protected override void Update(GameTime gameTime)
        {
            _previousKeyboardState = KeyboardState;
            _previousMouseState = MouseState;
            KeyboardState = Keyboard.GetState();
            MouseState = Mouse.GetState();

            if (KeyboardState.IsKeyDown(Keys.Escape)) Exit();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            base.Draw(gameTime);
        }
    }
}
