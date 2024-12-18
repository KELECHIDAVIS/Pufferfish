using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CreateBoard : MonoBehaviour
{
    int[] boardTiles = { 1, 0, 1, 0, 1, 0, 1, 0, 0, 1, 0, 1, 0, 1, 0, 1, 1, 0, 1, 0, 1, 0, 1, 0, 0, 1, 0, 1, 0, 1, 0, 1, 1, 0, 1, 0, 1, 0, 1, 0, 0, 1, 0, 1, 0, 1, 0, 1, 1, 0, 1, 0, 1, 0, 1, 0, 0, 1, 0, 1, 0, 1, 0, 1 };

    int boardSize = 8; // chess board is 8x8
    public GameObject darkTile, lightTile;
    public GameObject[] whitePieces, blackPieces;
    float tileSize;
    float totalBoardWidth;
    float totalBoardHeight;
    float startX;
    float startY;
    private Camera cam; 

    Board board = new Board(); 
    void Start()
    {
        initChessTiles();
        initPieces("4k2r/6r1/8/8/8/8/3R4/R3K3"); //rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR"); 
    }

    public void Update() {
        // Step 1: Get mouse position in world coordinates
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0; // Since we are working in 2D

        // Step 2: Calculate position relative to the board
        float relativeX = mouseWorldPos.x - startX;
        float relativeY = mouseWorldPos.y - startY;

        // Step 3: Determine file and rank
        int file = Mathf.FloorToInt(relativeX / tileSize);
        int rank = Mathf.FloorToInt(relativeY / tileSize);

        // Step 4: Clamp values to ensure they are within the board range
        file = Mathf.Clamp(file, 0, boardSize - 1);
        rank = Mathf.Clamp(rank, 0, boardSize - 1);

        // Display rank and file if the mouse is over the board
        if (relativeX >= 0 && relativeX < totalBoardWidth && relativeY >= 0 && relativeY < totalBoardHeight) {
            Debug.Log($"Mouse is over Rank: {rank}, File: {file}");
        } else {
            Debug.Log("Mouse is outside the board.");
        }
    }


    void initChessTiles()
    {
           cam = Camera.main;
        float boardHeight = cam.orthographicSize * 2;
        float boardWidth = boardHeight * cam.aspect;

        // Calculate tile size
         tileSize = Mathf.Min(boardWidth, boardHeight) / boardSize;

        // Calculate total board size
         totalBoardWidth = tileSize * boardSize;
         totalBoardHeight = tileSize * boardSize;

        // Offset to center the board in the camera's view
         startX = -totalBoardWidth / 2 + tileSize / 2;
         startY = -totalBoardHeight / 2 + tileSize / 2;

        

        for(int rank = 0; rank< boardSize; rank++) {
            for(int file = 0; file < boardSize; file++) {
                GameObject tilePrefab = (boardTiles[rank * 8 + file] == 0) ? lightTile : darkTile;

                // Instantiate tile
                GameObject tile = Instantiate(tilePrefab, transform);
                // Set position
                tile.transform.position = new Vector3(startX + file * tileSize, startY + rank * tileSize, 2);

                // Set scale
                tile.transform.localScale = new Vector3(tileSize, tileSize, 1);
            }
        }
    }

    void initPieces(string fen)
    {
        board.FEN_TO_BB(fen);

        // draw pieces onto board 
        for (int rank = 0; rank < boardSize; rank++) {
            for (int file = 0; file < boardSize; file++) {
                int index = rank * 8 + file; 
                if (board.pieceList[index] != (int )Piece.NONE) {
                    
                    // figure out what side the piece is on; if foudn in white side bb then it's a white piece 
                    Side side = (board.sideBB[(int) Side.White]& (1UL << index)) > 0 ? Side.White : Side.Black;

                    GameObject piecePrefab = (side == Side.White) ? whitePieces[(int) board.pieceList[index]] : blackPieces[(int)board.pieceList[index]];

                    // Instantiate tile
                    GameObject piece = Instantiate(piecePrefab, transform);

                    // Set position
                    piece.transform.position = new Vector3(startX + file * tileSize, startY + rank * tileSize, 1);

                    // Set scale
                    piece.transform.localScale = new Vector3(tileSize*.8f, tileSize*.8f, 1);
                }
            }
        }
    }

    


}
