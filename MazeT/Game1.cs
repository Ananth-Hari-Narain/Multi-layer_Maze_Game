using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        private Vector2 prevMazePos = new Vector2();       

        private Texture2D wall;
        private Texture2D TPpad;
        private Texture2D minimap_bg;
        private Texture2D player_icon;
        private List<CollisionCharacter>[] enemies;
        private Maze maze;
        private Player player;
        private SpriteFont testFont;
        private List<Point> testpath;
        private Point test_point = new(0, 0);
        private Point final_test_point = new(3, 4);

        private KeyboardState previousState;
        public Game1()
        {            
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = screen_width; 
            _graphics.PreferredBackBufferHeight = screen_height;
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
            player = new Player(44, 56, 0, 0, maze.collision_rects);
            testpath = maze.GenerateSingleLayerPath(new Point(5, 2), 8, 0);

            //Initialise enemy list
            enemies = new List<CollisionCharacter>[maze.max_layers];
            for (int i = 0; i < maze.max_layers; i++)
            {
                enemies[i] = new List<CollisionCharacter>();
            }

            //Generate enemies randomly but evenly across maze.
            enemies[0].Add(new BlindEnemy(2, testpath));
            enemies[0].Add(new SmartEnemy(2, new Vector2(170, 170), ref maze));            
            base.Initialize();            
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here            
            maze.maze_wall_H = Content.Load<Texture2D>("temp_wallH");
            maze.maze_wall_V = Content.Load<Texture2D>("temp_wallV");
            maze.maze_floor = Content.Load<Texture2D>("temp_floor");
            maze.tp_pad_design = Content.Load<Texture2D>("temp_pad");

            wall = new Texture2D(GraphicsDevice, 1, 1);
            wall.SetData(new Color[] { Color.Black });

            TPpad = new Texture2D(GraphicsDevice, 1, 1);
            TPpad.SetData(new Color[] { Color.DarkGreen });

            minimap_bg = new Texture2D(GraphicsDevice, 1, 1);
            minimap_bg.SetData(new Color[] { Color.White });
            player_icon = Content.Load<Texture2D>("dwarf_icon_small");
            
            player.walk[0].sprite_sheet = Content.Load<Texture2D>("dwarf_run"); 
            for (int i = 1; i < 4; i++)
            {
                player.walk[i].sprite_sheet = player.walk[0].sprite_sheet;
            }

            BlindEnemy.run[0].sprite_sheet = Content.Load<Texture2D>("ogre_run");
            BlindEnemy.run[1].sprite_sheet = BlindEnemy.run[0].sprite_sheet;

            SmartEnemy.run[0].sprite_sheet = Content.Load<Texture2D>("skeleton_run");
            SmartEnemy.run[1].sprite_sheet = SmartEnemy.run[0].sprite_sheet;

            testFont = Content.Load<SpriteFont>("testFont");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            KeyboardState currentKeys = Keyboard.GetState();
            if (currentKeys.IsKeyDown(Keys.P) && !previousState.IsKeyDown(Keys.P))
            {
                mazeTest = !mazeTest;
            }
            if (currentKeys.IsKeyDown(Keys.L) && !previousState.IsKeyDown(Keys.L))
            {
                test_point = maze.SingleLayerNextTileFinder(test_point, final_test_point);
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

            test_point.X = (int)(player.global_position.X + 98) / 128;
            test_point.Y = (int)(player.global_position.Y + 80) / 128;            
            
            player.Update(currentKeys, previousState, maze.pos, maze.current_layer, gameTime.ElapsedGameTime.Milliseconds);
            
            if (currentKeys.IsKeyDown(Keys.Q) && !previousState.IsKeyDown(Keys.Q))
            {
                HandleIfPlayerIsOnTeleportationPads();
            }
            HandleSideScrolling();
            HandleEnemyLogic(gameTime); //Handles enemy code, including damage detection

            maze.UpdateMazeRects(prevMazePos - maze.pos);

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
                maze.DisplayMaze(_spriteBatch);
            }
            else{
                maze.DisplayRects(_spriteBatch, wall);
            }
            //_spriteBatch.Draw(TPpad, player.collision_rect, Color.White);
            //_spriteBatch.DrawString(testFont,
            //    $"\nplayer pos: {player.global_position}" +
            //    $"\nplayer tile: {(int)(player.global_position.X + 98) / 128}, {(int)(player.global_position.Y + 80) / 128}" +
            //    $"\nenemy pos: {enemies[0][1].global_position}" +
            //    $"\nenemy tile: {new Point((int)enemies[0][1].global_position.X / 128, (int)enemies[0][1].global_position.Y / 128)}",
            //    new Vector2(0, 600), Color.White);
            //maze.DrawPath(testpath, _spriteBatch,TPpad);
            _spriteBatch.Draw(TPpad, player.sword_hitbox, Color.White);
            player.Display(_spriteBatch);            
            foreach (var enemy in enemies[maze.current_layer])
            {                
                if (enemy.health > 0)
                {                    
                    enemy.Display(_spriteBatch);
                }                
            }
            maze.DisplayTopCornerMinimapImage(_spriteBatch, minimap_bg ,wall, TPpad, player_icon, player.global_position);            
            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private void HandleSideScrolling()
        {
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

            player.UpdateLocalPosition(maze.pos);
        }

        private void HandleIfPlayerIsOnTeleportationPads()
        {
            foreach (var rect in maze.TP_Pads[maze.current_layer])
            {
                if (player.collision_rect.Intersects(rect))
                {
                    maze.current_layer = (maze.current_layer + 1) % maze.max_layers;
                }
            }
        }

        private void HandleEnemyLogic(GameTime gameTime)
        {
            foreach (var enemy in enemies[maze.current_layer])
            {       
                if (enemy.health > 0)
                {
                    if (enemy.GetType() == typeof(SmartEnemy))
                    {
                        enemy.Update(gameTime.ElapsedGameTime.Milliseconds, maze.current_layer, player.global_position);
                    }
                    else
                    {
                        enemy.Update(gameTime.ElapsedGameTime.Milliseconds, maze.current_layer);
                    }                    
                    enemy.UpdateLocalPosition(maze.pos);

                    if (player.sword_hitbox.Intersects(enemy.collision_rect))
                    {
                        enemy.TakeDamage(player.power, player.sword_hitbox.Center);
                    }
                    //If the player succesfully hit the enemy, the player should not take damage
                    // from that enemy
                    else if (player.collision_rect.Intersects(enemy.collision_rect))
                    {
                        player.TakeDamage(enemy.power);
                    }
                }
            }
        }
    }
}
