using System;
using System.Collections.Generic;

namespace Ants
{

    class ExNihiloBot : Bot
    {
        public static char NORTH = 'n';
        public static char EAST = 'e';
        public static char SOUTH = 's';
        public static char WEST = 'w';
        private GameState mState;

        private List<Location> previousPositions = new List<Location>();    // Used to not move back the way you came if possible
        private List<Location> positionsMoved = new List<Location>();       // Squares we have an ant on that has already moved
        private List<Location> currentMoves = new List<Location>();         // Squares we have moved to, useful for not going to the same place and dying

        private List<Location> zoneDanger = new List<Location>();   // Used to warn of immenent danger
        private List<Location> zoneHill = new List<Location>();     // Used to mark path to enemy hills
        private List<Location> zoneVisited = new List<Location>();  // Used to mark squares recently visited, try not to revist them again until a bit of time has passed


        private double ViewRadius { get; set; }

        public override void doSetup(GameState state)
        {
            ViewRadius = Math.Sqrt(state.ViewRadius2);
        }

        // doTurn is run once per turn
        public override void doTurn(GameState state)
        {
            mState = state;

            previousPositions = new List<Location>(positionsMoved);
            positionsMoved.Clear();
            currentMoves.Clear();

            getCloseItems();            
        }

        public Boolean moveTo(AntLoc ant, Location dest, char dir)
        {
            if (mState.unoccupied(dest) && !previousPositions.Contains(dest) && !currentMoves.Contains(dest))
            {
                issueOrder(ant, dir);

                positionsMoved.Add(ant);
                currentMoves.Add(dest);
                return true;
            }
            else
                return false;
        }

        public void getEnemyHills()
        {

        }

        public void getCloseItems()
        {
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
                foreach (AntLoc ant in mState.MyAnts)
                {
                    antDists.Add(new LocAndDist(ant, hill, mState.distance(ant, hill), false));
                }
            }

            antDists.Sort(SortByDistanceAndHill);

