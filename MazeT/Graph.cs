using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Diagnostics.SymbolStore;
using System.Security.Cryptography;

namespace MazeT
{

    /// <summary>
    /// This class is used to store the data of the tile, including which other tiles it connects to
    /// It also stores whether it is a teleportation pad or not.
    /// </summary>
    internal class Tile
    {
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

        public bool below
        {
            get { return tileConnections[4]; }
            set { tileConnections[4] = value; }
        }

        public bool above
        {
            get { return tileConnections[5]; }
            set { tileConnections[5] = value; }
        }

        public Tile()
        {
            tileConnections = new bool[] { false, false, false, false, false, false };
        }

        public Tile(bool[] tileConnections)
        {
            this.tileConnections = tileConnections;
        }

        public Tile(bool up, bool down, bool left, bool right, bool below, bool above)
        {
            tileConnections = new bool[] { up, down, left, right, below, above };
        }
    }

    internal class Maze
    {
        private Tile[,,] _tiles;
        public Tile[,,] tiles { get { return _tiles; } set { _tiles = value; } }

        private int _width;
        public int width { get { return _width; } }

        private int _height;
        public int height { get { return _height; } }

        private int _currentLayer;
        public int currentLayer { get; set; }

        private int _maxLayers;
        public int maxLayers { get; set; }

        /// <summary>
        /// Size in pixels
        /// </summary>
        public readonly int tileSize = 120;

        /// <summary>
        /// Size in pixels
        /// </summary>
        public readonly int mazeWallWidth = 10;

        /// <summary>
        /// Global poisition of top left corner of maze
        /// </summary>
        public Vector2 pos = new Vector2(0, 0);
        public int xmax, ymax;

        /// <summary>
        /// Generate the maze using a chosen algorithm
        /// </summary>
        public Maze(int width, int height, int layers = 2)
        {
            _width = width;
            _height = height;
            currentLayer = 0;
            maxLayers = layers;
            _tiles = new Tile[width, height, layers]; //2 layer maze
            WilsonAlgorithm();
            xmax = tileSize * width - (int)pos.X;
            ymax = tileSize * height - (int)pos.Y;

        }

