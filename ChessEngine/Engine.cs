using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class Engine
{
  
    public static void Main(string[] args)
    {
        // test the boards 
        Board board = new Board();
        board.initCustomChess();


        foreach (Side side in Enum.GetValues(typeof(Side))) {
            Console.WriteLine("All " + side + " pieces: ");
            Board.printBitBoard(board.sideBB[(int)side]);
            foreach (Piece piece in Enum.GetValues(typeof(Piece))) {
                if (piece == Piece.NONE) continue;

                Console.WriteLine(side + " " + piece);
                Board.printBitBoard(board.piecesBB[(int)side][(int)piece]);
            }
        }

        Console.WriteLine("Board With Piece Values");
        Board.printPieceList(board.pieceList);


        string possibleWhiteMoves = Moves.possibleMoves(Side.White, "", board.piecesBB, board.sideBB); 

        
        Console.WriteLine("Possible White Moves (Just pawn right captures for rn): "+ possibleWhiteMoves);
    }
}

    
