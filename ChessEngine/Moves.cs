
using System.Numerics;
public enum MoveType
{
    QUIET, // just moving a piece  
    CAPTURE, 
    EVASION, // get out of check
    ENPASSANT,
    CASTLE, 
}
public struct Move
{
    public int origin;
    public int destination;
    public Piece promoPieceType; // can only be queen, knight, bishop, or rook, or none 
    public MoveType moveType; 
}
class Moves {

    public static string fileNames = "abcdefgh" ;
    // ranks go from 0-7 : 1-8
    public static readonly ulong[] RANKS = {
                0b00000000_00000000_00000000_00000000_00000000_00000000_00000000_11111111,
                0b00000000_00000000_00000000_00000000_00000000_00000000_11111111_00000000,
                0b00000000_00000000_00000000_00000000_00000000_11111111_00000000_00000000,
                0b00000000_00000000_00000000_00000000_11111111_00000000_00000000_00000000,
                0b00000000_00000000_00000000_11111111_00000000_00000000_00000000_00000000,
                0b00000000_00000000_11111111_00000000_00000000_00000000_00000000_00000000,
                0b00000000_11111111_00000000_00000000_00000000_00000000_00000000_00000000,
                0b11111111_00000000_00000000_00000000_00000000_00000000_00000000_00000000,
    };

    // files go from 0-7 : A-H
    public static readonly ulong[] FILES = {
                0b00000001_00000001_00000001_00000001_00000001_00000001_00000001_00000001,
                0b00000010_00000010_00000010_00000010_00000010_00000010_00000010_00000010,
                0b00000100_00000100_00000100_00000100_00000100_00000100_00000100_00000100,
                0b00001000_00001000_00001000_00001000_00001000_00001000_00001000_00001000,
                0b00010000_00010000_00010000_00010000_00010000_00010000_00010000_00010000,
                0b00100000_00100000_00100000_00100000_00100000_00100000_00100000_00100000,
                0b01000000_01000000_01000000_01000000_01000000_01000000_01000000_01000000,
                0b10000000_10000000_10000000_10000000_10000000_10000000_10000000_10000000,
    };

    //The squares that need to be vacant in order to castle on that side 
    static ulong wKInBetween = 0b00000000_00000000_00000000_00000000_00000000_00000000_00000000_01100000; 
    static ulong wQInBetween = 0b00000000_00000000_00000000_00000000_00000000_00000000_00000000_00001110;
    static ulong bkInBetween = 0b01100000_00000000_00000000_00000000_00000000_00000000_00000000_00000000;
    static ulong bQInBetween = 0b00001110_00000000_00000000_00000000_00000000_00000000_00000000_00000000;

    static int wKSideCastleDest = (int)Square.G1; 
    static int wQSideCastleDest = (int)Square.C1;
    static int bKSideCastleDest = (int)Square.G8;
    static int bQSideCastleDest = (int)Square.C8;

    public static ulong PAWN_MOVES;  // to save on memory we just reassign this variable 
    public static ulong KNIGHT_MOVES;
    public static ulong KING_MOVES;
    public static ulong ROOK_MOVES;
    public static ulong BISHOP_MOVES; 
    public static ulong QUEEN_MOVES;
    /// <summary>
    /// Returns all possible moves for that side 
    /// </summary>
    /// <param name="side"></param>
    /// <param name="history"></param>
    /// <param name="piecesBB"></param>
    /// <param name="sideBB"></param>
    /// <returns></returns>
    public static List<Move>  possibleMoves(Side side, ulong[][] piecesBB, ulong[] sideBB, ulong EP, bool CWK, bool CBK, bool CWQ, bool CBQ) {
        if (side == Side.White) return possibleMovesWhite(piecesBB, sideBB, EP, CWK, CWQ);
        else {
            return possibleMovesBlack( piecesBB, sideBB, EP, CBK, CBQ);
        }
    }

    public static List<Move> possibleMoves ( Side side, Board board, ulong EP, bool CWK, bool CBK, bool CWQ, bool CBQ) {
        return possibleMoves(side , board.piecesBB, board.sideBB, EP, CWK, CBK, CWQ, CBQ);
    }


    private static List<Move> possibleMovesBlack( ulong[][] piecesBB, ulong[] sideBB, ulong EP, bool CBK, bool CBQ)
    {
        ulong nonCaptureBB = sideBB[(int)Side.Black] | piecesBB[(int) Side.White][(int) Piece.King];
        ulong captureBB = sideBB[(int)Side.White] ^ piecesBB[(int) Side.White] [(int) Piece.King];

        ulong emptyBB = ~(sideBB[(int)Side.Black] | sideBB[(int) Side.White]);

        List<Move> moveList = new List<Move>();
        possiblePawnBlack(moveList, piecesBB, sideBB, nonCaptureBB, captureBB, emptyBB, EP );
        possibleRook(moveList, piecesBB, sideBB, nonCaptureBB, captureBB, emptyBB, Side.Black);
        possibleBishop(moveList, piecesBB, sideBB, nonCaptureBB, captureBB, emptyBB, Side.Black);
        possibleQueen(moveList, piecesBB, sideBB, nonCaptureBB, captureBB, emptyBB, Side.Black); 
        possibleKnight(moveList, piecesBB, sideBB, nonCaptureBB, captureBB, emptyBB, Side.Black);
        possibleKing(moveList, piecesBB, sideBB, nonCaptureBB, captureBB, emptyBB, Side.Black, CBK,  CBQ);

        return moveList; 
    }

