using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Versioning;
using Microsoft.Xna.Framework.Input;
using System.Collections;
using System.Linq;

namespace MazeT
{

    /// <summary>
    /// This class is used to store the data of the tile, including which other tiles it connects to.
    /// </summary>
    internal class Tile
    {
        //An adjacency list of sorts
        /// <summary>
        /// Checks if the tile is connected to tile up, down, left, right, below or above
        /// in that order
        /// </summary>
        public bool[] tileConnections;

        public bool Up
        {
            get { return tileConnections[0]; }
            set { tileConnections[0] = value; }
        }

        public bool Down
        {
            get { return tileConnections[1]; }
            set { tileConnections[1] = value; }
        }

        public bool Left
        {
            get { return tileConnections[2]; }
            set { tileConnections[2] = value; }
        }

        public bool Right
        {
            get { return tileConnections[3]; }
            set { tileConnections[3] = value; }
        }

        public bool Below
        {
            get { return tileConnections[4]; }
            set { tileConnections[4] = value; }
        }

        public bool Above
        {
            get { return tileConnections[5]; }
            set { tileConnections[5] = value; }
        }

        public Tile()
        {
            tileConnections = new bool[] { false, false, false, false, false, false };
        }

        public Tile(bool up, bool down, bool left, bool right, bool below, bool above)
        {
            tileConnections = new bool[] { up, down, left, right, below, above };
        }

