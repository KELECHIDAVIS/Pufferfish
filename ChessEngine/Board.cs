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
    public ulong[,] bbPieces = new ulong[2,6];
    public ulong[] bbSide = new ulong[2];

    public int[] initPieceList ()
    {
        ulong bbWhite = getSideBitBoard(Side.White); 
        ulong bbBlack= getSideBitBoard(Side.Black);
        int[] pieceList = new int[64]; // 64 squares in a chess board

        for (int i = 0; i < 64; i++)
        {
            pieceList[i] = (int) Piece.NONE;  // to represent theres nothing in that square 
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
        return bbPieces[(int)side,(int) piece];
    }

    //returns the bitboard for all pieces of a certain color ( all black pieces for ex) 
    public ulong getSideBitBoard (Side side)
    {
        return bbSide[(int) side]; 
    }
}