    private static List<Move> possibleMovesWhite( ulong[][] piecesBB, ulong[] sideBB, ulong EP, bool CWK,  bool CWQ) {
        // Get all pieces white can and cannot capture 
        ulong nonCaptureBB = sideBB[(int)Side.White] | piecesBB[(int)Side.Black][(int)Piece.King]; // a bb that holds all white pieces and black king, because the player should never be able to cap. other king (illegal) 
        ulong captureBB = sideBB[(int)(Side.Black)] ^ piecesBB[(int)Side.Black][(int)Piece.King]; // every black piece except black king 

        // get all empty squares as well 
        ulong emptyBB = ~(sideBB[(int)(Side.White)] | sideBB[(int)Side.Black]); // bb of squares with no pieces on them 

        // get all the moves from each piece on this side 
        List<Move> moveList = new List<Move>();
        possiblePawnWhite(moveList, piecesBB, sideBB, nonCaptureBB, captureBB, emptyBB, EP);
        possibleRook(moveList, piecesBB, sideBB, nonCaptureBB, captureBB, emptyBB, Side.White);
        possibleBishop(moveList, piecesBB, sideBB, nonCaptureBB, captureBB, emptyBB, Side.White);
        possibleQueen(moveList, piecesBB, sideBB, nonCaptureBB, captureBB, emptyBB, Side.White);
        possibleKnight(moveList, piecesBB, sideBB, nonCaptureBB, captureBB, emptyBB, Side.White);
        possibleKing(moveList, piecesBB, sideBB, nonCaptureBB, captureBB, emptyBB, Side.White, CWK, CWQ);

        return moveList;



    }

