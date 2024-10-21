
class Moves
{
    public const ulong FILE_A = 0b00000001_00000001_00000001_00000001_00000001_00000001_00000001_00000001;
    public const ulong FILE_AB= 0b11000000_11000000_11000000_11000000_11000000_11000000_11000000_11000000;
    public const ulong FILE_H = 0b10000000_10000000_10000000_10000000_10000000_10000000_10000000_10000000; 
    public const ulong RANK_8 = 0b11111111_00000000_00000000_00000000_00000000_00000000_00000000_00000000;
    public const ulong RANK_4 = 0b00000000_00000000_00000000_00000000_11111111_00000000_00000000_00000000;

    public static ulong PAWN_MOVES;  // to save on memory we just reassign this variable 
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
        else {
            return possibleMovesBlack(history, piecesBB, sideBB);
        }
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
    /// Returns all possible white pawn moves ; 
    /// moves are in form: x1,y1,x2,y2x1,y1...
    /// </summary>
    /// <param name="history"> history of moves for en passant</param>
    /// <param name="piecesBB"></param>
    /// <param name="sideBB"></param>
    /// <param name="nonCaptureBB"> not capturable pieces </param>
    /// <param name="captureBB"></param>
    /// <param name="emptyBB"> places that are empty</param>
    /// <returns></returns>
    private static string possiblePawnWhite(string history, ulong[][] piecesBB, ulong[] sideBB, ulong nonCaptureBB, ulong captureBB, ulong emptyBB)
    {
        string moveList = "";
        // capture right ;white pawn can't be on rank 8 because that'd be a promotion;  shift bits 9 to left ; make sure there is a caputarable piece there and make sure that piece is not on a file (left column wrap around)
        PAWN_MOVES = ((piecesBB[(int)Side.White][(int)Piece.Pawn] & ~RANK_8) << 9) &(captureBB & ~FILE_A) ;

        // now if a bit is on in that bb convert into move notation
        //x1,y1,x2,y2 
        int currentIndex = 0;
        int x1, y1, x2, y2; 

        while (PAWN_MOVES > 0) { // EXTRACT VALID MOVES 
            while ((PAWN_MOVES & 1) == 0) { PAWN_MOVES >>= 1; currentIndex++; } //iterate based on index of bit

            // now translates current index into a move 
            y2 = (currentIndex / 8) + 1;  x2 = (currentIndex % 8) + 1;
            y1 = y2 - 1; x1 = x2 - 1; // prev row and col respectively  

            moveList += "" + x1 + "," + y1 + "," + x2 + "," + y2;
            currentIndex++;
            PAWN_MOVES >>= 1;
        }
        

        // left capture 
        //wp cant be on rank8; shift left 7; capturable piece has to be at destination and can't be on file h; 
        PAWN_MOVES = ((piecesBB[(int)Side.White][(int)Piece.Pawn] & ~RANK_8) << 7) & (captureBB & ~FILE_H);

        currentIndex = 0;

        while (PAWN_MOVES > 0) { // EXTRACT VALID MOVES 
            while ((PAWN_MOVES & 1) == 0) { PAWN_MOVES >>= 1; currentIndex++; } //iterate based on index of bit

            // now translates current index into a move 
            y2 = (currentIndex / 8) + 1; x2 = (currentIndex % 8) + 1;
            y1 = y2 - 1; x1 = x2 + 1; // prev row, next col 

            moveList += "" + x1 + "," + y1 + "," + x2 + "," + y2;
            currentIndex++;
            PAWN_MOVES >>= 1;
        }

        // push pawn 1 ; that spot has to be empty
        PAWN_MOVES = ((piecesBB[(int)Side.White][(int)Piece.Pawn] & ~RANK_8) << 8) & emptyBB;

        currentIndex = 0;

        while (PAWN_MOVES > 0) { // EXTRACT VALID MOVES 
            while ((PAWN_MOVES & 1) == 0) { PAWN_MOVES >>= 1; currentIndex++; } //iterate based on index of bit

            // now translates current index into a move 
            y2 = (currentIndex / 8) + 1; x2 = (currentIndex % 8) + 1;
            y1 = y2 - 1; x1 = x2 ; // prev row, same col ; 

            moveList += "" + x1 + "," + y1 + "," + x2 + "," + y2;
            currentIndex++;
            PAWN_MOVES >>= 1;
        }

        //push pawn 2 ; both spot in front and destination has to be empty ; destination has to be on rank 4
        PAWN_MOVES = (piecesBB[(int)Side.White][(int)Piece.Pawn] << 16) & emptyBB & (emptyBB<<8) & RANK_4;
        
        while (PAWN_MOVES > 0) { // EXTRACT VALID MOVES 
            while ((PAWN_MOVES & 1) == 0) { PAWN_MOVES >>= 1; currentIndex++; } //iterate based on index of bit

            // now translates current index into a move 
            y2 = (currentIndex / 8) + 1; x2 = (currentIndex % 8) + 1;
            y1 = y2 - 2; x1 = x2; // prev row, same col ; 

            moveList += "" + x1 + "," + y1 + "," + x2 + "," + y2;
            currentIndex++;
            PAWN_MOVES >>= 1;
        }


        //PROMOTIONS 

        // capture right promotion
        //destination has to be capturable, on rank 8, and can't be on file a (wrap around) 
        PAWN_MOVES = (piecesBB[(int)Side.White][(int)Piece.Pawn] << 9) & captureBB & RANK_8 & (~FILE_A);

        // extract valid promotions 

        // capture left promo 
        PAWN_MOVES = (piecesBB[(int)Side.White][(int)Piece.Pawn] << 7) & captureBB & RANK_8 & (~FILE_H);

        // extract valid promotions 

        // push 1 promo 
        PAWN_MOVES = (piecesBB[(int)Side.White][(int)Piece.Pawn] << 8) & emptyBB & RANK_8;

        // extract valid promos 
        return moveList; 

    }

    
}