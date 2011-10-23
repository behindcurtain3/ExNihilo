using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

namespace Ants
{

    class MyBot : Bot
    {
        public static char NORTH = 'n';
        public static char EAST = 'e';
        public static char SOUTH = 's';
        public static char WEST = 'w';
        private GameState mState;

        private MapSearch map;
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
            map = new MapSearch(GameState.Height, GameState.Width, Range);
        }

        public override void doEnd(GameState state)
        {            
            Bitmap img = new Bitmap(GameState.Width * 4, GameState.Height * 4);
            Brush on = Brushes.DarkGreen;
            Brush off = Brushes.Blue;

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
            
            getCloseItems();
            if(mState.TimeRemaining > 200) getUnexploredAreas();
            if(mState.TimeRemaining > 50) getOffMyHills();

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
            Debug.Write(" * Getting items");
            // attempt to find food
            List<LocAndDist> antDists = new List<LocAndDist>();
            foreach (Location food in mState.FoodTiles)
            {
                foreach (AntLoc ant in mState.MyAnts)
                {
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

                    antDists.Add(new LocAndDist(ant, knownEnemyHills[i], mState.distance(ant, knownEnemyHills[i]), false));
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
            Debug.Write(" * Exploring");

            if (mState.Unseen.Count == 0)
                return;

            // Update our map
            map.update(mState, mState.MyAnts);

            // Remove finished paths
            travelingTo.RemoveAll(delegate(Path p) { return p.Count == 0; });
            int paths = 0;
            foreach (AntLoc ant in mState.MyAnts)
            {
                if (orders.Contains(ant as Location))
                    continue;

                if (mState.TimeRemaining < 100)
                    return;

                // Try to use a path we've already calculated
                for(int i = travelingTo.Count - 1; i >= 0; i--)
                {
                    if (travelingTo[i].Peek().Location.Equals(ant))
                    {
                        // Yay!
                        travelingTo[i].Pop();

                        if (travelingTo[i].Count == 0)
                        {
                            travelingTo.RemoveAt(i);
                            break;
                        }

                        Location destination = travelingTo[i].Peek().Location;
                        foreach (char dir in mState.direction(ant, destination))
                        {
                            if (moveTo(ant, destination, dir))
                                break;
                        }
                    }
                }

                // Check if we've moved, again
                if (orders.Contains(ant as Location))
                    continue;

                Path p = mState.getPath(ant, map.Points);
                Location dest;
                if (p.Count == 0)
                    continue;

                paths++;
                dest = p.Peek().Location;
                foreach (char dir in mState.direction(ant, dest))
                {
                    if (moveTo(ant, dest, dir))
                        break;
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