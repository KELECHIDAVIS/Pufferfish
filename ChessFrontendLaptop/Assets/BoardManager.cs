
using System.Collections.Generic;
using UnityEngine;


public class CreateBoard : MonoBehaviour
{
    int[] boardTiles = { 1, 0, 1, 0, 1, 0, 1, 0, 0, 1, 0, 1, 0, 1, 0, 1, 1, 0, 1, 0, 1, 0, 1, 0, 0, 1, 0, 1, 0, 1, 0, 1, 1, 0, 1, 0, 1, 0, 1, 0, 0, 1, 0, 1, 0, 1, 0, 1, 1, 0, 1, 0, 1, 0, 1, 0, 0, 1, 0, 1, 0, 1, 0, 1 };

    int boardSize = 8; // chess board is 8x8
    public GameObject tileSprite; 
    public GameObject[] whitePieces, blackPieces;
    GameObject[] tileBoard;
    List<GameObject> pieceObjects = new() ; // all piece objects in the scene 
    Color highLightColor = new Color32(25, 171, 69, 255);
    Color darkColor = new Color32(75, 45, 45, 255); 
    Color lightColor = new Color32(226, 207, 165, 255);
    float tileSize;
    float totalBoardWidth;
    float totalBoardHeight;
    float startX;
    float startY;

    int selectedSquare = -1; // a square is not selected 
    List<Move> validMoves = new List<Move>(); 

    private Camera cam;

    bool cwk=true, cwq= true, cbk= true, cbq= true; // castling rights  
    ulong EP; // en passant 
    Board board = new Board(); 
    void Start()
    {
        initChessTiles();
        initPieces("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR"); 
    }

