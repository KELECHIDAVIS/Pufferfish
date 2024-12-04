using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class Engine
{

    public static void Main(string[] args) {
        /*Board board = new Board();
        board.initStandardChess();






        // test that the pawn moves still work
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
        var globalwatch = System.Diagnostics.Stopwatch.StartNew(); ;

        for (int rank = 0; rank <= 7; rank++)
        {
            for (int file = 7; file >= 0; file--)
            {
                int currentSquare = 63 - (rank * 8 + file);

                var watch = System.Diagnostics.Stopwatch.StartNew();

                var magicResult = SlidingMoves.findMagicNum(false, currentSquare);

                watch.Stop();
                var elapsedMs = watch.ElapsedMilliseconds;

                Console.Write(elapsedMs + " ");

            }
            Console.WriteLine();
        }
        Console.WriteLine("Global Time: " + globalwatch.ElapsedMilliseconds + "ms");

        //test magic num saving 

        SlidingMoves.SaveMoveTables();*/






        // these two blocking configs should hash to the same position given a magic num
        /*char[][] chessBoard1 = new char[][]{
            new char[] { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            new char[] { ' ', ' ', ' ', ' ', ' ', ' ', 'p', ' ' },
            new char[] { ' ', 'p', ' ', ' ', ' ', 'p', ' ', ' ' },
            new char[] { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            new char[] { ' ', ' ', 'P', 'B', ' ', ' ', ' ', ' ' },
            new char[] { ' ', ' ', ' ', 'P', 'p', ' ', ' ', ' ' },
            new char[] { ' ', ' ', ' ', ' ', ' ', 'p', ' ', ' ' },
            new char[] { 'p', 'p', ' ', ' ', ' ', ' ', ' ', ' ' },

        };
        char[][] chessBoard2 = new char[][]{
            new char[] { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            new char[] { 'p', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            new char[] { 'p', 'p', ' ', ' ', ' ', 'p', ' ', ' ' },
            new char[] { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            new char[] { 'p', ' ', 'p', 'B', ' ', ' ', ' ', ' ' },
            new char[] { ' ', ' ', ' ', 'p', 'p', ' ', ' ', ' ' },
            new char[] { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            new char[] { 'p', 'p', 'p', ' ', ' ', ' ', ' ', ' ' },

        };

        Board board1 = new Board(), board2 = new Board();
        Board.charArrayToBitboards(chessBoard1, board1.pieceList, board1.piecesBB, board1.sideBB);
        Board.charArrayToBitboards(chessBoard2, board2.pieceList, board2.piecesBB, board2.sideBB);

        Console.WriteLine("Bishop is on d4 for both boards");
        Console.WriteLine("First Board");
        ulong occ1 = (board1.sideBB[0] | board1.sideBB[1]) & ~board1.piecesBB[(int)Side.White][(int)Piece.Bishop]; // all occupied cept the rook
        ulong occ2 = (board2.sideBB[0] | board2.sideBB[1]) & ~board2.piecesBB[(int)Side.White][(int)Piece.Bishop];
        Board.printBitBoard(occ1);
        Console.WriteLine("Second Board");
        Board.printBitBoard(occ2);

        int rookPos = 27; // d4

        var magicResult = SlidingMoves.findMagicNum(true, rookPos);

        // when multiplied by this number both boards should give the same result 
        // they should both hash to the same moveset 

        Console.WriteLine("D4's magic entry: ");
        Console.WriteLine("Decimal magic Number: " + magicResult.entry.magicNum);
        Console.WriteLine("Index Shift: " + magicResult.entry.indexShift);
        Console.WriteLine("Relevant Blocker Mask: ");
        Board.printBitBoard(magicResult.entry.relevantBlockerMask);

        Console.WriteLine("Magic number bb representation: ");
        Board.printBitBoard(magicResult.entry.magicNum);

        // now both boards should get the same moveset from the table if they don't have the same exact key 
        int firstKey = (int)SlidingMoves.getMagicIndex(magicResult.entry, occ1);
        int secondKey = (int)SlidingMoves.getMagicIndex(magicResult.entry, occ2);
        Console.WriteLine("\n Board 1's key when entered into getMagicIndex: " + firstKey);
        Console.WriteLine("\n Board 2's key when entered into getMagicIndex: " + secondKey);

        //print each board's moveset, should be the same 
        Console.WriteLine("\n First Moveset: ");
        Board.printBitBoard(magicResult.hashTable[firstKey]);

        Console.WriteLine("\n Second Moveset: ");
        Board.printBitBoard(magicResult.hashTable[secondKey]);

*/

        /*// testing hardcoded copy and paste of tables works with each square 
        Random random = new Random();
        string fileNames = "abcdefgh"; 

        for (int i =0; i< 64;i++) {
            Console.WriteLine("******** Square " + fileNames[i%8]+""+(i/8 +1) +" ********");

            Console.WriteLine("Random Blocker Config: "); 
            // generate random blocker config
            ulong randBlockerConfig =(ulong) random.NextInt64();

            //remove current square bit if on 
            randBlockerConfig &= ~(1UL << i);

            Board.printBitBoard(randBlockerConfig); 

            // for both types 
            int rookKey = (int)SlidingMoves.getMagicIndex(SlidingMoves.RookInfoTable[i], randBlockerConfig); 
            int bishopKey = (int)SlidingMoves.getMagicIndex(SlidingMoves.BishopInfoTable[i], randBlockerConfig);


            ulong rookMoves= SlidingMoves.RookMoveHashTable[i][rookKey];
            ulong bishopMoves= SlidingMoves.BishopMoveHashTable[i][bishopKey];

            Console.WriteLine("Rook Moves (Key "+rookKey+"): ");
            Board.printBitBoard(rookMoves);

            Console.WriteLine("Bishop Moves (Key " + bishopKey + "): ");
            Board.printBitBoard(bishopMoves);
            Console.WriteLine("******** ***** ********\n");

        }*/

        // testing possible moves for rooks and sliding pieces 


        char[][] chessBoard1 = new char[][]{
            new char[] { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            new char[] { ' ', ' ', ' ', ' ', ' ', ' ', 'p', ' ' },
            new char[] { ' ', 'p', ' ', ' ', ' ', 'p', ' ', ' ' },
            new char[] { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            new char[] { ' ', ' ', 'p', 'Q', ' ', ' ', ' ', ' ' },
            new char[] { ' ', ' ', ' ', 'p', 'p', ' ', ' ', ' ' },
            new char[] { ' ', ' ', ' ', ' ', ' ', 'p', ' ', ' ' },
            new char[] { 'p', 'p', ' ', ' ', ' ', ' ', ' ', ' ' },

        };
        char[][] chessBoard2 = new char[][]{
            new char[] { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            new char[] { 'p', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            new char[] { 'p', 'p', ' ', ' ', ' ', 'p', ' ', ' ' },
            new char[] { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            new char[] { 'p', ' ', 'p', 'B', ' ', ' ', ' ', ' ' },
            new char[] { ' ', ' ', ' ', 'p', 'p', ' ', ' ', ' ' },
            new char[] { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            new char[] { 'p', 'p', 'p', ' ', ' ', ' ', ' ', ' ' },

        }; 
        Board board = Board.charArrayToBitboards(chessBoard1); 
       
        Board.printBitBoard(board.sideBB[0] | board.sideBB[1]);

        string moves = Moves.possibleMoves(Side.White, "", board);
        Console.WriteLine("\n"+ moves.Length / 4 + " Possible Moves :");

        for (int i = 0; i < moves.Length; i++) {
            if (i % 4 == 0 && i > 0)
                Console.Write("  ");
            Console.Write(moves[i]);
        }
    }
}



