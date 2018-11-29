using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChessEngine
{
    public class Move
    {
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
            return From == Coordinates.WKing && 
                To == Coordinates.WKingBig;
        }

        public bool IsWSmallCastle()
        {
            return From == Coordinates.WKing && 
                To == Coordinates.WKingSmall;
        }

        public bool IsBBigCastle()
        {
            return From == Coordinates.BKing && 
                To == Coordinates.BKingBig;
        }

        public bool IsBSmallCastle()
        {
            return From == Coordinates.BKing && 
                To == Coordinates.BKingSmall;
        }
    }
}