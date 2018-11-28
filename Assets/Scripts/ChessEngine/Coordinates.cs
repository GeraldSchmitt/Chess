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

        public Coordinates(string s)
        {
            if (s.Length != 2)
                throw new ArgumentException(s + " is not a valid coordinate");

            c = s[0] - 'a';
            l = s[1] - '1';
        }

        public override string ToString()
        {
            return string.Format("{0}{1}", (char)('a' + c), l + 1);
        }

        public Coordinates Move(int delta_c, int delta_l)
        {
            return new Coordinates(c + delta_c, l + delta_l);
        }

        public static bool operator ==(Coordinates c1, Coordinates c2)
        {
            return c1.Equals(c2);
        }

        public static bool operator !=(Coordinates c1, Coordinates c2)
        {
            return !c1.Equals(c2);
        }
    }
}
