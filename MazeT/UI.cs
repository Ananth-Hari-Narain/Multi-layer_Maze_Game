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
}
