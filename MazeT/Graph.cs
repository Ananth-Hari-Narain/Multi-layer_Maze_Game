using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public readonly int tileSize = 20;

        /// <summary>
        /// Size in pixels
        /// </summary>
        public readonly int mazeWallWidth = 5;

        /// <summary>
        /// Generate the maze using a chosen algorithm
        /// </summary>
        public Maze(int width, int height, MazeAlgorithms algorithm = MazeAlgorithms.BLANK)
        {
            _width = width;
            _height = height;
            _tiles = BlankCreation(width, height);
        }

        //Generate a tester maze
        private Tile[,] BlankCreation(int width, int height)
        {
            Tile[,] newTiles = new Tile[width, height];
            for (int i = 0; i <_height; i++)
            {
                newTiles[1, i].left = false;                
            }
            return newTiles;
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
                        //Draw a rectangle upwards
                        spriteBatch.Draw(rectColour, new Rectangle(20 * x, 5 * y, tileSize, mazeWallWidth), Color.White);
                    }
                }
            }
        }
    }
}