        public bool isDeadEnd()
        {
            //A dead end should only have one connection
            //i.e. only exactly 1 connection in the connection array
            //should be true. This includes teleportation pads
            int connection_count = 0;
            for (int direction = 0; direction < 6; direction++)
            {
                if (tileConnections[direction] == true)
                {
                    connection_count++;
                    if (connection_count > 1)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }

    internal class Maze
    {
        private Tile[,,] _tiles;

        public readonly int width;

        public readonly int height;
        public int current_layer;
        public int max_layers;

        static public Texture2D maze_wall_H;
        static public Texture2D maze_wall_V;
        static public Texture2D maze_floor;
        static public Texture2D tp_pad_design;
        static public Texture2D end_tp_pad_design;
        public List<Rectangle>[] collision_rects;
        public List<Rectangle>[] TP_Pads;
        public Rectangle end_goal; //This represents the hitbox for the end of the level.
        
        public Vector2 pos = Vector2.Zero; // Global poisition of top left corner of maze
        public int xmax;
        public int ymax;

        // Generate the maze using the Wilson algorithm
        public Maze(int width, int height, int layers = 2)
        {
            const int tileSize = 128;
            this.width = width;
            this.height = height;
            current_layer = 0;
            max_layers = layers;
            _tiles = new Tile[width, height, layers]; //2 layer maze
            collision_rects = new List<Rectangle>[max_layers];
            TP_Pads = new List<Rectangle>[max_layers];
            GenerateMaze();
            xmax = tileSize * width - (int)pos.X;
            ymax = tileSize * height - (int)pos.Y;
            pos = Vector2.Zero;
            //Initialise the collision rect array
            for (int z = 0; z < max_layers; z++)
            {
                collision_rects[z] = new List<Rectangle>();
                TP_Pads[z] = new List<Rectangle>();
            }
            SetMazeRectangles();

        }        

        //Used for maze generation to check if all values in a boolean array equal
        //a particular boolean value.
        private static bool IsBoolArrayFilled(bool[,,] array, bool value)
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

        //Generates the maze
        private void GenerateMaze()
        {
            Tile[,,] currentWalk = new Tile[width, height, max_layers];
            bool[,,] isVisited = new bool[width, height, max_layers];
            bool[,,] isPartOfMaze = new bool[width, height, max_layers];

            //Initialise all the values in the arrays
            //including the member _tiles
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    for (int k = 0; k < max_layers; k++)
                    {
                        _tiles[i, j, k] = new Tile();
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
                        for (int z = 0; z < max_layers && _continue; z++)
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
                        if (!hasUsedTPPad && totalTPPads < maxTPpads && walkLength > 2 
                            && !(currentX == width-1 && currentY == height - 1))
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
                            currentWalk[currentX, currentY, currentZ].Up = true;
                            currentWalk[currentX, currentY - 1, currentZ].Down = true;
                            //Go UP
                            currentY--;
                            direction = 0;

                        }
                        else
                        {
                            //Connect the tiles together
                            currentWalk[currentX, currentY, currentZ].Down = true;
                            currentWalk[currentX, currentY + 1, currentZ].Up = true;
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
                            currentWalk[currentX, currentY, currentZ].Down = true;
                            currentWalk[currentX, currentY + 1, currentZ].Up = true;
                            //Go down
                            currentY++;
                            direction = 1;
                        }
                        else
                        {
                            //Connect the tile
                            currentWalk[currentX, currentY, currentZ].Up = true;
                            currentWalk[currentX, currentY - 1, currentZ].Down = true;
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
                            currentWalk[currentX, currentY, currentZ].Right = true;
                            currentWalk[currentX + 1, currentY, currentZ].Left = true;
                            //Go Right
                            currentX++;
                            direction = 3;
                        }
                        else
                        {
                            //Connect the tiles together
                            currentWalk[currentX, currentY, currentZ].Left = true;
                            currentWalk[currentX - 1, currentY, currentZ].Right = true;
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
                            currentWalk[currentX, currentY, currentZ].Left = true;
                            currentWalk[currentX - 1, currentY, currentZ].Right = true;
                            //Go left
                            currentX--;
                            direction = 2;
                        }
                        else
                        {
                            //Connect the tiles together
                            currentWalk[currentX, currentY, currentZ].Right = true;
                            currentWalk[currentX + 1, currentY, currentZ].Left = true;
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
                            currentWalk[currentX, currentY, currentZ].Below = true;
                            currentWalk[currentX, currentY, currentZ - 1].Above = true;
                            //Go to layer below
                            currentZ--;
                            direction = 4;
                        }
                        else
                        {
                            //Connect the tiles together
                            currentWalk[currentX, currentY, currentZ].Above = true;
                            currentWalk[currentX, currentY, currentZ + 1].Below = true;
                            //Go to layer above
                            currentZ++;
                            direction = 5;
                        }

                    }

                    else
                    {
                        //Try going to the layer above
                        if (currentZ < max_layers - 1)
                        {
                            //Connect the tiles together
                            currentWalk[currentX, currentY, currentZ].Above = true;
                            currentWalk[currentX, currentY, currentZ + 1].Below = true;
                            //Go to layer above
                            currentZ++;
                            direction = 5;
                        }
                        else
                        {
                            //Connect the tiles together
                            currentWalk[currentX, currentY, currentZ].Below = true;
                            currentWalk[currentX, currentY, currentZ - 1].Above = true;
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
                                for (int z = 0; z < max_layers; z++)
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
                        for (int z = 0; z < max_layers; z++)
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

            } while (!IsBoolArrayFilled(isPartOfMaze, true));
            
        }
        
        //Draws the maze onto the screen.
        public void DisplayMaze(SpriteBatch spriteBatch)
        { 
            //Each tile is a 128x128 area
            const int tile_size_x = 128; 
            const int tile_size_y = 128;
            //Each tile is made up of four 64x64 tile segments
            const int tile_segment_width = 64;
            //This is the width of walls that are vertical
            const int wallV_width = 32;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {                    
                    spriteBatch.Draw(maze_floor, new Vector2(x * tile_size_y - pos.X, y * tile_size_x + tile_segment_width - pos.Y), Color.White);
                    spriteBatch.Draw(maze_floor, new Vector2(x * tile_size_y - pos.X, y * tile_size_x + 2 * tile_segment_width - pos.Y), Color.White);
                    spriteBatch.Draw(maze_floor, new Vector2(x * tile_size_y + tile_segment_width - pos.X, y * tile_size_x + tile_segment_width - pos.Y), Color.White);
                    spriteBatch.Draw(maze_floor, new Vector2(x * tile_size_y + tile_segment_width - pos.X, y * tile_size_x + 2 * tile_segment_width - pos.Y), Color.White);
                    
                    if (_tiles[x, y, current_layer].Above || _tiles[x, y, current_layer].Below)
                    {
                        spriteBatch.Draw(tp_pad_design, new Vector2(x * tile_size_y - pos.X, y * tile_size_x + tile_segment_width - pos.Y), Color.White);
                    }

                    if (x == width-1 && y == width - 1 && current_layer == 0)
                    {
                        spriteBatch.Draw(end_tp_pad_design, new Vector2(x * tile_size_y - pos.X, y * tile_size_x + tile_segment_width - pos.Y), Color.White);
                    }

                    //If we are at the top
                    if (y == 0)
                    {
                        spriteBatch.Draw(maze_wall_H, new Vector2(x * tile_size_y - pos.X, -pos.Y), Color.White);
                        spriteBatch.Draw(maze_wall_H, new Vector2(x * tile_size_y + tile_segment_width - pos.X, -pos.Y), Color.White);
                    }

                    if (_tiles[x, y, current_layer].Down == false)
                    {
                        spriteBatch.Draw(maze_wall_H, new Vector2(x * tile_size_y - pos.X, (y + 1) * tile_size_x - pos.Y), Color.White);
                        spriteBatch.Draw(maze_wall_H, new Vector2(x * tile_size_y + tile_segment_width - pos.X, (y + 1) * tile_size_x - pos.Y), Color.White);
                    }
                }
            }

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    //Display right walls (if applicable)  
                    if (!_tiles[x, y, current_layer].Right)
                    {
                        spriteBatch.Draw(maze_wall_V, new Vector2((x + 1) * tile_size_y - wallV_width - pos.X, y * tile_size_x - pos.Y), Color.White);
                        spriteBatch.Draw(maze_wall_V, new Vector2((x + 1) * tile_size_y - wallV_width - pos.X, y * tile_size_x + tile_segment_width - pos.Y), Color.White);
                        spriteBatch.Draw(maze_wall_V, new Vector2((x + 1) * tile_size_y - wallV_width - pos.X, y * tile_size_x + 2 * tile_segment_width - pos.Y), Color.White);
                    }                   

                    //If we are on the leftmost side of the maze
                    if (x == 0)
                    {
                        spriteBatch.Draw(maze_wall_V, new Vector2(-pos.X, y * tile_size_x - pos.Y), Color.White);
                        spriteBatch.Draw(maze_wall_V, new Vector2(-pos.X, y * tile_size_x + tile_segment_width - pos.Y), Color.White);
                    }
                }
            }
        }