    public void Update() {
        if (Input.GetMouseButtonDown(0))  // get piece's valid moves 
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            // normalize the Board so that the bottom left square is 0,0 and top right is 8*tile,8*tile
            float normalizedX = mouseWorldPos.x + (4 * tileSize);
            float normalizedY = mouseWorldPos.y + (4 * tileSize);


            int file = Mathf.FloorToInt(normalizedX / tileSize);
            int rank = Mathf.FloorToInt(normalizedY / tileSize);
            int square = rank * 8 + file;

            if (selectedSquare != -1 ) // a square is selected 
            {
                foreach (Move move in validMoves)
                {
                    if(move.destination == square) // if destination is a valid move 
                    {
                        Side side = (board.sideBB[(int)Side.White] & (1UL << square)) > 0 ? Side.White : Side.Black; 
                        board.movePiece(move , (Piece) board.pieceList[selectedSquare], side );
                        break; 
                    }
                }
                Board.printPieceList(board.pieceList); 
                //redraw pieces 
                redrawPieces(); 
                redrawBoard();
                selectedSquare = -1; // unselect square 
            }
            else
            {
                // clear colored moves 
                redrawBoard();
                

                // make sure rank and file is in bounds 
                if (rank >= 0 && rank < 8 && file >= 0 && file < 8)
                {
                    // get piece on squares valid moves 
                    
                    validMoves = getValidMoves(square);

                    foreach (Move move in validMoves) // highlight each valid move
                    {
                        tileBoard[move.destination].GetComponent<SpriteRenderer>().color = highLightColor;
                    }
                    selectedSquare = square; 
                }
            }
           
        }
    }

    private void redrawBoard()
    {
        for (int i = 0 ; i < tileBoard.Length; i++ )
        {
            if (boardTiles[i] == 0)
                tileBoard[i].GetComponent<SpriteRenderer>().color = lightColor;
            else
                tileBoard[i].GetComponent<SpriteRenderer>().color = darkColor;
        }
    }
    private void redrawPieces()
    {
        // destroy all previous pieces 
        foreach (GameObject piece in pieceObjects)
        {
            // destroy piece 
            Destroy( piece );
        }
        pieceObjects.Clear();

        // now redraw pieces in correct order 
        for (int rank = 0; rank < boardSize; rank++)
        {
            for (int file = 0; file < boardSize; file++)
            {
                int index = rank * 8 + file;
                if (board.pieceList[index] != (int)Piece.NONE)
                {

                    // figure out what side the piece is on; if foudn in white side bb then it's a white piece 
                    Side side = (board.sideBB[(int)Side.White] & (1UL << index)) > 0 ? Side.White : Side.Black;

                    GameObject piecePrefab = (side == Side.White) ? whitePieces[(int)board.pieceList[index]] : blackPieces[(int)board.pieceList[index]];

                    // Instantiate tile
                    GameObject piece = Instantiate(piecePrefab, transform);
                    pieceObjects.Add(piece);
                    // Set position
                    piece.transform.position = new Vector3(startX + file * tileSize, startY + rank * tileSize, 1);

                    // Set scale
                    piece.transform.localScale = new Vector3(tileSize * .8f, tileSize * .8f, 1);
                }
            }
        }
    }



    // The Moves class returns all moves from that certain piece type so remove all the one that don't originate from the square
    private List<Move>  getValidMoves(int square)
    {
        List<Move> moveList = new List<Move>();
        List<Move> finalMoveList =new List<Move>(); 
        if (board.pieceList[square] != (int)Piece.NONE)
        {
            Piece piece = (Piece)board.pieceList[square]; 
            Side side = (board.sideBB[(int)Side.White] & (1UL << square)) > 0 ? Side.White : Side.Black;
            ulong nonCaptureBB, captureBB, emptyBB;
            bool kingCastle, queenCastle; 
            // generate bitboards required for getting moves 
            emptyBB = ~(board.sideBB[(int)Side.Black] | board.sideBB[(int)Side.White]);

            if (side == Side.White)
            {
                nonCaptureBB = board.sideBB[(int)Side.White] | board.piecesBB[(int)Side.Black][(int)Piece.King];
                captureBB = board.sideBB[(int)Side.Black] ^ board.piecesBB[(int)Side.Black][(int)Piece.King];
                kingCastle = cwk;
                queenCastle = cwq; 
            }
            else
            {
                nonCaptureBB = board.sideBB[(int)Side.Black] | board.piecesBB[(int)Side.White][(int)Piece.King];
                captureBB = board.sideBB[(int)Side.White] ^ board.piecesBB[(int)Side.White][(int)Piece.King];
                kingCastle =cbk;
                queenCastle =cbq;
            }

            switch (piece)
            {
                case Piece.King:
                    Moves.possibleKing(moveList, board.piecesBB, board.sideBB, nonCaptureBB, captureBB, emptyBB, side, kingCastle, queenCastle);
                    break;
                case Piece.Queen:
                    Moves.possibleQueen(moveList, board.piecesBB, board.sideBB, nonCaptureBB, captureBB, emptyBB, side);
                    break;
                case Piece.Rook:
                    Moves.possibleRook(moveList, board.piecesBB, board.sideBB, nonCaptureBB, captureBB, emptyBB, side);
                    break;
                case Piece.Bishop:
                    Moves.possibleBishop(moveList, board.piecesBB, board.sideBB, nonCaptureBB, captureBB, emptyBB, side);
                    break;
                case Piece.Knight:
                    Moves.possibleKnight(moveList, board.piecesBB, board.sideBB, nonCaptureBB, captureBB, emptyBB, side);
                    break;
                case Piece.Pawn:
                    if (side == Side.White) { 
                        Moves.possiblePawnWhite(moveList, board.piecesBB, board.sideBB, nonCaptureBB, captureBB, emptyBB, EP);}
                    else { 
                        Moves.possiblePawnBlack(moveList, board.piecesBB, board.sideBB, nonCaptureBB, captureBB, emptyBB, EP);}
                    break;

            }

            foreach (Move move in moveList)
            {
                if (move.origin == square)
                    finalMoveList.Add(move);
            }
        }
        return finalMoveList; 
    }
    void initChessTiles()
    {
        cam = Camera.main;
        float boardHeight = cam.orthographicSize * 2;
        float boardWidth = boardHeight * cam.aspect;

        tileBoard = new GameObject[64]; 
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
                
                Color32 color = (boardTiles[rank * 8 + file] == 0) ? lightColor : darkColor;
                // Instantiate tile
                GameObject tile = Instantiate(tileSprite, transform);
                tile.GetComponent<SpriteRenderer>().color = color; 

                tileBoard[rank * 8 + file] = tile;    
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
                    pieceObjects.Add(piece);
                    // Set position
                    piece.transform.position = new Vector3(startX + file * tileSize, startY + rank * tileSize, 1);

                    // Set scale
                    piece.transform.localScale = new Vector3(tileSize*.8f, tileSize*.8f, 1);
                }
            }
        }
    }

    


}
