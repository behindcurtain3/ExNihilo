using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ants
{
    public class Ant
    {
        public enum Roles { SCOUT, WORKER, SOLDIER };

        public AntLoc Loc { get; set; }
        public Path Path { get; set; }
        public Location Target { get; set; }
        public Roles Role { get; set; }

        public Ant(AntLoc l)
        {
            Loc = l;
        }


    }
}
