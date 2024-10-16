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


        foreach (Board.Side side in Enum.GetValues(typeof(Board.Side))) {
            Console.WriteLine("All " + side + " pieces: ");
            Board.printBitBoard(board.sideBB[(int)side]); 
            foreach(Board.Piece piece in Enum.GetValues(typeof(Board.Piece))) {
                if(piece == Board.Piece.NONE) continue;

                Console.WriteLine(side + " " + piece);
                Board.printBitBoard(board.piecesBB[(int)side][(int)piece]); 
            }
        }

        Console.WriteLine("Board With Piece Values");
        Board.printPieceList(board.pieceList); */

        Console.WriteLine(Moves.FILE_AB);
        Board.printBitBoard(Moves.FILE_AB); 

    }
}

    