            foreach (LocAndDist pair in antDists)
            {
                if ((pair.Solo && currentMoves.Contains(pair.Dest)) || positionsMoved.Contains(pair.Ant))
                    continue;
                
                Path p = mState.getSinglePath(pair.Ant, pair.Dest);
                Location destination;
                if (p.Count == 0)
                    destination = pair.Dest;
                else
                    destination = p.Peek().Coords;


                foreach (char dir in mState.direction(pair.Ant, destination))
                {
                    //Location newLoc = mState.destination(pair.Ant, dir);
                    if (moveTo(pair.Ant, destination, dir))
                        break;
                }

                if (mState.TimeRemaining < 50) break;
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
        /*
        public static void Main(string[] args)
        {
            new Ants().playGame(new ExNihiloBot());
        }
        */
    }

}

/*********** OLD CODE ********/
/*
           getCloseItems();
           Debug.Write("Got items!");
           foreach (AntLoc ant in state.MyAnts)
           {
               if (currentPositions.Contains(ant))
                   continue;

               Location exploreTo = map.nearest(state, ant);

               List<Location> t = new List<Location>();
               t.Add(exploreTo);
               List<char> dirs = mState.getPath(ant, t);
               foreach (char dir in dirs)
               {
                   Location newLoc = mState.destination(ant, dir);
                   if (moveTo(ant, newLoc, dir))
                       break;
               }
           }
           Debug.Write("Searched!");
           /*

           // update our unseen tiles
           List<LocAndDist> unseenDist = new List<LocAndDist>();
           for(int i = unseen.Count - 1; i >= 0; i--)
           {
               foreach (AntLoc ant in state.MyAnts)
               {
                   int d = state.distance(ant, unseen[i]);
                   if (d <= ViewRadius)
                   {
                       unseen.Remove(unseen[i]);
                       break;
                   }
                   unseenDist.Add(new LocAndDist(ant, unseen[i], d));

                   if (mState.TimeRemaining < 200) break;
               }

               if (mState.TimeRemaining < 200) break;
           }

            

           /*
           if (mState.TimeRemaining < 50)
               return;
            
           Debug.Write("Nodes: " + unseenDist.Count);
           unseenDist.RemoveAll(delegate(LocAndDist a) { return currentPositions.Contains(a.Ant); });
           Debug.Write("After Pruning: " + unseenDist.Count);
           if (unseenDist.Count > 0 && mState.TimeRemaining > 100)
           {
               Debug.Write("T0: " + mState.TimeRemaining);
               unseenDist.Sort(SortByDistance);
               Debug.Write("T1: " + mState.TimeRemaining);

               foreach (LocAndDist pair in unseenDist)
               {
                   if (pair.Distance > (GameState.Width + GameState.Height) / 2)
                   {

                       foreach (char dir in mState.direction(pair.Ant, pair.Dest))
                       {
                           Location newLoc = mState.destination(pair.Ant, dir);
                           if (moveTo(pair.Ant, newLoc, dir))
                               break;
                       }
                   }
                   else
                   {
                       List<Location> t = new List<Location>();
                       t.Add(pair.Dest);
                       List<char> dirs = mState.getPath(pair.Ant, t);
                       foreach (char dir in dirs)
                       {
                           Location newLoc = mState.destination(pair.Ant, dir);
                           if (moveTo(pair.Ant, newLoc, dir))
                           {
                               if (pair.Solo)
                                   targets.Add(pair.Dest);
                               break;
                           }
                       }
                   }

                   if (mState.TimeRemaining < 75) break;
               }
           }
             
           // loop through all my ants and try to give them orders
           foreach (AntLoc ant in state.MyAnts) {
               if (currentPositions.Contains(ant))
                   continue;


               /*
               // Explore!
               Location dest;
               if (unseen.Contains(ant.East))
               {
                   dest = state.destination(ant, EAST);
                   if (moveTo(ant, dest, EAST))
                       continue;
               }
               else if (unseen.Contains(ant.South))
               {
                   dest = state.destination(ant, SOUTH);
                   if (moveTo(ant, dest, SOUTH))
                       continue;
               }
               else if (unseen.Contains(ant.West))
               {
                   dest = state.destination(ant, WEST);
                   if (moveTo(ant, dest, WEST))
                       continue;
               }
               else if (unseen.Contains(ant.North))
               {
                   dest = state.destination(ant, NORTH);
                   if (moveTo(ant, dest, NORTH))
                       continue;
               }
                
               // Go right then down then left then up
               Location randLoc = state.destination(ant, EAST);
               if (moveTo(ant, randLoc, EAST))
                   continue;
               randLoc = state.destination(ant, SOUTH);
               if (moveTo(ant, randLoc, SOUTH))
                   continue;
               randLoc = state.destination(ant, WEST);
               if (moveTo(ant, randLoc, WEST))
                   continue;
               randLoc = state.destination(ant, NORTH);
               if (moveTo(ant, randLoc, NORTH))
                   continue;
				
               // check if we have time left to calculate more orders
               if (state.TimeRemaining < 25) break;
           }
          
       }
       public void getCloseItems()
       {
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
           foreach (HillLoc hill in mState.EnemyHills)
           {
               foreach (AntLoc ant in mState.MyAnts)
               {
                   antDists.Add(new LocAndDist(ant, hill, mState.distance(ant, hill), false));
               }
           }

           antDists.Sort(SortByDistanceAndHill);

           List<Location> targets = new List<Location>();
           foreach (LocAndDist pair in antDists)
           {
               if (targets.Contains(pair.Dest))
                   continue;

               List<Location> t = new List<Location>();
               t.Add(pair.Dest);
               List<char> dirs = mState.getPath(pair.Ant, t);
               foreach (char dir in dirs)
               {
                   Location newLoc = mState.destination(pair.Ant, dir);
                   if (moveTo(pair.Ant, newLoc, dir))
                   {
                       if (pair.Solo)
                           targets.Add(pair.Dest);
                       break;
                   }
               }

               if (mState.TimeRemaining < 50) break;
           }
       }
       */