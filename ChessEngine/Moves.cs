
using System.Numerics;

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
    public static string possibleMoves(Side side, string history, ulong[][] piecesBB, ulong[] sideBB) {
        if (side == Side.White) return possibleMovesWhite(history, piecesBB, sideBB);
        else {
            return possibleMovesBlack(history, piecesBB, sideBB);
        }
    }

    public static string possibleMoves ( Side side, string history, Board board) {
        return possibleMoves(side , history , board.piecesBB, board.sideBB);
    }


    private static string possibleMovesBlack(string history, ulong[][] piecesBB, ulong[] sideBB)
    {
        ulong nonCaptureBB = sideBB[(int)Side.Black] | piecesBB[(int) Side.White][(int) Piece.King];
        ulong captureBB = sideBB[(int)Side.White] ^ piecesBB[(int) Side.White] [(int) Piece.King];

        ulong emptyBB = ~(sideBB[(int)Side.Black] | sideBB[(int) Side.White]);

        string moveList = possiblePawnBlack(history, piecesBB, sideBB, nonCaptureBB, captureBB, emptyBB)
            +possibleRook(piecesBB, sideBB, nonCaptureBB, captureBB, emptyBB, Side.Black)
            +possibleBishop(piecesBB, sideBB, nonCaptureBB, captureBB, emptyBB, Side.Black)
            +possibleQueen(piecesBB, sideBB, nonCaptureBB, captureBB, emptyBB, Side.Black)
            +possibleKnight(piecesBB, sideBB, nonCaptureBB, captureBB, emptyBB, Side.Black)
            +possibleKing(piecesBB, sideBB, nonCaptureBB, captureBB, emptyBB, Side.Black);


        return moveList; 
    }

    private static string possibleMovesWhite(string history, ulong[][] piecesBB, ulong[] sideBB) {
        // Get all pieces white can and cannot capture 
        ulong nonCaptureBB = sideBB[(int)Side.White] | piecesBB[(int)Side.Black][(int)Piece.King]; // a bb that holds all white pieces and black king, because the player should never be able to cap. other king (illegal) 
        ulong captureBB = sideBB[(int)(Side.Black)] ^ piecesBB[(int)Side.Black][(int)Piece.King]; // every black piece except black king 

        // get all empty squares as well 
        ulong emptyBB = ~(sideBB[(int)(Side.White)] | sideBB[(int)Side.Black]); // bb of squares with no pieces on them 

        // get all the moves from each piece on this side 
        string moveList = possiblePawnWhite(history, piecesBB, sideBB, nonCaptureBB, captureBB, emptyBB) 
            + possibleRook(piecesBB,sideBB,nonCaptureBB,captureBB,emptyBB, Side.White)
            + possibleBishop(piecesBB, sideBB, nonCaptureBB, captureBB, emptyBB, Side.White)
            + possibleQueen(piecesBB, sideBB, nonCaptureBB, captureBB, emptyBB, Side.White) 
            + possibleKnight(piecesBB, sideBB, nonCaptureBB, captureBB, emptyBB, Side.White)
            +possibleKing(piecesBB, sideBB, nonCaptureBB, captureBB, emptyBB, Side.White);  

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
    /// <returns></returns>
    private static string possiblePawnWhite(string history, ulong[][] piecesBB, ulong[] sideBB, ulong nonCaptureBB, ulong captureBB, ulong emptyBB) {
        string moveList = "";
        // capture right ;white pawn can't be on rank 8 because that'd be a promotion;  shift bits 9 to left ; make sure there is a caputarable piece there and make sure that piece is not on a file (left column wrap around)
        PAWN_MOVES = ((piecesBB[(int)Side.White][(int)Piece.Pawn] & ~RANKS[7]) << 9) & (captureBB & ~FILES[0]);

        // now if a bit is on in that bb convert into move notation
        //x1,y1,x2,y2 
        int currentIndex;
        int x1, y1, x2, y2;
        ulong mask;

        while (PAWN_MOVES > 0) {
            currentIndex = BitOperations.TrailingZeroCount(PAWN_MOVES);
            // so our destination is the currIndex ; do calcs 
            y2 = (currentIndex / 8)+1 ; x2 = (currentIndex % 8) ;
            y1 = y2 - 1; x1 = x2 - 1; // prev row and col respectively  

            moveList += "" + fileNames[x1] + "" + y1 + "" + fileNames[x2] + "" + y2;
            mask = ~(1UL << currentIndex);
            PAWN_MOVES &= mask;
        }

        // left capture 
        //wp cant be on rank8; shift left 7; capturable piece has to be at destination and can't be on file h; 
        PAWN_MOVES = ((piecesBB[(int)Side.White][(int)Piece.Pawn] & ~RANKS[7]) << 7) & (captureBB & ~FILES[7]);


        while (PAWN_MOVES > 0) {
            currentIndex = BitOperations.TrailingZeroCount(PAWN_MOVES);
            // so our destination is the currIndex ; do calcs 
            y2 = (currentIndex / 8) + 1; x2 = (currentIndex % 8) ;
            y1 = y2 - 1; x1 = x2 + 1; // prev row and col respectively  

            moveList += "" + fileNames[x1] + "" + y1 + "" + fileNames[x2] + "" + y2;
            mask = ~(1UL << currentIndex);
            PAWN_MOVES &= mask;
        }

        // push pawn 1 ; that spot has to be empty
        PAWN_MOVES = ((piecesBB[(int)Side.White][(int)Piece.Pawn] & ~RANKS[7]) << 8) & emptyBB;

        while (PAWN_MOVES > 0) {
            currentIndex = BitOperations.TrailingZeroCount(PAWN_MOVES);
            // so our destination is the currIndex ; do calcs 
            y2 = (currentIndex / 8) + 1; x2 = (currentIndex % 8) ;
            y1 = y2 - 1; x1 = x2; // prev row and col respectively  

            moveList += "" + fileNames[x1] + "" + y1 + "" + fileNames[x2] + "" + y2;
            mask = ~(1UL << currentIndex);
            PAWN_MOVES &= mask;
        }





        //push pawn 2 ; both spot in front and destination has to be empty ; destination has to be on rank 4
        PAWN_MOVES = (piecesBB[(int)Side.White][(int)Piece.Pawn] << 16) & RANKS[3] & emptyBB & (emptyBB << 8);

        while (PAWN_MOVES > 0) {
            currentIndex = BitOperations.TrailingZeroCount(PAWN_MOVES);
            // so our destination is the currIndex ; do calcs 
            y2 = (currentIndex / 8) + 1; x2 = (currentIndex % 8) ;
            y1 = y2 - 2; x1 = x2; // prev row and col respectively  

            moveList += "" + fileNames[x1] + "" + y1 + "" + fileNames[x2] + "" + y2;
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
            // so our destination is the currIndex ; do calcs 
            x2 = (currentIndex % 8) ;
            x1 = x2 - 1; // prev row and col respectively  

            moveList += "" + fileNames[x1] + "" + fileNames[x2] + "QP" + fileNames[x1] + "" + fileNames[x2] + "RP" + fileNames[x1] + "" + fileNames[x2] + "BP" + fileNames[x1] + "" + fileNames[x2] + "NP";
            mask = ~(1UL << currentIndex);
            PAWN_MOVES &= mask;
        }


        // capture left promo 
        PAWN_MOVES = (piecesBB[(int)Side.White][(int)Piece.Pawn] << 7) & captureBB & RANKS[7] & (~FILES[7]);

        while (PAWN_MOVES > 0) {
            currentIndex = BitOperations.TrailingZeroCount(PAWN_MOVES);
            // so our destination is the currIndex ; do calcs 
            x2 = (currentIndex % 8) ;
            x1 = x2 + 1; // prev row and col respectively  

            moveList += "" + fileNames[x1] + "" + fileNames[x2] + "QP" + fileNames[x1]   + "" + fileNames[x2] + "RP" + fileNames[x1] + "" + fileNames[x2] + "BP" + fileNames[x1] + "" + fileNames[x2] + "NP";
            mask = ~(1UL << currentIndex);
            PAWN_MOVES &= mask;
        }


        // push 1 promo 
        PAWN_MOVES = (piecesBB[(int)Side.White][(int)Piece.Pawn] << 8) & emptyBB & RANKS[7];

        // extract valid promos 
        while (PAWN_MOVES > 0) {
            currentIndex = BitOperations.TrailingZeroCount(PAWN_MOVES);
            // so our destination is the currIndex ; do calcs 
            x2 = (currentIndex % 8) ;
            x1 = x2; // prev row and col respectively  

            moveList += "" + fileNames[x1] + "" + fileNames[x2] + "QP" + fileNames[x1] + "" + fileNames[x2] + "RP" + fileNames[x1] + "" + fileNames[x2] + "BP" + fileNames[x1] + "" + fileNames[x2] + "NP";
            mask = ~(1UL << currentIndex);
            PAWN_MOVES &= mask;
        }


        //EN PASSANT 

        //history has to at least have a move and history's last move has to be a valid black pawn two move 
        if (history.Length >= 4) {

            //check two move validity 
            // if the files are letters, then make correct subtraction to get index value 
            y2 = history[history.Length - 1] - '0';
            x2 = (Char.IsLetter(history[history.Length - 2])) ? history[history.Length - 2] - 'a' : history[history.Length - 2] - '0';
            y1 = history[history.Length - 3] - '0';
            x1 = (Char.IsLetter(history[history.Length - 4])) ? history[history.Length - 4] - 'a' : history[history.Length - 4] - '0';

            // x1 has to equal x2 and y1 has to be 2 greater than y2 (black pawn moving two down), y2 has to equal rank 5 
            if (x1 == x2 && y2 == 5 && y1 == y2 + 2) {

                // right capture 
                // wp has to be left of bp that just moved, they both have to be on rank 5, move wp to space above 
                PAWN_MOVES = ((piecesBB[(int)Side.White][(int)Piece.Pawn] << 1) & piecesBB[(int)Side.Black][(int)Piece.Pawn] & FILES[x1 ] & RANKS[4]) << 8;

                int startFile, destFile; 
                // we know there is only going to be one 
                if (PAWN_MOVES > 0) {
                    currentIndex = BitOperations.TrailingZeroCount(PAWN_MOVES);
                    // so our destination is the currIndex ; do calcs 
                    destFile = (currentIndex % 8) ;
                    startFile = destFile - 1;

                    moveList += "" + fileNames[startFile] + "" + fileNames[destFile] + "EE"; // to make 4 total characters ; y's can be inferred bc always 5->6 for wps 

                    mask = ~(1UL << currentIndex);
                    PAWN_MOVES &= mask;
                }

                //left capture 

                PAWN_MOVES = ((piecesBB[(int)Side.White][(int)Piece.Pawn] >> 1) & piecesBB[(int)Side.Black][(int)Piece.Pawn] & FILES[x1 ] & RANKS[4]) << 8;

                // we know there is only going to be one 
                if (PAWN_MOVES > 0) {
                    currentIndex = BitOperations.TrailingZeroCount(PAWN_MOVES);
                    // so our destination is the currIndex ; do calcs 
                    destFile = (currentIndex % 8) ;
                    startFile = x2 + 1;

                    moveList += "" + fileNames[startFile] + "" + fileNames[destFile] + "EE"; // to make 4 total characters ; y's can be inferred bc always 5->6 for wps 

                    mask = ~(1UL << currentIndex);
                    PAWN_MOVES &= mask;
                }

            }



        }
        return moveList;




    }


    private static string possiblePawnBlack(string history, ulong[][] piecesBB, ulong[] sideBB, ulong nonCaptureBB, ulong captureBB, ulong emptyBB)
    {
        string moveList = "";
        // capture right ; current pawn can't be on rank 1 and result must be capturable and can't be on file a 
        PAWN_MOVES = ((piecesBB[(int)Side.Black][(int)Piece.Pawn] & ~RANKS[0]) >> 7) & (captureBB & ~FILES[0]);

        // now if a bit is on in that bb convert into move notation
        //x1,y1,x2,y2 
        int currentIndex;
        int x1, y1, x2, y2;
        ulong mask;

        while (PAWN_MOVES > 0)
        {
            currentIndex = BitOperations.TrailingZeroCount(PAWN_MOVES);
            // so our destination is the currIndex ; do calcs 
            y2 = (currentIndex / 8) + 1; x2 = (currentIndex % 8);
            y1 = y2 + 1; x1 = x2 - 1; // prev row and col respectively  

            moveList += "" + fileNames[x1] + "" + y1 + "" + fileNames[x2] + "" + y2;
            mask = ~(1UL << currentIndex);
            PAWN_MOVES &= mask;
        }

        // left capture 
        PAWN_MOVES = ((piecesBB[(int)Side.Black][(int)Piece.Pawn] & ~RANKS[0]) >> 9) & (captureBB & ~FILES[7]);


        while (PAWN_MOVES > 0)
        {
            currentIndex = BitOperations.TrailingZeroCount(PAWN_MOVES);
            // so our destination is the currIndex ; do calcs 
            y2 = (currentIndex / 8) + 1; x2 = (currentIndex % 8);
            y1 = y2 + 1; x1 = x2 + 1; // prev row and col respectively  

            moveList += "" + fileNames[x1] + "" + y1 + "" + fileNames[x2] + "" + y2;
            mask = ~(1UL << currentIndex);
            PAWN_MOVES &= mask;
        }

        // push pawn 1 ; that spot has to be empty
        PAWN_MOVES = ((piecesBB[(int)Side.Black][(int)Piece.Pawn] & ~RANKS[0]) >> 8) & emptyBB;

        while (PAWN_MOVES > 0)
        {
            currentIndex = BitOperations.TrailingZeroCount(PAWN_MOVES);
            // so our destination is the currIndex ; do calcs 
            y2 = (currentIndex / 8) + 1; x2 = (currentIndex % 8);
            y1 = y2 + 1; x1 = x2; // prev row and col respectively  

            moveList += "" + fileNames[x1] + "" + y1 + "" + fileNames[x2] + "" + y2;
            mask = ~(1UL << currentIndex);
            PAWN_MOVES &= mask;
        }





        //push pawn 2 ; both spot in front and destination has to be empty ; destination has to be on rank 5
        PAWN_MOVES = (piecesBB[(int)Side.Black][(int)Piece.Pawn] >> 16) & RANKS[4] & emptyBB & (emptyBB >> 8);

        while (PAWN_MOVES > 0)
        {
            currentIndex = BitOperations.TrailingZeroCount(PAWN_MOVES);
            // so our destination is the currIndex ; do calcs 
            y2 = (currentIndex / 8) + 1; x2 = (currentIndex % 8);
            y1 = y2 + 2; x1 = x2; // prev row and col respectively  

            moveList += "" + fileNames[x1] + "" + y1 + "" + fileNames[x2] + "" + y2;
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
            // so our destination is the currIndex ; do calcs 
            x2 = (currentIndex % 8);
            x1 = x2 - 1; // prev row and col respectively  

            moveList += "" + fileNames[x1] + "" + fileNames[x2] + "QP" + fileNames[x1] + "" + fileNames[x2] + "RP" + fileNames[x1] + "" + fileNames[x2] + "BP" + fileNames[x1] + "" + fileNames[x2] + "NP";
            mask = ~(1UL << currentIndex);
            PAWN_MOVES &= mask;
        }


        // capture left promo 
        PAWN_MOVES = (piecesBB[(int)Side.Black][(int)Piece.Pawn] >> 9) & captureBB & RANKS[0] & (~FILES[7]);

        while (PAWN_MOVES > 0)
        {
            currentIndex = BitOperations.TrailingZeroCount(PAWN_MOVES);
            // so our destination is the currIndex ; do calcs 
            x2 = (currentIndex % 8);
            x1 = x2 + 1; // prev row and col respectively  

            moveList += "" + fileNames[x1] + "" + fileNames[x2] + "QP" + fileNames[x1] + "" + fileNames[x2] + "RP" + fileNames[x1] + "" + fileNames[x2] + "BP" + fileNames[x1] + "" + fileNames[x2] + "NP";
            mask = ~(1UL << currentIndex);
            PAWN_MOVES &= mask;
        }


        // push 1 promo 
        PAWN_MOVES = (piecesBB[(int)Side.Black][(int)Piece.Pawn] >> 8) & emptyBB & RANKS[0];

        // extract valid promos 
        while (PAWN_MOVES > 0)
        {
            currentIndex = BitOperations.TrailingZeroCount(PAWN_MOVES);
            // so our destination is the currIndex ; do calcs 
            x2 = (currentIndex % 8);
            x1 = x2; // prev row and col respectively  

            moveList += "" + fileNames[x1] + "" + fileNames[x2] + "QP" + fileNames[x1] + "" + fileNames[x2] + "RP" + fileNames[x1] + "" + fileNames[x2] + "BP" + fileNames[x1] + "" + fileNames[x2] + "NP";
            mask = ~(1UL << currentIndex);
            PAWN_MOVES &= mask;
        }


        //EN PASSANT 

        //history has to at least have a move and history's last move has to be a valid black pawn two move 
        if (history.Length >= 4)
        {

            //check two move validity 
            // if the files are letters, then make correct subtraction to get index value 
            y2 = history[history.Length - 1] - '0';
            x2 = (Char.IsLetter(history[history.Length - 2])) ? history[history.Length - 2]-'a' : history[history.Length - 2] - '0';
            y1 = history[history.Length - 3] - '0';
            x1 = (Char.IsLetter(history[history.Length - 4])) ? history[history.Length - 4] - 'a' : history[history.Length - 4] - '0';

            // x1 has to equal x2 and y1 has to be 2 greater than y2 (black pawn moving two down), y2 has to equal rank 5 
            if (x1 == x2 && y2 == 4 && y1 == y2 - 2)
            {

                // right capture 
                // bp has to be left of wp that just moved, they both have to be on rank 4, move bp to space below 
                PAWN_MOVES = ((piecesBB[(int)Side.Black][(int)Piece.Pawn] << 1) & piecesBB[(int)Side.White][(int)Piece.Pawn] & FILES[x1] & RANKS[y2-1]) >> 8;
                int startFile, destFile; 
                // we know there is only going to be one 
                if (PAWN_MOVES > 0)
                {
                    currentIndex = BitOperations.TrailingZeroCount(PAWN_MOVES);
                    // so our destination is the currIndex ; do calcs 
                    destFile = (currentIndex % 8);
                    startFile = destFile - 1;

                    moveList += "" + fileNames[startFile] + "" + fileNames[destFile] + "EE"; // to make 4 total characters ; y's can be inferred bc always rank 4->3 for bps

                    mask = ~(1UL << currentIndex);
                    PAWN_MOVES &= mask;
                }

                //left capture 

                PAWN_MOVES = ((piecesBB[(int)Side.Black][(int)Piece.Pawn] >> 1) & piecesBB[(int)Side.White][(int)Piece.Pawn] & FILES[x1] & RANKS[y2 - 1]) >> 8;

                // we know there is only going to be one 
                if (PAWN_MOVES > 0)
                {
                    currentIndex = BitOperations.TrailingZeroCount(PAWN_MOVES);
                    // so our destination is the currIndex ; do calcs 
                    destFile = (currentIndex % 8);
                    startFile = destFile + 1;

                    moveList += "" + fileNames[startFile] + "" + fileNames[destFile] + "EE"; // to make 4 total characters ; y's can be inferred bc always rank 4->3 for bps

                    mask = ~(1UL << currentIndex);
                    PAWN_MOVES &= mask;
                }

            }



        }
        return moveList;




    }
    private static string possibleRook( ulong[][] piecesBB, ulong[] sideBB, ulong nonCaptureBB, ulong captureBB, ulong emptyBB , Side side) {
        string moveList = "";

        // iterate through all the rooks 
        ulong rookBB = piecesBB[(int)side][(int)Piece.Rook]; 

        while(rookBB > 0 ) {// for every rook 
            int square = BitOperations.TrailingZeroCount(rookBB);


            // get sliding moves 
            //first or all piece boards then remove the current space so we can get blocker board 
            ROOK_MOVES = (~emptyBB) & ~(1UL << square) ; // get blocker board 

            ROOK_MOVES = getRookMoves(ROOK_MOVES, square); // get possible rook moves from function 

            ROOK_MOVES &= (captureBB | emptyBB); // make sure that moves are only on capturable pieces and empty spaces by anding 


            // parse moves from ROOK MOVES 
            while(ROOK_MOVES > 0) {
                int index = BitOperations.TrailingZeroCount(ROOK_MOVES);

                //ex: a1b1
                moveList += (fileNames[square % 8] + "" + (square / 8 + 1)) + (fileNames[index % 8] + "" + (index / 8 + 1));
                ROOK_MOVES &= ~(1UL << index); 
            }
            // turn off the current index
            rookBB &= ~(1UL<<square);
        }
        return moveList; 
    }


    /// <summary>
    /// returns rook moves as a ulong 
    /// </summary>
    /// <returns></returns>
    private static ulong getRookMoves(ulong blockerConfig , int square ) {
        int rookKey = (int)SlidingMoves.getMagicIndex(SlidingMoves.RookInfoTable[square], blockerConfig);

        return SlidingMoves.RookMoveHashTable[square][rookKey];
    }


    private static string possibleBishop(ulong[][] piecesBB, ulong[] sideBB, ulong nonCaptureBB, ulong captureBB, ulong emptyBB, Side side) {
        string moveList = "";

        // iterate through all the bishops 
        ulong bishopBB = piecesBB[(int)side][(int)Piece.Bishop];

        while (bishopBB > 0) {// for every rook 
            int square = BitOperations.TrailingZeroCount(bishopBB);


            // get sliding moves 
            //first or all piece boards then remove the current space so we can get blocker board 
            BISHOP_MOVES = (~emptyBB) & ~(1UL << square); // get blocker board 

            BISHOP_MOVES = getBishopMoves(BISHOP_MOVES, square); // get possible bishop moves from function 

            BISHOP_MOVES &= (captureBB | emptyBB); // make sure that moves are only on capturable pieces and empty spaces by anding 


            // parse moves from ROOK MOVES 
            while (BISHOP_MOVES > 0) {
                int index = BitOperations.TrailingZeroCount(BISHOP_MOVES);

                //ex: a1b1
                moveList += (fileNames[square % 8] + "" + (square / 8 + 1)) + (fileNames[index % 8] + "" + (index / 8 + 1));
                BISHOP_MOVES &= ~(1UL << index);
            }
            // turn off the current index
            bishopBB &= ~(1UL << square);
        }
        return moveList;
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
    private static string possibleQueen(ulong[][] piecesBB, ulong[] sideBB, ulong nonCaptureBB, ulong captureBB, ulong emptyBB, Side side) {
        string moveList = "";

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


            // parse moves from QUEEN MOVES 
            while (QUEEN_MOVES > 0) {
                int index = BitOperations.TrailingZeroCount(QUEEN_MOVES);

                //ex: a1b1
                moveList += (fileNames[square % 8] + "" + (square / 8 + 1)) + (fileNames[index % 8] + "" + (index / 8 + 1));
                QUEEN_MOVES &= ~(1UL << index);
            }
            // turn off the current index
            queenBB &= ~(1UL << square);
        }
        return moveList; 
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

    private static string possibleKnight(ulong[][] piecesBB, ulong[] sideBB, ulong nonCaptureBB, ulong captureBB, ulong emptyBB, Side side) {
        string moveList = "";

        

        // make sure the move is either on empty or capturable square 

        KNIGHT_MOVES = northEastEast(piecesBB[(int)side][(int)Piece.Knight]) & (captureBB |emptyBB); // north east east 

        // parse moves for current moveset 
        while (KNIGHT_MOVES > 0) {
            int index = BitOperations.TrailingZeroCount(KNIGHT_MOVES);

            int destRank = index / 8; int destFile = index % 8; 
            int startRank = destRank-1 , startFile=destFile-2; 
            //ex: a1b1
            moveList += (fileNames[startFile] + "" + (startRank + 1)) + (fileNames[destFile] + "" + (destRank+ 1));
            KNIGHT_MOVES &= ~(1UL << index);
        }

        // make sure the move is either on empty or capturable square 

        KNIGHT_MOVES = northNorthEast(piecesBB[(int)side][(int)Piece.Knight]) & (captureBB | emptyBB); 

        // parse moves for current moveset 
        while (KNIGHT_MOVES > 0) {
            int index = BitOperations.TrailingZeroCount(KNIGHT_MOVES);

            int destRank = index / 8; int destFile = index % 8;
            int startRank = destRank - 2, startFile = destFile - 1;
            //ex: a1b1
            moveList += (fileNames[startFile] + "" + (startRank + 1)) + (fileNames[destFile] + "" + (destRank + 1));
            KNIGHT_MOVES &= ~(1UL << index);
        }

        KNIGHT_MOVES = northWestWest(piecesBB[(int)side][(int)Piece.Knight]) & (captureBB | emptyBB);

        // parse moves for current moveset 
        while (KNIGHT_MOVES > 0) {
            int index = BitOperations.TrailingZeroCount(KNIGHT_MOVES);

            int destRank = index / 8; int destFile = index % 8;
            int startRank = destRank - 1, startFile = destFile + 2;
            //ex: a1b1
            moveList += (fileNames[startFile] + "" + (startRank + 1)) + (fileNames[destFile] + "" + (destRank + 1));
            KNIGHT_MOVES &= ~(1UL << index);
        }

        KNIGHT_MOVES = northNorthWest(piecesBB[(int)side][(int)Piece.Knight]) & (captureBB | emptyBB);

        // parse moves for current moveset 
        while (KNIGHT_MOVES > 0) {
            int index = BitOperations.TrailingZeroCount(KNIGHT_MOVES);

            int destRank = index / 8; int destFile = index % 8;
            int startRank = destRank - 2, startFile = destFile + 1;
            //ex: a1b1
            moveList += (fileNames[startFile] + "" + (startRank + 1)) + (fileNames[destFile] + "" + (destRank + 1));
            KNIGHT_MOVES &= ~(1UL << index);
        }

        KNIGHT_MOVES = southEastEast(piecesBB[(int)side][(int)Piece.Knight]) & (captureBB | emptyBB);

        // parse moves for current moveset 
        while (KNIGHT_MOVES > 0) {
            int index = BitOperations.TrailingZeroCount(KNIGHT_MOVES);

            int destRank = index / 8; int destFile = index % 8;
            int startRank = destRank + 1, startFile = destFile -2;
            //ex: a1b1
            moveList += (fileNames[startFile] + "" + (startRank + 1)) + (fileNames[destFile] + "" + (destRank + 1));
            KNIGHT_MOVES &= ~(1UL << index);
        }

        KNIGHT_MOVES = southSouthEast(piecesBB[(int)side][(int)Piece.Knight]) & (captureBB | emptyBB);

        // parse moves for current moveset 
        while (KNIGHT_MOVES > 0) {
            int index = BitOperations.TrailingZeroCount(KNIGHT_MOVES);

            int destRank = index / 8; int destFile = index % 8;
            int startRank = destRank + 2, startFile = destFile - 1;
            //ex: a1b1
            moveList += (fileNames[startFile] + "" + (startRank + 1)) + (fileNames[destFile] + "" + (destRank + 1));
            KNIGHT_MOVES &= ~(1UL << index);
        }

        KNIGHT_MOVES = southWestWest(piecesBB[(int)side][(int)Piece.Knight]) & (captureBB | emptyBB);

        // parse moves for current moveset 
        while (KNIGHT_MOVES > 0) {
            int index = BitOperations.TrailingZeroCount(KNIGHT_MOVES);

            int destRank = index / 8; int destFile = index % 8;
            int startRank = destRank + 1, startFile = destFile +2;
            //ex: a1b1
            moveList += (fileNames[startFile] + "" + (startRank + 1)) + (fileNames[destFile] + "" + (destRank + 1));
            KNIGHT_MOVES &= ~(1UL << index);
        }

        KNIGHT_MOVES = southSouthWest(piecesBB[(int)side][(int)Piece.Knight]) & (captureBB | emptyBB);

        // parse moves for current moveset 
        while (KNIGHT_MOVES > 0) {
            int index = BitOperations.TrailingZeroCount(KNIGHT_MOVES);

            int destRank = index / 8; int destFile = index % 8;
            int startRank = destRank + 2, startFile = destFile + 1;
            //ex: a1b1
            moveList += (fileNames[startFile] + "" + (startRank + 1)) + (fileNames[destFile] + "" + (destRank + 1));
            KNIGHT_MOVES &= ~(1UL << index);
        }
        return moveList; 
    }
    //KNIGHT MOVES 


    // there is always only one king for a side 
    // a king can only move to a place that won't put it into check 
    // kings can castle if the rook or king hasn't moved yet nd if the castling isn't blocked 
    private static string possibleKing(ulong[][] piecesBB, ulong[] sideBB, ulong nonCaptureBB, ulong captureBB, ulong emptyBB, Side side) {
        string moveList = "";

        // find the kings reg attack pattern
        ulong currentKing = piecesBB[(int)side][(int)Piece.King];

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
        ulong unsafeBB = getUnsafeSquares(piecesBB, sideBB, nonCaptureBB, captureBB, emptyBB, side);
        

        // then check that these moves don't put the king in check; king can only move where is safe for the king 
        KING_MOVES &= unsafeBB; 
        
        
        
        
        
        return moveList; 

    }

    //this would return the unsafe squares for this sides to help with king safe moves 
    private static ulong getUnsafeSquares(ulong[][] piecesBB, ulong[] sideBB, ulong nonCaptureBB, ulong captureBB, ulong emptyBB, Side side) {

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
            blockerBB = ~(emptyBB) & ~(1UL << square);
            unsafeBB |= getBishopMoves(blockerBB, square);// extract squares protected by bishop 


            slidingPieceBB &= ~(1UL << square);
        }

        // Black rooks 

        slidingPieceBB = piecesBB[opponent][(int)Piece.Rook];
        while (slidingPieceBB > 0)
        {
            int square = BitOperations.TrailingZeroCount(slidingPieceBB);

            blockerBB = ~(emptyBB) & ~(1UL << square);
            unsafeBB |= getRookMoves(blockerBB, square);


            slidingPieceBB &= ~(1UL << square);
        }
        // Black queens 

        slidingPieceBB = piecesBB[opponent][(int)Piece.Queen];
        while (slidingPieceBB > 0)
        {
            int square = BitOperations.TrailingZeroCount(slidingPieceBB);

            blockerBB = ~(emptyBB) & ~(1UL << square);
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

