using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ants
{
    public class Ant
    {
        public AntLoc Loc { get; set; }

        public Ant(AntLoc l)
        {
            Loc = l;
        }
    }
}
