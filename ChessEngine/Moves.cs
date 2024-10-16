
class Moves
{
    public const ulong FILE_A = 0b00000001_00000001_00000001_00000001_00000001_00000001_00000001_00000001;
    public const ulong FILE_AB= 0b11000000_11000000_11000000_11000000_11000000_11000000_11000000_11000000;
    public const ulong FILE_H = 0b10000000_10000000_10000000_10000000_10000000_10000000_10000000_10000000; 
    public const ulong RANK_8 = 0b11111111_00000000_00000000_00000000_00000000_00000000_00000000_00000000;

    /// <summary>
    /// Returns all possible moves for that side 
    /// </summary>
    /// <param name="side"></param>
    /// <param name="history"></param>
    /// <param name="piecesBB"></param>
    /// <param name="sideBB"></param>
    /// <returns></returns>
    public static string possibleMoves( Side side, string history , ulong[][] piecesBB, ulong[] sideBB)
    {
        if(side == Side.White) return possibleMovesWhite(history, piecesBB, sideBB);

        return possibleMovesBlack(history, piecesBB, sideBB);
    }

    
    private static string possibleMovesBlack(string history, ulong[][] piecesBB, ulong[] sideBB)
    {
        throw new NotImplementedException();
    }

    private static string possibleMovesWhite(string history, ulong[][] piecesBB, ulong[] sideBB)
    {
        // Get all pieces white can and cannot capture 
        ulong nonCaptureBB = sideBB[(int)Side.White] | piecesBB[(int) Side.Black][(int) Piece.King]; // a bb that holds all white pieces and black king, because the player should never be able to cap. other king (illegal) 
        ulong captureBB = sideBB[(int)(Side.Black)] ^ piecesBB[(int) Side.Black] [(int) Piece.King]; // every black piece except black king 

        // get all empty squares as well 
        ulong emptyBB = ~(sideBB[(int)(Side.White)] | sideBB[(int) Side.Black]); // bb of squares with no pieces on them 

        string moveList = possiblePawnWhite(history, piecesBB, sideBB, nonCaptureBB, captureBB, emptyBB); // eventually add other pieces possible moves 

        return moveList; 

    }

    /// <summary>
    /// Returns all possible white pawn moves 
    /// </summary>
    /// <param name="history"></param>
    /// <param name="piecesBB"></param>
    /// <param name="sideBB"></param>
    /// <param name="nonCaptureBB"></param>
    /// <param name="captureBB"></param>
    /// <param name="emptyBB"></param>
    /// <returns></returns>
    private static string possiblePawnWhite(string history, ulong[][] piecesBB, ulong[] sideBB, ulong nonCaptureBB, ulong captureBB, ulong emptyBB)
    {
        // capture right ;white pawn can't be on rank 8 because that'd be a promotion;  shift bits 9 to left ; make sure there is a caputarable piece there and make sure that piece is not on a file (left column wrap around)
        if ((( (piecesBB[(int)Side.White][(int)Piece.Pawn] & ~RANK_8)<< 9) & (captureBB & ~FILE_A)) > 0)
        {
            // we have valid moves 
            ulong rightCapBB= (((piecesBB[(int)Side.White][(int)Piece.Pawn] & ~RANK_8) << 9) & (captureBB & ~FILE_A)); 

            // iterate through all bits and see which indexes are on 
        }
        return "";

    }
}