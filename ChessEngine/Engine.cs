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
        /*bool gameRunning = true;

        Board board = new Board(); 
        
        // ask user for fen string and init board 
        //FOR PROCESSING INPUT OUTPUT USES EXTRA THREAD
        while (gameRunning)
        {
            // process uci input 
        }*/

        /*char[][] chessBoard = new char[][]{
            new char[] { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            new char[] { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            new char[] { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            new char[] { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            new char[] { 'k', ' ', ' ', 'P', 'p', ' ', ' ', 'Q' },
            new char[] { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            new char[] { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            new char[] { ' ', ' ', ' ', 'K', ' ', ' ', ' ', ' ' },
        };
        Board board = Board.charArrayToBoard(chessBoard);
        Moves.printMoves(board); */

        Board board = new Board();
        board.initStandardChess();

        /*
         Board.printBoard(board);
            Console.Write("Made Moves: ");
            foreach (GameState state in board.gameHistory)
            {
                Console.Write((Square)state.nextMove.origin + "" + (Square)state.nextMove.destination + " ");
            }
            Console.WriteLine();

         */
        //board.makeMove(new Move { origin = (int)Square.A2, destination = (int)Square.A4, moveType = MoveType.QUIET, promoPieceType = Piece.NONE });
        //board.makeMove(new Move { origin = (int)Square.B7, destination = (int)Square.B5, moveType = MoveType.QUIET, promoPieceType = Piece.NONE });
        //board.makeMove(new Move { origin = (int)Square.A4, destination = (int)Square.B5, moveType = MoveType.CAPTURE, promoPieceType = Piece.NONE });
        //board.makeMove(new Move { origin = (int)Square.C7, destination = (int)Square.C5, moveType = MoveType.QUIET, promoPieceType = Piece.NONE });
        //board.makeMove(new Move { origin = (int)Square.B2, destination = (int)Square.B3, moveType = MoveType.QUIET, promoPieceType = Piece.NONE });

        long start = Stopwatch.GetTimestamp();
        long tot = 0; 
        for (int i = 0; i < 5; i++)
        {
            var result = Perft.perft(board, i, true);
            tot += result.nodes; 
            Console.WriteLine("Total Nodes: " + result.nodes);
            Console.WriteLine("Total Moves: " + result.moves);
        }

        TimeSpan time = Stopwatch.GetElapsedTime(start); 

        Console.WriteLine("NPS: "+ (long)(tot/(time.TotalSeconds)));

    }

}