        /// <summary>
        /// This function is used to create the list of collision rectangles. The rectangles should be in 
        /// the same place as the walls that they represent. Hence this function is very similar to the
        /// display function.
        /// </summary>
        private void SetMazeRectangles()
        {
            //These constants help to form the dimensions of the maze
            //They are the same as the constants in the display function
            const int tile_width_y = 128;
            const int tile_width_x = 128;
            const int tile_segment_width = 64;
            const int wallV_width = 32;

            //Generate the top and left hand-side rectangles
            for (int z = 0; z < max_layers; z++)
            {
                collision_rects[z].Add(new Rectangle(0, 0, (width - 1) * tile_width_x + tile_segment_width * 2, 20));
                collision_rects[z].Add(new Rectangle(0, 0, wallV_width, (height - 1) * tile_width_y + tile_segment_width * 2));
            }            

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int z = 0; z < max_layers; z++)
                    {
                        //Add bottom rectangles (this code should mirror display code)
                        if (_tiles[x, y, z].Down == false)
                        {
                            collision_rects[z].Add(new Rectangle(
                                x * tile_width_x,
                                (y + 1) * tile_width_y, 
                                tile_segment_width * 2, 
                                20
                                ));
                        }     
                        
                        //Add right rectangles
                        if (_tiles[x, y, z].Right == false)
                        {
                            collision_rects[z].Add(new Rectangle(
                                (x + 1) * tile_width_x - wallV_width,
                                y * tile_width_y, 
                                wallV_width, 
                                tile_segment_width * 3 - 44
                                ));
                        }

                        //Add teleportation pads
                        if (_tiles[x, y, z].Above || _tiles[x, y, z].Below)
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

            //Add the end goal
            end_goal = new Rectangle(
                                (width - 1) * 128,
                                (height - 1) * 128 + 64,
                                60,
                                60);
        } 

