﻿
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


    private static string possibleMovesBlack(string history, ulong[][] piecesBB, ulong[] sideBB) {
        throw new NotImplementedException();
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
            + possibleQueen(piecesBB, sideBB, nonCaptureBB, captureBB, emptyBB, Side.White) ;  

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
            y2 = history[history.Length - 1] - '0';
            x2 = history[history.Length - 2] - '0';
            y1 = history[history.Length - 3] - '0';
            x1 = history[history.Length - 4] - '0';

            // x1 has to equal x2 and y1 has to be 2 greater than y2 (black pawn moving two down), y2 has to equal rank 5 
            if (x1 == x2 && y2 == 5 && y1 == y2 + 2) {

                // right capture 
                // wp has to be left of bp that just moved, they both have to be on rank 5, move wp to space above 
                PAWN_MOVES = ((piecesBB[(int)Side.White][(int)Piece.Pawn] << 1) & piecesBB[(int)Side.Black][(int)Piece.Pawn] & FILES[x1 ] & RANKS[4]) << 8;

                // we know there is only going to be one 
                if (PAWN_MOVES > 0) {
                    currentIndex = BitOperations.TrailingZeroCount(PAWN_MOVES);
                    // so our destination is the currIndex ; do calcs 
                    x2 = (currentIndex % 8) ;
                    x1 = x2 - 1;

                    moveList += "" + fileNames[x1] + "" + fileNames[x2] + "EE"; // to make 4 total characters ; y's can be inferred bc always 5->6 for wps 

                    mask = ~(1UL << currentIndex);
                    PAWN_MOVES &= mask;
                }

                //left capture 

                PAWN_MOVES = ((piecesBB[(int)Side.White][(int)Piece.Pawn] >> 1) & piecesBB[(int)Side.Black][(int)Piece.Pawn] & FILES[x1 ] & RANKS[4]) << 8;

                // we know there is only going to be one 
                if (PAWN_MOVES > 0) {
                    currentIndex = BitOperations.TrailingZeroCount(PAWN_MOVES);
                    // so our destination is the currIndex ; do calcs 
                    x2 = (currentIndex % 8) ;
                    x1 = x2 + 1;

                    moveList += "" + fileNames[x1] + "" + fileNames[x2] + "EE"; // to make 4 total characters ; y's can be inferred bc always 5->6 for wps 

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


            // parse moves from ROOK MOVES 
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
    ulong northWestWest(ulong bb) {
        return (bb << 6) & ~(FILES[6]|FILES[7]);  //shift left 6 and make sure result isn't on file g or h (wrap around) 
    }
    ulong northEastEast(ulong bb) {
        return (bb << 10) & ~(FILES[0] | FILES[1]); // make sure result not on a or b 
    }
    ulong northNorthWest(ulong bb) {
        return (bb << 15) & ~(FILES[7] ); // make sure result not on h
    }
    ulong northNorthEast(ulong bb) {
        return (bb << 17) & ~(FILES[0]); // make sure result not on h
    }
    ulong southEastEast (ulong bb) {
        return (bb >> 6) & ~(FILES[0] | FILES[1]);  
    }
    ulong southWestWest(ulong bb) {
        return (bb >> 10) & ~(FILES[6] | FILES[7]);
    }
    ulong southSouthEast(ulong bb) {
        return (bb >> 15) & ~(FILES[0]);
    }
    ulong southSouthWest(ulong bb) {
        return (bb >> 17) & ~(FILES[7]);
    }
    //KNIGHT MOVES 

}

