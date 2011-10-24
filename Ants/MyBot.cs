using System;
using System.Collections.Generic;
using System.Linq;
//using System.Drawing;
//using System.IO;

namespace Ants
{

    class MyBot : Bot
    {
        public static char NORTH = 'n';
        public static char EAST = 'e';
        public static char SOUTH = 's';
        public static char WEST = 'w';
        private GameState mState;

        private Mapper map;
        private List<Location> previousPositions = new List<Location>();    // Used to not move back the way you came if possible
        private List<Location> orders = new List<Location>();               // Squares we have an ant on that has already moved
        private List<Location> targets = new List<Location>();              // Squares we are targeting
        private List<Location> currentMoves = new List<Location>();         // Squares we have moved to, useful for not going to the same place and dying
        private List<Location> knownEnemyHills = new List<Location>();
        private List<Path> travelingTo = new List<Path>();

        private List<Location> zoneDanger = new List<Location>();   // Used to warn of immenent danger
        private List<Location> zoneHill = new List<Location>();     // Used to mark path to enemy hills
        private List<Location> zoneVisited = new List<Location>();  // Used to mark squares recently visited, try not to revist them again until a bit of time has passed
        


        private double ViewRadius { get; set; }
        private int Range { get; set; }

        public override void doSetup(GameState state)
        {
            Debug.Write("---ExNihilo---");
            ViewRadius = Math.Sqrt(state.ViewRadius2);
            Range = (int)Math.Floor(ViewRadius);
            Debug.Write("--- View Radius: " + ViewRadius);
            map = new Mapper(GameState.Height, GameState.Width, Range);
        }

        public override void doEnd(GameState state)
        {
            /*
            File.Delete("F:\\Programming\\Projects\\C#\\ExNihilo\\tools\\map.png");
            Bitmap img = new Bitmap(GameState.Width * 4, GameState.Height * 4);
            Brush on = Brushes.DarkGreen;
            Brush off = Brushes.Blue;
            Brush unseen = Brushes.Black;

            using (var g = Graphics.FromImage(img))
            {

                for (int i = 0; i < GameState.Height; i++)
                {
                    for (int j = 0; j < GameState.Width; j++)
                    {
                        if (mState.nodes[i, j].Passable)
                            g.FillRectangle(on, j * 4, i * 4, 4, 4);
                        else
                            g.FillRectangle(off, j * 4, i * 4, 4, 4);
                    }
                }
            }
            img.Save("F:\\Programming\\Projects\\C#\\ExNihilo\\tools\\map.png");
             */
        }

        // doTurn is run once per turn
        public override void doTurn(GameState state)
        {
            Debug.Write("### TURN " + state.CurrentTurn + " ###");
            mState = state;

            previousPositions = new List<Location>(orders);
            orders.Clear();
            targets.Clear();
            currentMoves.Clear();

            int time = mState.TimeRemaining;

            getCloseItems();
            Debug.Write(" * Got items in " + (time - mState.TimeRemaining) + "ms");

            if (mState.TimeRemaining > 200)
            {
                time = mState.TimeRemaining;
                getUnexploredAreas();
                Debug.Write(" * Explored in " + (time - mState.TimeRemaining) + "ms");
            }
            if (mState.TimeRemaining > 50)
            {
                time = mState.TimeRemaining;
                getOffMyHills();
                Debug.Write(" * Off hills in " + (time - mState.TimeRemaining) + "ms");
            }

            if (travelingTo.Count > 0)
                Debug.Write(travelingTo.Count + " paths cached.");
            if(orders.Count / mState.MyAnts.Count < 0.75)
                Debug.Write(" * Orders Issued: " + orders.Count + "/" + mState.MyAnts.Count);

            if (mState.TimeRemaining < 100)
            {
                int used = 1000 - mState.TimeRemaining;
                Debug.Write(" * Time Used: " + used + "/1000");
            }
        }

