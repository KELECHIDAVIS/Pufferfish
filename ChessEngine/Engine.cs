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
            new char[] { ' ', 'k', ' ', ' ', 'b', ' ', ' ', ' ' },
            new char[] { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            new char[] { ' ', ' ', 'R', ' ', ' ', ' ', ' ', ' ' },
            new char[] { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            new char[] { 'K', ' ', ' ', 'Q', ' ', ' ', ' ', 'r' },
            new char[] { ' ', ' ', ' ', ' ', ' ', ' ', 'P', ' ' },
            new char[] { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            new char[] { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
        };
        Board board = Board.charArrayToBoard(chessBoard);
        Move[] moves = new Move[Moves.MAX_POSSIBLE_MOVES];

        Board.printBoard(board);
        int moveCount = Moves.possibleMoves(board, moves);
        for(int i = 0; i < moveCount; i++)
        {
            Console.WriteLine((Square)moves[i].origin + "" + (Square)moves[i].destination); 
        }*/
        Board board = new Board();
        board.initStandardChess();


        


        /*board.makeMove(new Move { origin = (int)Square.G2, destination = (int)Square.G3, moveType = MoveType.QUIET, promoPieceType = Piece.NONE });
        board.makeMove(new Move { origin = (int)Square.B7, destination = (int)Square.B6, moveType = MoveType.QUIET, promoPieceType = Piece.NONE });
        board.makeMove(new Move { origin = (int)Square.F1, destination = (int)Square.H3, moveType = MoveType.QUIET, promoPieceType = Piece.NONE });
        //board.makeMove(new Move { origin = (int)Square.C7, destination = (int)Square.C5, moveType = MoveType.QUIET, promoPieceType = Piece.NONE });
        //board.makeMove(new Move { origin = (int)Square.B2, destination = (int)Square.B3, moveType = MoveType.QUIET, promoPieceType = Piece.NONE });*/

        Board.printBoard(board);
        Console.Write("Made Moves: ");
        foreach (GameState state in board.gameHistory)
        {
            Console.Write((Square)state.nextMove.origin + "" + (Square)state.nextMove.destination + " ");
        }
        Console.WriteLine();

        long start = Stopwatch.GetTimestamp();
        long tot = 0;
        int depth = 4;
        int s = 1;

        bool testingSharper = false;

        s = testingSharper ? depth : 1; 

        Dictionary<string , int > map = new Dictionary<string , int>();

        for (int i = s ; i <= depth; i++)
        {
            var result = Perft.perft(board, i, true, map);
            tot += result.nodes;
            Console.WriteLine($"Nodes: {result.nodes} Captures: {result.captures} Ep: {result.eps} Castles: {result.castles} Promos: {result.promos} Checks: {result.checks} Checkms: {result.checkms} " + result.nodes);
            Console.WriteLine("Total Moves: " + result.moves);
        }

        TimeSpan time = Stopwatch.GetElapsedTime(start);

        Console.WriteLine("NPS: " + (long)(tot / (time.TotalSeconds)));

    }

}



