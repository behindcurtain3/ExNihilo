using System;
using System.Collections.Generic;
using System.Text;

namespace Ants
{
    public class Path : Stack<ANode>
    {
        public Path() { }
        public Path(ANode[] init) : base(init) { }
        public Path(Path p) :base(new Stack<ANode>(p)) { }
    }
}