        public Boolean moveTo(AntLoc ant, Location moveTo, char dir)
        {
            if (mState.unoccupied(moveTo) && !currentMoves.Contains(moveTo))
            {
                issueOrder(ant, dir);

                orders.Add(ant);
                currentMoves.Add(moveTo);
                return true;
            }
            else
                return false;
        }

        public void getCloseItems()
        {
            // attempt to find food
            List<LocAndDist> antDists = new List<LocAndDist>();
            int distance;
            foreach (Location food in mState.FoodTiles)
            {
                foreach (AntLoc ant in mState.MyAnts)
                {
                    distance = mState.distance(ant, food);
                    if(distance <= ViewRadius + 4)
                        antDists.Add(new LocAndDist(ant, food, mState.distance(ant, food)));
                }
            }
            // and hills
            foreach (Location hill in mState.EnemyHills)
            {
                if (!knownEnemyHills.Contains(hill))
                    knownEnemyHills.Add(hill);
            }

            for (int i = knownEnemyHills.Count - 1; i >= 0; i--)
            {
                foreach (AntLoc ant in mState.MyAnts)
                {
                    if (ant.Equals(knownEnemyHills[i]))
                    {
                        knownEnemyHills.RemoveAt(i);
                        break;
                    }
                    distance = mState.distance(ant, knownEnemyHills[i]);
                    if(distance <= ViewRadius * 2)
                        antDists.Add(new LocAndDist(ant, knownEnemyHills[i], distance, false));
                }
            }

            antDists.Sort(SortByDistance);

            foreach (LocAndDist pair in antDists)
            {
                if (mState.TimeRemaining < 250)
                    return;

                if ((pair.Solo && targets.Contains(pair.Dest)) || orders.Contains(pair.Ant))
                    continue;
                
                Path p = mState.getSinglePath(pair.Ant, pair.Dest);
                Location destination;
                if (p.Count == 0)
                    destination = pair.Dest;
                else
                    destination = p.Peek().Location;


                foreach (char dir in mState.direction(pair.Ant, destination))
                {
                    if (moveTo(pair.Ant, destination, dir))
                    {
                        if(pair.Solo)
                            targets.Add(pair.Dest);
                        break;
                    }
                }
            }
        }

        public void getUnexploredAreas()
        {
            if (mState.Unseen.Count == 0)
                return;

            // Update our map
            map.update(mState);

            // Remove finished paths
            travelingTo.RemoveAll(delegate(Path p) { return p.Count == 0; });
            int paths = 0;
            int cached = 0;
            foreach (AntLoc ant in mState.MyAnts)
            {
                if (mState.TimeRemaining < 100)
                    return;

                Path cachedPath = travelingTo.Find(delegate(Path p) { return p.Peek().Location.Equals(ant); });
                if (cachedPath != null)
                {
                    if (!orders.Contains(ant))
                    {
                        // Yay!
                        cached++;
                        cachedPath.Pop();

                        if (cachedPath.Count == 0)
                        {
                            travelingTo.Remove(cachedPath);
                        }
                        else
                        {
                            Location destination = cachedPath.Peek().Location;
                            foreach (char dir in mState.direction(ant, destination))
                            {
                                if (moveTo(ant, destination, dir))
                                    break;
                            }
                        }
                    }
                    else
                    {
                        travelingTo.Remove(cachedPath);
                    }
                }

                // Check if we've moved, again
                if (orders.Contains(ant))
                    continue;

                Location dest = map.nearest(mState, ant);
                if (dest == null)
                {
                    continue;
                }

                Path newPath = mState.getSinglePath(ant, dest);
                if (newPath.Count == 0)
                {
                    continue;
                }

                paths++;
                dest = newPath.Peek().Location;
                foreach (char dir in mState.direction(ant, dest))
                {
                    if (moveTo(ant, dest, dir))
                    {
                        travelingTo.Add(newPath);
                        break;
                    }
                }

                /*
                List<Location> searchFor = new List<Location>(mState.Unseen);

                if (searchFor.Count > 10)
                {
                    searchFor.Sort(delegate(Location n1, Location n2) { return mState.distance(n1, ant).CompareTo(mState.distance(n2, ant)); });
                    searchFor.RemoveRange(9, searchFor.Count - 9);
                }

                int radius = 25;
                while (searchFor.Count > 10)
                {
                    searchFor.RemoveAll(delegate(Location l) { return mState.distance(ant, l) > radius; });
                    radius -= 5;
                }

                Path p = mState.getPath(ant, searchFor);
                if (p.Count == 0)
                    continue;

                Location dest = p.Peek().Location;
                foreach (char dir in mState.direction(ant, dest))
                {
                    if (moveTo(ant, dest, dir))
                    {
                        travelingTo.Add(p);
                        break;
                    }
                }

                Dictionary<Location, int> antDist = new Dictionary<Location, int>();
                foreach (Location p in mState.Unseen)
                {
                    if (targets.Contains(p))
                        continue;

                    int d = mState.distance(ant, p);
                    antDist.Add(p, d);
                }
                // Sort distances
                var sortedDict = (from entry in antDist orderby entry.Value ascending select entry).ToDictionary(pair => pair.Key, pair => pair.Value);

                foreach (KeyValuePair<Location, int> pair in sortedDict)
                {
                    if (mState.TimeRemaining < 75)
                        return;

                    Path p = mState.getSinglePath(ant, pair.Key);
                    Location destination;
                    if (p.Count == 0)
                        destination = pair.Key;
                    else
                    {
                        destination = p.Peek().Location;
                        travelingTo.Add(p);
                    }

                    Boolean moved = false;
                    foreach (char dir in mState.direction(ant, destination))
                    {
                        if (moveTo(ant, destination, dir))
                        {
                            targets.Add(pair.Key);
                            moved = true;
                            break;
                        }
                    }
                    if (moved)
                        break;
                } */
                 
            }
            Debug.Write("   - " + cached + " cached paths used.");
            Debug.Write("   - " + paths + " paths generated.");

        }

