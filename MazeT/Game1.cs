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
        //Generate one randomiser to make numbers generated more random.
        private static Random rng = new(); 

        //These two variables are generated in the Monogame template
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        //General variables
        private const int screen_width = 800;
        private const int screen_height = 800;        
        private Vector2 prev_maze_pos = new Vector2();
        private KeyboardState previous_key_state;
        private MouseState mouse_state;
        private int noLevelsBeaten = 0;

        private enum GameState
        {
            START_MENU = 1,
            MAIN_GAME = 2,
            PAUSE_IN_GAME = 3,
            END_OF_LEVEL = 4
        }

        private GameState game_state = GameState.START_MENU;

        //These textures are used for displaying the minimap
        private Texture2D wall;
        private Texture2D TPpad;
        private Texture2D minimap_bg;
        private Texture2D player_icon;
        private Texture2D end_goal;

        //UI images and fonts
        private Texture2D heart_container_empty;
        private Texture2D heart_container_full;
        private SpriteFont score_font;

        //These are our entities and static items
        private List<CollisionCharacter>[] enemies;
        private List<Collectible>[] collectibles;
        private Maze maze;
        private Player player;

        //These are the sprite sheets used for our enemies
        private Texture2D blind_enemy_run_sheet;
        private Texture2D smart_enemy_run_sheet;

        //These are the sprites for the collectibles
        private Texture2D powerup;
        private Texture2D treasure_chest;

        //Start menu
        private Texture2D start_screen_background;
        private Button play_game_button;
        private Button how_game_works_button;
        private Button enemy_descriptions_button;
        private Button controls_button;
        private Texture2D how_game_works_screen;
        private Texture2D enemy_descriptions_screen;
        private Texture2D controls_screen;

        //In-game pause menu stuff
        private Button continue_game_button;
        private Button quit_game_button;

        //Power up menu
        private Button powerup_box_1;
        private Button powerup_box_2;
        private Button powerup_box_3;
        private int num_powerups_boxes_left;
        private int num_enemies_killed_this_level;
        private int num_treasure_chests_collected;
        private int total_treasure_chests_this_level;

        enum StartMenuScreenState
        {
            MAIN_PAGE,
            HOW_GAME_WORKS,
            ENEMY_DESCRIPTION,
            CONTROLS
        }

        StartMenuScreenState startMenuScreenState = StartMenuScreenState.MAIN_PAGE;

        //In game pause menu

        //Power-ups selection screen
        
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
            previous_key_state = Keyboard.GetState();
            player = new Player(44, 56, 0, 0);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
      
            Maze.maze_wall_H = Content.Load<Texture2D>("images/temp_wallH");
            Maze.maze_wall_V = Content.Load<Texture2D>("images/temp_wallV");
            Maze.maze_floor = Content.Load<Texture2D>("images/temp_floor");
            Maze.tp_pad_design = Content.Load<Texture2D>("images/TPpadsmall");
            Maze.end_tp_pad_design = Content.Load<Texture2D>("images/endTPpad");

            wall = new Texture2D(GraphicsDevice, 1, 1);
            wall.SetData(new Color[] { Color.Black });

            TPpad = new Texture2D(GraphicsDevice, 1, 1);
            TPpad.SetData(new Color[] { Color.DarkGreen });

            minimap_bg = new Texture2D(GraphicsDevice, 1, 1);
            minimap_bg.SetData(new Color[] { Color.White });

            end_goal = new Texture2D(GraphicsDevice, 1, 1);
            end_goal.SetData(new Color[] { Color.MonoGameOrange });

            player_icon = Content.Load<Texture2D>("images/dwarf_icon_small");
            heart_container_empty = Content.Load<Texture2D>("images/ui_heart_empty");
            heart_container_full = Content.Load<Texture2D>("images/ui_heart_full");

            player.walk[0].sprite_sheet = Content.Load<Texture2D>("images/dwarf_run"); 
            for (int i = 1; i < 4; i++)
            {
                player.walk[i].sprite_sheet = player.walk[0].sprite_sheet;
            }
            player.sword_swing = Content.Load<Texture2D>("images/sword_swing");

            blind_enemy_run_sheet = Content.Load<Texture2D>("images/ogre_run");
            smart_enemy_run_sheet = Content.Load<Texture2D>("images/skeleton_run");
            powerup = Content.Load<Texture2D>("images/test_potion");
            treasure_chest = Content.Load<Texture2D>("images/chest");

            //Load fonts
            score_font = Content.Load<SpriteFont>("fonts/score_font");

            //Initialise buttons and UI images
            //Start menu
            start_screen_background = Content.Load<Texture2D>("UI_images/main_background");

            play_game_button = new Button(new Point(252, 200),
                Content.Load<Texture2D>("UI_images/play_game_button"));

            how_game_works_button = new Button(new Point(252, 350),
                Content.Load<Texture2D>("UI_images/how_game_works_button"));

            how_game_works_screen = Content.Load<Texture2D>("UI_images/how_game_works_screen");

            enemy_descriptions_button = new Button(new Point(252, 500),
                Content.Load<Texture2D>("UI_images/enemy_description_button"));

            controls_button = new Button(new Point(252, 650),
                Content.Load<Texture2D>("UI_images/controls_button"));

            //In game menu
            continue_game_button = new Button(new Point(252, 300),
                Content.Load<Texture2D>("UI_images/play_game_button")); //Pls change soon

            quit_game_button = new Button(new Point(252, 500),
                Content.Load<Texture2D>("UI_images/how_game_works_button")); //Pls change soon
        }

        protected override void Update(GameTime gameTime)
        {    
            KeyboardState currentKeys = Keyboard.GetState();
            mouse_state = Mouse.GetState();
            
            if (game_state == GameState.MAIN_GAME)
            {
                //Main game loop
                player.Update(currentKeys, previous_key_state, maze.pos, maze.current_layer, gameTime.ElapsedGameTime.Milliseconds);
                if (currentKeys.IsKeyDown(Keys.Q) && !previous_key_state.IsKeyDown(Keys.Q))
                {
                    HandleIfPlayerIsOnTeleportationPads();
                }
                HandleSideScrolling();
                HandleEnemyLogic(gameTime); //Handles enemy code, including damage detection
                HandleCollectibles(); //Deal with picking up and updating collectibles

                maze.UpdateMazeRects(prev_maze_pos - maze.pos);

                previous_key_state = Keyboard.GetState();
                prev_maze_pos = new Vector2(maze.pos.X, maze.pos.Y);
                player.old_global_position = player.global_position;

                if (currentKeys.IsKeyDown(Keys.Escape))
                {
                    game_state = GameState.PAUSE_IN_GAME;
                }

                //If we have reached the end goal and are pressing Q
                if (maze.end_goal.Intersects(player.collision_rect) 
                    && maze.current_layer == 0
                    && currentKeys.IsKeyDown(Keys.Q))
                {
                    game_state = GameState.END_OF_LEVEL;
                }
            }

            //Needs finishing
            else if (game_state == GameState.START_MENU)
            {
                if (startMenuScreenState == StartMenuScreenState.MAIN_PAGE)
                {
                    if (mouse_state.LeftButton == ButtonState.Pressed)
                    {
                        if (play_game_button.IsMouseOnButton(mouse_state.Position))
                        {
                            //Start Game                        
                            GenerateLevel();
                            game_state = GameState.MAIN_GAME;
                        }
                        else if (how_game_works_button.IsMouseOnButton(mouse_state.Position))
                        {
                            startMenuScreenState = StartMenuScreenState.HOW_GAME_WORKS;
                        }
                        //else if (enemy_descriptions_button.IsMouseOnButton(mouse_state.Position))
                        //{
                        //    startMenuScreenState = StartMenuScreenState.ENEMY_DESCRIPTION;
                        //}
                        //else if (controls_button.IsMouseOnButton(mouse_state.Position))
                        //{
                        //    startMenuScreenState = StartMenuScreenState.CONTROLS;
                        //}
                    }                    
                }
                else
                {
                    if (currentKeys.IsKeyDown(Keys.Escape))
                    {
                        startMenuScreenState = StartMenuScreenState.MAIN_PAGE;
                    }
                }
            }       
            
            //Might need a settings button as well
            else if (game_state == GameState.PAUSE_IN_GAME)
            {
                //Check if user is clicking one of the buttona
                if (mouse_state.LeftButton == ButtonState.Pressed)
                {
                    if (continue_game_button.IsMouseOnButton(mouse_state.Position))
                    {
                        game_state = GameState.MAIN_GAME;
                    }

                    else
                    {
                        game_state = GameState.START_MENU;
                    }
                }
            }

            //Needs power up screen
            else if (game_state == GameState.END_OF_LEVEL)
            {
                noLevelsBeaten++;
                GenerateLevel();
                game_state = GameState.MAIN_GAME;
            }
            
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            //Clear the screen so we can redraw on it
            GraphicsDevice.Clear(Color.LightGray);
            
            _spriteBatch.Begin();

            if (game_state == GameState.MAIN_GAME)
            {
                maze.DisplayMaze(_spriteBatch);
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

                maze.DisplayTopCornerMinimapImage(_spriteBatch, minimap_bg, wall, TPpad, player_icon, player.global_position, end_goal);
                DrawHealthBar();
                DrawTreasureChestBar();
            }
            else if (game_state == GameState.START_MENU)
            {
                //Draw start menu
                if (startMenuScreenState == StartMenuScreenState.MAIN_PAGE)
                {
                    _spriteBatch.Draw(start_screen_background, new Rectangle(0, 0, screen_width, screen_height), Color.White);
                    play_game_button.Display(_spriteBatch);
                    how_game_works_button.Display(_spriteBatch);
                    enemy_descriptions_button.Display(_spriteBatch);
                    controls_button.Display(_spriteBatch);
                }
                else if (startMenuScreenState == StartMenuScreenState.HOW_GAME_WORKS)
                {
                    _spriteBatch.Draw(how_game_works_screen,
                        new Rectangle(Point.Zero, new Point(screen_width, screen_height)),
                        Color.White);
                }
                else if (startMenuScreenState == StartMenuScreenState.ENEMY_DESCRIPTION)
                {
                    _spriteBatch.Draw(enemy_descriptions_screen,
                        new Rectangle(Point.Zero, new Point(screen_width, screen_height)),
                        Color.White);
                }
                else
                {
                    _spriteBatch.Draw(controls_screen,
                        new Rectangle(Point.Zero, new Point(screen_width, screen_height)),
                        Color.White);
                }
            }
            else if (game_state == GameState.PAUSE_IN_GAME)
            {
                continue_game_button.Display(_spriteBatch);
                quit_game_button.Display(_spriteBatch);
            }
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
                        //For now, we only need to choose randomly between the blind and smart enemy
                        int random_number = rng.Next(0,2);
                        //We also want to choose a level for the enemy to determine their strength
                        //We want a distribution that favours higher levels (so if we have beaten 4 levels,
                        //we want most enemies to be level 3 or 4). To do this, I have opted to generate
                        //two random numbers and find their average.
                        int enemy_level = rng.Next(1, noLevelsBeaten+1) + rng.Next(noLevelsBeaten/2, noLevelsBeaten+1);
                        if (random_number == 0)
                        {                            
                            //Spawn a blind enemy
                            enemies[z].Add(new BlindEnemy(enemy_level, maze.GenerateSingleLayerPath(new Point(enemyX, enemyY), 8, z)));
                        }
                        else
                        {
                            //Spawn a smart enemy
                            //Convert the random point into coordinates
                            Vector2 enemy_location = new(enemyX * 128 + 30, enemyY * 128 + 30);                            
                            enemies[z].Add(new SmartEnemy(enemy_level, enemy_location, ref maze));
                        }

                    }
                }
            }
        }

        /// <summary>
        /// Generates collectibles uniformly across maze. Returns the number of treasure
        /// chests generated in the level
        /// </summary>
        private int GenerateCollectibles(int num_powerups_per_layer, int num_treasure_chests_per_layer)
        {
            double chunk_area = maze.width * maze.height / (double) (num_powerups_per_layer + num_treasure_chests_per_layer);
            //We use the ceiling here to ensure all parts of the maze are covered.
            int chunk_width = (int)Math.Ceiling(Math.Sqrt(chunk_area));
            //Initialise collectibles[] array
            for (int i = 0; i < collectibles.Length; i++)
            {
                collectibles[i] = new();
            }

            //We need to ensure we know exactly how many treasure chests are in this level
            int treasure_chest_count = 0;

            //Place power ups in random dead ends across both floors
            for (int z = 0; z < maze.max_layers; z++)
            {
                int num_treasure_chests_in_this_layer = 0;
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

                            //Determine the type and value of the collectible. We can have treasure chests now
                            //provided we have not generated all of them yet
                            CollectibleType type;
                            if (num_treasure_chests_per_layer > num_treasure_chests_in_this_layer)
                            {
                                type = (CollectibleType)rng.Next(0, 5);
                            }
                            else
                            {
                                type = (CollectibleType)rng.Next(1, 5);
                            }
                             
                            double value = 0;
                            //Determine a value for them
                            if (type == CollectibleType.TREASURE_CHEST)
                            {
                                value = 1;//Doesn't matter for treasure chests
                                treasure_chest_count++;
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

                            //Determine the power up type (there are 4 different types which aren't
                            // treasure chests).
                            CollectibleType type = (CollectibleType)rng.Next(1, 5);
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

                            collectibles[z].Add(new Collectible(type, value, new Point(powerupX, powerupY)));
                        }
                    }
                }               
            }
            return treasure_chest_count;
        }

        /// <summary>
        /// Generates one full maze with all the collectibles and enemies. This will be run to generate each new level.
        /// </summary>
        private void GenerateLevel()
        {
            //The maze should start small but get larger as you progress
            //It will start with a 10x10 but expand as you progress all the way
            //to a 15 x 15.
            int maze_size = 10 + noLevelsBeaten;
            if (maze_size > 15)
            {
                maze_size = 15;
            }
            maze = new Maze(maze_size, maze_size, 2);
            prev_maze_pos = Vector2.Zero; //Reset this variable
            
            player.ResetPlayerForNextLevel(0, 0, maze.collision_rects);
            num_enemies_killed_this_level = 0;
            num_treasure_chests_collected = 0;

            //Initialise enemy list
            enemies = new List<CollisionCharacter>[maze.max_layers];
            collectibles = new List<Collectible>[maze.max_layers];
            for (int i = 0; i < maze.max_layers; i++)
            {
                enemies[i] = new List<CollisionCharacter>();
            }
            
            //Determine how many enemies to generate in the maze. We want at most
            //90 enemies as otherwise it becomes too much.
            int num_enemies= 10 * (noLevelsBeaten + 1);
            if (num_enemies > 90)
            {
                num_enemies = 90;
            }

            //Generate enemies and collectibles randomly but evenly across maze.
            GenerateEnemies(num_enemies);
            total_treasure_chests_this_level = GenerateCollectibles(4, 3);

            //Now give each of the enemies and collectibles a sprite or spritesheet
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
                    if (collectible.type == CollectibleType.TREASURE_CHEST)
                    {
                        collectible.image = treasure_chest;
                    }
                    else
                    {
                        collectible.image = powerup;
                    }
                    collectible.SetRect(collectible.global_position.ToPoint());
                }
            }
        }

        private void HandleSideScrolling()
        {
            //Side scrolling code//
            //Only start the side scrolling once the player is near the centre of the page.
            if (player.local_position.X <= screen_width / 2 + 30 && player.local_position.X >= screen_width / 2 - 30)
            {
                maze.pos.X += player.global_position.X - player.old_global_position.X;
            }
            if (player.local_position.Y <= screen_height / 2 + 30 && player.local_position.Y >= screen_height / 2 - 30)
            {
                maze.pos.Y += player.global_position.Y - player.old_global_position.Y;
            }

            //Ensure that you cannot see any parts outside the maze, as there is nothing there.
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
                    //Works well for two layers, which is what I have decided for this game
                    maze.current_layer = (maze.current_layer + 1) % maze.max_layers;
                    //Give the player a little window of invincibility so they can't get jumped 
                    //by enemies on the TP pads
                    player.TakeDamage(0, 1200);
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
                        //If the player kills the enemy
                        if (enemy.health <= 0)
                        {
                            num_enemies_killed_this_level++;
                        }
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
                    if (collectibles[maze.current_layer][i].type == CollectibleType.TREASURE_CHEST)
                    {
                        num_treasure_chests_collected++;
                    }
                    collectibles[maze.current_layer].RemoveAt(i);
                }                
            }
        }

        private void DrawHealthBar()
        {            
            _spriteBatch.Draw(heart_container_full, new Vector2(1, 1), Color.White);
            _spriteBatch.DrawString(score_font, "x" + player.health, new Vector2(46, 0), Color.White);
        }

        private void DrawTreasureChestBar()
        {
            _spriteBatch.Draw(treasure_chest, new Rectangle(300, -5, 40, 40), Color.White);
            _spriteBatch.DrawString(score_font, $"{num_treasure_chests_collected}/{total_treasure_chests_this_level}", new Vector2(343, 3), Color.White);
        }
    }
}
