﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class BoardController : MonoBehaviour 
{
	public GameObject selectedPiece;
	public GameObject grid;
	public GameObject board_Prefab;
	public GameObject tile_Prefab;

	private Player playerTurn;
	private Highlighter highlighter;
	public List<GameObject> chessPiecePrefab;

	private int selectionX;
	private int selectionY;

	private Dictionary<string, List<Piece>> threatTable;

	// Use this for initialization
	void Start () 
	{
		selectedPiece = null;
		highlighter = GetComponent<Highlighter> ();
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (GameController.gameController.curGameState == GameController.GameStates.GAME_START) 
		{
			GameStart ();
			GameController.gameController.setGameState (GameController.GameStates.IN_GAME);
		} 
		else if (GameController.gameController.curGameState == GameController.GameStates.IN_GAME) 
		{
			UpdateCursorPos ();
			if(GameController.gameController.curTurnState == GameController.TurnStates.DEFAULT)
				GameController.gameController.setTurnState (GameController.TurnStates.TURN_START);	
		}
		if (GameController.gameController.curTurnState == GameController.TurnStates.TURN_START) 
		{
			GameController.gameController.curTurnState = GameController.TurnStates.CAN_SELECT;
		}
		if (GameController.gameController.curTurnState == GameController.TurnStates.CAN_SELECT) 
		{
			SelectCell ();
			highlighter.UpdateHighlightableOnMouseCollision (8, Color.blue);
		} 
		else if (GameController.gameController.curTurnState == GameController.TurnStates.CAN_MOVE) 
		{
			SelectCell ();
			highlighter.UpdateHighlightableOnMouseCollision (8, Color.blue);
			bool mouseClicked = Input.GetMouseButtonDown (1);
			if (mouseClicked) {	
				DeselectPiece ();
			}
		} 
		else if (GameController.gameController.curTurnState == GameController.TurnStates.HAS_MOVED) 
		{
			// updating pieces relevant to the move and their corresponding threat table entries would occur here
			GameController.gameController.curTurnState = GameController.TurnStates.END_TURN;
		}
		else if(GameController.gameController.curTurnState == GameController.TurnStates.END_TURN)
		{
			SwitchPlayer ();
		}
	}
	public void SwitchPlayer()
	{
		PlayerController playerController = GameController.gameController.playerController.GetComponent<PlayerController> ();
		if (playerTurn == playerController.player1_scr)
			playerTurn = playerController.player2_scr;
		else
			playerTurn = playerController.player1_scr;
		GameController.gameController.curTurnState = GameController.TurnStates.TURN_START;
	}
	public void GameStart()
	{
		if (grid == null) 
			CreateBoard ();
		PlayerController playerController = GameController.gameController.playerController.GetComponent<PlayerController> ();
		playerTurn = playerController.assignRandomColor ();
	}
	public void turnStart()
	{
		
	}
	public void turnEnd()
	{
		
	}
	private void CreateBoard()
	{
		grid = (GameObject)Instantiate (board_Prefab, new Vector3(transform.position.x, transform.position.y, transform.position.z), transform.rotation);
		grid.transform.parent = transform;

		Grid grid_scr = grid.GetComponent<Grid> ();
		int i,j;
		float xOffset, yOffset;
		xOffset = yOffset = 0.00f;
		int currCells = 0;

		for (i = 0; i < grid_scr.NumOfRows; i++) 
		{
			for (j = 0; j < grid_scr.NumOfColumns; j++) 
			{
				// instantiate board cells, assign associated sprite
				grid_scr.grid [i,j] = (GameObject)Instantiate (tile_Prefab, new Vector3(transform.position.x + xOffset, transform.position.y + yOffset, transform.position.z), transform.rotation);
				grid_scr.grid [i, j].transform.parent = grid.transform;
				SetCellCenter (grid_scr.grid [i, j]);
				grid_scr.grid [i, j].GetComponent<Cell> ().row = i;
				grid_scr.grid [i, j].GetComponent<Cell>().column = j;
				grid_scr.grid [i, j].name = (char)('a' + j) + "" + (i+1);
				currCells += 1;
				if (i % 2 == 0)
				{
					if (currCells % 2 == 0) 
					{
						grid_scr.grid[i,j].GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("vanilla_tile_edit1");
					} 
					else 
					{
						grid_scr.grid[i,j].GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("wood_tile");
					}
				} 
				else 
				{
					if (currCells % 2 == 0) 
					{
						grid_scr.grid[i,j].GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("wood_tile");
					} 
					else 
					{
						grid_scr.grid[i,j].GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("vanilla_tile_edit1");
					}
				}
				// create chess pieces and set their initial position
				switch (i) 
				{
				case 0:
					if (j == 0 || j == 7)
						InstantiateChessPiece (1, grid_scr.grid [i, j], false);
					else if (j == 1 || j == 6)
						InstantiateChessPiece (2, grid_scr.grid [i, j], false);
					else if (j == 2 || j == 5)
						InstantiateChessPiece (3, grid_scr.grid [i, j], false);
					else if (j == 4)
						InstantiateChessPiece (4, grid_scr.grid [i, j], false);
					else
						InstantiateChessPiece (5, grid_scr.grid [i, j], false);
					break;
				case 1:
					InstantiateChessPiece (0, grid_scr.grid [i, j], false);
					break;
				case 6:
					InstantiateChessPiece (0, grid_scr.grid [i, j], true);
					break;
				case 7:
					if (j == 0 || j == 7)
						InstantiateChessPiece (1, grid_scr.grid [i, j], true);
					else if (j == 1 || j == 6)
						InstantiateChessPiece (2, grid_scr.grid [i, j], true);
					else if (j == 2 || j == 5)
						InstantiateChessPiece (3, grid_scr.grid [i, j], true);
					else if (j == 4)
						InstantiateChessPiece (4, grid_scr.grid [i, j], true);
					else
						InstantiateChessPiece (5, grid_scr.grid [i, j], true);
					break;
				}
				xOffset += grid_scr.distanceBetweenTiles;
			}
			yOffset += grid_scr.distanceBetweenTiles;
			xOffset = 0;
		}

	}

	private void SetCellCenter(GameObject cell)
	{
		Grid grid_scr = grid.GetComponent<Grid> ();
		Cell cell_scr = cell.GetComponent<Cell> ();
		Vector3 origin = cell.GetComponent<Transform> ().position;
		Vector3 endPoint = origin + new Vector3(grid_scr.distanceBetweenTiles, grid_scr.distanceBetweenTiles, 0);
		cell_scr.CellMidPoint = (origin + endPoint) / 2;
	}

	// instantiates the specified chess piece prefab at the specified location
	// indexes: 0 [Pawn], 1[rook], 2[knight], 3[Bishop], 4[Queen], 5[King]
	// location: the cell where the Piece resides
	// isWhite: true whenever the piece is for player White, false when the piece is for player Black
	private void InstantiateChessPiece(int index, GameObject cell, bool isWhite)
	{
		Cell cell_scr = cell.GetComponent<Cell> ();
		// we want the piece to spawn at the center of its specified cell, so we call this function and store the data
		GameObject newPiece = (GameObject)Instantiate (chessPiecePrefab[index], cell_scr.CellMidPoint, Quaternion.identity);
		newPiece.transform.parent = cell.transform;
		cell.GetComponent<Cell>().MyPiece = newPiece; 

		// now we can set all the fields of our Piece
		if(newPiece.GetComponent<Piece>() != null)
		{
			Piece pieceScript = newPiece.GetComponent<Piece> ();
			pieceScript.isWhite = isWhite;
			pieceScript.enabled = true;
		}
		else
		{
			Debug.LogError("Error: the prefab at chessPiecePrefab["+index+"] does not contain a Piece script. Objects in this collection must have a Piece component.");
		}
	}

	// updates the x and y of our cursor's current position on the board and rounds the x and y to the lower integer bound
	private void UpdateCursorPos()
	{
		if ( !Camera.main ) 
		{
			Debug.Log ("No main camera, exiting.");
			return;
		}
		RaycastHit2D hit;
		hit = Physics2D.Raycast (Camera.main.ScreenToWorldPoint (Input.mousePosition), Vector2.zero, LayerMask.GetMask ("bottom"));
		if (hit) 
		{
			selectionX = Mathf.FloorToInt (hit.point.x);
			selectionY = Mathf.FloorToInt (hit.point.y);
		} 
		else 
		{
			selectionX = -1;
			selectionY = -1;
		}
	}

	public void SelectCell()
	{
		float gridOriginX = grid.GetComponent<Transform> ().position.x;
		float gridOriginY = grid.GetComponent<Transform> ().position.y;
		Grid scr_Grid = grid.GetComponent<Grid> ();
		bool mouseClicked = Input.GetMouseButtonDown (0);
		if (mouseClicked) 
		{
			if (selectionX >= gridOriginX &&
			    selectionX < gridOriginX + scr_Grid.NumOfColumns &&
			    selectionY >= gridOriginY &&
			    selectionY < gridOriginY + scr_Grid.NumOfRows &&
			    mouseClicked) 
			{
				GameObject cell = scr_Grid.grid [selectionY, selectionX];
				Cell scr_Cell = cell.GetComponent<Cell>();
				if (GameController.gameController.curTurnState == GameController.TurnStates.CAN_SELECT) 
				{
					if( scr_Cell.MyPiece != null && DoesColorMatch(playerTurn, scr_Cell.MyPiece.GetComponent<Piece>()) )
					{
						selectedPiece = scr_Cell.MyPiece;
						UpdateVisitableList (selectedPiece);
						highlighter.HighlightVisitableCells (selectedPiece);
						highlighter.AddHighlight (selectedPiece, Color.yellow);
						GameController.gameController.curTurnState = GameController.TurnStates.CAN_MOVE;
					}
				} 
				else if (GameController.gameController.curTurnState == GameController.TurnStates.CAN_MOVE) 
				{
					// if the player clicks on one of their own pieces while a piece is selected
					if (scr_Cell.MyPiece != null && DoesColorMatch (playerTurn, scr_Cell.MyPiece.GetComponent<Piece> ())) {
						if (scr_Cell.MyPiece.Equals (selectedPiece)) {
							// if the player clicks on the piece that's already selected, deselect piece
							DeselectPiece ();
						} else {
							// if the player clicks on a different piece of the same color, deselect piece
							// then select the new piece
							DeselectPiece ();
							SelectCell ();
						}
					} else {
						// else, if the player clicks an empty cell, attempt to move to the cell.
						// invalid moves are handled by the Move method
						Move (cell);
					}
				}
			}
		}
	}
	public void DeselectPiece()
	{
		highlighter.UndoHighlightVisitableCells (selectedPiece);
		highlighter.RemoveHighlight (selectedPiece);
		selectedPiece = null;
		GameController.gameController.curTurnState = GameController.TurnStates.CAN_SELECT;
	}

	public void UpdateVisitableList(GameObject piece)
	{
		// save relevant components for this calculation
		Cell cell_scr = piece.GetComponentInParent<Cell>();
		Grid grid_scr = grid.GetComponent<Grid> ();
		Piece piece_scr = piece.GetComponent<Piece> ();
		// clear the list to modify the list for the current piece position
		piece_scr.ValidCells.Clear ();
		if (piece_scr is Pawn) 
		{
			// update the pawn's visitable cells based on its set of movement vectors (default: forward by one or two) and whether or not it has moved before
			Pawn pawn_scr = (Pawn)piece_scr;
			int scaleMax = 1;
			if (!pawn_scr.hasMoved)
				scaleMax = 2;
			bool doScaling = true;
			PopulateVisitableList( cell_scr, grid_scr, pawn_scr, doScaling, scaleMax);
			// update the pawn's visitable cells based on its capture vectors (default: diagonally left or right)
			foreach (Vector3 distance in pawn_scr.captureVectors) 
			{
				if (InRange (cell_scr, distance, grid_scr)) 
				{
					// the way a pawn can capture is different from every other piece. i specify what i mean by that here.
					GameObject destinationCell = grid_scr.grid [cell_scr.row + (int)distance.y, cell_scr.column + (int)distance.x];
					Cell destCell_scr = destinationCell.GetComponent<Cell> ();
					if ((destCell_scr.MyPiece != null) && (!DoesColorMatch (playerTurn, destCell_scr.MyPiece.GetComponent<Piece> ()))) 
					{
						pawn_scr.ValidCells.Add (destCell_scr);
					}
				}
			}
		} 
		else if (piece_scr is King || piece_scr is Knight) 
		{
			PopulateVisitableList(cell_scr, grid_scr, piece_scr, false, 1);
		} 
		else 
		{
			PopulateVisitableList(cell_scr, grid_scr, piece_scr, true, 8);
		}
	}
	private void PopulateVisitableList( Cell sourceCell_scr, Grid grid_scr, Piece selectedPiece_scr, bool doScaling, int maxScale)
	{
		foreach (Vector3 distance in selectedPiece_scr.MovementVectors) 
		{
			int scale = 1;
			Vector3 newDistance = distance;
			bool pieceOccupies = false;
			bool applyScale = true;
			while (InRange (sourceCell_scr, newDistance, grid_scr) && !pieceOccupies && applyScale && (scale <= maxScale)) 
			{
				AddCell (sourceCell_scr, newDistance, grid_scr, selectedPiece_scr);
				GameObject destCell = grid_scr.grid [sourceCell_scr.row + (int)newDistance.y, sourceCell_scr.column + (int)newDistance.x];
				Cell destCell_scr = destCell.GetComponent<Cell> ();
				if (destCell_scr.MyPiece != null) 
				{
					pieceOccupies = true;
				}
				if (doScaling) 
				{
					scale++;
					newDistance = distance * scale;
				} 
				else
					applyScale = false;		
			}
		}
	}
	private bool InRange(Cell cell_scr, Vector3 vector, Grid grid_scr)
	{
		return (cell_scr.row + (int)vector.y < grid_scr.NumOfRows &&
		cell_scr.row + (int)vector.y >= 0 &&
		cell_scr.column + (int)vector.x < grid_scr.NumOfColumns &&
		cell_scr.column + (int)vector.x >= 0);
	}
	private void AddCell(Cell sourceCell_scr, Vector3 distance, Grid grid_scr, Piece somePiece)
	{
		GameObject destCell = grid_scr.grid [sourceCell_scr.row + (int)distance.y, sourceCell_scr.column + (int)distance.x];
		Cell destCell_scr = destCell.GetComponent<Cell> ();
		if (somePiece is Pawn) 
		{
			if (destCell_scr.myPiece == null )
				somePiece.ValidCells.Add (destCell_scr);
		} 
		else 
		{
			if (destCell_scr.myPiece == null || !DoesColorMatch (playerTurn, destCell_scr.myPiece.GetComponent<Piece> ()))
				somePiece.ValidCells.Add (destCell_scr);
		}
	}
	public bool DoesColorMatch(Player curPlayer, Piece piece)
	{
		return (!( curPlayer.IsWhite^piece.isWhite) );  
	}
	private void Move (GameObject destCell)
	{
		Cell destCell_scr = destCell.GetComponent<Cell> ();
		if ( selectedPiece.GetComponent<Piece>().ValidCells.Contains(destCell_scr) ) 
		{
			Cell sourceCell = selectedPiece.GetComponentInParent<Cell> ();
			// destroy is just for testing purposes; needs to be replaced with correct capturing code
			Destroy (destCell_scr.MyPiece);
			sourceCell.MyPiece = null;
			selectedPiece.transform.SetParent(destCell.transform, false);
			destCell_scr.MyPiece = selectedPiece;
			//updateMoveLog ();
			UpdatePiece(selectedPiece);
			DeselectPiece ();
			GameController.gameController.curTurnState = GameController.TurnStates.HAS_MOVED;
		}
	}
	public void UpdatePiece(GameObject piece)
	{
		if (selectedPiece.GetComponent<Pawn> ()) 
		{
			Pawn pawn_scr = selectedPiece.GetComponent<Pawn> ();
			pawn_scr.hasMoved = true;
		}
	}
}
