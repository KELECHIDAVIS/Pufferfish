using System;
using System.Collections.Generic;
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
        
       
        int totalMoves = Perft.perft(board, 1);
        Console.WriteLine("Total Nodes: " + totalMoves);
    }

}



