using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
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
        private List<CollisionCharacter>[] enemies;
        private Maze maze;
        private Player player;
        private SpriteFont testFont;
        private List<Point> testpath;

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
            previousState = Keyboard.GetState();            
            maze = new Maze(15, 15, 2);
            player = new Player(44, 56, -27, -86, maze.collisionRects);
            testpath = maze.GenerateSingleLayerPath(new Point(5, 2), 8, 0);

            //Initialise enemy list
            enemies = new List<CollisionCharacter>[maze.maxLayers];
            for (int i = 0; i < maze.maxLayers; i++)
            {
                enemies[i] = new List<CollisionCharacter>();
            }

            enemies[0].Add(new BlindEnemy(0, testpath));

            base.Initialize();            
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here            
            maze.mazeWallH = Content.Load<Texture2D>("temp_wallH");
            maze.mazeWallV = Content.Load<Texture2D>("temp_wallV");
            maze.mazeFloor = Content.Load<Texture2D>("temp_floor");
            maze.staircase = Content.Load<Texture2D>("temp_pad");

            wall = new Texture2D(GraphicsDevice, 1, 1);
            wall.SetData(new Color[] { Color.Black });

            TPpad = new Texture2D(GraphicsDevice, 1, 1);
            TPpad.SetData(new Color[] { Color.Purple });

            
            player.walk[0].sprite_sheet = Content.Load<Texture2D>("dwarf_run"); 
            for (int i = 1; i < 4; i++)
            {
                player.walk[i].sprite_sheet = player.walk[0].sprite_sheet;
            }

            BlindEnemy.run[0].sprite_sheet = Content.Load<Texture2D>("ogre_run");
            BlindEnemy.run[1].sprite_sheet = BlindEnemy.run[0].sprite_sheet;

            testFont = Content.Load<SpriteFont>("testFont");
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
            else if (currentKeys.IsKeyDown(Keys.K))
            {
                testpath = maze.GenerateSingleLayerPath(new Point(5, 2), 10, 0);
            }
            
            player.Update(currentKeys, previousState, maze.pos, maze.currentLayer, gameTime.ElapsedGameTime.Milliseconds);  
            foreach (var enemy in enemies[maze.currentLayer])
            {
                enemy.Update(gameTime.ElapsedGameTime.Milliseconds, maze.currentLayer);
            }
                      
            
            foreach (var rect in maze.TP_Pads[maze.currentLayer])
            {
                if (player.collision_rect.Intersects(rect) && currentKeys.IsKeyDown(Keys.Q) && !previousState.IsKeyDown(Keys.Q))
                {
                    maze.currentLayer = (maze.currentLayer + 1) % maze.maxLayers;
                }
            }
            
            
            //Side scrolling code//
            if (player.local_position.X <= screen_width / 2 + 30 && player.local_position.X >= screen_width / 2 - 30)
            {
                maze.pos.X += player.global_position.X - player.old_global_position.X;
            }
            if (player.local_position.Y <= screen_height / 2 + 30 && player.local_position.Y >= screen_height / 2 - 30)
            {
                maze.pos.Y += player.global_position.Y - player.old_global_position.Y;
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

            player.UpdateLocalPosition(maze.pos, 32, 32);
            player.UpdateRectanglePosition(maze.pos);
            foreach (var enemy in enemies[maze.currentLayer])
            {
                enemy.Update(gameTime.ElapsedGameTime.Milliseconds, maze.currentLayer);
                enemy.UpdateLocalPosition(maze.pos);
                enemy.UpdateRectanglePosition(maze.pos);
            }

            foreach (var enemy in enemies[maze.currentLayer])
            {
                if (player.collision_rect.Intersects(enemy.collision_rect))
                {
                    //Do player getting hit code. Include stuff like iframes, game pauses, damage taken
                    player.TakeDamage();
                }
            }

            maze.UpdateMazeRects(prevMazePos - maze.pos);
            test = maze.DrawPath(testpath);

            previousState = Keyboard.GetState();
            prevMazePos = new Vector2(maze.pos.X, maze.pos.Y);
            player.old_global_position = player.global_position;
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
            _spriteBatch.DrawString(testFont, $"player HP: {player.health}", new Vector2(0, 700), Color.White);
            //maze.DrawPath(testpath, _spriteBatch,TPpad);
            
            player.Display(_spriteBatch);            
            foreach (var enemy in enemies[maze.currentLayer])
            {                
                _spriteBatch.Draw(TPpad, enemy.collision_rect, Color.White);
                enemy.Display(_spriteBatch);
            }
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
