using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Mime;
using System.Threading;

namespace MazeT
{
    public class Game1 : Game
    {
        private static Random rng = new(); //Generate one randomiser to make numbers generated more random.
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private const int screen_width = 800;
        private const int screen_height = 800;
        private bool mazeTest = true; //testing code
        private Vector2 prevMazePos = new Vector2();       

        //These textures are used for display the minimap
        private Texture2D wall;
        private Texture2D TPpad;
        private Texture2D minimap_bg;
        private Texture2D player_icon;
        private Texture2D end_goal;

        //UI images
        private Texture2D heart_container_empty;
        private Texture2D heart_container_full;

        //These are our entities and static items
        private List<CollisionCharacter>[] enemies;
        private List<Collectible>[] collectibles;
        private Maze maze;
        private Player player;

        //These are the sprite sheets used for our enemies
        Texture2D blind_enemy_run_sheet;
        Texture2D smart_enemy_run_sheet;

        //Testing code
        private SpriteFont testFont;
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
            List<Point> testpath2 = maze.GenerateSingleLayerPath(new Point(3, 2), 8, 0); ;

            //Initialise enemy list
            enemies = new List<CollisionCharacter>[maze.max_layers];
            collectibles = new List<Collectible>[maze.max_layers];
            for (int i = 0; i < maze.max_layers; i++)
            {
                enemies[i] = new List<CollisionCharacter>();
            }

            //Generate enemies randomly but evenly across maze.
            GenerateEnemies(50);
            GenerateCollectibles(8, 3);
            base.Initialize();            
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
      
            maze.maze_wall_H = Content.Load<Texture2D>("temp_wallH");
            maze.maze_wall_V = Content.Load<Texture2D>("temp_wallV");
            maze.maze_floor = Content.Load<Texture2D>("temp_floor");
            maze.tp_pad_design = Content.Load<Texture2D>("temp_pad");
            maze.end_tp_pad_design = Content.Load<Texture2D>("endTPpad");

            wall = new Texture2D(GraphicsDevice, 1, 1);
            wall.SetData(new Color[] { Color.Black });

            TPpad = new Texture2D(GraphicsDevice, 1, 1);
            TPpad.SetData(new Color[] { Color.DarkGreen });

            minimap_bg = new Texture2D(GraphicsDevice, 1, 1);
            minimap_bg.SetData(new Color[] { Color.White });

            end_goal = new Texture2D(GraphicsDevice, 1, 1);
            end_goal.SetData(new Color[] { Color.MonoGameOrange });

            player_icon = Content.Load<Texture2D>("dwarf_icon_small");
            heart_container_empty = Content.Load<Texture2D>("ui_heart_empty");
            heart_container_full = Content.Load<Texture2D>("ui_heart_full");

            player.walk[0].sprite_sheet = Content.Load<Texture2D>("dwarf_run"); 
            for (int i = 1; i < 4; i++)
            {
                player.walk[i].sprite_sheet = player.walk[0].sprite_sheet;
            }
            player.sword_swing = Content.Load<Texture2D>("sword_swing");

            blind_enemy_run_sheet = Content.Load<Texture2D>("ogre_run");
            smart_enemy_run_sheet = Content.Load<Texture2D>("skeleton_run");

            
            for (int i = 0; i < maze.max_layers; i++)
            {
                //Now initialise the run AnimatedSpriteSheet for each enemy
                foreach (var enemy in enemies[i])
                {
                    if (enemy is SmartEnemy smartEnemy)
                    {
                        smartEnemy.run[0].sprite_sheet = smart_enemy_run_sheet;
                        smartEnemy.run[1].sprite_sheet = smart_enemy_run_sheet;
                    }
                    else if (enemy is BlindEnemy blind)
                    {
                        blind.run[0].sprite_sheet = blind_enemy_run_sheet;
                        blind.run[1].sprite_sheet = blind_enemy_run_sheet;
                    }
                }

                //Initialise the images for the collectibles
                foreach (Collectible collectible in collectibles[i])
                {
                    collectible.image = Content.Load<Texture2D>("test_potion");
                    collectible.SetRect(collectible.global_position.ToPoint());
                }
            }

            

            testFont = Content.Load<SpriteFont>("testFont");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            KeyboardState currentKeys = Keyboard.GetState();
            //Testing code
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

            test_point.X = (int)(player.global_position.X + 98) / 128;
            test_point.Y = (int)(player.global_position.Y + 80) / 128;     
            //End of testing code
            
            //Main game loop
            player.Update(currentKeys, previousState, maze.pos, maze.current_layer, gameTime.ElapsedGameTime.Milliseconds);
            
