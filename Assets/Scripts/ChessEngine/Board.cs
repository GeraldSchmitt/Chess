using System;
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

        private delegate IEnumerable<Move> moveRule(Coordinates coord, bool doNotCastle);
        private Dictionary<CellContent, moveRule> moveRules;

        public CellContent ActivePlayer { get { return activePlayer; } }
        private CellContent activePlayer = CellContent.White;

        public bool WhiteCheck { get; private set; }
        public bool BlackCheck { get; private set; }

        private bool canWhiteBigCastle = true;
        private bool canWhiteSmallCastle = true;
        private bool canBlackBigCastle = true;
        private bool canBlackSmallCastle = true;

        private Coordinates enPassant = Coordinates.None;

        private static readonly Coordinates[] blackSmallCastleCoords =
        {
            new Coordinates("f8"),
            new Coordinates("g8")
        };

        private static readonly Coordinates[] blackBigCastleCoords =
        {
            new Coordinates("d8"),
            new Coordinates("c8"),
            new Coordinates("b8")
        };

        private static readonly Coordinates[] whiteSmallCastleCoords =
        {
            new Coordinates("f1"),
            new Coordinates("g1")
        };

        private static readonly Coordinates[] whiteBigCastleCoords =
        {
            new Coordinates("d1"),
            new Coordinates("c1"),
            new Coordinates("b1")
        };

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

        private void Move(Coordinates from, Coordinates to)
        {
            CellsContent[to.c, to.l] = CellsContent[from.c, from.l];
            CellsContent[from.c, from.l] = CellContent.Empty;
        }

        public void Move(Move m)
        {
            var movingPiece = GetCellContent(m.From);

            // Normal move
            CellsContent[m.To.c, m.To.l] = CellsContent[m.From.c, m.From.l];
            CellsContent[m.From.c, m.From.l] = CellContent.Empty;

            // Castling move
            if (m.IsBBigCastle())
            {
                Move(Coordinates.BRookL, Coordinates.d8);
            }
            else if (m.IsBSmallCastle())
            {
                Move(Coordinates.BRookR, Coordinates.f8);
            }
            else if (m.IsWBigCastle())
            {
                Move(Coordinates.WRookL, Coordinates.d1);
            }
            else if (m.IsWSmallCastle())
            {
                Move(Coordinates.WRookR, Coordinates.f1);
            }

            // Is it prise en Passant ?
            if (m.To == enPassant && movingPiece.HasFlag(CellContent.Pawn))
            {
                if(m.To.l == 2)
                {
                    CellsContent[m.To.c, 3] = CellContent.Empty;
                }
                else
                {
                    CellsContent[m.To.c, 4] = CellContent.Empty;
                }
            }

            // Remember en passant
            enPassant = Coordinates.None;
            if (GetCellContent(m.To).HasFlag(CellContent.Pawn) &&
                Math.Abs(m.To.l - m.From.l) == 2)
            {
                enPassant = new Coordinates(
                    m.From.c,
                    (m.From.l + m.To.l) / 2);
                Debug.Log("Remember en passant " + enPassant.ToString());
            }

            // Next player
            activePlayer = activePlayer.OpponentColor();

            // Update board state
            WhiteCheck = IsCheck(CellContent.White, true);
            BlackCheck = IsCheck(CellContent.Black, true);

            // Remember if castling pieces moved
            if (m.From == Coordinates.WKing)
            {
                canWhiteBigCastle = false;
                canWhiteSmallCastle = false;
            }
            else if (m.From == Coordinates.WRookL)
            {
                canWhiteBigCastle = false;
            }
            else if (m.From == Coordinates.WRookR)
            {
                canWhiteSmallCastle = false;
            }
            else if (m.From == Coordinates.BKing)
            {
                canBlackBigCastle = false;
                canBlackSmallCastle = false;
            }
            else if (m.From == Coordinates.BRookL)
            {
                canBlackBigCastle = false;
            }
            else if (m.From == Coordinates.BRookR)
            {
                canBlackSmallCastle = false;
            }

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
            moveRules[CellContent.WPawn] = (c, x) => PawnRule(c, CellContent.White);
            moveRules[CellContent.BPawn] = (c, x) => PawnRule(c, CellContent.Black);
            moveRules[CellContent.WKing] = (c, x) => KingRule(c, CellContent.Black, x);
            moveRules[CellContent.BKing] = (c, x) => KingRule(c, CellContent.White);
            moveRules[CellContent.WBishop] = (c, x) => BishopRule(c, CellContent.Black);
            moveRules[CellContent.BBishop] = (c, x) => BishopRule(c, CellContent.White);
            moveRules[CellContent.WRook] = (c, x) => RookRule(c, CellContent.Black);
            moveRules[CellContent.BRook] = (c, x) => RookRule(c, CellContent.White);
            moveRules[CellContent.WQueen] = (c, x) => QueenRule(c, CellContent.Black);
            moveRules[CellContent.BQueen] = (c, x) => QueenRule(c, CellContent.White);
            moveRules[CellContent.WKnight] = (c, x) => KnightRule(c, CellContent.Black);
            moveRules[CellContent.BKnight] = (c, x) => KnightRule(c, CellContent.White);
        }

        public IEnumerable<Move> PossibleMoves(Coordinates coord, bool doNotCastle)
        {
            var res = new List<Move>();
            var content = _cellsContent[coord.c, coord.l];
            if (moveRules.ContainsKey(content))
            {
                return moveRules[content](coord, doNotCastle);
            }

            return res;
        }

        public IEnumerable<Move> LegalMoves(Coordinates from)
        {
            if (!GetCellContent(from).HasFlag(activePlayer))
            {
                yield break;
            }

            // A legal move is a move that do not put or let the king in check
            var possiblesMoves = PossibleMoves(from, false);
            foreach(var move in possiblesMoves)
            {
                var newBoard = PlayMove(move);
                if (!newBoard.IsCheck(activePlayer, true))
                {
                    yield return move;
                }
            }
        }

        public Board PlayMove(Move m)
        {
            var newBoard = new Board(this);
            newBoard.Move(m);
            return newBoard;
        }

        public bool IsCheck(CellContent playerColor, bool doNotCastle)
        {
            Coordinates kingPos;
            if (!KingPosition(playerColor, out kingPos))
                return false;

            return IsCheck(playerColor, kingPos, doNotCastle);
        }

        private bool IsCheck(CellContent playerColor, Coordinates position, bool doNotCastle)
        {
            var opponentColor = playerColor.OpponentColor();

            for (int l = 0; l < 8; l++)
            {
                for (int c = 0; c < 8; c++)
                {
                    if (_cellsContent[c, l].HasFlag(opponentColor))
                    {
                        var moves = PossibleMoves(new Coordinates(c, l), doNotCastle);
                        if (moves.Select(x => x.To).Contains(position))
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

        private IEnumerable<Move> KnightRule(Coordinates coord, CellContent opponentColor)
        {
            var res = new List<Move>();
            for(int g = -2; g <= 2; g+=4)
            {
                for (int p = -1; p <= 1; p += 2)
                {
                    var to = coord.Move(g, p);
                    var cell = GetCellContent(to);
                    if (cell == CellContent.Empty ||
                        cell.HasFlag(opponentColor))
                    {
                        res.Add(new Move(coord, to));
                    }

                    to = coord.Move(p, g);
                    cell = GetCellContent(to);
                    if (cell == CellContent.Empty ||
                        cell.HasFlag(opponentColor))
                    {
                        res.Add(new Move(coord, to));
                    }
                }
            }
            return res;
        }

        private IEnumerable<Move> QueenRule(Coordinates coord, CellContent opponentColor)
        {
            return RookRule(coord, opponentColor)
                .Concat(BishopRule(coord, opponentColor));
        }

        private IEnumerable<Move> RookRule(Coordinates coord, CellContent opponentColor)
        {
            var res = new List<Move>();
            for (int dc = -1; dc <= 1; dc++)
            {
                for (int dl = -1; dl <= 1; dl++)
                {
                    if(dl == 0 ^ dc == 0)
                    {
                        var to = coord;
                        bool continueMove = true;
                        do
                        {
                            to = to.Move(dc, dl);
                            var cell = GetCellContent(to);
                            if (cell.HasFlag(opponentColor))
                            {
                                res.Add(new Move(coord, to));
                                continueMove = false;
                            }
                            else if (cell == CellContent.Empty)
                            {
                                res.Add(new Move(coord, to));
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

        private IEnumerable<Move> BishopRule(Coordinates coord, CellContent opponentColor)
        {
            var res = new List<Move>();

            // For all for 4 directions :
            for (int dc = -1; dc <= 1; dc+=2)
            {
                for (int dl = -1; dl <= 1; dl+=2)
                {
                    var to = coord;
                    bool continueMove = true;
                    do
                    {
                        to = to.Move(dc, dl);
                        var cell = GetCellContent(to);
                        if (cell.HasFlag(opponentColor))
                        {
                            res.Add(new Move(coord, to));
                            continueMove = false;
                        }
                        else if(cell == CellContent.Empty)
                        {
                            res.Add(new Move(coord, to));
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

        private IEnumerable<Move> KingRule(
            Coordinates coord, 
            CellContent opponentColor,
            bool doNotCastle = false)
        {
            var playerColor = opponentColor.OpponentColor();
            var res = new List<Move>();
            for (int dc = -1; dc <= 1; dc++)
            {
                for (int dl = -1; dl <= 1; dl++)
                {
                    if (dc == 0 && dl == 0) continue;

                    var to = coord.Move(dc, dl);
                    var cell = GetCellContent(to);
                    if (cell.HasFlag(opponentColor) ||
                        cell == CellContent.Empty)
                    {
                        res.Add(new Move(coord, to));
                    }
                }
            }

            if (doNotCastle)
                return res;

            bool canBigCastle;
            bool canSmallCastle;
            Coordinates[] bigCastleCoords;
            Coordinates[] smallCastleCoords;
            if (playerColor == CellContent.White)
            {
                canBigCastle = canWhiteBigCastle;
                canSmallCastle = canWhiteSmallCastle;
                bigCastleCoords = whiteBigCastleCoords;
                smallCastleCoords = whiteSmallCastleCoords;
            }
            else
            {
                canBigCastle = canBlackBigCastle;
                canSmallCastle = canBlackSmallCastle;
                bigCastleCoords = blackBigCastleCoords;
                smallCastleCoords = blackSmallCastleCoords;
            }

            if (canBigCastle)
            {
                if (bigCastleCoords.All(x => GetCellContent(x).HasFlag(CellContent.Empty)) &&
                    bigCastleCoords.Take(2).All(x => !IsCheck(playerColor, x, true)))
                {
                    res.Add(new Move(coord, bigCastleCoords[1]));
                }
            }

            if (canSmallCastle)
            {
                if (smallCastleCoords.All(x => GetCellContent(x).HasFlag(CellContent.Empty)) &&
                    smallCastleCoords.Take(2).All(x => !IsCheck(playerColor, x, true)))
                {
                    res.Add(new Move(coord, smallCastleCoords[1]));
                }
            }

            return res;
        }

        private IEnumerable<Move> PawnRule(Coordinates coord, CellContent color)
        {
            var res = new List<Move>();
            var firstLine = color == CellContent.White ? 1 : 6;
            var direction = color == CellContent.White ? 1 : -1;
            var opponentColor = color.OpponentColor();

            // Can move ?
            Coordinates to = coord.Move(0, direction);
            if (GetCellContent(to) == CellContent.Empty)
            {
                res.Add(new Move(coord,to));

                // Can double move ?
                to = coord.Move(0, direction * 2);
                if (coord.l == firstLine && GetCellContent(to) == CellContent.Empty)
                    res.Add(new Move(coord, to));
            }

            // Can promote ?
            // TODO promote the pawn !

            // Can eat ?
            to = coord.Move(1, direction);
            if (GetCellContent(to).HasFlag(opponentColor)
                || to == enPassant)
            {
                res.Add(new Move(coord, to, to == enPassant));
            }

            to = coord.Move(-1, direction);
            if (GetCellContent(to).HasFlag(opponentColor)
                || to == enPassant)
            {
                res.Add(new Move(coord, to, to == enPassant));
            }

            return res;
        }

        private IEnumerable<Move> BPawnRule(Coordinates coord)
        {
            return PawnRule(coord, CellContent.Black);
        }

        private IEnumerable<Move> WPawnRule(Coordinates coord)
        {
            return PawnRule(coord, CellContent.White);
        }

        public CellContent GetCellContent(Coordinates c)
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
