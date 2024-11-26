using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class Engine
{
  
    public static void Main(string[] args)
    {
        Board board = new Board();
        board.initStandardChess();

        /*// test that the pawn moves still work
        Console.WriteLine("Current Board");
        Board.printBitBoard(board.sideBB[(int)Side.White] | board.sideBB[(int)Side.Black]); 
        string pawnMoves = Moves.possibleMoves(Side.White, "2725", board.piecesBB, board.sideBB);

        string currentMove = ""; 
        for(int i =0; i<pawnMoves.Length; i++) {
            if(i%4==0) {
                Console.WriteLine(currentMove);
                currentMove = ""; 
            }
            currentMove += pawnMoves[i]; 
        }
        Console.WriteLine(currentMove);*/


        /*//Testing relevant occupancy masks 
        Console.WriteLine("_________ROOKS__________");
        for (int i = 0; i < 64; i++)
        {
            ulong movementMask = SlidingMoves.getRookMovementMask(i); 
            Board.printBitBoard(movementMask);
            Console.WriteLine("On bits: "+ SlidingMoves.getNumberOnBits(movementMask));
        }
        Console.WriteLine("_________BISHOP__________"); 
        for (int i= 0; i<64; i++) {
            ulong movementMask = SlidingMoves.getBishopMovementMasks(i);
            Board.printBitBoard(movementMask);
            Console.WriteLine("On bits: " + SlidingMoves.getNumberOnBits(movementMask)); 
        }*/




        // testing magicNumber generation
        /*Console.WriteLine("Generating Magic Numbers for each Square (Time in ms)... ");

        for (int rank = 0; rank <= 7; rank++) {
            for (int file = 7; file >= 0; file--) {
                int currentSquare = 63 - (rank * 8 + file);

                var watch = System.Diagnostics.Stopwatch.StartNew();

                var magicResult = SlidingMoves.findMagicNum(false, currentSquare);

                watch.Stop();
                var elapsedMs = watch.ElapsedMilliseconds;

                Console.Write(elapsedMs + " ");

            }
            Console.WriteLine(); 
        }*/

        // these two blocking configs should hash to the same position given a magic num
        char[][] chessBoard1 = new char[][]{
            new char[] { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            new char[] { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            new char[] { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            new char[] { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            new char[] { 'p', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            new char[] { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            new char[] { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            new char[] { 'R', 'p', ' ', ' ', ' ', ' ', ' ', ' ' },

        };
        char[][] chessBoard2 = new char[][]{
            new char[] { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            new char[] { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            new char[] { 'p', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            new char[] { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            new char[] { 'p', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            new char[] { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            new char[] { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            new char[] { 'R', 'p', 'p', ' ', ' ', ' ', ' ', ' ' },

        };

        Board board1 = new Board(), board2 = new Board();
        Board.charArrayToBitboards(chessBoard1, board1.pieceList, board1.piecesBB, board1.sideBB); 
        Board.charArrayToBitboards(chessBoard2, board2.pieceList, board2.piecesBB, board2.sideBB);

        Console.WriteLine("Rook is on a1 for both boards"); 
        Console.WriteLine("First Board"); 
        Board.printBitBoard((board1.sideBB[0] | board1.sideBB[1]) & ~board1.piecesBB[(int)Side.White][(int)Piece.Rook]); 
        Console.WriteLine("Second Board");
        Board.printBitBoard((board2.sideBB[0] | board2.sideBB[1]) & ~board2.piecesBB[(int)Side.White][(int)Piece.Rook]); 

    }
}

    
