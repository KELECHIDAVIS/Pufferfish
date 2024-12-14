class UserInterface
{
    static ulong EP; // en passant bb, flags file that a pawn moved 2 in 
    static bool CWK = true, CWQ=true, CBK = true, CBQ = true; //which sides are still valid for castling (castle white king side etc) 
    Board board;



    public void newGame()
    {
        char[][] chessBoard = new char[][]{
            new char[] { 'r', ' ', ' ', ' ', 'k', ' ', ' ', 'r' },
            new char[] { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            new char[] { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            new char[] { ' ', ' ', ' ', ' ', 'q', ' ', ' ', ' ' },
            new char[] { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            new char[] { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            new char[] { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            new char[] { 'R', ' ', ' ', ' ', 'K', ' ', ' ', 'R' },
        };
        board = Board.charArrayToBoard(chessBoard); 

        List<Move> whiteMoveList = Moves.possibleMoves(Side.White, board, EP, CWK,CBK,CWQ,CBQ); 
        List <Move> blackMoveList = Moves.possibleMoves(Side.Black, board, EP, CWK, CBK, CWQ, CBQ);


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