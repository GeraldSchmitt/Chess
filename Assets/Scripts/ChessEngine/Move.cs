using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChessEngine
{
    public class Move
    {
        private static readonly Coordinates WKingPos = new Coordinates("e1");
        private static readonly Coordinates WKingPosBig = new Coordinates("c1");
        private static readonly Coordinates WKingPosSmall = new Coordinates("g1");

        private static readonly Coordinates BKingPos = new Coordinates("e8");
        private static readonly Coordinates BKingPosBig = new Coordinates("c8");
        private static readonly Coordinates BKingPosSmall = new Coordinates("g8");

        public Move()
        { }

        public Move(Coordinates from, Coordinates to)
        {
            From = from;
            To = to;
        }

        public Coordinates From { get; set; }
        public Coordinates To { get; set; }

        public bool IsWBigCastle()
        {
            return From == WKingPos && To == WKingPosBig;
        }

        public bool IsWSmallCastle()
        {
            return From == WKingPos && To == WKingPosSmall;
        }

        public bool IsBBigCastle()
        {
            return From == BKingPos && To == BKingPosBig;
        }

        public bool IsBSmallCastle()
        {
            return From == BKingPos && To == BKingPosSmall;
        }
    }
}