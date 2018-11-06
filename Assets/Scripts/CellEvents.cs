using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellEvents : MonoBehaviour {

    public int Line;

    public int Column;

    public Board Board { get; set; }

	// Use this for initialization
	void Start () {
		
	}

    public void Select()
    {
        Debug.Log(string.Format("[CellEvents] Select {0} {1}", Line, Column));
        Board.OnCellClick(Column, Line);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
