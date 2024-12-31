public struct GameState
{
    public Side sideToMove  ; // initially white
    public int castling; // 0000 no castling avail , 0001 white king side avail etc
    public ulong EP; // en passant bb; updated on a 2 pawn push' holds active ep square 
    public int halfMoveClock ; // how many halfmoves have been done ; at 100 theres a draw ;  moves without a pawn push or capture 
    public int fullMoveNum ;
    public Move nextMove;
    public ulong zobristKey; 
}
