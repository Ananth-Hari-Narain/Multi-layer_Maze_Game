using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

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
        //This is used as the floating point position of the enemies for smoother movement
        public Vector2 position; 
        public Vector2 velocity;

        /// <summary>
        /// This is the function that will handle collision with walls for all objects 
        /// in the maze game.
        /// </summary>
        public void HandleWallCollision(Rectangle[] wall_rects)
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
        public Player(int width, int height, int x, int y)
        {
            collision_rect = new Rectangle(x, y, width, height);
            position = new Vector2(x, y);
            velocity = new Vector2(0, 0);
            for (int i = 0; i < 4; i++)
            {
                walk[i] = new AnimatedSpriteSheet(128, 128, 4, 0, 128 * i);
            }
        }

        public void Update(KeyboardState currentKeys, KeyboardState previousKeys, double timeElapsed)
        {
            int walkAnimationDelay = 200;
            int playerSpeed = 2;
            if (currentKeys.IsKeyDown(Keys.LeftShift) || currentKeys.IsKeyDown(Keys.RightShift))
            {
                playerSpeed = 4;
                walkAnimationDelay = 120;
            }
            if (currentKeys.IsKeyDown(Keys.Up))
            {
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
            else if (currentKeys.IsKeyDown(Keys.Down))
            {
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
            else if (currentKeys.IsKeyDown(Keys.Right))
            {
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
            else if (currentKeys.IsKeyDown(Keys.Left))
            {
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
            else
            {
                velocity.X = 0;
                velocity.Y = 0;
                playerState = PlayerState.IDLE;
                walk[(int) direction].ResetAnimation();
            }

            position += velocity;

            internalTimer -= timeElapsed;
        }

        public void Display(SpriteBatch spriteBatch)
        {
            if (playerState == PlayerState.WALKING)
            {
                walk[(int)direction].Display(spriteBatch, collision_rect.Center);
            }            
            else if (playerState == PlayerState.IDLE)
            {
                walk[(int)direction].Display(spriteBatch, collision_rect.Center, 1);
            }
        }
    }
}