    /// <summary>
    /// Returns all possible white pawn moves ; 
    /// Each move is 4 characters: x1y1x2y2
    /// Same for promotions: x1x2TP; Ex: 45QP
    /// moves are in form: x1y1x2y2x1,y1...
    /// </summary>
    /// <param name="history"> history of moves for en passant</param>
    /// <param name="piecesBB"></param>
    /// <param name="sideBB"></param>
    /// <param name="nonCaptureBB"> not capturable pieces </param>
    /// <param name="captureBB"></param>
    /// <param name="emptyBB"> places that are empty</param>
    /// <param name="EP"> En passant bb that can be used to find if en passants are possible </param>
    /// <returns></returns>
    private static void possiblePawnWhite(List<Move> moveList, ulong[][] piecesBB, ulong[] sideBB, ulong nonCaptureBB, ulong captureBB, ulong emptyBB, ulong EP ) {
        
        // capture right ;white pawn can't be on rank 7 because that'd be a promotion;  shift bits 9 to left ; make sure there is a caputarable piece there and make sure that piece is not on a file (left column wrap around)
        PAWN_MOVES = ((piecesBB[(int)Side.White][(int)Piece.Pawn] & ~RANKS[6]) << 9) & (captureBB & ~FILES[0]);

        // now if a bit is on in that bb convert into move notation
        //x1,y1,x2,y2 
        int currentIndex, origin;
        ulong mask;

        while (PAWN_MOVES > 0) {
            currentIndex = BitOperations.TrailingZeroCount(PAWN_MOVES);
            origin = currentIndex - 9;  // for capture right 
            moveList.Add(new Move { origin= origin,destination= currentIndex, promoPieceType= Piece.NONE, moveType = MoveType.CAPTURE }); 
            mask = ~(1UL << currentIndex);
            PAWN_MOVES &= mask;
        }

        // left capture 
        //wp cant be on rank7; shift left 7; capturable piece has to be at destination and can't be on file h; 
        PAWN_MOVES = ((piecesBB[(int)Side.White][(int)Piece.Pawn] & ~RANKS[6]) << 7) & (captureBB & ~FILES[7]);


        while (PAWN_MOVES > 0) {
            currentIndex = BitOperations.TrailingZeroCount(PAWN_MOVES);
            origin = currentIndex -7;  // for capture left 
            moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.NONE, moveType = MoveType.CAPTURE });
            mask = ~(1UL << currentIndex);
            PAWN_MOVES &= mask;
        }

        // push pawn 1 ; that spot has to be empty
        PAWN_MOVES = ((piecesBB[(int)Side.White][(int)Piece.Pawn] & ~RANKS[6]) << 8) & emptyBB;

        while (PAWN_MOVES > 0) {
            currentIndex = BitOperations.TrailingZeroCount(PAWN_MOVES);
            origin = currentIndex - 8;  // for push 1
            moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.NONE, moveType = MoveType.QUIET });
            mask = ~(1UL << currentIndex);
            PAWN_MOVES &= mask;
        }

        //push pawn 2 ; both spot in front and destination has to be empty ; destination has to be on rank 4
        PAWN_MOVES = (piecesBB[(int)Side.White][(int)Piece.Pawn] << 16) & RANKS[3] & emptyBB & (emptyBB << 8);

        while (PAWN_MOVES > 0) {
            currentIndex = BitOperations.TrailingZeroCount(PAWN_MOVES);
            origin = currentIndex - 16;  // for push 2
            moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.NONE, moveType = MoveType.QUIET });
            mask = ~(1UL << currentIndex);
            PAWN_MOVES &= mask;
        }

        //PROMOTIONS 


        // capture right promotion
        //destination has to be capturable, on rank 8, and can't be on file a (wrap around) 
        PAWN_MOVES = (piecesBB[(int)Side.White][(int)Piece.Pawn] << 9) & captureBB & RANKS[7] & (~FILES[0]);

        // extract valid promotions 
        // in form of x1,x2,PromoType,'P'  ; Ex: 45QP: a pawn in col 4 captures right and promotes to queen

        while (PAWN_MOVES > 0) {
            currentIndex = BitOperations.TrailingZeroCount(PAWN_MOVES);
            origin = currentIndex - 9;  // for push 1
            moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.Queen, moveType = MoveType.CAPTURE });
            moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.Rook, moveType = MoveType.CAPTURE });
            moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.Bishop, moveType = MoveType.CAPTURE });
            moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.Knight, moveType = MoveType.CAPTURE });
            mask = ~(1UL << currentIndex);
            PAWN_MOVES &= mask;
        }


        // capture left promo 
        PAWN_MOVES = (piecesBB[(int)Side.White][(int)Piece.Pawn] << 7) & captureBB & RANKS[7] & (~FILES[7]);

        while (PAWN_MOVES > 0) {
            currentIndex = BitOperations.TrailingZeroCount(PAWN_MOVES);
            origin = currentIndex - 7;  // for push 1
            moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.Queen, moveType = MoveType.CAPTURE });
            moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.Rook, moveType = MoveType.CAPTURE });
            moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.Bishop, moveType = MoveType.CAPTURE });
            moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.Knight, moveType = MoveType.CAPTURE });
            mask = ~(1UL << currentIndex);
            PAWN_MOVES &= mask;
        }


        // push 1 promo 
        PAWN_MOVES = (piecesBB[(int)Side.White][(int)Piece.Pawn] << 8) & emptyBB & RANKS[7];

        // extract valid promos 
        while (PAWN_MOVES > 0) {
            currentIndex = BitOperations.TrailingZeroCount(PAWN_MOVES);
            origin = currentIndex - 9;  // for push 1
            moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.Queen, moveType = MoveType.QUIET });
            moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.Rook, moveType = MoveType.QUIET });
            moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.Bishop, moveType = MoveType.QUIET });
            moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.Knight, moveType = MoveType.QUIET });
            mask = ~(1UL << currentIndex);
            PAWN_MOVES &= mask;
        }


        //EN PASSANT 

        // right capture 
        // wp has to be left of bp, they both have to be on rank 5, can't wrap around to file a, and has to be on square where ep is possible 
        // gives the piece to remove, NOT THE DESTINATION 
        PAWN_MOVES = (piecesBB[(int)Side.White][(int)Piece.Pawn] << 1) & piecesBB[(int)Side.Black][(int)Piece.Pawn] & RANKS[4] & ~FILES[0] & EP;

        int destination;
        // we know there is only going to be one 
        if (PAWN_MOVES > 0)
        {
            currentIndex = BitOperations.TrailingZeroCount(PAWN_MOVES);
            origin = currentIndex - 1;  // right en passant 
            destination = origin + 9;
            moveList.Add(new Move { origin = origin, destination = destination, promoPieceType = Piece.NONE, moveType = MoveType.ENPASSANT });
            mask = ~(1UL << currentIndex);
            PAWN_MOVES &= mask;
        }

        //left capture 

        PAWN_MOVES = (piecesBB[(int)Side.White][(int)Piece.Pawn] >> 1) & piecesBB[(int)Side.Black][(int)Piece.Pawn] & RANKS[4] & ~FILES[7] & EP;

        // we know there is only going to be one 
        if (PAWN_MOVES > 0)
        {
            currentIndex = BitOperations.TrailingZeroCount(PAWN_MOVES);
            origin = currentIndex + 1;   
            destination = origin + 7;
            moveList.Add(new Move { origin = origin, destination = destination, promoPieceType = Piece.NONE, moveType = MoveType.ENPASSANT });
            mask = ~(1UL << currentIndex);
            PAWN_MOVES &= mask;
        }
        
    }


    private static void possiblePawnBlack(List<Move> moveList, ulong[][] piecesBB, ulong[] sideBB, ulong nonCaptureBB, ulong captureBB, ulong emptyBB, ulong EP)
    {
        // capture right ; current pawn can't be on rank 2 cus that just promo and result must be capturable and can't be on file a 
        PAWN_MOVES = ((piecesBB[(int)Side.Black][(int)Piece.Pawn] & ~RANKS[1]) >> 7) & (captureBB & ~FILES[0]);

        // now if a bit is on in that bb convert into move notation
        //x1,y1,x2,y2 
        int currentIndex;
        int origin, destination; 
        ulong mask;

        while (PAWN_MOVES > 0)
        {
            currentIndex = BitOperations.TrailingZeroCount(PAWN_MOVES);
            origin = currentIndex +7;  // for capture right 
            moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.NONE, moveType = MoveType.CAPTURE });
            mask = ~(1UL << currentIndex);
            PAWN_MOVES &= mask;
        }

        // left capture 
        PAWN_MOVES = ((piecesBB[(int)Side.Black][(int)Piece.Pawn] & ~RANKS[1]) >> 9) & (captureBB & ~FILES[7]);


        while (PAWN_MOVES > 0)
        {
            currentIndex = BitOperations.TrailingZeroCount(PAWN_MOVES);
            origin = currentIndex + 9; 
            moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.NONE, moveType = MoveType.CAPTURE });
            mask = ~(1UL << currentIndex);
            PAWN_MOVES &= mask;
        }

        // push pawn 1 ; that spot has to be empty
        PAWN_MOVES = ((piecesBB[(int)Side.Black][(int)Piece.Pawn] & ~RANKS[1]) >> 8) & emptyBB;

        while (PAWN_MOVES > 0)
        {
            currentIndex = BitOperations.TrailingZeroCount(PAWN_MOVES);
            origin = currentIndex + 8;  //
            moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.NONE, moveType = MoveType.QUIET });
            mask = ~(1UL << currentIndex);
            PAWN_MOVES &= mask;
        }





        //push pawn 2 ; both spot in front and destination has to be empty ; destination has to be on rank 5
        PAWN_MOVES = (piecesBB[(int)Side.Black][(int)Piece.Pawn] >> 16) & RANKS[4] & emptyBB & (emptyBB >> 8);

        while (PAWN_MOVES > 0)
        {
            currentIndex = BitOperations.TrailingZeroCount(PAWN_MOVES);
            origin = currentIndex + 16;  //
            moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.NONE, moveType = MoveType.QUIET });
            mask = ~(1UL << currentIndex);
            PAWN_MOVES &= mask;
        }

        //PROMOTIONS 


        // capture right promotion
        //destination has to be capturable, on rank 1, and can't be on file a (wrap around) 
        PAWN_MOVES = (piecesBB[(int)Side.Black][(int)Piece.Pawn] >> 7) & captureBB & RANKS[0] & (~FILES[0]);

        // extract valid promotions 
        // in form of x1,x2,PromoType,'P'  ; Ex: 45QP: a pawn in col 4 captures right and promotes to queen

        while (PAWN_MOVES > 0)
        {
            currentIndex = BitOperations.TrailingZeroCount(PAWN_MOVES);
            origin = currentIndex +7;  // for push 1
            moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.Queen, moveType = MoveType.CAPTURE });
            moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.Rook, moveType = MoveType.CAPTURE });
            moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.Bishop, moveType = MoveType.CAPTURE });
            moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.Knight, moveType = MoveType.CAPTURE });
            mask = ~(1UL << currentIndex);
            PAWN_MOVES &= mask;
        }


        // capture left promo 
        PAWN_MOVES = (piecesBB[(int)Side.Black][(int)Piece.Pawn] >> 9) & captureBB & RANKS[0] & (~FILES[7]);

        while (PAWN_MOVES > 0)
        {
            currentIndex = BitOperations.TrailingZeroCount(PAWN_MOVES);
            origin = currentIndex + 9;  // for push 1
            moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.Queen, moveType = MoveType.CAPTURE });
            moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.Rook, moveType = MoveType.CAPTURE });
            moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.Bishop, moveType = MoveType.CAPTURE });
            moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.Knight, moveType = MoveType.CAPTURE });
            mask = ~(1UL << currentIndex);
            PAWN_MOVES &= mask;
        }


        // push 1 promo 
        PAWN_MOVES = (piecesBB[(int)Side.Black][(int)Piece.Pawn] >> 8) & emptyBB & RANKS[0];

        // extract valid promos 
        while (PAWN_MOVES > 0)
        {
            currentIndex = BitOperations.TrailingZeroCount(PAWN_MOVES);
            origin = currentIndex + 8;  // for push 1
            moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.Queen, moveType = MoveType.QUIET });
            moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.Rook, moveType = MoveType.QUIET });
            moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.Bishop, moveType = MoveType.QUIET });
            moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.Knight, moveType = MoveType.QUIET });
            mask = ~(1UL << currentIndex);
            PAWN_MOVES &= mask;
        }


        //EN PASSANT 

        // right capture 
        // bp has to be left of wp, both have to be on rank 4, can't wrap around to file a, has to be on a valid ep square 
        // give square of piece to remove NOT DESTINATION 
        PAWN_MOVES = (piecesBB[(int)Side.Black][(int)Piece.Pawn] << 1) & piecesBB[(int)Side.White][(int)Piece.Pawn] &  RANKS[3] & ~FILES[0] &EP;
        // we know there is only going to be one 
        if (PAWN_MOVES > 0)
        {
            currentIndex = BitOperations.TrailingZeroCount(PAWN_MOVES);
            origin = currentIndex - 1;  // right en passant 
            destination = origin - 7;
            moveList.Add(new Move { origin = origin, destination = destination, promoPieceType = Piece.NONE, moveType = MoveType.ENPASSANT });
            mask = ~(1UL << currentIndex);
            PAWN_MOVES &= mask;
        }

        //left capture 

        PAWN_MOVES = (piecesBB[(int)Side.Black][(int)Piece.Pawn] >> 1) & piecesBB[(int)Side.White][(int)Piece.Pawn] & RANKS[3] & ~FILES[7] & EP;

        // we know there is only going to be one 
        if (PAWN_MOVES > 0)
        {
            currentIndex = BitOperations.TrailingZeroCount(PAWN_MOVES);
            origin = currentIndex + 1;  // right en passant 
            destination = origin - 9;
            moveList.Add(new Move { origin = origin, destination = destination, promoPieceType = Piece.NONE, moveType = MoveType.ENPASSANT });
            mask = ~(1UL << currentIndex);
            PAWN_MOVES &= mask;
        }




    }
    private static void possibleRook( List<Move> moveList, ulong[][] piecesBB, ulong[] sideBB, ulong nonCaptureBB, ulong captureBB, ulong emptyBB , Side side) {
   
        // iterate through all the rooks 
        ulong rookBB = piecesBB[(int)side][(int)Piece.Rook]; 

        while(rookBB > 0 ) {// for every rook 
            int square = BitOperations.TrailingZeroCount(rookBB);


            // get sliding moves 
            //first or all piece boards then remove the current space so we can get blocker board 
            ROOK_MOVES = (~emptyBB) & ~(1UL << square) ; // get blocker board 

            ROOK_MOVES = getRookMoves(ROOK_MOVES, square); // get possible rook moves from function 

            ROOK_MOVES &= (captureBB | emptyBB); // make sure that moves are only on capturable pieces and empty spaces by anding 

            MoveType captureOrNot;
            ulong indexMask; 
            // parse moves from ROOK MOVES 
            while (ROOK_MOVES > 0) {
                int index = BitOperations.TrailingZeroCount(ROOK_MOVES);
                indexMask = 1UL << index; 

                // see if they captured something or not; if > 0 that means it captured a capturable piece 
                if ((indexMask & captureBB) > 0){
                    captureOrNot = MoveType.CAPTURE;
                }
                else { captureOrNot = MoveType.QUIET;  }

                moveList.Add(new Move { origin = square, destination= index, moveType= captureOrNot, promoPieceType= Piece.NONE});
                ROOK_MOVES &= ~indexMask; 
            }
            // turn off the current index
            rookBB &= ~(1UL<<square);
        }
    }


    /// <summary>
    /// returns rook moves as a ulong 
    /// </summary>
    /// <returns></returns>
    private static ulong getRookMoves(ulong blockerConfig , int square ) {
        int rookKey = (int)SlidingMoves.getMagicIndex(SlidingMoves.RookInfoTable[square], blockerConfig);

        return SlidingMoves.RookMoveHashTable[square][rookKey];
    }


    private static void possibleBishop(List<Move> moveList, ulong[][] piecesBB, ulong[] sideBB, ulong nonCaptureBB, ulong captureBB, ulong emptyBB, Side side) {
        

        // iterate through all the bishops 
        ulong bishopBB = piecesBB[(int)side][(int)Piece.Bishop];

        while (bishopBB > 0) {// for every rook 
            int square = BitOperations.TrailingZeroCount(bishopBB);


            // get sliding moves 
            //first or all piece boards then remove the current space so we can get blocker board 
            BISHOP_MOVES = (~emptyBB) & ~(1UL << square); // get blocker board 

            BISHOP_MOVES = getBishopMoves(BISHOP_MOVES, square); // get possible bishop moves from function 

            BISHOP_MOVES &= (captureBB | emptyBB); // make sure that moves are only on capturable pieces and empty spaces by anding 

            MoveType captureOrNot;
            ulong indexMask;
            // parse moves from ROOK MOVES 
            while (BISHOP_MOVES > 0) {
                int index = BitOperations.TrailingZeroCount(BISHOP_MOVES);
                indexMask = 1UL << index;

                // see if they captured something or not; if > 0 that means it captured a capturable piece 
                if ((indexMask & captureBB) > 0)
                {
                    captureOrNot = MoveType.CAPTURE;
                }
                else { captureOrNot = MoveType.QUIET; }

                moveList.Add(new Move { origin = square, destination = index, moveType = captureOrNot, promoPieceType = Piece.NONE });
                BISHOP_MOVES &= ~(indexMask);
            }
            // turn off the current index
            bishopBB &= ~(1UL << square);
        }
        
    }


    private static ulong getBishopMoves(ulong blockerConfig, int square) {
        int bishopKey = (int)SlidingMoves.getMagicIndex(SlidingMoves.BishopInfoTable[square], blockerConfig);

        return SlidingMoves.BishopMoveHashTable[square][bishopKey];
    }


    /// <summary>
    /// To get queen moves just add the rook's and bishop's protocol into one bb; 
    /// </summary>
    /// <param name="piecesBB"></param>
    /// <param name="sideBB"></param>
    /// <param name="nonCaptureBB"></param>
    /// <param name="captureBB"></param>
    /// <param name="emptyBB"></param>
    /// <returns></returns>
    private static void possibleQueen(List<Move> moveList, ulong[][] piecesBB, ulong[] sideBB, ulong nonCaptureBB, ulong captureBB, ulong emptyBB, Side side) {

        // get all queen positions 
        ulong queenBB = piecesBB[(int)side][(int)Piece.Queen];

        while (queenBB > 0) {// for every rook 
            int square = BitOperations.TrailingZeroCount(queenBB);


            // get sliding moves 
            //first or all piece boards then remove the current space so we can get blocker board 
            QUEEN_MOVES = (~emptyBB) & ~(1UL << square); // get blocker board 

            // or rook moves and bishop moves to get all possible queens moves 
            QUEEN_MOVES = getRookMoves(QUEEN_MOVES, square) | getBishopMoves(QUEEN_MOVES,square) ; 

            QUEEN_MOVES &= (captureBB | emptyBB); // make sure that moves are only on capturable pieces and empty spaces by anding 

            MoveType captureOrNot;
            ulong indexMask;
            // parse moves from QUEEN MOVES 
            while (QUEEN_MOVES > 0) {
                int index = BitOperations.TrailingZeroCount(QUEEN_MOVES);
                indexMask = 1UL << index;

                // see if they captured something or not; if > 0 that means it captured a capturable piece 
                if ((indexMask & captureBB) > 0)
                {
                    captureOrNot = MoveType.CAPTURE;
                }
                else { captureOrNot = MoveType.QUIET; }

                moveList.Add(new Move { origin = square, destination = index, moveType = captureOrNot, promoPieceType = Piece.NONE });
                QUEEN_MOVES &= ~(indexMask);
            }
            // turn off the current index
            queenBB &= ~(1UL << square);
        }
    }

    //KNIGHT MOVES 
    static ulong northWestWest(ulong bb) {
        return (bb << 6) & ~(FILES[6]|FILES[7]);  //shift left 6 and make sure result isn't on file g or h (wrap around) 
    }
    static ulong northEastEast(ulong bb) {
        return (bb << 10) & ~(FILES[0] | FILES[1]); // make sure result not on a or b 
    }
    static ulong northNorthWest(ulong bb) {
        return (bb << 15) & ~(FILES[7] ); // make sure result not on h
    }
    static ulong northNorthEast(ulong bb) {
        return (bb << 17) & ~(FILES[0]); // make sure result not on h
    }
    static ulong southEastEast (ulong bb) {
        return (bb >> 6) & ~(FILES[0] | FILES[1]);  
    }
    static ulong southWestWest(ulong bb) {
        return (bb >> 10) & ~(FILES[6] | FILES[7]);
    }
    static ulong southSouthEast(ulong bb) {
        return (bb >> 15) & ~(FILES[0]);
    }
    static ulong southSouthWest(ulong bb) {
        return (bb >> 17) & ~(FILES[7]);
    }

    private static void possibleKnight(List<Move> moveList , ulong[][] piecesBB, ulong[] sideBB, ulong nonCaptureBB, ulong captureBB, ulong emptyBB, Side side) {



        int origin;
        ulong indexMask;
        MoveType captureOrNot; 
        // make sure the move is either on empty or capturable square 

        KNIGHT_MOVES = northEastEast(piecesBB[(int)side][(int)Piece.Knight]) & (captureBB |emptyBB); // north east east 
        // parse moves for current moveset 
        while (KNIGHT_MOVES > 0) {
            int index = BitOperations.TrailingZeroCount(KNIGHT_MOVES);
            indexMask = (1UL << index);

            if ((indexMask & captureBB) > 0){// something was captured 
                captureOrNot = MoveType.CAPTURE; 
            }else{
                captureOrNot = MoveType.QUIET;     
            }
            origin = index - 10; 
            moveList.Add(new Move { origin = origin, destination = index , moveType= captureOrNot, promoPieceType= Piece.NONE }); 
            KNIGHT_MOVES &= ~indexMask;
        }

        // make sure the move is either on empty or capturable square 

        KNIGHT_MOVES = northNorthEast(piecesBB[(int)side][(int)Piece.Knight]) & (captureBB | emptyBB); 

        // parse moves for current moveset 
        while (KNIGHT_MOVES > 0) {
            int index = BitOperations.TrailingZeroCount(KNIGHT_MOVES);
            indexMask = (1UL << index);

            if ((indexMask & captureBB) > 0)
            {// something was captured 
                captureOrNot = MoveType.CAPTURE;
            }
            else
            {
                captureOrNot = MoveType.QUIET;
            }
            origin = index - 17;
            moveList.Add(new Move { origin = origin, destination = index, moveType = captureOrNot, promoPieceType = Piece.NONE });
            KNIGHT_MOVES &= ~indexMask;
        }

        KNIGHT_MOVES = northWestWest(piecesBB[(int)side][(int)Piece.Knight]) & (captureBB | emptyBB);

        // parse moves for current moveset 
        while (KNIGHT_MOVES > 0) {
            int index = BitOperations.TrailingZeroCount(KNIGHT_MOVES);
            indexMask = (1UL << index);

            if ((indexMask & captureBB) > 0)
            {// something was captured 
                captureOrNot = MoveType.CAPTURE;
            }
            else
            {
                captureOrNot = MoveType.QUIET;
            }
            origin = index - 6;
            moveList.Add(new Move { origin = origin, destination = index, moveType = captureOrNot, promoPieceType = Piece.NONE });
            KNIGHT_MOVES &= ~indexMask;
        }

        KNIGHT_MOVES = northNorthWest(piecesBB[(int)side][(int)Piece.Knight]) & (captureBB | emptyBB);

        // parse moves for current moveset 
        while (KNIGHT_MOVES > 0) {
            int index = BitOperations.TrailingZeroCount(KNIGHT_MOVES);
            indexMask = (1UL << index);

            if ((indexMask & captureBB) > 0)
            {// something was captured 
                captureOrNot = MoveType.CAPTURE;
            }
            else
            {
                captureOrNot = MoveType.QUIET;
            }
            origin = index - 15;
            moveList.Add(new Move { origin = origin, destination = index, moveType = captureOrNot, promoPieceType = Piece.NONE });
            KNIGHT_MOVES &= ~indexMask;
        }

        KNIGHT_MOVES = southEastEast(piecesBB[(int)side][(int)Piece.Knight]) & (captureBB | emptyBB);

        // parse moves for current moveset 
        while (KNIGHT_MOVES > 0) {
            int index = BitOperations.TrailingZeroCount(KNIGHT_MOVES);
            indexMask = (1UL << index);

            if ((indexMask & captureBB) > 0)
            {// something was captured 
                captureOrNot = MoveType.CAPTURE;
            }
            else
            {
                captureOrNot = MoveType.QUIET;
            }
            origin = index +6;
            moveList.Add(new Move { origin = origin, destination = index, moveType = captureOrNot, promoPieceType = Piece.NONE });
            KNIGHT_MOVES &= ~indexMask;
        }

        KNIGHT_MOVES = southSouthEast(piecesBB[(int)side][(int)Piece.Knight]) & (captureBB | emptyBB);

        // parse moves for current moveset 
        while (KNIGHT_MOVES > 0) {
            int index = BitOperations.TrailingZeroCount(KNIGHT_MOVES);
            indexMask = (1UL << index);

            if ((indexMask & captureBB) > 0)
            {// something was captured 
                captureOrNot = MoveType.CAPTURE;
            }
            else
            {
                captureOrNot = MoveType.QUIET;
            }
            origin = index + 15;
            moveList.Add(new Move { origin = origin, destination = index, moveType = captureOrNot, promoPieceType = Piece.NONE });
            KNIGHT_MOVES &= ~indexMask;
        }

        KNIGHT_MOVES = southWestWest(piecesBB[(int)side][(int)Piece.Knight]) & (captureBB | emptyBB);

        // parse moves for current moveset 
        while (KNIGHT_MOVES > 0) {
            int index = BitOperations.TrailingZeroCount(KNIGHT_MOVES);
            indexMask = (1UL << index);

            if ((indexMask & captureBB) > 0)
            {// something was captured 
                captureOrNot = MoveType.CAPTURE;
            }
            else
            {
                captureOrNot = MoveType.QUIET;
            }
            origin = index + 10;
            moveList.Add(new Move { origin = origin, destination = index, moveType = captureOrNot, promoPieceType = Piece.NONE });
            KNIGHT_MOVES &= ~indexMask; 
        }

        KNIGHT_MOVES = southSouthWest(piecesBB[(int)side][(int)Piece.Knight]) & (captureBB | emptyBB);

        // parse moves for current moveset 
        while (KNIGHT_MOVES > 0) {
            int index = BitOperations.TrailingZeroCount(KNIGHT_MOVES);
            indexMask = (1UL << index);

            if ((indexMask & captureBB) > 0)
            {// something was captured 
                captureOrNot = MoveType.CAPTURE;
            }
            else
            {
                captureOrNot = MoveType.QUIET;
            }
            origin = index + 17;
            moveList.Add(new Move { origin = origin, destination = index, moveType = captureOrNot, promoPieceType = Piece.NONE });
            KNIGHT_MOVES &= ~indexMask;
        }
        
    }
    //KNIGHT MOVES 


    // there is always only one king for a side 
    // a king can only move to a place that won't put it into check 
    // kings can castle if the rook or king hasn't moved yet nd if the castling isn't blocked 
    private static void possibleKing(List<Move> moveList, ulong[][] piecesBB, ulong[] sideBB, ulong nonCaptureBB, ulong captureBB, ulong emptyBB, Side side, bool castleKingSide, bool castleQueenSide) {


        // find the kings reg attack pattern
        ulong currentKing = piecesBB[(int)side][(int)Piece.King];
        int originOfKing= BitOperations.TrailingZeroCount(currentKing); // get current square of king; we know there's only ever one 

        // left moves; the result can't be on file h 
        // left, left up , left down ; check they aren't on file h 
        KING_MOVES |= (currentKing >> 1 | (currentKing<<7) | (currentKing >>9 )) & ~FILES[7];

        //right , right up , right down; can't be on file a 
        KING_MOVES |= (currentKing << 1 | (currentKing << 9) | (currentKing >> 7)) & ~FILES[0];

        // up and down ( check not neccessary because bits don't rollover 
        KING_MOVES|= (currentKing <<8 ) | (currentKing >>8 );

        

        // now we have to check that these moves are on empty or capturable squares
        KING_MOVES &= (emptyBB | captureBB);

        
        // a bb that holds all the square that are in sight of black pieces 
        ulong unsafeBB = getUnsafeSquares(piecesBB, sideBB, nonCaptureBB, captureBB, emptyBB, side, originOfKing);
        

        // then check that these moves don't put the king in check; king can only move where is safe for the king 
        KING_MOVES &= ~unsafeBB;


        //if the king is currently in check, the next safe move would be an evasion? ; if greater than zero then king is in check 
        bool currentlyInCheck = (currentKing & unsafeBB) > 0; 

        MoveType captureOrNot;
        ulong indexMask;
       
        while (KING_MOVES > 0)
        {
            int index = BitOperations.TrailingZeroCount(KING_MOVES);
            indexMask = (1UL << index);

            if (currentlyInCheck)
            {
                captureOrNot = MoveType.EVASION; // if the king is in check, a safe move is an evasion 
            }
            else if ((indexMask & captureBB) > 0)
            {// something was captured 
                captureOrNot = MoveType.CAPTURE;
            }
            else
            {
                captureOrNot = MoveType.QUIET;
            }
            
            moveList.Add(new Move { origin = originOfKing, destination = index, moveType = captureOrNot, promoPieceType = Piece.NONE });
            KING_MOVES &= ~indexMask;
        }
        
       
        //CASTLING
        ulong kingInBetween = (side==Side.White) ? wKInBetween : bkInBetween; // the squares in between the rook and king 
        ulong queenInBetween = (side == Side.White) ? wQInBetween : bQInBetween;

        int kingDest = (side == Side.White) ? wKSideCastleDest : bKSideCastleDest; // destination on where you're castling 
        int queenDest = (side==Side.White) ? wQSideCastleDest: bQSideCastleDest;

        // and the in between squares with all pieces, if returns 0 there are no pieces in there
        bool kingSideIsVacant = (kingInBetween & ~emptyBB) == 0;
        bool queenSideIsVacant = (queenInBetween & ~emptyBB) == 0;

        // now all in between squares and the king squares have to be safe 
        // so and a combination of inbetween squares nd king bb with unsafe; if == 0 theres no overlap so the castle is safe 
        bool kingSideSafe = ((kingInBetween | currentKing) & unsafeBB) == 0;
        bool queenSideSafe = ((queenInBetween | currentKing) & unsafeBB) == 0;

        // so now if the king has castling rights AND the spots are vacant AND they are all safe the king can castle on that side 
        if (castleKingSide && kingSideIsVacant && kingSideSafe)
            moveList.Add(new Move { origin = originOfKing, destination = kingDest, moveType = MoveType.CASTLE, promoPieceType = Piece.NONE });
        if (castleQueenSide && queenSideIsVacant && queenSideSafe)
            moveList.Add(new Move { origin = originOfKing, destination = queenDest, moveType = MoveType.CASTLE, promoPieceType = Piece.NONE });

    }

    //this would return the unsafe squares for this sides to help with king safe moves 
    private static ulong getUnsafeSquares(ulong[][] piecesBB, ulong[] sideBB, ulong nonCaptureBB, ulong captureBB, ulong emptyBB, Side side, int kingsCurrentSquare) {

        ulong unsafeBB = 0;
        // if we looking unsafe squares for the white side the opponent would be black 
        int opponent = (side == Side.White) ? (int) Side.Black : (int) Side.White;

        // find the squares protected by black squares 

        //for pawn moves 
        if (side == Side.White) {
            // get black pawns right and left attacks 
            unsafeBB |= (piecesBB[opponent][(int)Piece.Pawn]>>7) & ~FILES[0]; // black pawns right attack; make sure result not on file a  
            unsafeBB |= (piecesBB[opponent][(int)Piece.Pawn]>>9) & ~FILES[7];
        }
        else{
            unsafeBB |= (piecesBB[opponent][(int)Piece.Pawn] << 9) & ~FILES[0]; // white pawns right attack; make sure result not on file a  
            unsafeBB |= (piecesBB[opponent][(int)Piece.Pawn] << 7) & ~FILES[7];
        }

        // black knight attacks ; don't have to check if their attacks are empty because if there is a piece on there that means they are protecting it 
        unsafeBB |= northNorthEast(piecesBB[opponent][(int)Piece.Knight])
            | northEastEast(piecesBB[opponent][(int)Piece.Knight])
            | northNorthWest(piecesBB[opponent][(int)Piece.Knight])
            | northWestWest(piecesBB[opponent][(int)Piece.Knight])
            | southEastEast(piecesBB[opponent][(int)Piece.Knight])
            | southSouthEast(piecesBB[opponent][(int)Piece.Knight])
            | southWestWest(piecesBB[opponent][(int)Piece.Knight])
            | southSouthWest(piecesBB[opponent][(int)Piece.Knight]);

        // Black bishops 
        // for each black bishop add it's moves to the unsafe 
        ulong slidingPieceBB = piecesBB[opponent][(int)Piece.Bishop];
        ulong blockerBB;
        while (slidingPieceBB > 0)
        {
            int square = BitOperations.TrailingZeroCount(slidingPieceBB);

            // get bishop move from current spot; remove current bishop from blocker bb  
            // you want to also remove current side king from blocker so that it doesn't allow king to make illegal moves 
            blockerBB = ~(emptyBB | piecesBB[(int) side][(int)Piece.King]) & ~(1UL << square);
            unsafeBB |= getBishopMoves(blockerBB, square);// extract squares protected by bishop 


            slidingPieceBB &= ~(1UL << square);
        }

        // Black rooks 

        slidingPieceBB = piecesBB[opponent][(int)Piece.Rook];
        while (slidingPieceBB > 0)
        {
            int square = BitOperations.TrailingZeroCount(slidingPieceBB);

            blockerBB = ~(emptyBB | piecesBB[(int)side][(int)Piece.King]) & ~(1UL << square);
            unsafeBB |= getRookMoves(blockerBB, square);


            slidingPieceBB &= ~(1UL << square);
        }
        // Black queens 

        slidingPieceBB = piecesBB[opponent][(int)Piece.Queen];
        while (slidingPieceBB > 0)
        {
            int square = BitOperations.TrailingZeroCount(slidingPieceBB);

            blockerBB = ~(emptyBB | piecesBB[(int)side][(int)Piece.King]) & ~(1UL << square);
            unsafeBB |= getBishopMoves(blockerBB, square);
            unsafeBB |= getRookMoves(blockerBB, square);

            slidingPieceBB &= ~(1UL << square);
        }

        // black kings attacks 
        unsafeBB |= (piecesBB[opponent][(int)Piece.King] >> 1 | (piecesBB[opponent][(int)Piece.King] << 7) | (piecesBB[opponent][(int)Piece.King] >> 9)) & ~FILES[7];

        //right , right up , right down; can't be on file a 
        unsafeBB |= (piecesBB[opponent][(int)Piece.King] << 1 | (piecesBB[opponent][(int)Piece.King] << 9) | (piecesBB[opponent][(int)Piece.King] >> 7)) & ~FILES[0];

        // up and down ( check not neccessary because bits don't rollover 
        unsafeBB |= (piecesBB[opponent][(int)Piece.King] << 8) | (piecesBB[opponent][(int)Piece.King] >> 8);


        return unsafeBB; 
    }
}

