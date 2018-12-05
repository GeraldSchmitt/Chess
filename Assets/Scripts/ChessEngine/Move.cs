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
        public CellContent Promotion { get; set; }

        public Move()
        { }

        public Move(Coordinates from, Coordinates to, bool enPassant = false)
        {
            From = from;
            To = to;
            EnPassant = enPassant;
            Promotion = CellContent.Empty;
        }

        public Move(Coordinates from, Coordinates to, CellContent promotion)
        {
            From = from;
            To = to;
            EnPassant = false;
            Promotion = promotion;
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

        public override string ToString()
        {
            string res = To.ToString();
            switch(Promotion)
            {
                case CellContent.BQueen:
                case CellContent.WQueen:
                    res += "Q";
                    break;

                case CellContent.BBishop:
                case CellContent.WBishop:
                    res += "B";
                    break;

                case CellContent.BKnight:
                case CellContent.WKnight:
                    res += "C";
                    break;

                case CellContent.BRook:
                case CellContent.WRook:
                    res += "R";
                    break;
            }

            return res;
        }
    }
}