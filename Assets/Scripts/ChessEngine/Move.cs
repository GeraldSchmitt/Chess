using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChessEngine
{
    public class Move
    {
        public Coordinates From { get; set; }
        public Coordinates To { get; set; }
        public bool EnPassant { get; set; }

        public Move()
        { }

        public Move(Coordinates from, Coordinates to, bool enPassant = false)
        {
            From = from;
            To = to;
            EnPassant = enPassant;
            if (enPassant)
            {
                Debug.Log("Move en passant !");
            }
        }

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