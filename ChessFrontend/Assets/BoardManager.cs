
using System.Collections.Generic;
using UnityEngine;


public class CreateBoard : MonoBehaviour
{
    struct SelectedPiece {
        public Square square;
        public Side side;
        public Piece piece;
        public List<Move> validMoves;
        public GameObject pieceObject; 
    }
    int[] boardTiles = { 1, 0, 1, 0, 1, 0, 1, 0, 0, 1, 0, 1, 0, 1, 0, 1, 1, 0, 1, 0, 1, 0, 1, 0, 0, 1, 0, 1, 0, 1, 0, 1, 1, 0, 1, 0, 1, 0, 1, 0, 0, 1, 0, 1, 0, 1, 0, 1, 1, 0, 1, 0, 1, 0, 1, 0, 0, 1, 0, 1, 0, 1, 0, 1 };

    int boardSize = 8; // chess board is 8x8
    public GameObject tileSprite; 
    public GameObject[] whitePieces, blackPieces;
    GameObject[] tileBoard;
    GameObject[] pieceObjects = new GameObject[64] ;  
    Color highLightColor = new Color32(25, 171, 69, 255);
    Color darkColor = new Color32(75, 45, 45, 255); 
    Color lightColor = new Color32(226, 207, 165, 255);
    float tileSize;
    float totalBoardWidth;
    float totalBoardHeight;
    float startX;
    float startY;

    
    private Camera cam;

    bool cwk=true, cwq= true, cbk= true, cbq= true; // castling rights  
    ulong EP; // en passant 
    Board board = new Board();

    SelectedPiece? selected = null; 
    void Start()
    {
        initChessTiles();
        initPieces("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR"); 
    }

    public void Update() {
        
        if (Input.GetMouseButtonDown(0)) {
            // get square that was clicked 
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            // normalize the Board so that the bottom left square is 0,0 and top right is 8*tile,8*tile
            float normalizedX = mouseWorldPos.x + (4 * tileSize);
            float normalizedY = mouseWorldPos.y + (4 * tileSize);

            int file = Mathf.FloorToInt(normalizedX / tileSize);
            int rank = Mathf.FloorToInt(normalizedY / tileSize);
            int square = rank * 8 + file;
            Debug.Log($"Rank {rank} file {file}"); 
            // if rank and file are valid 
            if(rank >= 0 && rank < 8 && file >= 0 && file < 8) {
                // if there is no piece selected when clicked 
                if (selected == null) {
                    //if there is a piece on that square select piece 
                    ulong allPieces = board.sideBB[0] | board.sideBB[1];
                    ulong indexMask = (1UL << square);

                    if ((allPieces & indexMask) > 0) {
                        // select piece and get their valid moves 
                        Piece piece = (Piece)board.pieceList[square];
                        Side side = (board.sideBB[(int)Side.White] & indexMask) > 0 ? Side.White : Side.Black;
                        Square sq = (Square)square;
                        List<Move> validMoves = getValidMoves(piece, side, sq);
                        selected = new SelectedPiece { piece = piece, side = side, square = sq, validMoves = validMoves, pieceObject = pieceObjects[square] };

                        foreach (Move move in validMoves) // highlight each valid move
                        {
                            tileBoard[move.destination].GetComponent<SpriteRenderer>().color = highLightColor;
                        }
                    }
                } else { // piece is alr selected, place piece if valid move was selected 
                         // if clicked on square matches the destination of one of the valid moves, move piece to there 
                    SelectedPiece currPiece = selected.GetValueOrDefault();
                    foreach (Move move in currPiece.validMoves) {
                        if (move.destination == square) {
                            board.movePiece(move, currPiece.piece, currPiece.side);

                           
                            // draw Piece at destination 
                            redrawPieces(move);
                            break;
                        }

                    }
                    // redraw tiles to get rid of highlights and unselect piece
                    redrawBoard();
                    selected = null;
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
    private void redrawPieces(Move move )
    {
        // destroy destination obj to free space 
        if(pieceObjects[ move.destination]) 
            Destroy (pieceObjects[ move.destination] );
        // give place in array to moving object 
        pieceObjects[move.destination] = pieceObjects[ move.origin ];

        //change moving object's position 
        int file = move.destination % 8, rank = move.destination / 8; 
        pieceObjects[move.destination].transform.position = new Vector3(startX + file * tileSize, startY + rank * tileSize, 1);

        // set origin's place in array to null 
        pieceObjects[move.origin] = null; 
    }



    // The Moves class returns all moves from that certain piece type so remove all the one that don't originate from the square
    private List<Move>  getValidMoves(Piece piece , Side side , Square square )
    {
        List<Move> moveList = new List<Move>();
        List<Move> finalMoveList =new List<Move>(); 
        if (board.pieceList[(int) square] != (int)Piece.NONE)
        {
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

            foreach (Move move in moveList)// since it finds the moves for all types of the same piece just get the one we're after
            {
                if (move.origin == (int)square)
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
                    pieceObjects[index] = piece;
                    // Set position
                    piece.transform.position = new Vector3(startX + file * tileSize, startY + rank * tileSize, 1);

                    // Set scale
                    piece.transform.localScale = new Vector3(tileSize*.8f, tileSize*.8f, 1);
                }
            }
        }
    }

    


}
