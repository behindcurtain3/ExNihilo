using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ants
{
    public class MapSearch
    {
        public int Height { get; set; }
        public int Width { get; set; }
        public int Sight { get; set; } // How far do our units see?
        public double Range { get; set; }

        public List<Location> Points { get; set; }

        public MapSearch(int h, int w, int s)
        {
            Height = h;
            Width = w;
            Sight = s * 2;
            Range = s;

            int start = (int)(s * 0.5);

            Points = new List<Location>();
            String line = "";
            for (int y = start; y < Height; y += Sight)
            {
                line = "";
                for (int x = start; x < Width; x += Sight)
                {
                    Location l = new Location(y, x);
                    line += " " + l;
                    Points.Add(l);
                }
                Debug.Write(line);
            }

            Debug.Write("Map setup.");
        }

        public void update(GameState state, List<AntLoc> ants)
        {
            for (int i = Points.Count - 1; i >= 0; i--)
            {
                foreach (AntLoc ant in ants)
                {
                    if (state.TimeRemaining < 50)
                        return;

                    if (state.distance(ant, Points[i]) <= Range)
                    {
                        Debug.Write("   - Removed point: " + Points[i]);
                        Points.RemoveAt(i);
                        break;
                    }
                }
            }
        }
    }
}
