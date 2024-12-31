public class Perft {

    const int maxDepth = 3; 

    // return the number of legal moves that can be made from this board state
    public static int perft(Board board, int depth){
        //To test that my move gen is working correctly we do this test recursively : 
        /*Generate all moves that side can make with current board 
         * Make move on temp board then see all the responses that can be made
         * Continue until max depth has been reached 
         * 
         * THINGS TO NOTE
         * On every king or rook moves the castling rights should be updated if not alr 
         * On every two pawn push the boards ep should be updated 
         */

        // if maxdepth has been reached count the move 
        if (depth > maxDepth) return 1;

        // Depending on side to move generate all moves 
        List<Move> moves = Moves.possibleMoves(board.state.sideToMove, board, board.state.EP, board.state.castling);
        int totalMoves = 0; 
        // for each move:
        // create child board, make move, update state, recursively call on perft on child board 
        foreach (Move move in moves) {
            Board child = Board.initCopy(board); 
            child.makeMove(move); // SHOULD UPDATE ALL STATE RELATED TO MOVE MADE ; as well as switch the side 
            int childResponses=  perft(child, depth+1);

            // only print out the moves if this is the first iteration 
            if (depth == 1)
                Console.WriteLine((Square)move.origin + "->" + (Square)move.destination + ": " + childResponses);

            totalMoves += childResponses; 
        }

        return totalMoves; 
    }


}
