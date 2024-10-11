using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum Piece
{
   King, 
   Queen, 
   Rook, 
   Bishop,
   Knight, 
   Pawn,
   NONE,
}
public enum  Side
{
    White,
    Black,
    BOTH,
}
class Board
{
    // the row is the side , the columns is the type of piece 
    // The bb pieces carries all bit boards for pieces  ex: (white , king); row is side, col is piece 
    // bbside carries all piece info for that side: 0 is white , 1 is black 
    public ulong[][] bbPieces = initBitBoardPiecesArray(); // inits the jagged array
    public ulong[] bbSide = new ulong[2];

    static ulong[][] initBitBoardPiecesArray()
    {
        ulong[][] temp = new ulong[2][];
        for(int i = 0; i< 2; i++)
        {
            temp[i] = new ulong[6] { 0UL, 0UL, 0UL, 0UL, 0UL, 0UL }; 

        }
        return temp;
    }

    
    public int[] initPieceList ()
    {
        ulong[] bbWhite = bbPieces[(int) Side.White];  // all 6 bitboards for white pieces 
        ulong[] bbBlack= bbPieces[(int)Side.Black];

        int[] pieceList = new int[64]; // 64 squares in a chess board

        for (int i = 0; i < 64; i++)
        {
            pieceList[i] = (int) Piece.NONE;  // to represent theres nothing in that square 
        }

        // now for each piece put the corresponding piece type into the piece list 
        for(int i =0; i< bbWhite.Length; i++)
        {
            Piece pieceType = (Piece)i; 
            ulong whitePieces = bbWhite[(int) pieceType]; //white bitBoard of type Piece type 
            ulong blackPieces = bbBlack[(int) pieceType]; // black bitBoard of type Piece type 

            // now for each of this piece in the pieces bitboard put piece of that type in pieces list at position 
            // get the next bit that is turned on to find at what position the piece is located 
            int currIndex = 0; 
            while(whitePieces >0)
            {
                while((whitePieces & 1UL) == 0)
                {
                    currIndex++;
                    whitePieces >>= 1; 
                }
                // now set that current index to the piece type 
                pieceList[currIndex] = (int)pieceType;
                whitePieces >>= 1;
                currIndex++;

            }

            // get the next bit that is turned on to find at what position the piece is located 
            currIndex = 0;
            // now for each of this piece in the pieces bitboard put piece of that type in pieces list at position 
            while (blackPieces > 0)
            {
                while ((blackPieces & 1UL) == 0) // while were looking at a zero
                {
                    currIndex++;
                    blackPieces >>= 1;
                }
                // now set that current index to the piece type and go to next bit 
                pieceList[currIndex] = (int)pieceType;
                blackPieces >>= 1;
                currIndex++;
            }

        }

        return pieceList; 
    }
    // takes in a bit board and prints out its representation
    public void printBitBoard(ulong bb) 
    {
        int LAST_BIT = 63; // we want to look at the 63rd bit 

        for (int rank = 0; rank < 8; rank++)
        {
            for (int file = 7; file >= 0; file--)
            {
                int amountShifted = (LAST_BIT - (rank * 8) - file);
                ulong mask = 1UL << amountShifted;

                // basically now we have to check if there is a bit at the current position (and) 
                char bit = (bb & mask) != 0 ? '1' : '0';

                Console.Write(bit + " ");
            }
            Console.WriteLine();
        }
    }

    //Returns the corresponding piece's bitboard which contains the postiion of all pieces of that certain type (white pawns for example) 
    public ulong getPieceBitBoard ( Side side , Piece piece)
    {
        return bbPieces[(int)side] [(int) piece];
    }

    //returns the bitboard for all pieces of a certain color ( all black pieces for ex) 
    public ulong getSideBitBoard (Side side)
    {
        return bbSide[(int) side]; 
    }
}
