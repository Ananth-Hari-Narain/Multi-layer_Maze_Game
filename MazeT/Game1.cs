using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;
using System.Net.Mime;

namespace MazeT
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private const int screen_width = 800;
        private const int screen_height = 800;

        private Texture2D wall;
        private Texture2D TPpad;
        private Maze maze;
        private Player player;

        private KeyboardState previousState;
        public Game1()
        {            
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = screen_width; // Set your desired maze.width
            _graphics.PreferredBackBufferHeight = screen_height; // Set your desired maze.height
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

            player = new Player(48, 48, 400, 400);
            player.walk[0].sprite_sheet = Content.Load<Texture2D>("dwarf_run");

            for (int i = 1; i < 4; i++)
            {
                player.walk[i].sprite_sheet = player.walk[0].sprite_sheet;
            }            
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            KeyboardState currentKeys = Keyboard.GetState();
            if (currentKeys.IsKeyDown(Keys.Space) && !previousState.IsKeyDown(Keys.Space))
            {
                
            }            
            player.Update(currentKeys, previousState, gameTime.ElapsedGameTime.TotalMilliseconds);
            if (player.collision_rect.X < screen_width/2 + 30 && player.collision_rect.X > screen_width / 2 - 30)
            {
                maze.pos.X += player.velocity.X;
            }
            if (player.collision_rect.Y < screen_height / 2 + 10 && player.collision_rect.Y > screen_height / 2 - 50)
            {
                maze.pos.Y += player.velocity.Y;
            }
            
            if (maze.pos.X < 0)
            {
                maze.pos.X = 0;
            }
            else if (maze.pos.X > maze.xmax - screen_width)
            {
                maze.pos.X = maze.xmax - screen_width;
            }

            if (maze.pos.Y < 0)
            {
                maze.pos.Y = 0;
            }
            else if (maze.pos.Y > maze.ymax - screen_height)
            {
                maze.pos.Y = maze.ymax - screen_height;
            }

            player.collision_rect.X = (int) (player.position.X - maze.pos.X);
            player.collision_rect.Y = (int)(player.position.Y - maze.pos.Y);

            previousState = Keyboard.GetState();
            
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.LightGray);
            // TODO: Add your drawing code here
            _spriteBatch.Begin();
            maze.displayMaze(_spriteBatch, wall, TPpad);
            player.Display(_spriteBatch);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
