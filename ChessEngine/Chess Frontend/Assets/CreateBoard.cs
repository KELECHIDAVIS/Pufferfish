using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CreateBoard : MonoBehaviour
{
    int[] boardTiles = { 1, 0, 1, 0, 1, 0, 1, 0, 0, 1, 0, 1, 0, 1, 0, 1, 1, 0, 1, 0, 1, 0, 1, 0, 0, 1, 0, 1, 0, 1, 0, 1, 1, 0, 1, 0, 1, 0, 1, 0, 0, 1, 0, 1, 0, 1, 0, 1, 1, 0, 1, 0, 1, 0, 1, 0, 0, 1, 0, 1, 0, 1, 0, 1 };

    int boardSize = 8; // chess board is 8x8
    public GameObject darkTile, lightTile; 
    public float startX, startY;


    Board board = new Board(); 
    void Start()
    {
        initChessTiles(); 
    }


    void initChessTiles()
    {
        Camera cam = Camera.main;
        float boardHeight = cam.orthographicSize * 2;
        float boardWidth = boardHeight * cam.aspect;

        // Calculate tile size
        float tileSize = Mathf.Min(boardWidth, boardHeight) / boardSize;

        // Calculate total board size
        float totalBoardWidth = tileSize * boardSize;
        float totalBoardHeight = tileSize * boardSize;

        // Offset to center the board in the camera's view
        float startX = -totalBoardWidth / 2 + tileSize / 2;
        float startY = -totalBoardHeight / 2 + tileSize / 2;

        // Create tiles
        for (int x = 0; x < boardSize; x++)
        {
            for (int y = 0; y < boardSize; y++)
            {
                GameObject tilePrefab = (boardTiles[x * 8 + y] == 0) ? lightTile : darkTile; 
                
                // Instantiate tile
                GameObject tile = Instantiate(tilePrefab, transform);

                // Set position
                tile.transform.position = new Vector3(startX + x * tileSize, startY + y * tileSize, 0);

                // Set scale
                tile.transform.localScale = new Vector3(tileSize, tileSize, 1);

                
            }
        }
    }

    void initPieces(string fen)
    {
        board.FEN_TO_BB(fen);
    }

    


}
