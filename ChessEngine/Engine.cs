using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

class Engine
{
  
    public static void Main(string[] args)
    {
        // test the boards 
        /* Board board = new Board();
         board.initCustomChess();


         *//*foreach (Side side in Enum.GetValues(typeof(Side))) {
             Console.WriteLine("All " + side + " pieces: ");
             Board.printBitBoard(board.sideBB[(int)side]);
             foreach (Piece piece in Enum.GetValues(typeof(Piece))) {
                 if (piece == Piece.NONE) continue;

                 Console.WriteLine(side + " " + piece);
                 Board.printBitBoard(board.piecesBB[(int)side][(int)piece]);
             }
         }*//*

         Console.WriteLine("Board With Piece Values");
         Board.printPieceList(board.pieceList);


         string possibleWhiteMoves = Moves.possibleMoves(Side.White, "", board.piecesBB, board.sideBB); 


         Console.WriteLine("Possible White Moves (Just pawn rn): "+ possibleWhiteMoves);*/

        Board board = new Board();
        board.initStandardChess();

        // testing concept 
        string moveList = "";
        ulong captureBB = board.sideBB[(int)(Side.Black)] ^ board.piecesBB[(int)Side.Black][(int)Piece.King];
        // capture right ;white pawn can't be on rank 8 because that'd be a promotion;  shift bits 9 to left ; make sure there is a caputarable piece there and make sure that piece is not on a file (left column wrap around)
        ulong PAWN_MOVES = ((board.piecesBB[(int)Side.White][(int)Piece.Pawn] & ~Moves.RANK_8) << 9) & (captureBB & ~Moves.FILE_A);

        // now if a bit is on in that bb convert into move notation
        //x1,y1,x2,y2 
        int currentIndex;
        int x1, y1, x2, y2;

        Console.WriteLine("Black Pawn Locations: ");
        Board.printBitBoard(board.piecesBB[(int)(Side.Black)][(int)Piece.Pawn]);

        Console.WriteLine("White Pawn Locations: ");
        Board.printBitBoard(board.piecesBB[(int)(Side.White)][(int)Piece.Pawn]);

        Console.WriteLine("All pawn locations: ");
        Board.printBitBoard(board.piecesBB[(int)(Side.Black)][(int)Piece.Pawn] | board.piecesBB[(int)(Side.White)][(int)Piece.Pawn]);


        Console.WriteLine("Possible White Pawn Moves (x,y) (1->8): ");
        moveList = Moves.possibleMoves(Side.White, "", board.piecesBB, board.sideBB);

        for (int i = 0; i < moveList.Length; i++) {
            if (i % 4 == 0) Console.WriteLine();
            Console.Write(moveList[i]);
        }



    }
}

    
