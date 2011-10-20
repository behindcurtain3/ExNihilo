using System;
using System.Collections.Generic;

namespace Ants {
	
	public class GameState {
		
		public int Width { get; private set; }
		public int Height { get; private set; }
		
		public int LoadTime { get; private set; }
		public int TurnTime { get; private set; }
		
		private DateTime turnStart;
		public int TimeRemaining {
			get {
				TimeSpan timeSpent = DateTime.Now - turnStart;
				return TurnTime - timeSpent.Milliseconds;
			}
		}

		public int ViewRadius2 { get; private set; }
		public int AttackRadius2 { get; private set; }
		public int SpawnRadius2 { get; private set; }
		
		public List<AntLoc> MyAnts;
		public List<AntLoc> EnemyAnts;
        public List<HillLoc> MyHills;
        public List<HillLoc> EnemyHills;
		public List<Location> DeadTiles;
		public List<Location> FoodTiles;
		
		private Tile[,] map;
        private ANode[,] nodes;
		
		public GameState (int width, int height, 
		                  int turntime, int loadtime, 
		                  int viewradius2, int attackradius2, int spawnradius2) {
			
			Width = width;
			Height = height;
            AStar.Height = Height;
            AStar.Width = Width;
			
			LoadTime = loadtime;
			TurnTime = turntime;
			
			ViewRadius2 = viewradius2;
			AttackRadius2 = attackradius2;
			SpawnRadius2 = spawnradius2;
			
			MyAnts = new List<AntLoc>();
			EnemyAnts = new List<AntLoc>();
            MyHills = new List<HillLoc>();
            EnemyHills = new List<HillLoc>();
            DeadTiles = new List<Location>();
			FoodTiles = new List<Location>();
			
			map = new Tile[height, width];
            nodes = new ANode[height, width];
			for (int row = 0; row < height; row++) {
				for (int col = 0; col < width; col++) {
					map[row, col] = Tile.Land;
                    nodes[row, col] = new ANode(new Location(row, col));                     
				}
			}
            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    addNeighborToNode(nodes[row, col], row - 1, col);
                    addNeighborToNode(nodes[row, col], row + 1, col);
                    addNeighborToNode(nodes[row, col], row, col - 1);
                    addNeighborToNode(nodes[row, col], row, col + 1);
                }
            }
		}

        private void addNeighborToNode(ANode node, int r, int c)
        {
            if (r < 0)
                r = Height - 1;
            if (r >= Height)
                r = 0;
            if (c < 0)
                c = Width - 1;
            if (c >= Width)
                c = 0;

            if (!node.Neighbors.ContainsKey(nodes[r, c]))
                node.Neighbors.Add(nodes[r, c], true);
        }
		
		public void startNewTurn () {
			// start timer
			turnStart = DateTime.Now;
			
			// clear ant data
			foreach (Location loc in MyAnts) map[loc.row, loc.col] = Tile.Land;
			foreach (Location loc in EnemyAnts) map[loc.row, loc.col] = Tile.Land;
            foreach (Location loc in MyHills) map[loc.row, loc.col] = Tile.Land;
            foreach (Location loc in EnemyHills) map[loc.row, loc.col] = Tile.Land;
			foreach (Location loc in DeadTiles) map[loc.row, loc.col] = Tile.Land;
			
			MyAnts.Clear();
			EnemyAnts.Clear();
            MyHills.Clear();
            EnemyHills.Clear();
			DeadTiles.Clear();
			
			// set all known food to unseen
			foreach (Location loc in FoodTiles) map[loc.row, loc.col] = Tile.Land;
			FoodTiles.Clear();
		}
		
		public void addAnt (int row, int col, int team) {
			map[row, col] = Tile.Ant;
			
			AntLoc ant = new AntLoc(row, col, team);
			if (team == 0) {
				MyAnts.Add(ant);
			} else {
				EnemyAnts.Add(ant);
			}
		}

        public void addHill(int row, int col, int team)
        {
            HillLoc hill = new HillLoc(row, col, team);
            if (team == 0)
                MyHills.Add(hill);
            else
                EnemyHills.Add(hill);
        }
		
		public void addFood (int row, int col) {
			map[row, col] = Tile.Food;
			FoodTiles.Add(new Location(row, col));
		}
		
		public void removeFood (int row, int col) {
			// an ant could move into a spot where a food just was
			// don't overwrite the space unless it is food
			if (map[row, col] == Tile.Food) {
				map[row, col] = Tile.Land;
			}
			FoodTiles.Remove(new Location(row, col));
		}
		
		public void addWater (int row, int col) {
			map[row, col] = Tile.Water;
            nodes[row, col].Passable = false;
		}
		
		public void deadAnt (int row, int col) {
			// food could spawn on a spot where an ant just died
			// don't overwrite the space unless it is land
			if (map[row, col] == Tile.Land) {
				map[row, col] = Tile.Dead;
			}
			
			// but always add to the dead list
			DeadTiles.Add(new Location(row, col));
		}

        public List<char> getPath(Location start, List<Location> targets)
        {
            Debug.Write("Getting a path");
            ANode startNode = null;            
            List<ANode> nodeTargets = new List<ANode>();


            foreach (ANode n in nodes)
            {
                if (n.Coords.Equals(start))
                {
                    startNode = n;
                    Debug.Write("Start Node: " + n.Coords);
                }

                foreach(Location t in targets)
                {
                    if(n.Coords.Equals(t))
                    {
                        nodeTargets.Add(n);
                    }
                }
            }

            if(startNode == null)
                return new List<char>();

            Path p = AStar.getPath(startNode, nodeTargets);

            if (p.Count > 0)
                return direction(start, p.Peek().Coords);
            else
                return new List<char>();
        }

		
		public bool passable (Location loc) {
			// true if not water
			return map[loc.row, loc.col] != Tile.Water;
		}
		
		public bool unoccupied (Location loc) {
			// true if no ants are at the location
			return passable(loc) && map[loc.row, loc.col] != Tile.Ant;
		}
		
		public Location destination (Location loc, char direction) {
			// calculate a new location given the direction and wrap correctly
			Location delta = Ants.Aim[direction];
			
			int row = (loc.row + delta.row) % Height;
			if (row < 0) row += Height; // because the modulo of a negative number is negative
			
			int col = (loc.col + delta.col) % Width;
			if (col < 0) col += Width;
			
			return new Location(row, col);
		}
		
		public int distance (Location loc1, Location loc2) {
			// calculate the closest distance between two locations
			int d_row = Math.Abs(loc1.row - loc2.row);
			d_row = Math.Min(d_row, Height - d_row);
			
			int d_col = Math.Abs(loc1.col - loc2.col);
			d_col = Math.Min(d_col, Width - d_col);
			
			return d_row + d_col;
		}
		
		public List<char> direction (Location loc1, Location loc2) {
			// determine the 1 or 2 fastest (closest) directions to reach a location
			List<char> directions = new List<char>();
			
			if (loc1.row < loc2.row) {
				if (loc2.row - loc1.row >= Height / 2)
					directions.Add('n');
				if (loc2.row - loc1.row <= Height / 2)
					directions.Add('s');
			}
			if (loc2.row < loc1.row) {
				if (loc1.row - loc2.row >= Height / 2)
					directions.Add('s');
				if (loc1.row - loc2.row <= Height / 2)
					directions.Add('n');
			}
			
			if (loc1.col < loc2.col) {
				if (loc2.col - loc1.col >= Width / 2)
					directions.Add('w');
				if (loc2.col - loc1.col <= Width / 2)
					directions.Add('e');
			}
			if (loc2.col < loc1.col) {
				if (loc1.col - loc2.col >= Width / 2)
					directions.Add('e');
				if (loc1.col - loc2.col <= Width / 2)
					directions.Add('w');
			}
			
			return directions;
		}

	}
}

