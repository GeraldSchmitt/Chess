using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour {
    //------------------------
    // Editor properties
    //------------------------
    public GameObject BlackCellPrefab;
    public GameObject WhiteCellPrefab;
    public GameObject SelectedCellPrefab;
    public Texture2D Sprites;

    public float CellSize = 2.56f;

    public enum CellContent
    {
        Empty,
        WPawn,
        WBishop,
        WHorse,
        WRook,
        WQueen,
        WKing,
        BPawn,
        BBishop,
        BHorse,
        BRook,
        BQueen,
        BKing,
    }

    private CellContent[,] CellsContent = new CellContent[8,8];

    public struct Coordinates
    {
        public int l;
        public int c;
    }

    public struct CellStruct
    {
        public GameObject BoardCell;
        public GameObject PieceSprite;
    }

    private CellStruct[,] BoardCells = new CellStruct[8, 8];

	// Use this for initialization
	void Start () {

        // Draw the board
        for (int l = 0; l < 8; l++)
        {
            for (int c = 0; c < 8; c++)
            {
                var cellPrefab = (c + l) % 2 == 1 ? WhiteCellPrefab : BlackCellPrefab;
                var cell = Instantiate(cellPrefab, new Vector3(CellSize * (c - 3.5f), CellSize * (l - 3.5f), 0), Quaternion.identity);
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
            CellContent.BKing, CellContent.BQueen, CellContent.BBishop, CellContent.BHorse, CellContent.BRook, CellContent.BPawn,
            CellContent.WKing, CellContent.WQueen, CellContent.WBishop, CellContent.WHorse, CellContent.WRook, CellContent.WPawn};
        int spriteOrderId = 0;
        for (int l = 0; l < 2; l++)
        {
            for (int c = 0; c < 6; c++)
            {
                var s = Sprite.Create(
                    Sprites,
                    new Rect(spriteWidth * c, spriteHeight * l, spriteWidth, spriteHeight),
                    //Vector2.zero);
                    new Vector2(0.5f, 0.5f));
                Pieces[spriteOrder[spriteOrderId]] = s; 
                spriteOrderId++;
            }
        }

        // Init the piece positions
        for (int c = 0; c < 8; c++)
        {
            CellsContent[c, 1] = CellContent.WPawn;
            CellsContent[c, 6] = CellContent.BPawn;
        }
        CellsContent[0, 0] = CellContent.WRook;
        CellsContent[1, 0] = CellContent.WHorse;
        CellsContent[2, 0] = CellContent.WBishop;
        CellsContent[3, 0] = CellContent.WQueen;
        CellsContent[4, 0] = CellContent.WKing;
        CellsContent[5, 0] = CellContent.WBishop;
        CellsContent[6, 0] = CellContent.WHorse;
        CellsContent[7, 0] = CellContent.WRook;
        CellsContent[0, 7] = CellContent.BRook;
        CellsContent[1, 7] = CellContent.BHorse;
        CellsContent[2, 7] = CellContent.BBishop;
        CellsContent[3, 7] = CellContent.BQueen;
        CellsContent[4, 7] = CellContent.BKing;
        CellsContent[5, 7] = CellContent.BBishop;
        CellsContent[6, 7] = CellContent.BHorse;
        CellsContent[7, 7] = CellContent.BRook;

        // Draw the pieces
        for (int l = 0; l < 8; l++)
        {
            for (int c = 0; c < 8; c++)
            {
                var piece = CellsContent[c, l];
                if (piece == CellContent.Empty) continue;
                var sprite = Instantiate(
                    Pieces[piece],
                    new Vector3(0, 0, 0),
                    Quaternion.identity);
                var go = new GameObject();
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = sprite;
                go.transform.position = new Vector3(CellSize * (c - 3.5f), CellSize * (l - 3.5f), -0.01f);
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
                new Vector3(0, 0, -0.001f);
            _selectedCoordinates.l = l;
            _selectedCoordinates.c = c;

            if (CellsContent[c,l] != CellContent.Empty)
                _isSelection = true;
        }
        else
        {
            Debug.Log(string.Format(
                "{0}-{1} to {2}-{3}", 
                _selectedCoordinates.c,
                _selectedCoordinates.l,
                c,
                l));
            _selectedCell.SetActive(false);
            _isSelection = false;
            var newCoord = new Coordinates { c = c, l = l };
            Move(_selectedCoordinates, newCoord);
        }
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
            + new Vector3(0,0,-0.1f);
        fromCell.PieceSprite = null;
        CellsContent[to.c, to.l] = CellsContent[from.c, from.l];
        CellsContent[from.c, from.l] = CellContent.Empty;

        // Because struct is passed by value
        BoardCells[from.c, from.l] = fromCell;
        BoardCells[to.c, to.l] = toCell;
    }
}
