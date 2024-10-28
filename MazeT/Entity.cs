using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace MazeT
{
    /// <summary>
    /// This class is used as the base class for objects that do not have animation.
    /// </summary>
    internal class StandardSprite2D
    {
        private Texture2D image;
        public Rectangle rect;
        
        public StandardSprite2D(string imagePath, GraphicsDevice graphics)
        {
            image = Texture2D.FromFile(graphics, imagePath);
            rect = image.Bounds;
        }

        public virtual void Display(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(image, rect, Color.White);            
        }
    }
    
    /// <summary>
    /// This class is used to represent one full animation sequence (for instance, walk forwards or walk backwards).
    /// For classes (e.g. the player class), who have multiple animation sequences, you will need multiple objects 
    /// of this class.
    /// </summary>
    internal class AnimatedSpriteSheet
    {
        public Texture2D sprite_sheet;
        private readonly Rectangle[] _drawingBounds;
        private int _animationIndex;


        /// <summary>
        /// This is used to determine the order of animation. 
        /// If 0, it indicates the order is 1, 2, 3, 1, 2, 3
        /// If 1, it indicates the order is 1, 2, 3, 2, 1, 2, 3
        /// If -1, it indicates the same as above.
        /// </summary>
        private int _orderOfSprites; 
        
        public AnimatedSpriteSheet(Rectangle[] rects, int order, int startingIndex = 0)
        {
            _orderOfSprites = order;
            _animationIndex = startingIndex;
            _drawingBounds = rects;
        }
        
        //Assuming that sprites are going across and not vertically down
        //This constructor is used to automatically create the bounding rectangles array.
        public AnimatedSpriteSheet(int width, int height, int noRects, int order, int startY, int spacing = 0, int startingIndex = 0)
        {
            _orderOfSprites = order;
            _animationIndex = startingIndex;
            _drawingBounds = new Rectangle[noRects];
            for (int i = 0; i < noRects; i++)
            {
                _drawingBounds[i] = new Rectangle(i * (width + spacing), startY, width, height);
            }
        }

        public void UpdateAnimationFrame()
        {
            //If the order goes 1, 2, 3, 1, 2, 3, 1, 2, ...
            if (_orderOfSprites == 0)
            {
                _animationIndex++;
                if (_animationIndex >= _drawingBounds.Length)
                {
                    _animationIndex = 0;
                }
            }

            //If the order goes 1, 2, 3, 2, 1, 2, 3, 2, 1
            else
            {
                _animationIndex += _orderOfSprites;
                if (_animationIndex < 0)
                {
                    _animationIndex = 1;
                    _orderOfSprites = 1;
                }
                else if (_animationIndex >= _drawingBounds.Length)
                {
                    _animationIndex = _drawingBounds.Length - 2;
                    _orderOfSprites = -1;
                }
            }            
        }

        public void ResetAnimation(int resetIndex = 0)
        {
            _animationIndex = resetIndex;
        }

        public void Display(SpriteBatch spriteBatch, Vector2 position, int animationIndex = -1)
        {
            if (animationIndex == -1)
            {
                spriteBatch.Draw(sprite_sheet, position, _drawingBounds[_animationIndex], Color.White);
            }
            else
            {
                //This is for fixed sprites
                spriteBatch.Draw(sprite_sheet, position, _drawingBounds[animationIndex], Color.White);
            }
        }

        public void Display(SpriteBatch spriteBatch, Point position, int animationIndex = -1)
        {
            //Convert position to vector2 form
            Vector2 pos = new Vector2(position.X, position.Y);
            if (animationIndex == -1)
            {
                spriteBatch.Draw(sprite_sheet, pos, _drawingBounds[_animationIndex], Color.White);
            }
            else
            {
                //This is for fixed sprites
                spriteBatch.Draw(sprite_sheet, pos, _drawingBounds[animationIndex], Color.White);
            }
        }
    }

    /// <summary>
    /// This class is used as the base class for all objects which have collision with
    /// the walls
    /// </summary>
    internal class CollisionCharacter
    {
        public Rectangle collision_rect;        
        public int health; //How much health the character has remaining
        public int power; //How much damage a character deals
        //This is used as the floating point position of the enemies for smoother movement
        public Vector2 global_position;
        public Vector2 local_position; 
        protected Vector2 velocity;
        protected static List<Rectangle>[] wall_rects;
        protected Vector2 coll_rect_offset;

        /// <summary>
        /// This is the function that will handle collision with walls for all objects 
        /// in the maze game.
        /// </summary>
        protected void HandleWallCollision(int maze_layer, bool checkX)
        {
            foreach (Rectangle rect in wall_rects[maze_layer])
            {                
                if (collision_rect.Intersects(rect))
                {
                    if (checkX)
                    {
                        global_position.X -= velocity.X;
                        collision_rect.X -= (int)velocity.X;
                    }
                    else
                    {
                        global_position.Y -= velocity.Y;
                        collision_rect.Y -= (int)velocity.Y;
                    }
                }
            }
        }

        public virtual void Update(int timeElapsedinMilliseconds, int mazeLayer)
        {

        }

        public virtual void Update(int timeElapsedinMilliseconds, int mazeLayer, Vector2 player_pos)
        {

        }

        public void UpdateLocalPosition(Vector2 mazePos, int offsetX = 0, int offsetY = 0)
        {
            local_position.X = (int)(global_position.X - mazePos.X + offsetX);
            local_position.Y = (int)(global_position.Y - mazePos.Y + offsetY);            
        }

        public void UpdateRectanglePosition(Vector2 mazePos)
        {
            collision_rect.Location = (global_position - mazePos + coll_rect_offset).ToPoint();
        }

        public virtual void TakeDamage(int damage_taken, Point source_of_damage)
        {
            
        }

        public virtual void Display(SpriteBatch spritebatch)
        {

        }
    }

    /// <summary>
    /// This class is used to represent the character that is controlled by the user.
    /// </summary>
    internal class Player : CollisionCharacter
    {
        private enum PlayerState
        {
            IDLE,
            WALKING,
            ATTACKING
        }

        private enum FacingDirections
        {
            NORTH = 0,
            EAST = 1,
            SOUTH = 2,
            WEST = 3
        }

        public AnimatedSpriteSheet[] walk = new AnimatedSpriteSheet[4];
        private int internal_anim_timer = 0;
        private int internal_iframes_timer = 0;

        private int sword_range; //This will change depending on the sword the player wields
        public Rectangle sword_hitbox;
        private int sword_out_timer; //Records how long the sword has been out for
        private int sword_cooldown_timer; //Records how long there is till the next sword can be used

        public Vector2 old_global_position; //This is used for the side scrolling code

        private FacingDirections direction = FacingDirections.NORTH;
        private PlayerState player_state = PlayerState.IDLE;
        public Player(int width, int height, int x, int y, List<Rectangle>[] wall_rects)
        {
            CollisionCharacter.wall_rects = wall_rects;
            health = 5;
            sword_range = 2;
            power = 1;
            coll_rect_offset = new Vector2(75, 104);
            collision_rect = new Rectangle(x+75, y+104, width, height);
            global_position = new Vector2(x, y);
            velocity = new Vector2(0, 0);
            
            for (int i = 0; i < 4; i++)
            {
                walk[i] = new AnimatedSpriteSheet(128, 128, 4, 0, 128 * i);
            }
        }

        public void Update(KeyboardState current_keys, KeyboardState previous_keys, Vector2 maze_pos, int maze_layer, int time_elapsed)
        {
            int walkAnimationDelay = 200;
            int playerSpeed = 2;
            bool directionKeyPressed = false;

            if (current_keys.IsKeyDown(Keys.Space) && sword_cooldown_timer <= 0)
            {
                player_state = PlayerState.ATTACKING;
                sword_out_timer = 200;
                sword_cooldown_timer = 300;
            }

            //If the player is not attacking, they can walk
            if (player_state != PlayerState.ATTACKING)
            {               
                if (current_keys.IsKeyDown(Keys.LeftShift) || current_keys.IsKeyDown(Keys.RightShift))
                {
                    playerSpeed = 4;
                    walkAnimationDelay = 120;
                }
                if (current_keys.IsKeyDown(Keys.Up) || current_keys.IsKeyDown(Keys.W))
                {
                    directionKeyPressed = true;
                    direction = FacingDirections.NORTH;
                    velocity.X = 0;
                    velocity.Y = -playerSpeed;
                    if (player_state != PlayerState.WALKING)
                    {
                        player_state = PlayerState.WALKING;
                        internal_anim_timer = walkAnimationDelay;
                    }
                    if (internal_anim_timer <= 0)
                    {
                        walk[(int)direction].UpdateAnimationFrame();
                        internal_anim_timer = walkAnimationDelay;
                    }
                }
                else if (current_keys.IsKeyDown(Keys.Down) || current_keys.IsKeyDown(Keys.S))
                {
                    directionKeyPressed = true;
                    direction = FacingDirections.SOUTH;
                    velocity.X = 0;
                    velocity.Y = playerSpeed;
                    if (player_state != PlayerState.WALKING)
                    {
                        player_state = PlayerState.WALKING;
                        internal_anim_timer = walkAnimationDelay;
                    }
                    if (internal_anim_timer <= 0)
                    {
                        walk[(int)direction].UpdateAnimationFrame();
                        internal_anim_timer = walkAnimationDelay;
                    }
                }

                collision_rect.Offset(0, velocity.Y);
                global_position.Y += velocity.Y;
                HandleWallCollision(maze_layer, false);

                if (current_keys.IsKeyDown(Keys.Up) || current_keys.IsKeyDown(Keys.W) || current_keys.IsKeyDown(Keys.Down) || current_keys.IsKeyDown(Keys.S))
                {
                    //pass
                }
                else if (current_keys.IsKeyDown(Keys.Right) || current_keys.IsKeyDown(Keys.D))
                {
                    directionKeyPressed = true;
                    direction = FacingDirections.EAST;
                    velocity.Y = 0;
                    velocity.X = playerSpeed;
                    if (player_state != PlayerState.WALKING)
                    {
                        player_state = PlayerState.WALKING;
                        internal_anim_timer = walkAnimationDelay;
                    }
                    if (internal_anim_timer <= 0)
                    {
                        walk[(int)direction].UpdateAnimationFrame();
                        internal_anim_timer = walkAnimationDelay;
                    }
                }
                else if (current_keys.IsKeyDown(Keys.Left) || current_keys.IsKeyDown(Keys.A))
                {
                    directionKeyPressed = true;
                    direction = FacingDirections.WEST;
                    velocity.Y = 0;
                    velocity.X = -playerSpeed;
                    if (player_state != PlayerState.WALKING)
                    {
                        player_state = PlayerState.WALKING;
                        internal_anim_timer = walkAnimationDelay;
                    }
                    if (internal_anim_timer <= 0)
                    {
                        walk[(int)direction].UpdateAnimationFrame();
                        internal_anim_timer = walkAnimationDelay;
                    }
                }

                collision_rect.Offset(velocity.X, 0);
                global_position.X += velocity.X;
                HandleWallCollision(maze_layer, true);

                if (!directionKeyPressed)
                {
                    velocity.X = 0;
                    velocity.Y = 0;
                    player_state = PlayerState.IDLE;
                    walk[(int)direction].ResetAnimation();
                }

                //Lower sword cooldown timer
                if (sword_cooldown_timer > 0)
                {
                    sword_cooldown_timer -= time_elapsed;
                }
            }

            //The player will slide to a halt if they attack
            else
            {
                //Let the player slide to a halt.
                if (velocity.X > 0)
                {
                    velocity.X -= 0.3f;
                    if (velocity.X < 0)
                    {
                        velocity.X = 0;
                    }
                }
                else if (velocity.X < 0)
                {
                    velocity.X += 0.3f;
                    if (velocity.X > 0)
                    {
                        velocity.X = 0;
                    }
                }

                collision_rect.Offset(velocity.X, 0);
                global_position.X += velocity.X;
                HandleWallCollision(maze_layer, true);

                if (velocity.Y > 0)
                {
                    velocity.Y -= 0.3f;
                    if (velocity.Y < 0)
                    {
                        velocity.Y = 0;
                    }
                }
                else if (velocity.Y < 0)
                {
                    velocity.Y += 0.3f;
                    if (velocity.Y > 0)
                    {
                        velocity.Y = 0;
                    }
                }

                collision_rect.Offset(0, velocity.Y);
                global_position.Y += velocity.Y;
                HandleWallCollision(maze_layer, false);

                //Calculate sword's dimensions. This will depend on which
                //way the player is facing 
                if (direction == FacingDirections.NORTH || direction == FacingDirections.SOUTH)
                {
                    sword_hitbox.Height = sword_range * 20;
                    sword_hitbox.Width = 60;
                }
                else
                {
                    sword_hitbox.Height = 60;
                    sword_hitbox.Width = sword_range * 20;
                }

                //Position sword in front of player
                //Since the x and y positions of the sword's hitbox refer to the
                //topleft, need to adjust sword position accordingly
                if (direction == FacingDirections.NORTH)
                {
                    sword_hitbox.X = collision_rect.Center.X - (sword_hitbox.Width/2);
                    sword_hitbox.Y = collision_rect.Top - sword_hitbox.Height;
                }
                else if (direction == FacingDirections.EAST)
                {
                    sword_hitbox.X = collision_rect.Right;
                    sword_hitbox.Y = collision_rect.Center.Y - (sword_hitbox.Height/2);
                }
                else if (direction == FacingDirections.SOUTH)
                {
                    sword_hitbox.X = collision_rect.Center.X - (sword_hitbox.Width / 2);
                    sword_hitbox.Y = collision_rect.Bottom;
                }
                else
                {
                    sword_hitbox.X = collision_rect.Left - sword_hitbox.Width;
                    sword_hitbox.Y = collision_rect.Center.Y - (sword_hitbox.Height / 2);
                }

                sword_out_timer -= time_elapsed;
                if (sword_out_timer <= 0)
                {
                    player_state = PlayerState.IDLE;
                    //Move the sword offscreen so that it can't affect enemies
                    sword_hitbox.Location = new(-100, -100);
                }

            }
           
            UpdateRectanglePosition(maze_pos);
            internal_anim_timer -= time_elapsed;
            if (internal_iframes_timer > 0)
            {
                internal_iframes_timer -= time_elapsed;
            }
        }

        public override void Display(SpriteBatch spriteBatch)
        {
            //Display sword
            if (player_state == PlayerState.ATTACKING)
            {
                //Display sword code
            }
            //Display player every 100ms if they are invulnerable to create
            //a blinking animation
            if (internal_iframes_timer % 200 <= 100)
            {
                if (player_state == PlayerState.WALKING)
                {
                    walk[(int)direction].Display(spriteBatch, local_position);
                }
                //TEMPORARY: WILL ADD ATTACKING ANIMATION EVENTUALLY
                else if (player_state == PlayerState.IDLE || player_state == PlayerState.ATTACKING)
                {
                    walk[(int)direction].Display(spriteBatch, local_position, 1);
                }
            }            
        }

        public void TakeDamage(int damage_taken = 1)
        {
            //If the player is not invincible
            if (internal_iframes_timer <= 0)
            {
                //Deal damage
                health -= damage_taken;

                //Activate invincibility frames
                //2 seconds of invulnerability
                internal_iframes_timer = 2000;
            }
            
        }
    }

    /// <summary>
    /// This enemy walks blindly between two spots on the map, through a predetermined path, 
    /// which can be determined through a BFS.
    /// </summary>
    internal class BlindEnemy : CollisionCharacter
    {        
        //The blind enemy can run left or right (but can move up or down)
        public static AnimatedSpriteSheet[] run = new AnimatedSpriteSheet[2];
        private int internal_anim_timer = 0; //Used for animation
        public List<Point> path;
        private int target_index;
        private int prev_target_index;
        private bool is_facing_left = true;
        private int being_hit_timer = 0;

        public BlindEnemy(int current_level, List<Point> path)
        {
            //Subject to change
            health = 90;
            power = current_level;
            run.Initialize();
            //Path should be determined in the Game1.cs to help
            //avoid conflicting paths
            this.path = path;
            global_position = path[0].ToVector2();
            prev_target_index = 0;
            target_index = 1;
            collision_rect.Width = 40;
            collision_rect.Height = 30;
            coll_rect_offset = new Vector2(10, 15);
            //Create animation frames
            run[0] = new AnimatedSpriteSheet(64, 56, 4, 0, 56); //left
            run[1] = new AnimatedSpriteSheet(64, 56, 4, 0, 0); //right
        }
        
        public override void Update(int time_elapsed, int maze_layer)
        {
            const int walk_anim_delay = 310;
            //If enemy is not being attacked (in which case they are pushed back slightly)
            if (being_hit_timer <= 0)
            {
                //Determine a velocity based on enemy's centre's position and target position                
                velocity = path[target_index].ToVector2() - global_position;
                //make sure magnitude of velocity = set speed (divide velocity by magnitude)
                velocity = velocity * 1 / velocity.Length();

                //Update X coords but ignore collision
                //(prevents enemy getting stuck in the wall)
                collision_rect.Offset(velocity.X, 0);
                global_position.X += velocity.X;

                //Update Y coords and do same thing
                collision_rect.Offset(0, velocity.Y);
                global_position.Y += velocity.Y;

            }
            //If the enemy has been hit recently
            else
            {
                //Reduce the speed exponentially.
                velocity /= 1.05f;
                being_hit_timer -= time_elapsed;

                //Update X coords and do collision handling function
                collision_rect.Offset(0, velocity.X);
                global_position.X += velocity.X;
                HandleWallCollision(maze_layer, true);

                //Update Y coords and do same thing
                collision_rect.Offset(0, velocity.Y);
                global_position.Y += velocity.Y;
                HandleWallCollision(maze_layer, false);
            }           

            //Decide facing direction only if the enemy is not being hit
            if (being_hit_timer <= 0)
            {
                if (velocity.X < 0)
                {
                    is_facing_left = true;
                }
                else if (velocity.X > 0)
                {
                    is_facing_left = false;
                }
            }            

            //Update animation frame
            if (internal_anim_timer <= 0)
            {                
                if (is_facing_left)
                {
                    run[0].UpdateAnimationFrame();
                }
                else
                {
                    run[1].UpdateAnimationFrame();
                }
                internal_anim_timer = walk_anim_delay;

            }
            internal_anim_timer -= time_elapsed;
            

            //Update target position once reached (when you are within 5 units from the target)
            if (Vector2.DistanceSquared(path[target_index].ToVector2(), global_position) <= 25)
            {
                if (target_index != path.Count - 1 && target_index != 0)
                {
                    //If enemy is walking "forward" on path
                    if (prev_target_index < target_index)
                    {
                        prev_target_index = target_index;
                        target_index++;                        
                    }
                    //If enemy is walking back along the path
                    else
                    {
                        prev_target_index = target_index;
                        target_index--;                        
                    }
                    
                }
                //If we need to turn around and we are at the start
                else if (target_index == 0)
                {
                    prev_target_index = target_index;
                    target_index++;
                }

                //If we are at the end of the path
                else
                {
                    prev_target_index = target_index;
                    target_index--;
                }
            }
        }

        public override void TakeDamage(int damage_taken, Point source_of_damage)
        {
            if (being_hit_timer <= 0)
            {
                health -= damage_taken;
                being_hit_timer = 500; //Enemy will be stunned for 500ms

                //Decide which way to push enemy back
                //This is calculated by finding the vector between the centres,
                // and finding a vector in the same direction of magnitude 4.
                velocity = (collision_rect.Center - source_of_damage).ToVector2();
                velocity = velocity * 4 / velocity.Length();               
            }  
            
        }

        public override void Display(SpriteBatch spritebatch)
        {
            if (is_facing_left)
            {
                run[0].Display(spritebatch, local_position);
            }
            else
            {
                run[1].Display(spritebatch, local_position);
            }
        }
    }

    /// <summary>
    /// This enemy will simply follow the player around the map once they have sighted the 
    /// player. It will require a path finding algorithm.
    /// </summary>
    internal class SmartEnemy: CollisionCharacter
    {
        public static AnimatedSpriteSheet[] run = new AnimatedSpriteSheet[2];
        private int internal_anim_timer = 0; //Used for animation
        private int being_hit_timer = 0;
        private bool is_targetting_player = false;
        private Vector2 destination;
        private static Maze maze_copy; //This is needed for the path finding algorithm

        public SmartEnemy(int current_level, Vector2 initial_position, ref Maze maze)
        {
            health = 5;
            power = 2;
            global_position = initial_position;
            destination = new(-2, -2);
            collision_rect.Width = 40;
            collision_rect.Height = 30;
            maze_copy = maze;
            coll_rect_offset = new(100, 128);
        }

        public override void Update(int time_elapsed, int maze_layer, Vector2 player_pos)
        {
            //Determine whether the player is in enemy's vicinity (128 pixel radius)
            //only if the enemy is not targetting the player currently.
            if ((player_pos-global_position).LengthSquared() < 16384 && !is_targetting_player)
            {
                is_targetting_player = true;
                //Maybe do enraged animation or an exclamation icon
            }

            if (is_targetting_player)
            {

                //Use path finding algorithm to get onto the same tile as the player, then hone
                //in on the player

                //The top corner of the player is not quite (0,0), so there is a bit of an offset.
                //Hence we add the vector (98, 80) to the player's position and then divide it.
                Point player_tile = new((int)(player_pos.X + 98) / 128, (int)(player_pos.Y + 80) / 128);
                Point enemy_tile = new((int)global_position.X / 128, (int)global_position.Y / 128);

                //If the player and enemy are on the same tile, the enemy can walk directly towards the player
                if (true)
                {
                    velocity = player_pos - global_position;
                    velocity = velocity * 6 / velocity.Length(); //speed of 6
                }

                //The enemy
                else
                {
                    //Only choose a new destination once the enemy has reached its previously assigned destination
                    //or if a destination has not been assigned yet.
                    destination = maze_copy.SingleLayerNextTileFinder(enemy_tile, player_tile).ToVector2();                    

                    if (float.IsNaN((destination - global_position).LengthSquared()))
                    {
                        throw new("WHAT???");
                    }
                        
                    if (!(destination.X == -1 && destination.Y == -1))
                    {
                        //Convert maze indices to global coordinates of the centres of these tiles
                        destination.X = destination.X * 128 + 64 - collision_rect.Width/2;
                        destination.Y = destination.Y * 128 + 64 + collision_rect.Height/2;

                        velocity = destination - global_position;
                        velocity = velocity * 4 / velocity.Length(); //Speed of 4      
                    }
                    //If the enemy cannot path find to the player, make it stop attacking the player.
                    else
                    {
                        is_targetting_player = false;
                    }            
                }

                //Update X coords and do collision handling function
                collision_rect.Offset(velocity.X, 0);
                global_position.X += velocity.X;
                //HandleWallCollision(maze_layer, true);

                //Update Y coords and do same thing
                collision_rect.Offset(0, velocity.Y);
                global_position.Y += velocity.Y;
                //HandleWallCollision(maze_layer, false);

                //Do animation stuff
            }
        }
    }
}

