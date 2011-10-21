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

		// doTurn is run once per turn
		public override void doTurn (GameState state) {
			mState = state;
            //Debug.Write("***** TURN " + state.CurrentTurn + " *****");
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
                        a.Role = Ant.Roles.SCOUT;
                    else
                    {
                        switch (ants.Count % 3)
                        {
                            case 0:
                                a.Role = Ant.Roles.SCOUT;
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
                        List<char> d = state.direction(ant.Loc, ant.Path.Peek().Coords);
                        foreach (char dir in d)
                        {
                            Location newLoc = state.destination(ant.Loc, dir);
                            if (moveTo(ant.Loc, newLoc, dir))
                            {
                                ant.Loc = new AntLoc(newLoc.row, newLoc.col, ant.Loc.team);
                                if (ant.Path.Count > 1 && ant.Path.Peek().Coords.Equals(newLoc))
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
                        }*/
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
		
		public static void Main (string[] args) {
			new Ants().playGame(new MyBot());
		}

	}
	
}