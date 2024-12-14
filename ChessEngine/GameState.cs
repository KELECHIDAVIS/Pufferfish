public struct GameState
{
    Side sideToMove  ; // initially white
    bool CWK , CWQ  , CBK  , CBQ  ; //castling rights; updated when king or rook moves 
    ulong EP; // en passant bb; updated on a 2 pawn push 
    int halfMoveClock ; // how many halfmoves have been done ; at 100 theres a draw
    int fullMoveNum ;
    Move nextMove; 
}
