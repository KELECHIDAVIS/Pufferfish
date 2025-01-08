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

        board.makeMove(new Move { origin = (int)Square.A2, destination = (int)Square.A4, moveType = MoveType.QUIET, promoPieceType = Piece.NONE });
        board.makeMove(new Move { origin = (int)Square.B7, destination = (int)Square.B5, moveType = MoveType.QUIET, promoPieceType = Piece.NONE });
        board.makeMove(new Move { origin = (int)Square.A4, destination = (int)Square.B5, moveType = MoveType.CAPTURE, promoPieceType = Piece.NONE });
        //board.makeMove(new Move { origin = (int)Square.E8, destination = (int)Square.E7, moveType = MoveType.QUIET, promoPieceType = Piece.NONE });


        Board.printBoard(board); 
        Console.Write("Made Moves: "); 
        foreach(GameState state in board.gameHistory) {
            Console.Write((Square) state.nextMove.origin+""+(Square)state.nextMove.destination+" ");
        }Console.WriteLine(); 

        int maxDepth = 3;

        Dictionary<string , int> map = new Dictionary<string , int>();
        Console.WriteLine("MaxDepth: " + maxDepth);
        var timer = Stopwatch.StartNew();
        int totalMoves = Perft.perft(board, 1, maxDepth, map);
        timer.Stop();
        Console.WriteLine("Total Nodes: " + totalMoves);
        Console.WriteLine(timer.ElapsedMilliseconds / 1000f + " sec");
        

        Console.WriteLine("\nSharper comparisons:"); 
        string[] sharperResult = "c7c6 567\r\nc7c5 569\r\nd7d6 672\r\nd7d5 698\r\ne7e6 747\r\ne7e5 748\r\nf7f6 473\r\nf7f5 499\r\ng7g6 523\r\ng7g5 524\r\nh7h6 473\r\nh7h5 523\r\nb8c6 567\r\nb8a6 473\r\nc8b7 646\r\nc8a6 476\r\na7a6 476\r\na7a5 504\r\ng8h6 498\r\ng8f6 548".ToUpper().Split("\n"); 

        foreach(string move in sharperResult) {
            string[] pair = move.Split(" ");

            if (!map.ContainsKey(pair[0])){// my engine didn't find this move 
                Console.WriteLine("MY ENGINE DIDN'T FIND THE MOVE {} in this posistion"); continue;
            }

            int myResult = map[pair[0]];
            map.Remove(pair[0]); 
            if (myResult != int.Parse(pair[1])) {
                Console.WriteLine($"Mismatch on {pair[0]}\nI got : {myResult}\nsharper:{pair[1]}\n"); 
            }
        }

        if (map.Count > 0 ) {
            Console.WriteLine("MY ENGINE GENERATED THESE EXTRA MOVES: "); 
            foreach (var pair in map) {
                Console.WriteLine(pair.Key); 
            }
        }
        /*char[][] chessBoard = new char[][]{
            new char[] { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            new char[] { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            new char[] { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            new char[] { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            new char[] { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            new char[] { ' ', ' ', ' ', ' ', ' ', ' ', 'P', ' ' },
            new char[] { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            new char[] { ' ', ' ', ' ', ' ', 'b', ' ', ' ', ' ' },
        };
        Board board = Board.charArrayToBoard(chessBoard);
        Board.printBoard(board);

        ulong bishopBB = board.piecesBB[(int)Side.Black][(int)Piece.Bishop]; 
        int origin= BitOperations.TrailingZeroCount(bishopBB);
        ulong blockerBB = (board.sideBB[(int)Side.White] | board.sideBB[(int)Side.Black]) ^ (1UL << origin);

        ulong rawBishopMoves = Moves.getBishopMoves(blockerBB, origin);
        Console.WriteLine("Raw Bishop Moves");
        Board.printBitBoard(rawBishopMoves); */




    }

}



