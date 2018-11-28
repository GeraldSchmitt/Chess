using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChessEngine
{
    [Flags]
    public enum CellContent
    {
        Empty = 0x01,

        White = 0x02,
        Black = 0x04,

        Pawn = 0x08,
        Bishop = 0x10,
        Knight = 0x20,
        Rook = 0x40,
        Queen = 0x80,
        King = 0x100,

        WPawn = White | Pawn,
        WBishop = White | Bishop,
        WKnight = White | Knight,
        WRook = White | Rook,
        WQueen = White | Queen,
        WKing = White | King,

        BPawn = Black | Pawn,
        BBishop = Black | Bishop,
        BKnight = Black | Knight,
        BRook = Black | Rook,
        BQueen = Black | Queen,
        BKing = Black | King,

        Outside = 0x200,
        EnPassant = 0x400,
    }

    public static class CellContentExtensions
    {
        public static bool HasFlag(this CellContent value, CellContent flag)
        {
            return (value & flag) == flag; 
        }

        public static CellContent OpponentColor(this CellContent value)
        {
            if (value.HasFlag(CellContent.White)) return CellContent.Black;

            if (value.HasFlag(CellContent.Black)) return CellContent.White;

            return CellContent.Empty;
        }
    }
}
