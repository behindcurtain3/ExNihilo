using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ants
{
    public class Hill
    {
        public HillLoc Location { get; set; }

        public Hill(HillLoc location)
        {
            Location = location;
        }
    }
}
