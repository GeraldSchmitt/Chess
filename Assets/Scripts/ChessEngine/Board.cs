﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ChessEngine
{
    class Board
    {
        public CellContent[,] CellsContent { get { return _cellsContent; } }   
        private CellContent[,] _cellsContent = new CellContent[8, 8];

        public delegate void MoveEvent(Move m);
        public event MoveEvent OnMove;

        private delegate IEnumerable<Coordinates> moveRule(Coordinates coord);
        private Dictionary<CellContent, moveRule> moveRules;

        public CellContent ActivePlayer { get { return activePlayer; } }
        private CellContent activePlayer = CellContent.White;

        public bool WhiteCheck { get; private set; }
        public bool BlackCheck { get; private set; }

        public Board()
        {
            InitPieces();
            InitMovementRules();
        }

        public Board(Board b)
        {
            _cellsContent = (CellContent[,])b.CellsContent.Clone();
            InitMovementRules();
            activePlayer = b.activePlayer;
        }

        public void Move(Move m)
        {
            CellsContent[m.To.c, m.To.l] = CellsContent[m.From.c, m.From.l];
            CellsContent[m.From.c, m.From.l] = CellContent.Empty;

            activePlayer = activePlayer.OpponentColor();

            WhiteCheck = IsCheck(CellContent.White);
            BlackCheck = IsCheck(CellContent.Black);

            if (OnMove != null)
                OnMove.Invoke(m);
        }

        private void InitPieces()
        {
            for (int c = 0; c < 8; c++)
                for (int l = 0; l < 8; l++)
                {
                    _cellsContent[c, l] = CellContent.Empty;
                }

            for (int c = 0; c < 8; c++)
            {
                _cellsContent[c, 1] = CellContent.WPawn;
                _cellsContent[c, 6] = CellContent.BPawn;
            }
            _cellsContent[0, 0] = CellContent.WRook;
            _cellsContent[1, 0] = CellContent.WKnight;
            _cellsContent[2, 0] = CellContent.WBishop;
            _cellsContent[3, 0] = CellContent.WQueen;
            _cellsContent[4, 0] = CellContent.WKing;
            _cellsContent[5, 0] = CellContent.WBishop;
            _cellsContent[6, 0] = CellContent.WKnight;
            _cellsContent[7, 0] = CellContent.WRook;
            _cellsContent[0, 7] = CellContent.BRook;
            _cellsContent[1, 7] = CellContent.BKnight;
            _cellsContent[2, 7] = CellContent.BBishop;
            _cellsContent[3, 7] = CellContent.BQueen;
            _cellsContent[4, 7] = CellContent.BKing;
            _cellsContent[5, 7] = CellContent.BBishop;
            _cellsContent[6, 7] = CellContent.BKnight;
            _cellsContent[7, 7] = CellContent.BRook;
        }

        private void InitMovementRules()
        {
            moveRules = new Dictionary<CellContent, moveRule>();
            moveRules[CellContent.WPawn] = WPawnRule;
            moveRules[CellContent.BPawn] = BPawnRule;
            moveRules[CellContent.WKing] = (c) => KingRule(c, CellContent.Black);
            moveRules[CellContent.BKing] = (c) => KingRule(c, CellContent.White);
            moveRules[CellContent.WBishop] = (c) => BishopRule(c, CellContent.Black);
            moveRules[CellContent.BBishop] = (c) => BishopRule(c, CellContent.White);
            moveRules[CellContent.WRook] = (c) => RookRule(c, CellContent.Black);
            moveRules[CellContent.BRook] = (c) => RookRule(c, CellContent.White);
            moveRules[CellContent.WQueen] = (c) => QueenRule(c, CellContent.Black);
            moveRules[CellContent.BQueen] = (c) => QueenRule(c, CellContent.White);
            moveRules[CellContent.WKnight] = (c) => KnightRule(c, CellContent.Black);
            moveRules[CellContent.BKnight] = (c) => KnightRule(c, CellContent.White);
        }

        public IEnumerable<Coordinates> PossibleMoves(Coordinates coord)
        {
            var res = new List<Coordinates>();
            var content = _cellsContent[coord.c, coord.l];
            if (moveRules.ContainsKey(content))
            {
                return moveRules[content](coord);
            }

            return res;
        }

        public IEnumerable<Coordinates> LegalMoves(Coordinates from)
        {
            if (!GetCellContent(from).HasFlag(activePlayer))
            {
                yield break;
            }

            // A legal move is a move that do not put or let the king in check
            var possiblesMoves = PossibleMoves(from);
            foreach(var to in possiblesMoves)
            {
                var move = new Move(from, to); 
                var newBoard = PlayMove(move);
                if (!newBoard.IsCheck(activePlayer))
                {
                    yield return to;
                }
            }
        }

        public Board PlayMove(Move m)
        {
            var newBoard = new Board(this);
            newBoard.Move(m);
            return newBoard;
        }

        public bool IsCheck(CellContent playerColor)
        {
            //calculer toutes les cases atteintes par les pièces adverses
            Coordinates kingPos;
            if (!KingPosition(playerColor, out kingPos))
                return false;

            var opponentColor = playerColor.OpponentColor();

            for (int l = 0; l < 8; l++)
            {
                for (int c = 0; c < 8; c++)
                {
                    if (_cellsContent[c, l].HasFlag(opponentColor))
                    {
                        var moves = PossibleMoves(new Coordinates(c, l));
                        if (moves.Contains(kingPos))
                            return true;
                    }
                }
            }

            return false;
        }

        private bool KingPosition(CellContent playerColor, out Coordinates kingPos)
        {
            CellContent king = playerColor | CellContent.King;
            for (int l = 0; l < 8; l++)
            {
                for (int c = 0; c < 8; c++)
                {
                    if (_cellsContent[c,l] == king)
                    {
                        kingPos = new Coordinates(c, l);
                        return true;
                    }
                }
            }
            kingPos = new Coordinates();
            return false;
        }

        private IEnumerable<Coordinates> KnightRule(Coordinates coord, CellContent opponentColor)
        {
            var res = new List<Coordinates>();
            for(int g = -2; g <= 2; g+=4)
            {
                for (int p = -1; p <= 1; p += 2)
                {
                    var move = coord.Move(g, p);
                    var cell = GetCellContent(move);
                    if (cell == CellContent.Empty ||
                        cell.HasFlag(opponentColor))
                    {
                        res.Add(move);
                    }

                    move = coord.Move(p, g);
                    cell = GetCellContent(move);
                    if (cell == CellContent.Empty ||
                        cell.HasFlag(opponentColor))
                    {
                        res.Add(move);
                    }
                }
            }
            return res;
        }

        private IEnumerable<Coordinates> QueenRule(Coordinates coord, CellContent opponentColor)
        {
            return RookRule(coord, opponentColor)
                .Concat(BishopRule(coord, opponentColor));
        }

        private IEnumerable<Coordinates> RookRule(Coordinates coord, CellContent opponentColor)
        {
            var res = new List<Coordinates>();
            for (int dc = -1; dc <= 1; dc++)
            {
                for (int dl = -1; dl <= 1; dl++)
                {
                    if(dl == 0 ^ dc == 0)
                    {
                        var move = coord;
                        bool continueMove = true;
                        do
                        {
                            move = move.Move(dc, dl);
                            var cell = GetCellContent(move);
                            if (cell.HasFlag(opponentColor))
                            {
                                res.Add(move);
                                continueMove = false;
                            }
                            else if (cell == CellContent.Empty)
                            {
                                res.Add(move);
                            }
                            else
                            {
                                continueMove = false;
                            }
                        }
                        while (continueMove);
                    }
                }
            }

            return res;
        }

        private IEnumerable<Coordinates> BishopRule(Coordinates coord, CellContent opponentColor)
        {
            var res = new List<Coordinates>();

            // For all for 4 directions :
            for (int dc = -1; dc <= 1; dc+=2)
            {
                for (int dl = -1; dl <= 1; dl+=2)
                {
                    var move = coord;
                    bool continueMove = true;
                    do
                    {
                        move = move.Move(dc, dl);
                        var cell = GetCellContent(move);
                        if (cell.HasFlag(opponentColor))
                        {
                            res.Add(move);
                            continueMove = false;
                        }
                        else if(cell == CellContent.Empty)
                        {
                            res.Add(move);
                        }
                        else
                        {
                            continueMove = false;
                        }
                    }
                    while (continueMove);
                }
            }

            return res;
        }

        private IEnumerable<Coordinates> KingRule(Coordinates coord, CellContent opponentColor)
        {
            var res = new List<Coordinates>();
            for (int dc = -1; dc <= 1; dc++)
            {
                for (int dl = -1; dl <= 1; dl++)
                {
                    if (dc == 0 && dl == 0) continue;

                    var move = coord.Move(dc, dl);
                    var cell = GetCellContent(move);
                    if (cell.HasFlag(opponentColor) ||
                        cell == CellContent.Empty)
                    {
                        res.Add(move);
                    }
                }
            }

            return res;
        }

        private IEnumerable<Coordinates> PawnRule(Coordinates coord, CellContent color)
        {
            var res = new List<Coordinates>();
            var firstLine = color == CellContent.White ? 1 : 6;
            var direction = color == CellContent.White ? 1 : -1;
            var opponentColor = color == CellContent.White ? CellContent.Black : CellContent.White;

            // Can move ?
            Coordinates move = coord.Move(0, direction);
            if (GetCellContent(move) == CellContent.Empty)
            {
                res.Add(move);

                // Can double move ?
                move = coord.Move(0, direction * 2);
                if (coord.l == firstLine && GetCellContent(move) == CellContent.Empty)
                    res.Add(move);
            }

            // Can promote ?
            // TODO promote the pawn !

            // Can eat ?
            move = coord.Move(1, direction);
            if (GetCellContent(move).HasFlag(opponentColor))
            {
                res.Add(move);
            }

            move = coord.Move(-1, direction);
            if (GetCellContent(move).HasFlag(opponentColor))
            {
                res.Add(move);
            }

            return res;
        }

        private IEnumerable<Coordinates> BPawnRule(Coordinates coord)
        {
            return PawnRule(coord, CellContent.Black);
        }

        private IEnumerable<Coordinates> WPawnRule(Coordinates coord)
        {
            return PawnRule(coord, CellContent.White);
        }

        private CellContent GetCellContent(Coordinates c)
        {
            if (c.c < 0 || c.l < 0 || c.c > 7 || c.l > 7)
                return CellContent.Outside;
            return _cellsContent[c.c, c.l];
        }

        public override string ToString()
        {
            Dictionary<CellContent, char> letters = new Dictionary<CellContent, char>();
            letters[CellContent.Empty] = '_';
            letters[CellContent.WPawn] = 'p';
            letters[CellContent.BPawn] = 'P';
            letters[CellContent.WRook] = 'r';
            letters[CellContent.BRook] = 'R';
            letters[CellContent.WKnight] = 'c';
            letters[CellContent.BKnight] = 'C';
            letters[CellContent.WBishop] = 'b';
            letters[CellContent.BBishop] = 'B';
            letters[CellContent.WQueen] = 'q';
            letters[CellContent.BQueen] = 'Q';
            letters[CellContent.WKing] = 'k';
            letters[CellContent.BKing] = 'K';

            string res = string.Empty;

            for (int l = 7; l >= 0; l--)
            {
                for (int c = 0; c < 8; c++)
                {
                    res += letters[_cellsContent[c, l]];
                }
                res += '\n';
            }

            return res;
        }
    }
}