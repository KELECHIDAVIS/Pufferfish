using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

class Engine
{ 
    public static void Main(string[] args)
    {
        // Perft.runTest(); 
        Board board = new Board();
        board.initStandardChess();
        Console.WriteLine(board.toFEN()); 
        board.makeMove(new Move { origin = (int) Square.E2, destination = (int) Square.E4, moveType = MoveType.QUIET, promoPieceType = Piece.NONE  });
        Console.WriteLine(board.toFEN()); 
        board.makeMove(new Move { origin = (int) Square.C7, destination = (int) Square.C5, moveType = MoveType.QUIET, promoPieceType = Piece.NONE  }); 
        Console.WriteLine(board.toFEN());
        board.makeMove(new Move { origin = (int)Square.G1, destination = (int)Square.F3, moveType = MoveType.QUIET, promoPieceType = Piece.NONE });
        Console.WriteLine(board.toFEN());
    }

}



