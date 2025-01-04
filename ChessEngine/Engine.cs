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

        /*board.makeMove(new Move { origin = (int)Square.F2, destination = (int)Square.F4, moveType = MoveType.QUIET, promoPieceType = Piece.NONE });
        board.makeMove(new Move { origin = (int)Square.E7, destination = (int)Square.E5, moveType = MoveType.QUIET, promoPieceType = Piece.NONE });
        board.makeMove(new Move { origin = (int)Square.F4, destination = (int)Square.F5, moveType = MoveType.QUIET, promoPieceType = Piece.NONE });
        board.makeMove(new Move { origin = (int)Square.E8, destination = (int)Square.E7, moveType = MoveType.QUIET, promoPieceType = Piece.NONE });
*/

        Board.printBoard(board); 
        Console.Write("Made Moves: "); 
        foreach(GameState state in board.gameHistory) {
            Console.Write((Square) state.nextMove.origin+""+(Square)state.nextMove.destination+" ");
        }Console.WriteLine(); 

        int maxDepth = 5;

        Dictionary<string , int> map = new Dictionary<string , int>();
        Console.WriteLine("MaxDepth: " + maxDepth);
        var timer = Stopwatch.StartNew();
        int totalMoves = Perft.perft(board, 1, maxDepth, map);
        timer.Stop();
        Console.WriteLine("Total Nodes: " + totalMoves);
        Console.WriteLine(timer.ElapsedMilliseconds / 1000f + " sec");
        

        Console.WriteLine("\nSharper comparisons:"); 
        string[] sharperResult = "b1c3 234656\r\nb1a3 198572\r\ng1h3 198502\r\ng1f3 233491\r\na2a3 181046\r\na2a4 217832\r\nb2b3 215255\r\nb2b4 216145\r\nc2c3 222861\r\nc2c4 240082\r\nd2d3 328511\r\nd2d4 361790\r\ne2e3 402988\r\ne2e4 405385\r\nf2f3 178889\r\nf2f4 198473\r\ng2g3 217210\r\ng2g4 214048\r\nh2h3 181044\r\nh2h4 218829".ToUpper().Split("\n"); 

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



