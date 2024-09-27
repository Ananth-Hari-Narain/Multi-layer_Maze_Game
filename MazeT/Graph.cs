using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

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
        /// Checks if the tile is connected to tile up, down, left, right, below or above
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
        public int currentLayer { get; set; }
        public int maxLayers { get; set; }

        public Texture2D mazeWallH;
        public Texture2D mazeWallV;
        public Texture2D mazeFloor;
        public Texture2D staircase;
        private Rectangle[] wallRects; //to help divide up the sprite sheet
        private Rectangle[] floorRects; //to help divide up the sprite sheet
        public List<Rectangle>[] collisionRects;
        public List<Rectangle>[] TP_Pads;

        /// <summary>
        /// Size in pixels
        /// </summary>
        public readonly int tileSize = 128;

        /// <summary>
        /// Size in pixels
        /// </summary>
        public readonly int mazeWallWidth = 16;

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
            collisionRects = new List<Rectangle>[maxLayers];
            TP_Pads = new List<Rectangle>[maxLayers];
            WilsonAlgorithm();
            xmax = tileSize * width - (int)pos.X;
            ymax = tileSize * height - (int)pos.Y;

            //Initialise the collision rect array
            for (int z = 0; z < maxLayers; z++)
            {
                collisionRects[z] = new List<Rectangle>();
                TP_Pads[z] = new List<Rectangle>();
            }
            setMazeRectangles();

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
                //Flip the direction since the current walk will not connect to
                //the maze correctly. We can do this by flipping the last bit,
                //due to how I have set out the connections array in the tile class.                
                direction = direction ^ 1;
                _tiles[currentX, currentY, currentZ].tileConnections[direction] = true;
                hasUsedTPPad = false;
                noTPPadsInCurrentWalk = 0;
                walkLength = 0;

            } while (!isBoolArrayFilled(isPartOfMaze, true));
            
        }
        

        public void displayMaze(SpriteBatch spriteBatch)
        {
            //Rename "offset variables"
            const int offsetY = 128;
            const int offsetX = 128;
            const int tileW = 64;
            const int wallVWidth = 32;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {                    
                    spriteBatch.Draw(mazeFloor, new Vector2(x * offsetX - pos.X, y * offsetY + tileW - pos.Y), Color.White);
                    spriteBatch.Draw(mazeFloor, new Vector2(x * offsetX - pos.X, y * offsetY + 2 * tileW - pos.Y), Color.White);
                    spriteBatch.Draw(mazeFloor, new Vector2(x * offsetX + tileW - pos.X, y * offsetY + tileW - pos.Y), Color.White);
                    spriteBatch.Draw(mazeFloor, new Vector2(x * offsetX + tileW - pos.X, y * offsetY + 2 * tileW - pos.Y), Color.White);
                    
                    if (_tiles[x, y, currentLayer].above || _tiles[x, y, currentLayer].below)
                    {
                        spriteBatch.Draw(staircase, new Vector2(x * offsetX - pos.X, y * offsetY + tileW - pos.Y), Color.White);
                    }

                    //If we are at the top
                    if (y == 0)
                    {
                        spriteBatch.Draw(mazeWallH, new Vector2(x * offsetX - pos.X, -pos.Y), Color.White);
                        spriteBatch.Draw(mazeWallH, new Vector2(x * offsetX + tileW - pos.X, -pos.Y), Color.White);
                    }

                    if (_tiles[x, y, currentLayer].down == false)
                    {
                        spriteBatch.Draw(mazeWallH, new Vector2(x * offsetX - pos.X, (y + 1) * offsetY - pos.Y), Color.White);
                        spriteBatch.Draw(mazeWallH, new Vector2(x * offsetX + tileW - pos.X, (y + 1) * offsetY - pos.Y), Color.White);
                    }
                }
            }

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    //Display right walls (if applicable)  
                    if (!_tiles[x, y, currentLayer].right)
                    {
                        spriteBatch.Draw(mazeWallV, new Vector2((x + 1) * offsetX - wallVWidth - pos.X, y * offsetY - pos.Y), Color.White);
                        spriteBatch.Draw(mazeWallV, new Vector2((x + 1) * offsetX - wallVWidth - pos.X, y * offsetY + tileW - pos.Y), Color.White);
                        spriteBatch.Draw(mazeWallV, new Vector2((x + 1) * offsetX - wallVWidth - pos.X, y * offsetY + 2 * tileW - pos.Y), Color.White);
                    }                   

                    //If we are on the leftmost side of the maze
                    if (x == 0)
                    {
                        spriteBatch.Draw(mazeWallV, new Vector2(-pos.X, y * offsetY - pos.Y), Color.White);
                        spriteBatch.Draw(mazeWallV, new Vector2(-pos.X, y * offsetY + tileW - pos.Y), Color.White);
                    }
                }
            }
        }

        /// <summary>
        /// This function is used to create the list of collision rectangles
        /// </summary>
        public void setMazeRectangles()
        {
            //These constants help to form the dimensions of the maze
            const int offsetY = 128;
            const int offsetX = 128;
            const int tileW = 64;
            const int wallVWidth = 32;

            //Generate the top and left hand-side rectangles
            for (int z = 0; z < maxLayers; z++)
            {
                collisionRects[z].Add(new Rectangle(0, 0, (width - 1) * offsetX + tileW * 2, 20));
                collisionRects[z].Add(new Rectangle(0, 0, wallVWidth, (height - 1) * offsetY + tileW * 2));
            }            

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int z = 0; z < maxLayers; z++)
                    {
                        //Add bottom rectangles (this code should mirror display code)
                        if (_tiles[x, y, z].down == false)
                        {
                            collisionRects[z].Add(new Rectangle(
                                x * offsetX,
                                (y + 1) * offsetY, 
                                tileW * 2, 
                                20
                                ));
                        }     
                        
                        //Add right rectangles
                        if (_tiles[x, y, z].right == false)
                        {
                            collisionRects[z].Add(new Rectangle(
                                (x + 1) * offsetX - wallVWidth,
                                y * offsetY, 
                                wallVWidth, 
                                tileW * 3 - 44
                                ));
                        }

                        //Add teleportation pads
                        if (_tiles[x, y, z].above || _tiles[x, y, z].below)
                        {
                            TP_Pads[z].Add(new Rectangle(
                                x * 128,
                                y * 128,
                                100,
                                100
                                ));
                        }
                    }                    
                }
            }
        } 

        public void UpdateMazeRects(Vector2 deltaPos)
        {
            
            for (int z = 0; z < maxLayers; z++)
            {
                for (int i = 0; i < collisionRects[z].Count; i++)
                {
                    Rectangle rect = collisionRects[z][i];
                    rect.Offset(deltaPos);
                    collisionRects[z][i] = rect;                    
                }
            }

            for (int z = 0; z < maxLayers; z++)
            {
                for (int i = 0; i < TP_Pads[z].Count; i++)
                {
                    Rectangle rect = TP_Pads[z][i];
                    rect.Offset(deltaPos);
                    TP_Pads[z][i] = rect;
                }
            }
        }        
        

        public List<Point> GenerateSingleLayerPath(Point start, int pathlength, int layer)
        {
            List<List<Point>> all_paths = new();
            List<Point> beginning = new() { start };
            GenerateSingleLayerPaths(ref all_paths, beginning, pathlength, -1, layer);
            //Choose a random path from the list
            Random random = new();
            List<Point> chosen_path = all_paths[random.Next(all_paths.Count)];

            //Convert list indices to global coordinates.
            for (int i = 0; i < chosen_path.Count; i++)
            {
                chosen_path[i] = new Point(chosen_path[i].X * 128 + 64, chosen_path[i].Y * 128 + 64);
            }

            return chosen_path;
        }

        //Recursive algorithm to generate paths for an enemy.
        //Returns a list of points in a path
        private void GenerateSingleLayerPaths(ref List<List<Point>> all_paths, List<Point> currentPath, int pathlength, int prevDirection, int layer)
        {
            Point currentNode = currentPath[currentPath.Count - 1];
            Tile currentTile = tiles[currentNode.X, currentNode.Y, layer];            

            if (pathlength > 0)
            {
                if (currentTile.up == true && prevDirection != 1)
                {

                    currentPath.Add(new Point(currentNode.X, currentNode.Y - 1));
                    GenerateSingleLayerPaths(ref all_paths, currentPath, pathlength - 1, 0, layer);
                    currentPath.RemoveAt(currentPath.Count - 1);
                }
                if (currentTile.down == true && prevDirection != 0)
                {
                    currentPath.Add(new Point(currentNode.X, currentNode.Y + 1));
                    GenerateSingleLayerPaths(ref all_paths, currentPath, pathlength - 1, 1, layer);
                    currentPath.RemoveAt(currentPath.Count - 1);
                }
                if (currentTile.left == true && prevDirection != 3)
                {
                    currentPath.Add(new Point(currentNode.X - 1, currentNode.Y));
                    GenerateSingleLayerPaths(ref all_paths, currentPath, pathlength - 1, 2, layer);
                    currentPath.RemoveAt(currentPath.Count - 1);
                }
                if (currentTile.right == true && prevDirection != 2)
                {
                    currentPath.Add(new Point(currentNode.X + 1, currentNode.Y));
                    GenerateSingleLayerPaths(ref all_paths, currentPath, pathlength - 1, 3, layer);
                    currentPath.RemoveAt(currentPath.Count - 1);
                }
                //else if (!(currentTile.up == true && prevDirection != 1) && !(currentTile.down == true && prevDirection != 0) && !(currentTile.left == true && prevDirection != 3))
                //{
                //    List<Point> newPath = new();
                //    foreach (Point p in currentPath)
                //    {
                //        newPath.Add(new Point(p.X, p.Y));
                //    }
                //    all_paths.Add(newPath);
                //}
            }
            else
            {
                List<Point> newPath = new();
                foreach (Point p in currentPath)
                {
                    newPath.Add(new Point(p.X, p.Y));
                }
                all_paths.Add(newPath);
            }

            
        }
        public string DrawPath(List<Point> path)
        {
            string output = "";
            foreach (Point p in path)
            {
                output += $"({p.X / 128},{p.Y /128}) ";
            }
            return output;
        }


        public void DrawPath(List<Point> path, SpriteBatch spriteBatch, Texture2D colour)
        {
            int height;
            int width;
            int top;
            int left;
            for (int i = 1; i < path.Count; i++)
            {
                if (path[i].Y < path[i - 1].Y)
                {
                    width = 20;
                    height = 128;
                    top = path[i].Y - (int) pos.Y;
                    left = path[i].X - (int) pos.X;
                }
                else if (path[i].Y > path[i - 1].Y)
                {
                    width = 20;
                    height = 128;
                    top = path[i - 1].Y - (int)pos.Y;
                    left = path[i - 1].X - (int)pos.X;
                }
                else if (path[i].X < path[i-1].X)
                {
                    width = 128;
                    height = 20;                   
                    left = path[i].X - (int)pos.X;
                    top = path[i].Y - (int)pos.Y;
                }

                else
                {
                    width = 128;
                    height = 20;
                    left = path[i - 1].X - (int)pos.X;
                    top = path[i - 1].Y - (int)pos.Y;
                }

                spriteBatch.Draw(colour, new Rectangle(left, top, width, height), Color.White);
            }
        }

        /// <summary>
        /// This is a tester function that will display the rectangles.
        /// </summary>
        public void displayRects(SpriteBatch spriteBatch, Texture2D rectColour)
        {
            foreach (Rectangle rect in collisionRects[currentLayer])
            {
                spriteBatch.Draw(rectColour, rect, Color.White);
            }
        }
    }
}
