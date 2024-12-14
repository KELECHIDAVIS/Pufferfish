class UserInterface
{
    static ulong EP; // en passant bb, flags file that a pawn moved 2 in 
    static bool CWK, CWQ, CBK, CBQ; //which sides are still valid for castling (castle white king side etc) 
    Board board;



    public void newGame()
    {
        board = new Board(); 

        board.initStandardChess();

        List<Move> whiteMoveList = Moves.possibleMoves(Side.White, board, EP); 
        List <Move> blackMoveList = Moves.possibleMoves(Side.Black, board, EP);


        Board.printBoard(board); 
        Console.WriteLine("White Moves: "); 
        foreach (Move move in whiteMoveList)
        {
            Console.Write(move.moveType+": "+ (Square)move.origin+"->"+ (Square)move.destination+" ,"); 
        }
        Console.WriteLine("\nBlack Moves: ");
        foreach (Move move in blackMoveList)
        {
            Console.Write(move.moveType + ": " + (Square)move.origin + "->" + (Square)move.destination + " ,");

        }
    }
}