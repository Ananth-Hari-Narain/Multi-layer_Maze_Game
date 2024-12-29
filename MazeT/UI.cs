using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MazeT
{
    /// <summary>
    /// These are rectangular buttons that will be used for
    /// the UI.
    /// </summary>
    internal class Button: StandardSprite2D
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

    ///// <summary>
    ///// This class will be used to represent the power up box, 
    ///// which is used on one screen
    ///// </summary>
    //internal class PowerUpBox
    //{
    //    public Button button;
    //    public string description;
    //    public bool is_alive; //Determines if the power up box has been used or not.
    //    public int type; //Correspond

    //    public void Display(SpriteBatch sprite_batch)
    //    {
    //        button.Display(sprite_batch);
    //    }
    //}
}