        public void getOffMyHills()
        {
            Debug.Write(" * Get off my hill!");
            foreach (Location hill in mState.MyHills)
            {
                if (orders.Contains(hill))
                    continue;

                AntLoc antOnHill = mState.MyAnts.Find(delegate(AntLoc loc) { return loc.Equals(hill); });
                if (antOnHill != null) 
                {
                    foreach (char dir in Ants.Aim.Keys)
                    {
                        Location newLoc = mState.destination(hill, dir);

                        if (currentMoves.Contains(newLoc))
                            continue;

                        if (moveTo(antOnHill, newLoc, dir))
                            break;
                    }
                }
            }
        }
        
        private int SortByDistance(LocAndDist a, LocAndDist b)
        {
            if (a.Distance == b.Distance)
                return 0;
            else if (a.Distance > b.Distance)
                return 1;
            else
                return -1;
        }

        private int SortByDistanceAndHill(LocAndDist a, LocAndDist b)
        {
            if (a.Solo != b.Solo)
            {
                if (a.Solo)
                    return 1;
                else
                    return -1;
            }

            if (a.Distance == b.Distance)
                return 0;
            else if (a.Distance > b.Distance)
                return 1;
            else
                return -1;
        }
        
        public static void Main(string[] args)
        {
            new Ants().playGame(new MyBot());
        }
        
    }

}


