using System.Runtime.CompilerServices;

public class Perft {


    // return the number of legal moves that can be made from this board state based on depth 
    // moves refers to the moves possible in current position 
    public static (long nodes , long moves, long captures, long eps, long castles, long promos, long checks, long checkms)  perft(Board board, int depth,  bool firstCall ) {
        long tot = 0, captures = 0, eps = 0, castles = 0, promos = 0, checks = 0, checkms = 0;
        
        // if this current board is in check then add 1 to checks  and return 
        // if in check and has no moves then add 1 to check mate and return 
        //bool inCheck = Moves.isInCheck(board);

        if (depth == 0)
            return (1,0, captures, eps, castles, promos, checks, checkms);

        Move[] moves = new Move[Moves.MAX_POSSIBLE_MOVES];
        
        int moveCount = Moves.possibleMoves(board, moves);

        for(int i = 0; i< moveCount; i++)
        {
            Board child = Board.initCopy(board);

            child.makeMove(moves[i]);

            long childResponses = perft(child, depth - 1, false).nodes;



            if (firstCall)
                Console.WriteLine((Square)moves[i].origin + "" + (Square)moves[i].destination + " " + childResponses);

            tot += childResponses; // account for child as well 
        }

        return (tot, moveCount, captures, eps, castles, promos, checks, checkms); 
         
    }


}
