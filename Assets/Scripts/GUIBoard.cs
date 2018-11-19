using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ChessEngine;

public class GUIBoard : MonoBehaviour {
    //------------------------
    // Editor properties
    //------------------------
    public GameObject BlackCellPrefab;
    public GameObject WhiteCellPrefab;
    public GameObject SelectedCellPrefab;
    public GameObject OkMovePrefab;
    public Texture2D Sprites;

    public float CellSize = 2.56f;
    
    private Board _board;

    public struct CellStruct
    {
        public GameObject BoardCell;
        public GameObject PieceSprite;
    }

    private CellStruct[,] BoardCells = new CellStruct[8, 8];

    private List<GameObject> OkMoves = new List<GameObject>();

    private const float PieceZDepth = -0.01f;
    private const float SelectionZDepth = -0.001f;
    private const float TopZDepth = -0.02f;

	// Use this for initialization
	void Start () {
        _board = new Board();
        _board.OnMove += OnMove;

        // Draw the board
        for (int l = 0; l < 8; l++)
        {
            for (int c = 0; c < 8; c++)
            {
                var cellPrefab = (c + l) % 2 == 1 ? WhiteCellPrefab : BlackCellPrefab;
                var cell = Instantiate(cellPrefab, GetPosition(c, l), Quaternion.identity);
                cell.AddComponent<CellEvents>();
                cell.transform.parent = transform;
                var events = cell.GetComponent<CellEvents>();
                events.Line = l;
                events.Column = c;
                events.Board = this;
                cell.AddComponent<BoxCollider>();
                BoardCells[c, l].BoardCell = cell;
            }
        }

        // Load the sprites pieces
        Dictionary<CellContent, Sprite> Pieces = new Dictionary<CellContent, Sprite>();
        int spriteWidth = Sprites.width / 6;
        int spriteHeight = Sprites.height / 2;
        List<CellContent> spriteOrder = new List<CellContent> {
            CellContent.BKing, CellContent.BQueen, CellContent.BBishop, CellContent.BKnight, CellContent.BRook, CellContent.BPawn,
            CellContent.WKing, CellContent.WQueen, CellContent.WBishop, CellContent.WKnight, CellContent.WRook, CellContent.WPawn};
        int spriteOrderId = 0;
        for (int l = 0; l < 2; l++)
        {
            for (int c = 0; c < 6; c++)
            {
                var s = Sprite.Create(
                    Sprites,
                    new Rect(spriteWidth * c, spriteHeight * l, spriteWidth, spriteHeight),
                    new Vector2(0.5f, 0.5f));
                Pieces[spriteOrder[spriteOrderId]] = s; 
                spriteOrderId++;
            }
        }

        // Draw the pieces
        for (int l = 0; l < 8; l++)
        {
            for (int c = 0; c < 8; c++)
            {
                var piece = _board.CellsContent[c, l];
                if (piece == CellContent.Empty) continue;
                var sprite = Instantiate(
                    Pieces[piece],
                    new Vector3(0, 0, 0),
                    Quaternion.identity);
                var go = new GameObject();
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = sprite;
                go.transform.position = GetPosition(c, l, PieceZDepth);
                go.transform.localScale = new Vector3(0.8f, 0.8f);
                BoardCells[c, l].PieceSprite = go;
            }
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    private GameObject _selectedCell;
    private bool _isSelection = false;
    private Coordinates _selectedCoordinates;
    private IEnumerable<Coordinates> _possibleMoves;
    public void OnCellClick(int c, int l)
    {
        if (!_isSelection)
        {
            if (_selectedCell == null)
            {
                _selectedCell = Instantiate(SelectedCellPrefab);
                _selectedCell.transform.parent = transform;
            }

            _selectedCell.SetActive(true);
            _selectedCell.transform.position =
                BoardCells[c, l].BoardCell.transform.position +
                new Vector3(0, 0, SelectionZDepth);
            _selectedCoordinates.l = l;
            _selectedCoordinates.c = c;

            if (_board.CellsContent[c,l] != CellContent.Empty)
                _isSelection = true;

            // Get possible moves
            _possibleMoves = _board.PossibleMoves(_selectedCoordinates);
            ShowPossibleMoves(_possibleMoves);
            Debug.Log(string.Format("{0} possible moves :", _possibleMoves.Count()));
            Debug.Log(string.Join(", ", _possibleMoves.Select(x => x.ToString()).ToArray()));
        }
        else
        {
            ShowPossibleMoves(null);
            Debug.Log(string.Format(
                "{0}-{1} to {2}-{3}", 
                _selectedCoordinates.c,
                _selectedCoordinates.l,
                c,
                l));
            _selectedCell.SetActive(false);
            _isSelection = false;
            var newCoord = new Coordinates { c = c, l = l };
            if (_possibleMoves.Contains(newCoord))
            {
                Move m = new Move(_selectedCoordinates, newCoord);
                _board.Move(m);
            }

        }
    }

    private void OnMove(Move m)
    {
        Move(m.From, m.To);
    }

    private void Move(Coordinates from, Coordinates to)
    {
        if (from.c == to.c && from.l == to.l) return;

        var fromCell = BoardCells[from.c, from.l];
        var toCell = BoardCells[to.c, to.l];

        if (toCell.PieceSprite != null)
        {
            toCell.PieceSprite.SetActive(false);
        }

        toCell.PieceSprite = fromCell.PieceSprite;
        toCell.PieceSprite.transform.position = 
            toCell.BoardCell.transform.position
            + new Vector3(0,0, PieceZDepth);
        fromCell.PieceSprite = null;

        // Because struct is passed by value
        BoardCells[from.c, from.l] = fromCell;
        BoardCells[to.c, to.l] = toCell;
    }

    private void ShowPossibleMoves(IEnumerable<Coordinates> okMoves)
    {
        // Delete existing indication
        foreach(var go in OkMoves)
        {
            // todo continue here
            Destroy(go);
        }
        OkMoves.Clear();

        if (okMoves == null)
            return;

        // Draw new indication
        foreach(var move in okMoves)
        {
            var go = Instantiate(
                OkMovePrefab,
                GetPosition(move, TopZDepth),
                Quaternion.identity);
            OkMoves.Add(go);
        }
    }

    private Vector3 GetPosition(int c, int l, float zDepth = 0)
    {
        return new Vector3(CellSize * (c - 3.5f), CellSize * (l - 3.5f), zDepth);
    }

    private Vector3 GetPosition(Coordinates coord, float zDepth = 0)
    {
        return GetPosition(coord.c, coord.l, zDepth);
    }
}