            if (currentKeys.IsKeyDown(Keys.Q) && !previousState.IsKeyDown(Keys.Q))
            {
                HandleIfPlayerIsOnTeleportationPads();
            }
            HandleSideScrolling();
            HandleEnemyLogic(gameTime); //Handles enemy code, including damage detection
            HandleCollectibles(); //Deal with picking up and updating collectibles
            

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
            _spriteBatch.DrawString(testFont,
                $"\nplayer health: {player.health}",
                new Vector2(0, 600), Color.White);
            //maze.DrawPath(testpath, _spriteBatch,TPpad);
            player.Display(_spriteBatch);            
            foreach (var enemy in enemies[maze.current_layer])
            {                
                if (enemy.health > 0)
                {                    
                    enemy.Display(_spriteBatch);
                }                
            }

            foreach (Collectible collectible in collectibles[maze.current_layer])
            {
                collectible.Display(_spriteBatch);
            }

            maze.DisplayTopCornerMinimapImage(_spriteBatch, minimap_bg ,wall, TPpad, player_icon, player.global_position, end_goal);
            DrawHealthBar();
            _spriteBatch.End();

            base.Draw(gameTime);
        }

        /// <summary>
        /// The goal of this function is to generate enemies randomly but
        /// overall uniformly across the maze. We want to ensure that enemies 
        /// cannot spawn on the same tile, as enemies stacked on top of each
        /// other may confuse the user.
        /// </summary>

        private void GenerateEnemies(int noEnemiesPerLayer)
        {
            //Determine the size of each "chunk". Every chunk is a square region
            //which will have one enemy exactly.
            double chunk_area = maze.width * maze.height / (double)noEnemiesPerLayer;
            //We use the ceiling here to ensure all parts of the maze are covered.
            int chunk_width = (int) Math.Ceiling(Math.Sqrt(chunk_area));
            //Each layer will be done separately
            for (int z = 0; z < maze.max_layers; z++)
            {
                //Iterate through each chunk
                for (int x = 0; x < maze.width; x+=chunk_width)
                {
                    for (int y = 0; y < maze.height; y+=chunk_width)
                    {
                        //Determine a random tile in the chunk that is not the starting tile
                        //The player will always spawn at tile (0, 0) so as long as we only consider
                        //tiles with an x > 0, we are fine.
                        int enemyX = rng.Next(Math.Max(x, 1), Math.Min(maze.width, x + chunk_width));
                        int enemyY = rng.Next(y, Math.Min(maze.height, y + chunk_width));                        

                        //Determine which enemy to choose based on settings
                        //For now, just do random between blind enemy and actual enemy
                        int random_number = rng.Next(0,2);
                        if (random_number == 0)
                        {
                            //Spawn a blind enemy
                            enemies[z].Add(new BlindEnemy(1, maze.GenerateSingleLayerPath(new Point(enemyX, enemyY), 8, z)));
                        }
                        else
                        {
                            //Spawn a smart enemy
                            //Convert the random point into coordinates
                            Vector2 enemy_location = new(enemyX * 128 + 30, enemyY * 128 + 30);                            
                            enemies[z].Add(new SmartEnemy(1, enemy_location, ref maze));
                        }

                    }
                }
            }
        }

