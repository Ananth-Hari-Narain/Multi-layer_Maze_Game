using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace MazeT
{
    /// <summary>
    /// This class is used as the base class for objects that do not have animation.
    /// </summary>
    internal class StandardSprite2D
    {
        public Texture2D image;
        public Rectangle rect;

        //Set the rectangle dimesions to perfectly encompass the image        
        public void SetRect(Point pos)
        {
            rect = image.Bounds;
            rect.Location = pos;
        }       

        public virtual void Display(SpriteBatch spritebatch)
        {
            spritebatch.Draw(image, rect, Color.White);
        }
    }

    enum CollectibleType
    {
        STANDARD,
        HEAL,
        DAMAGEUP,
        ATTACKSPEEDUP,
        SPEEDUP,
        SWORDRANGEUP
    }

    /// <summary>
    /// This class contains the code used for all of the items that the player can 
    /// collect in the maze. This includes power-ups as well. Some of these will last
    /// for a certain amount of time, some will be permanent.
    /// </summary>
    internal class Collectible : StandardSprite2D
    {
        /*
        Since the code for each collectible is very similar,
        we can use an enumeration to determine what each collectible object
        does(e.g. if it heals the player or increases damage or is just there
        to encourage exploration). 
        */

        public CollectibleType type;
        public int value;
        public bool isCollected;
        //This is used to determine how long the collectible will last once it has been collected.
        private int self_kill_timer;
        public Vector2 global_position;

        public Collectible(CollectibleType type, int value)
        {
            this.type = type;
            this.value = value;
        }

        public void Update(int time_elapsed)
        {
            if (isCollected)
            {
                self_kill_timer -= time_elapsed;
            }            
        }

        public void UpdateRectanglePosition(Vector2 maze_pos)
        {
            rect.Location = (global_position - maze_pos).ToPoint();
        }

        public bool IsAlive()
        {
            //If the collectible has expired and is not a permanent power-up
            if (self_kill_timer < 0 && type != CollectibleType.DAMAGEUP && type != CollectibleType.SWORDRANGEUP)
            {
                return false;
            }
            return true;
        }

        public void GetCollected()
        {
            if (type == CollectibleType.HEAL || type == CollectibleType.STANDARD)
            {
                //This collectible "dies" instantly as it is a one use item
                self_kill_timer = 1; 
            }
            else 
            {
                self_kill_timer = 20000; //Lasts for 20 seconds.
            }
        }

        public override void Display(SpriteBatch spritebatch)
        {
            if (!isCollected)
            {
                base.Display(spritebatch);
            }
        }
    }
}