/*




using System;
using System.Collections.Generic;

namespace Ants {
    
	class MyBot : Bot {
        public static char NORTH = 'n';
        public static char EAST = 'e';
        public static char SOUTH = 's';
        public static char WEST = 'w';
		private GameState mState;

        private List<Ant> ants = new List<Ant>();
        private List<Location> enemyBases = new List<Location>();
        private List<Location> previousPositions = new List<Location>();
        private List<Location> currentPositions = new List<Location>();
        private List<Location> currentMoves = new List<Location>();


        private double ViewRadius { get; set; }
        private MapSearch map;

        public override void doSetup(GameState state)
        {
            ViewRadius = Math.Sqrt(state.ViewRadius2);
            map = new MapSearch(GameState.Height, GameState.Width, (int)ViewRadius);
            ants = new List<Ant>();
        }

        public override void doEnd(GameState state)
        {
            
        }

		// doTurn is run once per turn
		public override void doTurn (GameState state) {
			mState = state;
            Debug.Write("***** TURN " + state.CurrentTurn + " *****");
            previousPositions = new List<Location>(currentPositions);
            currentPositions.Clear();
            currentMoves.Clear();

            // Update our map
            map.update(state, state.MyAnts);

            // Add any new ants to our list
            foreach(AntLoc ant in state.MyAnts)
            {
                if (ants.Find(delegate(Ant a) { return a.Loc.Equals(ant); }) == null)
                {
                    Ant a = new Ant(ant);

                    if (ants.Count < 10)
                        a.Role = Ant.Roles.WORKER;
                    else
                    {
                        switch (ants.Count % 3)
                        {
                            case 0:
                                a.Role = Ant.Roles.WORKER;
                                break;
                            case 1:
                                a.Role = Ant.Roles.SCOUT;
                                break;
                            case 2:
                                a.Role = Ant.Roles.SOLDIER;
                                break;
                            default:
                                a.Role = Ant.Roles.SCOUT;
                                break;
                        }
                    }
                    ants.Add(a);
                    // move off the spawn
                    foreach (char dir in Ants.Aim.Keys)
                    {
                        Location newLoc = state.destination(a.Loc, dir);
                        if (moveTo(a.Loc, newLoc, dir))
                        {
                            a.Loc = new AntLoc(newLoc.row, newLoc.col, a.Loc.team);
                            break;
                        }
                    }
                }
            }

            // Remove dead ants
            foreach (Location loc in state.DeadTiles)
                ants.RemoveAll(delegate(Ant a) { return a.Loc.Equals(loc); });

            foreach (Location hill in state.EnemyHills)
            {
                if (enemyBases.Find(delegate(Location h) { return h.Equals(hill); }) == null)
                    enemyBases.Add(hill);
            }

            for(int i = enemyBases.Count - 1; i >= 0; i--)
            {
                foreach (Ant ant in ants)
                {
                    if (ant.Loc.Equals(enemyBases[i]))
                    {
                        enemyBases.RemoveAt(i);
                        break;
                    }
                }
            }

            if (state.MyAnts.Count != ants.Count)
            {
                Debug.Write("TURN: " + state.CurrentTurn + " --- Numbers don't equal! " + ants.Count + " ants in my list when there is actually " + state.MyAnts.Count + " ants!");

                for (int i = ants.Count - 1; i >= 0; i--)
                {
                    if (state.MyAnts.Find(delegate(AntLoc a) { return a.Equals(ants[i].Loc); }) == null)
                        ants.RemoveAt(i);
                }
            }

            // MOVE OFF THE SPAWN -- double check!
            foreach (Location hill in state.MyHills)
            {
                foreach (Ant a in ants)
                {
                    if (hill.Equals(a.Loc))
                    {
                        foreach (char dir in Ants.Aim.Keys)
                        {
                            Location newLoc = state.destination(a.Loc, dir);
                            if (moveTo(a.Loc, newLoc, dir))
                            {
                                a.Loc = new AntLoc(newLoc.row, newLoc.col, a.Loc.team);
                                break;
                            }
                        }
                    }
                }
            }

            int pathCalc = 0;
            // Do moves for any ants that already have a target
            foreach (Ant ant in ants)
            {
                if (ant.Path != null)
                {
                    if (ant.Path.Count > 0)
                    {
                        List<char> d = state.direction(ant.Loc, ant.Path.Peek().Location);
                        foreach (char dir in d)
                        {
                            Location newLoc = state.destination(ant.Loc, dir);
                            if (moveTo(ant.Loc, newLoc, dir))
                            {
                                ant.Loc = new AntLoc(newLoc.row, newLoc.col, ant.Loc.team);
                                if (ant.Path.Count > 1 && ant.Path.Peek().Location.Equals(newLoc))
                                    ant.Path.Pop();
                                else
                                    ant.Path = null;
                            }
                            else
                            {
                                // we had an invalid path
                                ant.Path = null;
                            }
                        }
                    }
                    else
                    {
                        /*
                        // This allows us to move off the hill if we just spawned
                        Location randLoc = state.destination(ant.Loc, EAST);
                        if (moveTo(ant.Loc, randLoc, EAST))
                        {
                            ant.Loc = new AntLoc(randLoc.row, randLoc.col, ant.Loc.team);
                            continue;
                        }
                        randLoc = state.destination(ant.Loc, SOUTH);
                        if (moveTo(ant.Loc, randLoc, SOUTH))
                        {
                            ant.Loc = new AntLoc(randLoc.row, randLoc.col, ant.Loc.team);
                            continue;
                        }
                        randLoc = state.destination(ant.Loc, WEST);
                        if (moveTo(ant.Loc, randLoc, WEST))
                        {
                            ant.Loc = new AntLoc(randLoc.row, randLoc.col, ant.Loc.team);
                            continue;
                        }
                        randLoc = state.destination(ant.Loc, NORTH);
                        if (moveTo(ant.Loc, randLoc, NORTH))
                        {
                            ant.Loc = new AntLoc(randLoc.row, randLoc.col, ant.Loc.team);
                            continue;
                        }
                    }
                }                
            }
            foreach(Ant ant in ants)
            {
                if(ant.Path == null)
                {
                    List<Location> targets = new List<Location>();
                    switch (ant.Role)
                    {
                        case Ant.Roles.WORKER:
                            targets.AddRange(state.FoodTiles);
                            break;
                        case Ant.Roles.SCOUT:
                            targets.AddRange(map.Points);
                            break;
                        case Ant.Roles.SOLDIER:
                            targets.AddRange(enemyBases);
                            break;
                    }
                    // if the ants default search space is empty try scouting
                    if(targets.Count == 0)
                        targets.AddRange(map.Points);

                    if (targets.Count > 0)
                    {
                        ant.Path = mState.getPath(ant.Loc, targets);
                        pathCalc++;
                    }
                }
                
                if (mState.TimeRemaining < 50) break;
            }

            Debug.Write("Calc'd " + pathCalc + " paths.");
        }

        public Boolean getCloseItems(GameState state, Ant ant)
        {
            Boolean moved = false;
            if (ant.Role == Ant.Roles.SCOUT)
            {
                // Look for food right around us
                foreach (Location food in state.FoodTiles)
                {
                    if (moved)
                        break;

                    int dist = state.distance(ant.Loc, food);
                    if (dist <= 3)
                    {
                        List<char> dirs = state.direction(ant.Loc, food);
                        foreach (char dir in dirs)
                        {
                            Location newLoc = state.destination(ant.Loc, dir);
                            if (moveTo(ant.Loc, newLoc, dir))
                            {
                                ant.Loc = new AntLoc(newLoc.row, newLoc.col, ant.Loc.team);
                                moved = true;
                                break;
                            }
                        }

                    }
                }
            }
            return moved;
        }

        public Boolean moveTo(AntLoc ant, Location dest, char dir)
        {
            if (mState.unoccupied(dest) && !previousPositions.Contains(dest) && !currentMoves.Contains(dest))
            {
                issueOrder(ant, dir);

                currentPositions.Add(ant);
                currentMoves.Add(dest);
                return true;
            }
            else
                return false;
        }

        private int SortByDistance(LocAndDist a, LocAndDist b)
        {
            if (a.Distance == b.Distance)
                return 0;
            else if (a.Distance > b.Distance)
                return 1;
            else
                return -1;
        }

        private int SortByDistanceAndHill(LocAndDist a, LocAndDist b)
        {
            if (a.Solo != b.Solo)
            {
                if (a.Solo)
                    return 1;
                else
                    return -1;
            }

            if (a.Distance == b.Distance)
                return 0;
            else if (a.Distance > b.Distance)
                return 1;
            else
                return -1;
        }
		/*
		public static void Main (string[] args) {
			new Ants().playGame(new MyBot());
		}
        
	}
	
}*/