using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace MazeT
{
    
    enum TileTypes
    {
        TP_PAD = 0,
        BLANK = 1
    };

    enum MazeAlgorithms
    {
        BLANK = 0,
        WILSON = 1,
        PRIM = 2,
        RandomisedDFS = 3
    }

    /// <summary>
    /// This class is used to store the data of the tile, including which other tiles it connects to
    /// It also stores whether it is a teleportation pad or not.
    /// </summary>
    internal class Tile
    {
        //Is the tile special or not
        private TileTypes _tileType;
        public TileTypes tileType { get {  return _tileType; } }
        
        //An adjacency list of sorts
        /// <summary>
        /// Checks if the tile is connected to tile up, down, left, or right
        /// in that order
        /// </summary>
        public bool[] tileConnections;

        public bool up
        {
            get { return tileConnections[0]; }
            set { tileConnections[0] = value; }
        }

        public bool down
        {
            get { return tileConnections[1]; }
            set { tileConnections[1] = value; }
        }

        public bool left
        {
            get { return tileConnections[2]; }
            set { tileConnections[2] = value; }
        }

        public bool right
        {
            get { return tileConnections[3]; }
            set { tileConnections[3] = value; }
        }

        public Tile()
        {
            _tileType = TileTypes.BLANK;
            tileConnections = new bool[] { false, false, false, false };
        }

        public Tile(bool[] tileConnections, TileTypes tileType = TileTypes.BLANK)
        {
            _tileType = tileType;
            this.tileConnections = tileConnections;
        }

        public Tile(bool up, bool down, bool left, bool right, TileTypes tileType = TileTypes.BLANK)
        {
            _tileType = tileType;
            tileConnections = new bool[]{ up, down, left, right };
        }      
    }

    internal class Maze
    {
        private Tile[,] _tiles;
        public Tile[,] tiles { get { return _tiles; } }

        private int _width;
        public int width { get { return _width; } }

        private int _height;
        public int height { get { return _height; } }

        /// <summary>
        /// Size in pixels
        /// </summary>
        public readonly int tileSize = 60;

        /// <summary>
        /// Size in pixels
        /// </summary>
        public readonly int mazeWallWidth = 10;

        /// <summary>
        /// Where the topleft corner of the maze is positioned
        /// </summary>
        public readonly int xOffset = 30;

        /// <summary>
        /// Where the topleft corner of the maze is positioned
        /// </summary>
        public readonly int yOffset = 30;

        /// <summary>
        /// Generate the maze using a chosen algorithm
        /// </summary>
        public Maze(int width, int height, MazeAlgorithms algorithm = MazeAlgorithms.BLANK)
        {
            _width = width;
            _height = height;
            _tiles = new Tile[width, height];
            WilsonAlgorithm(width, height);
        }

        //Generate a tester maze
        private void BlankCreation(int width, int height)
        {
            _tiles.Initialize();
            //Initialise all of the new tiles
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    _tiles[x, y] = new Tile(new bool[] { true, true, true, true });
                    if (x == 0)
                    {
                        _tiles[x, y].left = false;
                    }
                    else if (x == 9)
                    {
                        _tiles[x, y].right = false;
                    }

                    if (y == 0)
                    {
                        _tiles[x, y].up = false;
                    }
                    else if (y == 9)
                    {
                        _tiles[x, y].down = false;
                    }

                    if (x == 3 && y == 4)
                    {
                        _tiles[x, y].right = false;
                        _tiles[x, y].left = false;
                        _tiles[x, y].down = false;
                    }
                    else if (x == 7 && y == 4)
                    {
                        _tiles[x, y].up = false;
                        _tiles[x, y].down = false;
                    }
                }
            }
        }

        private static bool isBoolArrayFilled(bool[,] array, bool value)
        {
            for (int x = 0; x < array.GetLength(0); x++)
            {
                for (int y = 0; y < array.GetLength(1); y++)
                {
                    if (array[x, y] != value)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private void WilsonAlgorithm(int width, int height)
        {
            
            Tile[,] currentWalk = new Tile[width, height];
            bool[,] isVisited = new bool[width, height];
            bool[,] isPartOfMaze = new bool[width, height];

            //Initialise all the values in the arrays
            //including the member _tiles
            for (int i = 0; i < width; i++)
            {
                for (int j=0; j < height; j++)
                {
                    tiles[i, j] = new Tile();
                    currentWalk[i, j] = new Tile();
                    isVisited[i, j] = false;
                    isPartOfMaze[i, j] = false;
                }
            }

            //Choose a random cell to be the first cell in the maze
            Random rng = new Random();
            int initialX = rng.Next(width/2, width);
            int initialY = rng.Next(height/2, height);
            isPartOfMaze[initialX, initialY] = true;

            int currentX = 0;
            int currentY = 0;

            int direction;

            int test = 0;
            do
            {
                //Start at a point that's not part of the maze, setting the value of isVisited to true.
                bool continueForLoop = true;
                for (int  x = 0; x < width && continueForLoop; x++)
                {
                    for (int y = 0; y < height && continueForLoop; y++)
                    {
                        if (!isPartOfMaze[x, y])
                        {
                            //Add that to the isVisited list
                            isVisited[x, y] = true;
                            currentX = x;
                            currentY = y;
                            continueForLoop = false;
                        }
                    }
                }

                do
                {
                    //Add the current node to the isVisited list
                    isVisited[currentX, currentY] = true;
                    //Choose a random adjacent cell to the current tile.
                    direction = rng.Next(0, 4);
                    if (direction == 0)
                    {
                        //Try and move up, otherwise move down
                        if (currentY != 0)
                        {
                            //Connect the tiles together
                            currentWalk[currentX, currentY].up = true;
                            currentWalk[currentX, currentY - 1].down = true;
                            //Go UP
                            currentY--;
                            
                        }
                        else
                        {
                            //Connect the tiles together
                            currentWalk[currentX, currentY].down = true;
                            currentWalk[currentX, currentY+1].up = true;
                            //Go down
                            currentY++;
                            direction = 1;//Change the direction (important for later)
                        }
                    }

                    else if (direction == 1)
                    {
                        //Try and move down, otherwise move up
                        if (currentY != height-1)
                        {
                            //Connect the tiles together
                            currentWalk[currentX, currentY].down = true;
                            currentWalk[currentX, currentY+1].up = true;
                            //Go down
                            currentY++;
                        }
                        else
                        {
                            //Connect the tile
                            currentWalk[currentX, currentY].up = true;
                            currentWalk[currentX, currentY-1].down = true;
                            //Go up
                            currentY--;
                            direction = 0;
                        }
                    }

                    else if (direction == 3)
                    {
                        //Try and move right, otherwise move left
                        if (currentX != width-1)
                        {
                            //Connect the tiles together
                            currentWalk[currentX, currentY].right = true;
                            currentWalk[currentX+1, currentY].left = true;
                            //Go Right
                            currentX++;
                        }
                        else
                        {
                            //Connect the tiles together
                            currentWalk[currentX, currentY].left = true;
                            currentWalk[currentX-1, currentY].right = true;
                            //Go left
                            currentX--;
                            direction = 2;
                        }
                    }

                    //If direction == 2
                    else
                    {
                        //Try and move left, otherwise move right
                        if (currentX != 0)
                        {
                            //Connect the tiles together
                            currentWalk[currentX, currentY].left = true;
                            currentWalk[currentX-1, currentY].right = true;
                            //Go left
                            currentX--;
                        }
                        else
                        {
                            //Connect the tiles together
                            currentWalk[currentX, currentY].right = true;
                            currentWalk[currentX+1, currentY].left = true;
                            //Go right
                            currentX++;
                            direction = 3;
                        }
                    }

                    //Determine if the current tile has been visited or not
                    if (isVisited[currentX, currentY])
                    {
                        // If it has been visited already, reset all the tiles visited 
                        // during the random walk. 
                        for (int i = 0; i < width; i++)
                        {
                            for (int j = 0; j < height; j++)
                            {
                                currentWalk[i, j] = new Tile();
                                isVisited[i, j] = false;
                            }
                        }
                    }                    

                } while (!isPartOfMaze[currentX, currentY]);

                //Now add all of the tiles into the maze
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        //Add it to the maze and reset the tile
                        if (isVisited[x, y])
                        {
                            currentWalk[x, y].tileConnections.CopyTo(_tiles[x, y].tileConnections, 0);                            
                            currentWalk[x, y].tileConnections = new bool[]{ false, false, false, false};
                            isVisited[x, y] = false;
                            isPartOfMaze[x, y] = true;
                        }
                    }
                }
                //Add the current tile to the maze to update the new connections
                //Flip the direction since it will not connect correctly otherwise
                //We can do this by flipping the last bit. 
                direction = direction ^ 1;
                _tiles[currentX, currentY].tileConnections[direction] = true;
                test++;
            } while (!isBoolArrayFilled(isPartOfMaze, true));
                                    
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="spriteBatch">The spritebatch being used</param>
        /// <param name="rectColour">The one pixel used to display a rectangle</param>
        public void displayMaze(SpriteBatch spriteBatch, Texture2D rectColour)
        {
            //Assumes one has begun the sprite batch
            for (int x = 0; x < _width; x++)
            {
                for (int y  = 0; y < _height; y++)
                {
                    if (!_tiles[x, y].up)
                    {
                        //Draw a rectangle above the tile
                        spriteBatch.Draw(rectColour, new Rectangle(tileSize * x + xOffset, tileSize * y + yOffset, tileSize, mazeWallWidth), Color.White);
                    }
                    if (!_tiles[x, y].down)
                    {
                        //Draw a rectangle below the tile
                        spriteBatch.Draw(rectColour, new Rectangle(tileSize * x + xOffset, tileSize * (y + 1) + yOffset-mazeWallWidth, tileSize, mazeWallWidth), Color.White);
                    }
                    if (!_tiles[x, y].left)
                    {
                        //Draw a rectangle to the left of the tile
                        spriteBatch.Draw(rectColour, new Rectangle(tileSize * x + xOffset, tileSize * y + yOffset, mazeWallWidth, tileSize), Color.White);
                    }
                    if (!_tiles[x, y].right)
                    {
                        //Draw a rectangle to the right of the tile
                        spriteBatch.Draw(rectColour, new Rectangle(tileSize * (x+1) + xOffset-mazeWallWidth, tileSize * y + yOffset, mazeWallWidth, tileSize), Color.White);
                    }
                }
            }

            //End the spritebatch afterwards.
        }
    }
}