        /// <summary>
        /// This function updates the position of all the rectangles in the maze. This is
        /// necessary for the side scrolling code.
        /// </summary>
        /// <param name="deltaPos">How much the rectangles move by</param>
        public void UpdateMazeRects(Vector2 deltaPos)
        {
            
            for (int z = 0; z < max_layers; z++)
            {
                for (int i = 0; i < collision_rects[z].Count; i++)
                {
                    Rectangle rect = collision_rects[z][i];
                    rect.Offset(deltaPos);
                    collision_rects[z][i] = rect;                    
                }
            }

            for (int z = 0; z < max_layers; z++)
            {
                for (int i = 0; i < TP_Pads[z].Count; i++)
                {
                    Rectangle rect = TP_Pads[z][i];
                    rect.Offset(deltaPos);
                    TP_Pads[z][i] = rect;
                }
            }

            //Update end goal rectangle
            end_goal.Offset(deltaPos);            
        }        
        
        //This function will return one random path that starts at a specified point.
        //It will try and ensure the path is as long as specified, but it will choose the next
        //longest path if it cannot find it.
        public List<Point> GenerateSingleLayerPath(Point start, int pathlength, int layer)
        {
            List<List<Point>> all_paths = new();
            List<Point> beginning = new() { start };
            GenerateSingleLayerPaths(ref all_paths, beginning, pathlength, -1, layer);

            //Step 1: Find the longest path in all_paths
            int longest_pathlength = 0; 
            foreach (var path in all_paths)
            {
                //If we have found the longest possible path, we can stop the search
                if (path.Count == pathlength)
                {
                    longest_pathlength = pathlength;
                    break;
                }
                else if (path.Count > longest_pathlength)
                {
                    longest_pathlength = path.Count;
                }
            }

            //Since deleting stuff from a list whilst iterating through it can 
            //throw errors, I need to add valid paths to a separate list instead.
            List<List<Point>> new_all_paths = new(); 

            //Remove all paths that are shorter than the longest path
            foreach (var path in all_paths)
            {
                if (path.Count == longest_pathlength)
                {
                    new_all_paths.Add(path);
                }
            }

            //Choose a random path from the cleaned up list
            Random random = new();
            List<Point> chosen_path = new_all_paths[random.Next(new_all_paths.Count)];

            //Convert list indices to global coordinates.
            for (int i = 0; i < chosen_path.Count; i++)
            {
                chosen_path[i] = new Point(chosen_path[i].X * 128 + 16, chosen_path[i].Y * 128 + 64);
            }

            return chosen_path;
        }