        /// <summary>
        /// Generates collectibles across maze.
        /// </summary>
        private void GenerateCollectibles(int num_powerups_per_layer, int num_standard_collectibles_per_layer)
        {
            double chunk_area = maze.width * maze.height / (double) (num_powerups_per_layer + num_standard_collectibles_per_layer);
            //We use the ceiling here to ensure all parts of the maze are covered.
            int chunk_width = (int)Math.Ceiling(Math.Sqrt(chunk_area));
            //Initialise collectibles[] array
            for (int i = 0; i < collectibles.Length; i++)
            {
                collectibles[i] = new();
            }

            //Actual code
            //Place power ups in random dead ends across both floors
            for (int z = 0; z < maze.max_layers; z++)
            {
                int num_standard_collectibles_in_this_layer = 0;
                HashSet<Point> dead_ends = maze.GetDeadEnds(z);
                //Iterate through each chunk. Choose only the dead ends (if there are dead ends in the chunk)
                for (int x = 0; x < maze.width; x += chunk_width)
                {
                    for (int y = 0; y < maze.height; y += chunk_width)
                    {
                        //Go through each tile in the chunk to see if it is maze
                        List<Point> dead_ends_in_chunk = new();
                        for (int x_tile = x; x_tile < x+chunk_width && x_tile < maze.width; x_tile++)
                        {
                            for (int y_tile = y; y_tile < y+chunk_width && y_tile < maze.height; y_tile++)
                            {
                                Point current_tile = new Point(x_tile, y_tile);
                                if (dead_ends.Contains(current_tile))
                                {
                                    dead_ends_in_chunk.Add(current_tile);
                                }
                            }
                        }

                        if (dead_ends_in_chunk.Count > 0)
                        {
                            //Add one random collectible in
                            Point chosen_point = dead_ends_in_chunk[rng.Next(dead_ends_in_chunk.Count)];
                            //Covert point into global coordinates
                            chosen_point.X = chosen_point.X * 128 + 32;
                            chosen_point.Y = chosen_point.Y * 128 + 32;

                            //Determine the type and value of the collectible. We can have standard collectibles now
                            //provided we have not generated all of them yet
                            CollectibleType type;
                            if (num_standard_collectibles_per_layer > num_standard_collectibles_in_this_layer)
                            {
                                type = (CollectibleType)rng.Next(0, 6);
                            }
                            else
                            {
                                type = (CollectibleType)rng.Next(1, 6);
                            }
                             
                            double value = 0;
                            //Determine a value for them
                            if (type == CollectibleType.STANDARD)
                            {
                                value = 1;//Doesn't matter for standard collectibles
                            }
                            else if (type == CollectibleType.HEAL)
                            {
                                //Heal anywhere from one to three hearts
                                value = rng.Next(1, 4);
                            }
                            else if (type == CollectibleType.DAMAGEUP)
                            {
                                //This is effectively a new weapon.
                                //This needs to be one point stronger than the user's current weapon
                                value = player.power + 1;
                            }
                            else if (type == CollectibleType.ATTACKSPEEDUP)
                            {
                                //The time between attacks should be between 100 and 180ms
                                value = rng.Next(100, 181);
                            }
                            else if (type == CollectibleType.SPEEDUP)
                            {
                                //The speed should only be increased to 3 or 4 (otherwise you go too fast)
                                value = rng.Next(3, 5);
                            }
                            else if (type == CollectibleType.SWORDRANGEUP)
                            {
                                //This is like getting a longer weapon
                                //This should only increase by a small amount from the player's current range
                                //The increase in range should go from 0.1 to 0.25 
                                value = player.sword_range + rng.Next(110, 250) / 1000.0;
                            }

                            collectibles[z].Add(new Collectible(type, value, chosen_point));
                        }
                        //If there are no dead ends
                        else
                        {
                            //Add a powerup at a random point in the chunk
                            int powerupX = rng.Next(Math.Max(x, 1), Math.Min(maze.width, x + chunk_width));
                            int powerupY = rng.Next(y, Math.Min(maze.height, y + chunk_width));

                            //Convert the tile position into coordinates
                            powerupX = powerupX * 128 + 32;
                            powerupY = powerupY * 128 + 32;

                            //Determine the power up type (there are 5 different types which aren't
                            //standard types.
                            CollectibleType type = (CollectibleType)rng.Next(1, 6);
                            double value = 0;
                            //Determine a value for them
                            if (type == CollectibleType.HEAL)
                            {
                                //Heal anywhere from one to three hearts
                                value = rng.Next(1, 4);
                            }
                            else if (type == CollectibleType.DAMAGEUP)
                            {
                                //This is effectively a new weapon.
                                //This needs to be one point stronger than the user's current weapon
                                value = player.power + 1;
                            }
                            else if (type == CollectibleType.ATTACKSPEEDUP)
                            {
                                //The time between attacks should be between 100 and 180ms
                                value = rng.Next(100, 181);
                            }
                            else if (type == CollectibleType.SPEEDUP)
                            {
                                //The speed should only be increased to 3 or 4 (otherwise you go too fast)
                                value = rng.Next(3, 5);
                            }
                            else if (type == CollectibleType.SWORDRANGEUP)
                            {
                                //This is like getting a longer weapon
                                //This should only increase by a small amount from the player's current range
                                //The increase in range should go from 0.1 to 0.25 
                                value = player.sword_range + rng.Next(110, 250) / 1000.0;
                            }

                            collectibles[z].Add(new Collectible(type, value, new Point(powerupX, powerupY)));
                        }
                    }
                }
            }             

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

        private void HandleCollectibles()
        {
            //Iterate backwards through list to remove items
            for (int i = 0; i < collectibles[maze.current_layer].Count; i++)
            {
                //Update local position of collectible
                collectibles[maze.current_layer][i].UpdateRectanglePosition(maze.pos);

                //If the collectible intersects with the player, collect the collectible
                if (collectibles[maze.current_layer][i].rect.Intersects(player.collision_rect))
                {
                    player.CollectCollectible(collectibles[maze.current_layer][i]);
                    collectibles[maze.current_layer].RemoveAt(i);
                }                
            }
        }

        private void DrawHealthBar()
        {
            for (int i = 0; i < player.health; i++)
            {
                _spriteBatch.Draw(heart_container_full, new Vector2(1 + 45 * i, 1), Color.White);
            }
        }
    }
}
