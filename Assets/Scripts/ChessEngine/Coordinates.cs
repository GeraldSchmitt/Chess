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

        public static readonly Coordinates WKing = new Coordinates("e1");
        public static readonly Coordinates WKingBig = new Coordinates("c1");
        public static readonly Coordinates WKingSmall = new Coordinates("g1");

        public static readonly Coordinates BKing = new Coordinates("e8");
        public static readonly Coordinates BKingBig = new Coordinates("c8");
        public static readonly Coordinates BKingSmall = new Coordinates("g8");

        public static readonly Coordinates d8 = new Coordinates("d8");
        public static readonly Coordinates f8 = new Coordinates("f8");
        public static readonly Coordinates d1 = new Coordinates("d1");
        public static readonly Coordinates f1 = new Coordinates("f1");
        
        public static readonly Coordinates WRookL = new Coordinates("a1");
        public static readonly Coordinates WRookR = new Coordinates("h1");
        public static readonly Coordinates BRookL = new Coordinates("a8");
        public static readonly Coordinates BRookR = new Coordinates("h8");

        public static readonly Coordinates None = new Coordinates(-1, -1);
    }
}
