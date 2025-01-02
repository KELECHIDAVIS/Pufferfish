using System.Diagnostics;

public class Perft {


    // return the number of legal moves that can be made from this board state
    public static (long totalNodes, long moveGenTimeMS, long copyTimeMS, long makeMoveTimeMS) perft(Board board, int depth, int maxDepth) {
        //To test that my move gen is working correctly we do this test recursively : 
        /*Generate all moves that side can make with current board 
         * Make move on temp board then see all the responses that can be made
         * Continue until max depth has been reached 
         * 
         * THINGS TO NOTE
         * On every king or rook moves the castling rights should be updated if not alr 
         * On every two pawn push the boards ep should be updated 
         */


        long totalMoves = 0;
        long moveGenTimeMS=0, copyTimeMS=0, makeMoveTimeMS=0; // total time each costly function will take 
        
        // if maxdepth has been reached count the move 
        if (depth > maxDepth) return (1, moveGenTimeMS, copyTimeMS, makeMoveTimeMS);

        // Depending on side to move generate all moves 
        var genTimer = Stopwatch.StartNew();
        List<Move> moves = Moves.possibleMoves(board.state.sideToMove, board, board.state.EP, board.state.castling);
        genTimer.Stop(); 
        moveGenTimeMS = genTimer.ElapsedMilliseconds; 
        // for each move:
        // create child board, make move, update state, recursively call on perft on child board 

        
        foreach (Move move in moves) {
            var copyTimer = Stopwatch.StartNew();
            Board child = Board.initCopy(board); // creates an identical board based on sent board 
            copyTimer.Stop(); 
            copyTimeMS= copyTimer.ElapsedMilliseconds;

            var makeTimer = Stopwatch.StartNew();
            child.makeMove(move); // SHOULD UPDATE ALL STATE RELATED TO MOVE MADE ; as well as switch the side 
            makeTimer.Stop(); 
            makeMoveTimeMS= makeTimer.ElapsedMilliseconds;

            var res = perft(child, depth + 1, maxDepth);

            if (depth == 1) // print the children out ; less 
                Console.WriteLine((Square)move.origin + "" + (Square)move.destination + ": " + res.totalNodes);
            
            totalMoves += res.totalNodes;
            moveGenTimeMS += res.moveGenTimeMS; 
            copyTimeMS += res.copyTimeMS; 
            makeMoveTimeMS += res.makeMoveTimeMS;
        }
        
        
        return (totalMoves, moveGenTimeMS, copyTimeMS, makeMoveTimeMS); 
    }


}
