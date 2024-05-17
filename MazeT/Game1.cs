using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Net.Mime;

namespace MazeT
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Texture2D wall;
        private Texture2D TPpad;
        private Maze maze;

        private KeyboardState previousState;
        public Game1()
        {
            
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = 1280; // Set your desired maze.width
            _graphics.PreferredBackBufferHeight = 1280; // Set your desired maze.height
            _graphics.ApplyChanges();
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            
        }
        
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
            maze = new Maze(12, 12, 2);
            previousState = Keyboard.GetState();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            wall = new Texture2D(GraphicsDevice, 1, 1);
            wall.SetData(new Color[] { Color.White });

            TPpad = new Texture2D(GraphicsDevice, 1, 1);
            TPpad.SetData(new Color[] { Color.Purple });
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            KeyboardState currentKeys = Keyboard.GetState();
            if (currentKeys.IsKeyDown(Keys.Space) && !previousState.IsKeyDown(Keys.Space))
            {
                maze.currentLayer = (maze.currentLayer + 1) % maze.maxLayers;
            }

            previousState = Keyboard.GetState();
            
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            // TODO: Add your drawing code here
            _spriteBatch.Begin();
            maze.displayMaze(_spriteBatch, wall, TPpad);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