        //Recursive algorithm to generate paths for an enemy.
        //Returns a list of points in a path
        private void GenerateSingleLayerPaths(ref List<List<Point>> all_paths, List<Point> currentPath, int pathlength, int prevDirection, int layer)
        {
            Point currentNode = currentPath[currentPath.Count - 1];
            Tile currentTile = _tiles[currentNode.X, currentNode.Y, layer];            

            if (pathlength > 0)
            {
                if (currentTile.Up == true && prevDirection != 1)
                {
                    currentPath.Add(new Point(currentNode.X, currentNode.Y - 1));
                    GenerateSingleLayerPaths(ref all_paths, currentPath, pathlength - 1, 0, layer);
                    currentPath.RemoveAt(currentPath.Count - 1);
                }
                if (currentTile.Down == true && prevDirection != 0)
                {
                    currentPath.Add(new Point(currentNode.X, currentNode.Y + 1));
                    GenerateSingleLayerPaths(ref all_paths, currentPath, pathlength - 1, 1, layer);
                    currentPath.RemoveAt(currentPath.Count - 1);
                }
                if (currentTile.Left == true && prevDirection != 3)
                {
                    currentPath.Add(new Point(currentNode.X - 1, currentNode.Y));
                    GenerateSingleLayerPaths(ref all_paths, currentPath, pathlength - 1, 2, layer);
                    currentPath.RemoveAt(currentPath.Count - 1);
                }
                if (currentTile.Right == true && prevDirection != 2)
                {
                    currentPath.Add(new Point(currentNode.X + 1, currentNode.Y));
                    GenerateSingleLayerPaths(ref all_paths, currentPath, pathlength - 1, 3, layer);
                    currentPath.RemoveAt(currentPath.Count - 1);
                }
                else if (!(currentTile.Up == true && prevDirection != 1) && !(currentTile.Down == true && prevDirection != 0) && !(currentTile.Left == true && prevDirection != 3))
                {
                    List<Point> newPath = new();
                    foreach (Point p in currentPath)
                    {
                        newPath.Add(new Point(p.X, p.Y));
                    }
                    all_paths.Add(newPath);
                }
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

        //Calculate shortest path between two points on the maze and return the next point in the path.
        //Note that these points are NOT coordinates on the screen
        //but they are indices for the maze array instead.This function will use a
        //BFS as the maze is an unweighted graph.This function will not
        //traverse between layers (good for enemies).
        public Point SingleLayerNextTileFinder(Point start, Point end)
        {
            //Since we only need to return the next tile to go to,
            //all paths just need to store the 2nd tile in the path
            //where the 1st tile is the starting tile.
            Queue<Point> tiles_to_visit = new();
            //This simply stores which is the next tile we need to go to
            //Since we know this tile
            Queue<int> return_value = new();
            HashSet<Point> visited = new() { start };

            //Add all neighbouring tiles to the queue
            if (_tiles[start.X, start.Y, current_layer].Up)
            {
                tiles_to_visit.Enqueue(new(start.X, start.Y - 1));
                return_value.Enqueue(0);
            }
            if (_tiles[start.X, start.Y, current_layer].Down)
            {
                tiles_to_visit.Enqueue(new(start.X, start.Y + 1));
                return_value.Enqueue(1);
            }
            if (_tiles[start.X, start.Y, current_layer].Left)
            {
                tiles_to_visit.Enqueue(new(start.X - 1, start.Y));
                return_value.Enqueue(2);
            }
            if (_tiles[start.X, start.Y, current_layer].Right)
            {
                tiles_to_visit.Enqueue(new(start.X + 1, start.Y));
                return_value.Enqueue(3);
            }
            Point current_tile;
            int current_result; 
            //Begin the algorithm
            //While the tiles_to_visit queue is not empty
            while (tiles_to_visit.Count > 0)
            {
                current_tile = tiles_to_visit.Dequeue();
                current_result = return_value.Dequeue();
                //If we reach the end, we have found the shortest path
                if (current_tile == end)
                {
                    if (current_result == 0)
                    {
                        start.Y -= 1;
                        return start;
                    }
                    else if (current_result == 1)
                    {
                        start.Y += 1;
                        return start;
                    }
                    else if (current_result == 2)
                    {
                        start.X -= 1;
                        return start;
                    }
                    else
                    {
                        start.X += 1;
                        return start;
                    }
                }
                else
                {
                    //Mark current tile as visited
                    visited.Add(current_tile);

                    //Add all neighbours to the "to visit" queue if they are not visited
                    if (!visited.Contains(new(current_tile.X, current_tile.Y - 1)) 
                        && _tiles[current_tile.X, current_tile.Y, current_layer].Up)
                    {
                        tiles_to_visit.Enqueue(new(current_tile.X, current_tile.Y - 1));
                        return_value.Enqueue(current_result);
                    }
                    if (!visited.Contains(new(current_tile.X, current_tile.Y + 1)) &&
                        _tiles[current_tile.X, current_tile.Y, current_layer].Down)
                    {
                        tiles_to_visit.Enqueue(new(current_tile.X, current_tile.Y + 1));
                        return_value.Enqueue(current_result);
                    }
                    if (!visited.Contains(new(current_tile.X - 1, current_tile.Y))
                        && _tiles[current_tile.X, current_tile.Y, current_layer].Left)
                    {
                        tiles_to_visit.Enqueue(new(current_tile.X - 1, current_tile.Y));
                        return_value.Enqueue(current_result);
                    }
                    if (!visited.Contains(new(current_tile.X + 1, current_tile.Y))
                        && _tiles[current_tile.X, current_tile.Y, current_layer].Right)
                    {
                        tiles_to_visit.Enqueue(new(current_tile.X + 1, current_tile.Y));
                        return_value.Enqueue(current_result);
                    }
                }
                
            }

            return new(-1, -1); //If cannot find a path
        }

        /// <summary>
        /// Display a minimap onto the screen
        /// </summary>
        public void DisplayTopCornerMinimapImage(SpriteBatch spritebatch, Texture2D bg_colour,
            Texture2D wall_colour, Texture2D TP_pad, Texture2D player_icon, Vector2 player_pos,
            Texture2D end_goal_colour)
        {
            //These constants help to form the dimensions of the maze
            //They are the same as the constants in the display function
            const int tile_width_y = 16;
            const int tile_width_x = 16;
            const int tile_segment_width = 8;
            const int wallV_width = 3;

            Point location = new(550, 0);

            //Determine the tile the player is on.
            //The centre of the player is not quite (0,0), so there is a bit of an offset.
            //Hence we add the vector (98, 80) to the player's position and then divide it.
            int player_tile_x = (int)(player_pos.X + 98) / 128;
            int player_tile_y = (int)(player_pos.Y + 80) / 128;

            //Draw the background first
            spritebatch.Draw(bg_colour, new Rectangle(location, 
                new((width - 1) * tile_width_x + tile_segment_width * 2,
                 (height - 1) * tile_width_y + tile_segment_width * 2)),
                Color.White);

            //Draw the top and left hand-side walls
            spritebatch.Draw(wall_colour,
                new Rectangle(location.X,
                location.Y,
                (width - 1) * tile_width_x + tile_segment_width * 2,
                5), Color.White);
            spritebatch.Draw(wall_colour, 
                new Rectangle(location.X, 
                location.Y,
                wallV_width, 
                (height - 1) * tile_width_y + tile_segment_width * 2), Color.White);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {                    
                    //Add bottom rectangles (this code should mirror display code)
                    if (_tiles[x, y, current_layer].Down == false)
                    {
                        spritebatch.Draw(wall_colour,
                            new Rectangle(
                            x * tile_width_x + location.X,
                            (y + 1) * tile_width_y + location.Y,
                            tile_segment_width * 2,
                            5
                            ), Color.White); ;
                    }

                    //Add right rectangles
                    if (_tiles[x, y, current_layer].Right == false)
                    {
                        spritebatch.Draw(wall_colour, 
                            new Rectangle(
                            (x + 1) * tile_width_x - wallV_width + location.X, 
                            y * tile_width_y + location.Y, 
                            wallV_width, 
                            tile_segment_width * 3
                            ), Color.White);
                    }

                    //Add teleportation pads
                    if (_tiles[x, y, current_layer].Above || _tiles[x, y, current_layer].Below)
                    {
                        spritebatch.Draw(TP_pad,
                            new Rectangle(
                            x * tile_width_x + (tile_width_x / 6) + location.X,
                            y * tile_width_y + (2 * tile_width_y / 3) + location.Y,
                            tile_width_x/3,
                            tile_width_y/3
                            ), Color.White);
                    }

                    //Draw the end goal on the minimap
                    //This is in the bottom right corner of the first layer
                    if (x == width-1 && y == width-1 && current_layer == 0)
                    {
                        spritebatch.Draw(end_goal_colour, 
                            new Rectangle(
                            x * tile_width_x + (tile_width_x / 4) + location.X,
                            y * tile_width_y + (tile_width_y/ 2) + location.Y,
                            tile_width_x / 2,
                            tile_width_y / 2
                            ), Color.White);
                    }
                    
                    //Draw the player onto the minimap
                    if (x == player_tile_x && y == player_tile_y)
                    {
                        Vector2 player_icon_location = new(x * tile_width_x + location.X, 
                            y * tile_width_y + location.Y+ 4);
                        spritebatch.Draw(player_icon, player_icon_location,  Color.White);
                    }
                }
            }
        }

        //Iterates through the layer and returns the dead ends.
        public HashSet<Point> GetDeadEnds(int layer)
        {
            HashSet<Point> dead_ends = new();
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (_tiles[x, y, layer].isDeadEnd())
                    {
                        dead_ends.Add(new Point(x, y));
                    }
                }
            }
            return dead_ends;
        }        
    }
}
