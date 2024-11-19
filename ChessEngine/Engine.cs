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
        /*Board board = new Board();
        board.initStandardChess();


        foreach (Board.Side side in Enum.GetValues(typeof(Board.Side)))
        {
            Console.WriteLine("All " + side + " pieces: ");
            Board.printBitBoard(board.sideBB[(int)side]);
            foreach (Board.Piece piece in Enum.GetValues(typeof(Board.Piece)))
            {
                if (piece == Board.Piece.NONE) continue;

                Console.WriteLine(side + " " + piece);
                Board.printBitBoard(board.piecesBB[(int)side][(int)piece]);
            }
        }

        Console.WriteLine("Board With Piece Values");
        Board.printPieceList(board.pieceList);*/


        string fileNames = "ABCDEFGH"; 
        for(int i =0; i< Moves.FILES.Length; i++) {
            Console.WriteLine("File " + fileNames[i]);
            Board.printBitBoard(Moves.FILES[i]); 
        }
        for (int i = 0; i < Moves.RANKS.Length; i++) {
            Console.WriteLine("Rank " + (i+1));
            Board.printBitBoard(Moves.RANKS[i]);
        }

    }
}

    
