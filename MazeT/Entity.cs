using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Numerics;
using Vector2 = Microsoft.Xna.Framework.Vector2;

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
        private Rectangle[] _drawingBounds;
        private int _animationIndex;


        /// <summary>
        /// This is used to determine the order of animation. 
        /// If 0, it indicates the order is 1, 2, 3, 1, 2, 3
        /// If 1, it indicates the order is 1, 2, 3, 2, 1, 2, 3
        /// If -1, it indicates the same as above.
        /// </summary>
        private sbyte _orderOfSprites; 
        
        public AnimatedSpriteSheet(Rectangle[] rects, sbyte order, int startingIndex = 0)
        {
            _orderOfSprites = order;
            _animationIndex = startingIndex;
            _drawingBounds = rects;
        }
        
        //Assuming that sprites are going across and not vertically down
        //This constructor is used to automatically create the bounding rectangles array.
        public AnimatedSpriteSheet(int width, int height, int noRects, sbyte order, int startY, int spacing = 0, int startingIndex = 0)
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
        public Vector2 old_global_position;
        //This is used as the floating point position of the enemies for smoother movement
        public Vector2 globalPosition;
        public Vector2 localPosition; //Used for displaying
        public Vector2 velocity;
        public static List<Rectangle>[] wall_rects;
        protected Vector2 coll_rect_offset;

        /// <summary>
        /// This is the function that will handle collision with walls for all objects 
        /// in the maze game.
        /// </summary>
        public void HandleWallCollision(List<Rectangle> wall_rects, bool checkX)
        {
            foreach (Rectangle rect in wall_rects)
            {                
                if (collision_rect.Intersects(rect))
                {
                    if (checkX)
                    {
                        globalPosition.X -= velocity.X;
                        collision_rect.X -= (int)velocity.X;
                    }
                    else
                    {
                        globalPosition.Y -= velocity.Y;
                        collision_rect.Y -= (int)velocity.Y;
                    }
                }
            }
        }

        public virtual void Update(long timeElapsedinMilliseconds, int mazeLayer)
        {

        }        

        public void UpdateLocalPosition(Vector2 mazePos, int offsetX = 0, int offsetY = 0)
        {
            localPosition.X = (int)(globalPosition.X - mazePos.X + offsetX);
            localPosition.Y = (int)(globalPosition.Y - mazePos.Y + offsetY);
            collision_rect.Location = (globalPosition - mazePos + coll_rect_offset).ToPoint();
        }

        public void RefreshRectanglePosition(Vector2 mazePos)
        {
            collision_rect.Location = (globalPosition - mazePos + coll_rect_offset).ToPoint();
        }

        public virtual void Display(SpriteBatch _spritebatch)
        {

        }
    }

    internal class Player : CollisionCharacter
    {        
        public AnimatedSpriteSheet[] walk = new AnimatedSpriteSheet[4];
        private double internalTimer = 0;
        private FacingDirections direction = FacingDirections.NORTH;
        
        private enum PlayerState
        {
            IDLE,
            WALKING,
            ATTACK            
        }

        private enum FacingDirections
        {
            NORTH = 0,
            EAST = 1,
            SOUTH = 2,
            WEST = 3
        }

        private PlayerState playerState = PlayerState.IDLE;
        public Player(int width, int height, int x, int y, List<Rectangle>[] wall_rects)
        {
            CollisionCharacter.wall_rects = wall_rects;
            coll_rect_offset = new Vector2(75, 104);
            collision_rect = new Rectangle(x+75, y+104, width, height);
            globalPosition = new Vector2(x, y);
            velocity = new Vector2(0, 0);
            
            for (int i = 0; i < 4; i++)
            {
                walk[i] = new AnimatedSpriteSheet(128, 128, 4, 0, 128 * i);
            }
        }
        

        public void Update(KeyboardState currentKeys, KeyboardState previousKeys, Vector2 mazePos, int mazeLayer, double timeElapsed)
        {
            int walkAnimationDelay = 200;
            int playerSpeed = 2;
            bool directionKeyPressed = false;
            if (currentKeys.IsKeyDown(Keys.LeftShift) || currentKeys.IsKeyDown(Keys.RightShift))
            {
                playerSpeed = 4;
                walkAnimationDelay = 120;
            }
            if (currentKeys.IsKeyDown(Keys.Up) || currentKeys.IsKeyDown(Keys.W))
            {
                directionKeyPressed = true;
                direction = FacingDirections.NORTH;
                velocity.X = 0;
                velocity.Y = -playerSpeed;
                if (playerState != PlayerState.WALKING)
                {
                    playerState = PlayerState.WALKING;
                    internalTimer = walkAnimationDelay;
                }                
                if (internalTimer <= 0)
                {
                    walk[(int) direction].UpdateAnimationFrame();
                    internalTimer = walkAnimationDelay;
                }
            }
            else if (currentKeys.IsKeyDown(Keys.Down) || currentKeys.IsKeyDown(Keys.S))
            {
                directionKeyPressed = true;
                direction = FacingDirections.SOUTH;
                velocity.X = 0;
                velocity.Y = playerSpeed;
                if (playerState != PlayerState.WALKING)
                {
                    playerState = PlayerState.WALKING;
                    internalTimer = walkAnimationDelay;
                }
                if (internalTimer <= 0)
                {
                    walk[(int)direction].UpdateAnimationFrame();
                    internalTimer = walkAnimationDelay;
                }
            }

            collision_rect.Offset(0, velocity.Y);
            globalPosition.Y += velocity.Y;
            HandleWallCollision(wall_rects[mazeLayer], false);

            if (currentKeys.IsKeyDown(Keys.Up) || currentKeys.IsKeyDown(Keys.W) || currentKeys.IsKeyDown(Keys.Down) || currentKeys.IsKeyDown(Keys.S))
            {
                //pass
            }
            else if (currentKeys.IsKeyDown(Keys.Right) || currentKeys.IsKeyDown(Keys.D))
            {
                directionKeyPressed = true;
                direction = FacingDirections.EAST;
                velocity.Y = 0;
                velocity.X = playerSpeed;
                if (playerState != PlayerState.WALKING)
                {
                    playerState = PlayerState.WALKING;
                    internalTimer = walkAnimationDelay;
                }
                if (internalTimer <= 0)
                {
                    walk[(int)direction].UpdateAnimationFrame();
                    internalTimer = walkAnimationDelay;
                }
            }
            else if (currentKeys.IsKeyDown(Keys.Left) || currentKeys.IsKeyDown(Keys.A))
            {
                directionKeyPressed = true;
                direction = FacingDirections.WEST;
                velocity.Y = 0;
                velocity.X = -playerSpeed;
                if (playerState != PlayerState.WALKING)
                {
                    playerState = PlayerState.WALKING;
                    internalTimer = walkAnimationDelay;
                }
                if (internalTimer <= 0)
                {
                    walk[(int)direction].UpdateAnimationFrame();
                    internalTimer = walkAnimationDelay;
                }
            }

            collision_rect.Offset(velocity.X, 0);
            globalPosition.X += velocity.X;
            HandleWallCollision(wall_rects[mazeLayer], true);

            if (!directionKeyPressed)
            {
                velocity.X = 0;
                velocity.Y = 0;
                playerState = PlayerState.IDLE;
                walk[(int) direction].ResetAnimation();
            }
           
            RefreshRectanglePosition(mazePos);
            internalTimer -= timeElapsed;
        }

        public override void Display(SpriteBatch spriteBatch)
        {
            if (playerState == PlayerState.WALKING)
            {
                walk[(int)direction].Display(spriteBatch, localPosition);
            }            
            else if (playerState == PlayerState.IDLE)
            {
                walk[(int)direction].Display(spriteBatch, localPosition, 1);
            }
        }
    }

    /// <summary>
    /// This enemy walks blindly between two spots on the map, through a predetermined path, 
    /// which can be determined through a BFS.
    /// </summary>
    internal class BlindEnemy : CollisionCharacter
    {
        public int health;
        public int power;
        //The ogre can run left or right (but can move up or down)
        public static AnimatedSpriteSheet[] run = new AnimatedSpriteSheet[2];
        private int _internalAnimTimer = 0; //Used for animation
        public List<Point> path;         
        private Point target;
        private int targetIndex;
        private int prevTargetIndex;
        private bool isFacingLeft = true;

        public BlindEnemy(int currentLevel, List<Point> path)
        {
            //Subject to change
            health = currentLevel;
            power = currentLevel;
            run.Initialize();
            //Path should be determined in the main game loop to help
            //avoid conflicting paths
            this.path = path;
            globalPosition = path[0].ToVector2();
            target = path[1];
            prevTargetIndex = 0;
            targetIndex = 1;

            //Create animation frames
            run[0] = new AnimatedSpriteSheet(128, 112, 4, 0, 0);
            run[1] = new AnimatedSpriteSheet(128, 112, 4, 0, 112);

        }

        public override void Update(long timeElapsedinMilliseconds, int mazeLayer)
        {
            const int walk_anim_delay = 200;
            //If enemy is not being attacked (in which case they are pushed back slightly)
            if (true)
            {
                //Determine a velocity based on current position and target position                
                velocity = target.ToVector2() - globalPosition;
                //make sure magnitude of velocity = set speed (divide velocity by magnitude)
                velocity = velocity * 1 / velocity.Length();
                
            }
            else
            {
                //Deal with enemy getting damaged
            }


            //Update X coords and do collision handling function
            collision_rect.Offset(0, velocity.X);
            globalPosition.X += velocity.X;
            HandleWallCollision(wall_rects[mazeLayer], true);

            //Update Y coords and do same thing
            collision_rect.Offset(0, velocity.Y);
            globalPosition.Y += velocity.Y;
            HandleWallCollision(wall_rects[mazeLayer], false);

            //Decide facing direction
            if (velocity.X < 0)
            {
                isFacingLeft = true;
            }
            else if (velocity.X > 0)
            {
                isFacingLeft = false;
            }

            //Update animation frame
            if (_internalAnimTimer <= 0)
            {
                _internalAnimTimer = walk_anim_delay;
                if (isFacingLeft)
                {
                    run[1].UpdateAnimationFrame();
                }
                else
                {
                    run[0].UpdateAnimationFrame();
                }
                
            }
            _internalAnimTimer -= (int) timeElapsedinMilliseconds;
            

            //Update target position once reached (when you are within 5 units from the target)
            if (Vector2.DistanceSquared(target.ToVector2(), globalPosition) <= 25)
            {
                if (targetIndex != path.Count - 1 && targetIndex != 0)
                {
                    //If enemy is walking "forward" on path
                    if (prevTargetIndex < targetIndex)
                    {
                        prevTargetIndex = targetIndex;
                        targetIndex++;                        
                    }
                    //If enemy is walking back along the path
                    else
                    {
                        prevTargetIndex = targetIndex;
                        targetIndex--;                        
                    }
                    
                }
                //If we need to turn around and we are at the start
                else if (targetIndex == 0)
                {
                    prevTargetIndex = targetIndex;
                    targetIndex++;
                }

                //If we are at the end of the path
                else
                {
                    prevTargetIndex = targetIndex;
                    targetIndex--;
                }
                target = path[targetIndex];
            }
        }

        public override void Display(SpriteBatch spritebatch)
        {
            if (isFacingLeft)
            {
                run[0].Display(spritebatch, localPosition);
            }
            else
            {
                run[1].Display(spritebatch, localPosition);
            }
        }
    }

}

