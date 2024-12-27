public struct GameState
{
    public Side sideToMove  ; // initially white
    public bool CWK , CWQ  , CBK  , CBQ  ; //castling rights; updated when king or rook moves 
    public ulong EP; // en passant bb; updated on a 2 pawn push 
    public int halfMoveClock ; // how many halfmoves have been done ; at 100 theres a draw
    public int fullMoveNum ;
    public Move nextMove; 
}
