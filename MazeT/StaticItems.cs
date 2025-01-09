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

        public virtual void Display(SpriteBatch spritebatch)
        {
            spritebatch.Draw(image, rect, Color.White);
        }
    }

    enum CollectibleType
    {
        TREASURE_CHEST,
        HEAL,
        DAMAGEUP,
        ATTACKSPEEDUP,
        SPEEDUP
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
        public double value;
        private bool is_collected;
        //This is used to determine how long the collectible will last once it has been collected.
        private int self_kill_timer;
        public Vector2 global_position;

        public Collectible(CollectibleType type, double value, Point position)
        {
            this.type = type;
            this.value = value;
            global_position = position.ToVector2();
        }

        //Set dimensions of rectangle
        public void SetRect(Point pos)
        {
            //We know the sprite is 51x63 pixels
            rect = new Rectangle(pos.X, pos.Y, 51, 63);
        }

        public void Update(int time_elapsed, Vector2 maze_pos)
        {
            if (is_collected)
            {
                self_kill_timer -= time_elapsed;
            }
            else
            {
                //Update local position
                rect.Offset(maze_pos);
            }

        }

        public void UpdateRectanglePosition(Vector2 maze_pos)
        {
            rect.Location = (global_position - maze_pos).ToPoint();
        }

        public bool IsAlive()
        {
            //If the collectible has expired and is not a permanent power-up
            if (self_kill_timer < 0)
            {
                return false;
            }
            return true;
        }

        public void BeCollected()
        {
            is_collected = true;
            if (type == CollectibleType.HEAL 
                || type == CollectibleType.TREASURE_CHEST
                || type == CollectibleType.DAMAGEUP)
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
            if (!is_collected)
            {
                base.Display(spritebatch);
            }
        }
    }

    /// <summary>
    /// These are rectangular buttons that will be used for the UI.
    /// </summary>
    internal class Button : StandardSprite2D
    {
        public Button(Point position, Texture2D background)
        {
            //Background of button likely to be same for all buttons
            image = background;
            rect = image.Bounds;
            rect.Location = position;
        }

        public bool IsMouseOnButton(Point mouse_position)
        {
            return rect.Contains(mouse_position);
        }
    }
}
