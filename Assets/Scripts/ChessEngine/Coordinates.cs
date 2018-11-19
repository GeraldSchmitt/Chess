using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChessEngine
{
    public struct Coordinates
    {
        public int l;
        public int c;

        public Coordinates(int column, int line)
        {
            l = line;
            c = column;
        }

        public override string ToString()
        {
            return string.Format("{0}{1}", (char)('A' + c), l + 1);
        }

        public Coordinates Move(int delta_c, int delta_l)
        {
            return new Coordinates(c + delta_c, l + delta_l);
        }
    }
}
