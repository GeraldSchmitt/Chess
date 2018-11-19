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
    }
}