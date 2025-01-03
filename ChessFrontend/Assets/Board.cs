﻿using System;
using UnityEngine;

enum Square
{
    A1, B1, C1, D1, E1, F1, G1, H1,
    A2, B2, C2, D2, E2, F2, G2, H2,
    A3, B3, C3, D3, E3, F3, G3, H3,
    A4, B4, C4, D4, E4, F4, G4, H4,
    A5, B5, C5, D5, E5, F5, G5, H5,
    A6, B6, C6, D6, E6, F6, G6, H6,
    A7, B7, C7, D7, E7, F7, G7, H7,
    A8, B8, C8, D8, E8, F8, G8, H8
};
public enum Piece
{
    King,
    Queen,
    Rook,
    Bishop,
    Knight,
    Pawn,
    NONE,
};
public enum Side
{
    White,
    Black
}
class Board {
   

    const int NUM_PIECE_TYPES = 6;
    const int NUM_SIDES = 2;
    const int NUM_SQUARES = 64;

    public ulong[][] piecesBB = initPiecesBitBoard(); // row is color, col is piece type Ex: [white][king]
    public ulong[] sideBB = new ulong[NUM_SIDES]; // hold all pieces from a certain color Ex: [white] 
    
    //PIECE LIST IS LEAST SIG BIT TO MOST SIG; LSB->MSB; 0->63
    public int[] pieceList = initPieceList(); // rep of the board that holds piece info so we can tell which piece is on which square


    public GameState state;
    public GameHistory history; 

    // need zobrist randoms 



    /// <summary>
    /// Inits the pieces bitboard that holds piece bitboards like [side][piecetype]
    /// </summary>
    /// <returns> a 2d array of bitboards for all 12 color/piece types</returns>
    private static ulong[][] initPiecesBitBoard() {
        ulong[][] temp = new ulong[NUM_SIDES][]; 

        for(int i = 0; i< NUM_SIDES; i++) {
            temp[i] = new ulong[NUM_PIECE_TYPES];
            
        }
        return temp; 
    }
    private static int[] initPieceList()
    {
        int[] list = new int[NUM_SQUARES]; 
        for(int i = 0; i< NUM_SQUARES; i++ )
        {
            list[i] = (int) Piece.NONE; 
        }
        return list; 
    }

