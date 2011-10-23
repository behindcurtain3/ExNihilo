using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ants
{
    public class Mapper
    {
        private List<Location> view_offsets = new List<Location>();
        private Boolean[,] vision;

        public int Height { get; set; }
        public int Width { get; set; }
        public int Radius { get; set; }

        public Mapper(int h, int w, int radius)
        {
            Debug.Write("Mapper init: " + h + ", " + w + ", " + radius);
            vision = new Boolean[h, w];

            for (int i = 0; i < h; i++)
                for (int j = 0; j < w; j++)
                    vision[i, j] = false;

            String line = "";
            for (int row = -radius; row < radius + 1; row++)
            {
                line = "";
                for (int col = -radius; col < radius + 1; col++)
                {
                    int d = Math.Abs(row + col);
                    if (d < radius)
                    {
                        Location l = new Location(row, col);
                        view_offsets.Add(l);
                        line += l + " ";
                    }
                }
                Debug.Write(line);
            }
            Debug.Write("Map setup.");
        }

        public void update(GameState state)
        {
            Location l;
            foreach (AntLoc ant in state.MyAnts)
            {
                if (state.TimeRemaining < 50)
                    return;

                //vision[ant.row, ant.col] = true;
                //state.Unseen.Remove(ant);
                
                foreach (Location offset in view_offsets)
                {
                    l = new Location(ant.row + offset.row, ant.col + offset.col, true);

                    vision[l.row, l.col] = true;
                    state.Unseen.Remove(l);
                }
            }
            Debug.Write("   - Map updated.");
        }

        public Boolean visible(Location l)
        {
            return vision[l.row, l.col];
        }
    }
}