        //Generate a tester maze
        private void BlankCreation(int width, int height)
        {
            //Initialise all of the new tiles
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    _tiles[x, y, 1] = new Tile(new bool[] { true, true, true, true });
                    if (x == 0)
                    {
                        _tiles[x, y, 1].left = false;
                    }
                    else if (x == 9)
                    {
                        _tiles[x, y, 1].right = false;
                    }

                    if (y == 0)
                    {
                        _tiles[x, y, 1].up = false;
                    }
                    else if (y == 9)
                    {
                        _tiles[x, y, 1].down = false;
                    }

                    if (x == 3 && y == 4)
                    {
                        _tiles[x, y, 1].right = false;
                        _tiles[x, y, 1].left = false;
                        _tiles[x, y, 1].down = false;
                    }
                    else if (x == 7 && y == 4)
                    {
                        _tiles[x, y, 1].up = false;
                        _tiles[x, y, 1].down = false;
                    }
                }
            }
        }

        public static bool isBoolArrayFilled(bool[,,] array, bool value)
        {
            for (int x = 0; x < array.GetLength(0); x++)
            {
                for (int y = 0; y < array.GetLength(1); y++)
                {
                    for (int z = 0; z < array.GetLength(2); z++)
                        if (array[x, y, z] != value)
                        {
                            return false;
                        }
                }
            }
            return true;
        }

        private void WilsonAlgorithm()
        {
            Tile[,,] currentWalk = new Tile[width, height, maxLayers];
            bool[,,] isVisited = new bool[width, height, maxLayers];
            bool[,,] isPartOfMaze = new bool[width, height, maxLayers];

            //Initialise all the values in the arrays
            //including the member _tiles
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    for (int k = 0; k < maxLayers; k++)
                    {
                        tiles[i, j, k] = new Tile();
                        currentWalk[i, j, k] = new Tile();
                        isVisited[i, j, k] = false;
                        isPartOfMaze[i, j, k] = false;
                    }                   
                }
            }

            //Set the middle on the base floor as part of maze
            isPartOfMaze[width/2, height/2, 0] = true;

            int currentX = 0;
            int currentY = 0;
            int currentZ = 0;

            int direction = 0;
            int randomNumber = 0;
            bool hasUsedTPPad = false;
            int maxTPpads = 11;
            int totalTPPads = 0;
            int noTPPadsInCurrentWalk = 0;
            int walkLength = 0;
            Random rng = new Random();

            do
            {
                bool _continue = true;
                for (int x = 0; x < width && _continue; x++)
                {
                    for (int y = 0; y < height && _continue; y++)
                    {
                        for (int z = 0; z < maxLayers && _continue; z++)
                        {
                            if (!isPartOfMaze[x, y, z])
                            {
                                isVisited[x, y, z] = true;
                                currentX = x;
                                currentY = y;
                                currentZ = z;
                                _continue = false;
                            }
                        }
                    }
                }                

                do
                {
                    //Add the current tile to the isVisited list
                    isVisited[currentX, currentY, currentZ] = true;
                    do
                    {
                        if (!hasUsedTPPad && totalTPPads < maxTPpads && walkLength > 2)
                        {
                            //Tile can only connect layers after it has travelled for
                            //enough time across the layer
                            randomNumber = rng.Next(0, 6);
                        }
                        else
                        {
                            randomNumber = rng.Next(0, 4);
                        }
                    } while (randomNumber == direction); //Prevents the maze from backtracking on itself
                    direction = randomNumber;

                    if (direction > 3)
                    {
                        totalTPPads++;
                        noTPPadsInCurrentWalk++;
                    }
                    else
                    {
                        walkLength++;
                    }

                    if (direction == 0)
                    {
                        //Try and move up, otherwise move down
                        if (currentY != 0)
                        {
                            //Connect the tiles together
                            currentWalk[currentX, currentY, currentZ].up = true;
                            currentWalk[currentX, currentY - 1, currentZ].down = true;
                            //Go UP
                            currentY--;
                            direction = 0;

                        }
                        else
                        {
                            //Connect the tiles together
                            currentWalk[currentX, currentY, currentZ].down = true;
                            currentWalk[currentX, currentY + 1, currentZ].up = true;
                            //Go down
                            currentY++;
                            direction = 1;//Change the direction (important for later)
                        }
                    }

                    else if (direction == 1)
                    {
                        //Try and move down, otherwise move up
                        if (currentY != height - 1)
                        {
                            //Connect the tiles together
                            currentWalk[currentX, currentY, currentZ].down = true;
                            currentWalk[currentX, currentY + 1, currentZ].up = true;
                            //Go down
                            currentY++;
                            direction = 1;
                        }
                        else
                        {
                            //Connect the tile
                            currentWalk[currentX, currentY, currentZ].up = true;
                            currentWalk[currentX, currentY - 1, currentZ].down = true;
                            //Go up
                            currentY--;
                            direction = 0;
                        }
                    }

                    else if (direction == 2)
                    {
                        //Try and move right, otherwise move left
                        if (currentX != width - 1)
                        {
                            //Connect the tiles together
                            currentWalk[currentX, currentY, currentZ].right = true;
                            currentWalk[currentX + 1, currentY, currentZ].left = true;
                            //Go Right
                            currentX++;
                            direction = 3;
                        }
                        else
                        {
                            //Connect the tiles together
                            currentWalk[currentX, currentY, currentZ].left = true;
                            currentWalk[currentX - 1, currentY, currentZ].right = true;
                            //Go left
                            currentX--;
                            direction = 2;
                        }
                    }

                    else if (direction == 3)
                    {
                        //Try and move left, otherwise move right
                        if (currentX != 0)
                        {
                            //Connect the tiles together
                            currentWalk[currentX, currentY, currentZ].left = true;
                            currentWalk[currentX - 1, currentY, currentZ].right = true;
                            //Go left
                            currentX--;
                            direction = 2;
                        }
                        else
                        {
                            //Connect the tiles together
                            currentWalk[currentX, currentY, currentZ].right = true;
                            currentWalk[currentX + 1, currentY, currentZ].left = true;
                            //Go right
                            currentX++;
                            direction = 3;
                        }
                    }

                    else if (direction == 4)
                    {
                        //Try going to the layer below
                        if (currentZ != 0)
                        {
                            //Connect the tiles together
                            currentWalk[currentX, currentY, currentZ].below = true;
                            currentWalk[currentX, currentY, currentZ - 1].above = true;
                            //Go to layer below
                            currentZ--;
                            direction = 4;
                        }
                        else
                        {
                            //Connect the tiles together
                            currentWalk[currentX, currentY, currentZ].above = true;
                            currentWalk[currentX, currentY, currentZ + 1].below = true;
                            //Go to layer above
                            currentZ++;
                            direction = 5;
                        }

                    }

                    else
                    {
                        //Try going to the layer above
                        if (currentZ < maxLayers - 1)
                        {
                            //Connect the tiles together
                            currentWalk[currentX, currentY, currentZ].above = true;
                            currentWalk[currentX, currentY, currentZ + 1].below = true;
                            //Go to layer above
                            currentZ++;
                            direction = 5;
                        }
                        else
                        {
                            //Connect the tiles together
                            currentWalk[currentX, currentY, currentZ].below = true;
                            currentWalk[currentX, currentY, currentZ - 1].above = true;
                            //Go to layer below
                            currentZ--;
                            direction = 4;
                        }

                    }

                    if (isVisited[currentX, currentY, currentZ])
                    {
                        //Reset the current walk since we have now looped back to our original position
                        for (int x = 0; x < width; x++)
                        {
                            for (int y = 0; y < height; y++)
                            {
                                for (int z = 0; z < maxLayers; z++)
                                {
                                    isVisited[x, y, z] = false;
                                    currentWalk[x, y, z].tileConnections = new bool[] { false, false, false, false, false, false };
                                    
                                }
                            }
                        }
                        totalTPPads -= noTPPadsInCurrentWalk;
                        noTPPadsInCurrentWalk = 0;
                        walkLength = 0;
                    }

                } while (!isPartOfMaze[currentX, currentY, currentZ]);

                //Add all the tiles to the current walk
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        for (int z = 0; z < maxLayers; z++)
                        {
                            if (isVisited[x, y, z])
                            {
                                currentWalk[x, y, z].tileConnections.CopyTo(_tiles[x, y, z].tileConnections, 0);
                                isPartOfMaze[x, y, z] = true;
                                isVisited[x, y, z] = false;
                                currentWalk[x, y, z].tileConnections= new bool[] { false, false, false, false, false, false };
                            }

                        }
                    }
                }
                //Add the current tile to the maze to update the new connections
                //Flip the direction since it will not connect correctly otherwise
                //We can do this by flipping the last bit. 
                direction = direction ^ 1;
                _tiles[currentX, currentY, currentZ].tileConnections[direction] = true;
                hasUsedTPPad = false;
                noTPPadsInCurrentWalk = 0;
                walkLength = 0;

            } while (!isBoolArrayFilled(isPartOfMaze, true));
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="spriteBatch">The spritebatch being used</param>
        /// <param name="rectColour">The one pixel used to display a rectangle</param>
        public void displayMaze(SpriteBatch spriteBatch, Texture2D rectColour, int layer)
        {
            //Assumes one has begun the sprite batch
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    if (!_tiles[x, y, layer].up)
                    {
                        //Draw a rectangle above the tile
                        spriteBatch.Draw(rectColour, new Rectangle(tileSize * x + (int) pos.X, tileSize * y + (int) pos.Y, tileSize, mazeWallWidth), Color.White);
                    }
                    if (!_tiles[x, y, layer].down)
                    {
                        //Draw a rectangle below the tile
                        spriteBatch.Draw(rectColour, new Rectangle(tileSize * x + (int) pos.X, tileSize * (y + 1) - (int)pos.Y - mazeWallWidth, tileSize, mazeWallWidth), Color.White);
                    }
                    if (!_tiles[x, y, layer].left)
                    {
                        //Draw a rectangle to the left of the tile
                        spriteBatch.Draw(rectColour, new Rectangle(tileSize * x - (int)pos.X, tileSize * y - (int)pos.Y, mazeWallWidth, tileSize), Color.White);
                    }
                    if (!_tiles[x, y, layer].right)
                    {
                        //Draw a rectangle to the right of the tile
                        spriteBatch.Draw(rectColour, new Rectangle(tileSize * (x + 1) - (int)pos.X - mazeWallWidth, tileSize * y - (int)pos.Y, mazeWallWidth, tileSize), Color.White);
                    }
                }
            }

            //End the spritebatch afterwards.
        }

        //Display current layer
        public void displayMaze(SpriteBatch spriteBatch, Texture2D rectColour, Texture2D TPColour)
        {
            //Assumes one has begun the sprite batch
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    if (!_tiles[x, y, currentLayer].up)
                    {
                        //Draw a rectangle above the tile
                        spriteBatch.Draw(rectColour, new Rectangle(tileSize * x - (int)pos.X, tileSize * y - (int)pos.Y, tileSize, mazeWallWidth), Color.White);
                    }
                    if (!_tiles[x, y, currentLayer].down)
                    {
                        //Draw a rectangle below the tile
                        spriteBatch.Draw(rectColour, new Rectangle(tileSize * x - (int)pos.X, tileSize * (y + 1) - (int)pos.Y - mazeWallWidth, tileSize, mazeWallWidth), Color.White);
                    }
                    if (!_tiles[x, y, currentLayer].left)
                    {
                        //Draw a rectangle to the left of the tile
                        spriteBatch.Draw(rectColour, new Rectangle(tileSize * x - (int)pos.X, tileSize * y - (int)pos.Y, mazeWallWidth, tileSize), Color.White);
                    }
                    if (!_tiles[x, y, currentLayer].right)
                    {
                        //Draw a rectangle to the right of the tile
                        spriteBatch.Draw(rectColour, new Rectangle(tileSize * (x + 1) - (int)pos.X - mazeWallWidth, tileSize * y - (int)pos.Y, mazeWallWidth, tileSize), Color.White);
                    }
                    if (_tiles[x, y, currentLayer].above || _tiles[x, y, currentLayer].below)
                    {
                        //Draw a coloured rectangle in the middle of the screen
                        spriteBatch.Draw(TPColour, new Rectangle(tileSize * x - (int)pos.X + mazeWallWidth, tileSize * y - (int)pos.Y + mazeWallWidth, tileSize-mazeWallWidth*2, tileSize - mazeWallWidth*2), Color.White);
                    }
                }
            }

            //End the spritebatch afterwards.
        }
    }
}