    /// <summary>
    /// init standard chess ; allows for easier debugging and inputting certain boards 
    /// </summary>
    public void initStandardChess () {
        char[][] chessBoard = new char[][]{
            new char[] { 'r', 'n', 'b', 'q', 'k', 'b', 'n', 'r' },
            new char[] { 'p', 'p', 'p', 'p', 'p', 'p', 'p', 'p' },
            new char[] { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            new char[] { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            new char[] { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            new char[] { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            new char[] { 'P', 'P', 'P', 'P', 'P', 'P', 'P', 'P' },
            new char[] { 'R', 'N', 'B', 'Q', 'K', 'B', 'N', 'R' },
        };
        charArrayToBitboards(chessBoard, pieceList, piecesBB, sideBB);

    }
    

    /// <summary>
    /// Print inputted bitboard into 8x8 chess board 
    /// </summary>
    /// <param name="board"></param>
    public static void printBitBoard(ulong board) {
        const int LAST_BIT = 63; // helps with calcs 
        string rankString = ""; 

        for (int rank =0;rank<=7; rank++) {
            rankString = (8-rank) + " "; 
            for(int file= 7; file>=0; file--) {
                int currentBit = LAST_BIT - (rank * 8 + file);

                ulong mask = 1UL;
                mask <<= currentBit; // shift 1 to current bit 

                if((board & mask) > 0) { // check if there is a piece at the current piece 
                    rankString += "X ";
                } else { rankString += ". "; } 

            }
            Debug.Log(rankString); 
        }
        string fileNames = "abcdefgh";
        rankString = "  "; 
        for (int i = 0; i < 8; i++)
        {
            rankString+= fileNames[i] + " ";
        }
        Debug.Log(rankString);

    }
    public static void printBoard(Board board)
    {
        printBitBoard(board.sideBB[0] | board.sideBB[1]);
    }
    /// <summary>
    /// Prints out piecelist that has all piece values into an 8x8 chess board form
    /// </summary>
    /// <param name="list"></param>
    public static void printPieceList(int[] list) {
        const int LAST_BIT = 63;

        for (int rank = 0; rank <= 7; rank++)
        {
            string rankString = ""; 
            for (int file = 7; file >= 0; file--)
            {
                int currentIndx = LAST_BIT - (rank * 8 +file );

                rankString+= list[currentIndx]+" ";
            }
            Debug.Log(rankString);
        }
    }
    public static Board charArrayToBoard(char[][] chessBoard) {
        const int LAST_BIT = 63; // helps with calcs 

        Board board = new Board();

        for (int i = 0; i <= 7; i++) {

            for (int j = 0; j <= 7; j++) {

                int pieceVal = (int)Piece.NONE; // current piece val ; 
                Side side;  // current side of piece 

                switch (char.ToLower(chessBoard[i][j])) { // find val of piece 
                    case 'r': pieceVal = (int)Piece.Rook; break;
                    case 'n': pieceVal = (int)Piece.Knight; break;
                    case 'b': pieceVal = (int)Piece.Bishop; break;
                    case 'q': pieceVal = (int)Piece.Queen; break;
                    case 'k': pieceVal = (int)Piece.King; break;
                    case 'p': pieceVal = (int)Piece.Pawn; break;
                }

                // from left -> right the indexes go 56 57 58... 63
                int currentIndex = LAST_BIT - (8 * i + (7 - j)); // also where bit is going to be 

                board.pieceList[currentIndex] = pieceVal; // info about currentPiece

                if (pieceVal == (int)Piece.NONE) {

                    continue; // skip if there is no piece there 
                }



                side = char.IsUpper(chessBoard[i][j]) ? Side.White : Side.Black; // if uppercase its white 
                ulong mask = 1UL; // initialize mask 
                mask <<= currentIndex; // now shift the bit to currentPos


                board.piecesBB[(int)side][pieceVal] |= mask; // or the mask to bb to preserve data

                // or mask to side specific as well 
                board.sideBB[(int)side] |= mask;
            }
        }

        return board;
    }
    public static void charArrayToBitboards(char[][] chessBoard, int[] pieceList, ulong[][] piecesBB, ulong[] sidesBB) {
        const int LAST_BIT = 63; // helps with calcs 

        for(int i=0; i<=7; i++) {
           
            for(int j =0; j<=7; j++) {
                
                int pieceVal = (int) Piece.NONE; // current piece val ; 
                Side side;  // current side of piece 

                switch (char.ToLower(chessBoard[i][j])) { // find val of piece 
                    case 'r': pieceVal = (int)Piece.Rook;  break;
                    case 'n': pieceVal = (int) Piece.Knight; break;
                    case 'b': pieceVal = (int) Piece.Bishop; break;
                    case 'q': pieceVal = (int) Piece.Queen; break;
                    case 'k': pieceVal = (int) Piece.King; break;
                    case 'p': pieceVal = (int) Piece.Pawn; break;
                }

                // from left -> right the indexes go 56 57 58... 63
                int currentIndex = LAST_BIT - (8*i + (7-j) ); // also where bit is going to be 

                pieceList[currentIndex] = pieceVal; // info about currentPiece

                if (pieceVal == (int)Piece.NONE) {

                    continue; // skip if there is no piece there 
                }

                

                side = char.IsUpper(chessBoard[i][j]) ? Side.White : Side.Black; // if uppercase its white 
                ulong mask  = 1UL; // initialize mask 
                mask <<= currentIndex; // now shift the bit to currentPos


                piecesBB[(int)side][pieceVal] |= mask; // or the mask to bb to preserve data

                // or mask to side specific as well 
                sidesBB[(int)side] |= mask; 
            }
        }
    }
    public void printBoard() {
        Board.printBitBoard(sideBB[(int)Side.White] | sideBB[(int)Side.Black]); 
    }

    /// <summary>
    /// Translates fen string to our board representation
    /// uppercase white, lowercase black, k king, q queen, b bishop, r rook, n knight, p pawn, / new line ,
    /// fen is read right to left, top to bottom 
    /// </summary>
    /// <param name="fen"> </param>
    public void FEN_TO_BB (string fen) {

        int currentSquare = 56; // we start at top left for reading fen 

        foreach (char c in fen)
        {
            // if num then iterate by however many 
            if (char.IsDigit(c))
            {
                currentSquare+= c-'0' ;
            }else if (c == '/')//if / then go next rank 
            {
                currentSquare = ((currentSquare-1) / 8 - 1) * 8;  // current rank -1 then translate to square
            }
            else// if char is piece place piece at current square 
            {
                Side side = char.IsUpper(c) ? Side.White : Side.Black;
                Piece piece = Piece.NONE;
                switch (char.ToLower(c))
                {
                    case 'p':
                        piece = Piece.Pawn; 
                        break;
                    case 'n':
                        piece = Piece.Knight;
                        break;
                    case 'b':
                        piece = Piece.Bishop;
                        break;
                    case 'r':
                        piece = Piece.Rook;
                        break;
                    case 'q':
                        piece = Piece.Queen;
                        break;
                    case 'k':
                        piece = Piece.King;
                        break;
                }

                // place piece at current square
                // update pieceBB, sideBB, and pieceList to pieceVal
                ulong indexMask = (1UL << currentSquare);

                piecesBB[(int)side][(int)piece] |= (indexMask);
                sideBB[(int)side] |= (indexMask);
                pieceList[currentSquare] = (int)piece; 


                currentSquare++; 
            }
            if (currentSquare < 0)
                break; // for now since we don't care about other info 
        }
        
    }

    

    public void movePiece (Move move ,  Piece piece , Side side)
    {
        // remove current piece from origin square in it's personal bb, side bb , and piecelist 
        ulong indexMask = (1UL << move.origin);

        this.piecesBB[(int)side][(int)piece] &= ~(indexMask);  
        this.sideBB[(int)side] &= ~(indexMask);
        this.pieceList[move.origin] = (int)Piece.NONE;


        // if there is a piece on the destination square have to update that piece's bb , side bb, and piecelist to capturing piece
        ulong destMask = (1UL << move.destination); 

        Piece capturedPiece = (Piece)this.pieceList[move.destination]; 
        
        // only check side if there is a piece there 
        if(capturedPiece != Piece.NONE ) { // there is fs a piece here 
            Side capturedSide = (this.sideBB[(int) Side.White] & destMask ) > 0 ? Side.White : Side.Black;

            this.piecesBB[(int)capturedSide][(int)capturedPiece] &= ~(destMask);
            this.sideBB[(int)capturedSide] &= ~(destMask);
        }

        // update pieceList to capturing piece at destination 
        this.pieceList[move.destination] = (int)piece; 

    }
}