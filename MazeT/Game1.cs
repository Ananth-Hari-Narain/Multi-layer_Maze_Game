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
        private bool mazeTest = true; //testing code
        private string test = "";
        private Vector2 prevMazePos = new Vector2();

        private Texture2D wall;
        private Texture2D TPpad;
        private Maze maze;
        private Player player;
        private SpriteFont testFont;

        private KeyboardState previousState;
        public Game1()
        {            
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = screen_width; // Set your desired maze.width
            _graphics.PreferredBackBufferHeight = screen_height; // Set your desired maze.height
            _graphics.ApplyChanges();
            this.IsFixedTimeStep = true;
            
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            
        }
        
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();            
            previousState = Keyboard.GetState();
            
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            maze = new Maze(20, 20, 2);
            maze.mazeWallH = Content.Load<Texture2D>("temp_wallH");
            maze.mazeWallV = Content.Load<Texture2D>("temp_wallV");
            maze.mazeFloor = Content.Load<Texture2D>("temp_floor");

            wall = new Texture2D(GraphicsDevice, 1, 1);
            wall.SetData(new Color[] { Color.Black });

            TPpad = new Texture2D(GraphicsDevice, 1, 1);
            TPpad.SetData(new Color[] { Color.Purple });

            player = new Player(48, 8, -27, -86);
            player.walk[0].sprite_sheet = Content.Load<Texture2D>("dwarf_run");

            testFont = Content.Load<SpriteFont>("testFont");

            for (int i = 1; i < 4; i++)
            {
                player.walk[i].sprite_sheet = player.walk[0].sprite_sheet;
            }            
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            KeyboardState currentKeys = Keyboard.GetState();
            if (currentKeys.IsKeyDown(Keys.Space) && !previousState.IsKeyDown(Keys.Space))
            {
                mazeTest = !mazeTest;
            }
            if (currentKeys.IsKeyDown(Keys.M))
            {
                this.TargetElapsedTime = TimeSpan.FromSeconds(1d / 2d);
            }
            else if (currentKeys.IsKeyDown(Keys.N))
            {
                this.TargetElapsedTime = TimeSpan.FromSeconds(1d / 60d);
            }
            
            player.Update(currentKeys, previousState, maze.pos, maze.collisionRects[maze.currentLayer], gameTime.ElapsedGameTime.TotalMilliseconds);
            
            //Side scrolling code//
            if (player.localPosition.X <= screen_width / 2 + 30 && player.localPosition.X >= screen_width / 2 - 30)
            {
                maze.pos.X += player.globalPosition.X - player.old_global_position.X;
            }
            if (player.localPosition.Y <= screen_height / 2 + 30 && player.localPosition.Y >= screen_height / 2 - 30)
            {
                maze.pos.Y += player.globalPosition.Y - player.old_global_position.Y;
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
            else if (maze.pos.Y - 64 > maze.ymax - screen_height)
            {
                maze.pos.Y = maze.ymax - screen_height + 64;
            }

            player.localPosition.X = (int) (player.globalPosition.X - maze.pos.X + 32);
            player.localPosition.Y = (int)(player.globalPosition.Y - maze.pos.Y + 32);
            player.RefreshRectanglePosition(maze.pos);
            maze.UpdateCollisionRects(prevMazePos - maze.pos);

            previousState = Keyboard.GetState();
            prevMazePos = new Vector2(maze.pos.X, maze.pos.Y);
            player.old_collision_pos = player.collision_rect.Location;
            player.old_global_position = player.globalPosition;
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.LightGray);
            // TODO: Add your drawing code here
            _spriteBatch.Begin();
            if (mazeTest)
            {
                maze.displayMaze(_spriteBatch);
            }
            else{
                maze.displayRects(_spriteBatch, wall);
            }
            //_spriteBatch.Draw(TPpad, player.collision_rect, Color.White);
            _spriteBatch.DrawString(testFont, $"coll_rect_pos {player.collision_rect.Location}\n mazePos= {maze.pos}\n{test}", new Vector2(0, 700), Color.White);
            player.Display(_spriteBatch);
            
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
