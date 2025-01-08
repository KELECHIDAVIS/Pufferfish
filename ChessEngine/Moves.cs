
using System;
using System.ComponentModel;
using System.Numerics;
using System.Security;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

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

    public static readonly ulong[] DiagMasks = new ulong[] { 1, 258, 66052, 16909320, 4328785936, 1108169199648, 283691315109952, 72624976668147840, 145249953336295424, 290499906672525312, 580999813328273408, 1161999622361579520, 2323998145211531264, 4647714815446351872, 9223372036854775808, };
    public static readonly ulong[] AntiDiagMasks= new ulong[] { 128, 32832, 8405024, 2151686160, 550831656968, 141012904183812, 36099303471055874, 9241421688590303745, 4620710844295151872, 2310355422147575808, 1155177711073755136, 577588855528488960, 288794425616760832, 144396663052566528, 72057594037927936, };

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
    public static ulong ROOK_MOVES;
    public static ulong BISHOP_MOVES; 
    public static ulong QUEEN_MOVES;


    private static ulong[] KNIGHT_MOVES= new ulong[] { 132096, 329728, 659712, 1319424, 2638848, 5277696, 10489856, 4202496, 33816580, 84410376, 168886289, 337772578, 675545156, 1351090312, 2685403152, 1075839008, 8657044482, 21609056261, 43234889994, 86469779988, 172939559976, 345879119952, 687463207072, 275414786112, 2216203387392, 5531918402816, 11068131838464, 22136263676928, 44272527353856, 88545054707712, 175990581010432, 70506185244672, 567348067172352, 1416171111120896, 2833441750646784, 5666883501293568, 11333767002587136, 22667534005174272, 45053588738670592, 18049583422636032, 145241105196122112, 362539804446949376, 725361088165576704, 1450722176331153408, 2901444352662306816, 5802888705324613632, 11533718717099671552, 4620693356194824192, 288234782788157440, 576469569871282176, 1224997833292120064, 2449995666584240128, 4899991333168480256, 9799982666336960512, 1152939783987658752, 2305878468463689728, 1128098930098176, 2257297371824128, 4796069720358912, 9592139440717824, 19184278881435648, 38368557762871296, 4679521487814656, 9077567998918656, };
    private static ulong[] KING_MOVES = new ulong[] { 770, 1797, 3594, 7188, 14376, 28752, 57504, 49216, 197123, 460039, 920078, 1840156, 3680312, 7360624, 14721248, 12599488, 50463488, 117769984, 235539968, 471079936, 942159872, 1884319744, 3768639488, 3225468928, 12918652928, 30149115904, 60298231808, 120596463616, 241192927232, 482385854464, 964771708928, 825720045568, 3307175149568, 7718173671424, 15436347342848, 30872694685696, 61745389371392, 123490778742784, 246981557485568, 211384331665408, 846636838289408, 1975852459884544, 3951704919769088, 7903409839538176, 15806819679076352, 31613639358152704, 63227278716305408, 54114388906344448, 216739030602088448, 505818229730443264, 1011636459460886528, 2023272918921773056, 4046545837843546112, 8093091675687092224, 16186183351374184448, 13853283560024178688, 144959613005987840, 362258295026614272, 724516590053228544, 1449033180106457088, 2898066360212914176, 5796132720425828352, 11592265440851656704, 4665729213955833856, };
    /// <summary>
    /// Returns all possible moves for that side 

    public static List<Move>  possibleMoves(Side side, ulong[][] piecesBB, ulong[] sideBB, ulong EP, int castling) {
        if (side == Side.White) return possibleMovesWhite(piecesBB, sideBB, EP, castling);
        else {
            return possibleMovesBlack( piecesBB, sideBB, EP, castling);
        }
    }

    public static List<Move> possibleMoves ( Side side, Board board, ulong EP, int castling) {
        return possibleMoves(side , board.piecesBB, board.sideBB, EP,castling);
    }


    private static List<Move> possibleMovesBlack( ulong[][] piecesBB, ulong[] sideBB, ulong EP, int castling)
    {
        ulong nonCaptureBB = sideBB[(int)Side.Black] | piecesBB[(int) Side.White][(int) Piece.King];
        ulong captureBB = sideBB[(int)Side.White] ^ piecesBB[(int) Side.White] [(int) Piece.King];

        ulong emptyBB = ~(sideBB[(int)Side.Black] | sideBB[(int) Side.White]);

        List<Move> moveList = new List<Move>();
        // initially max val because when not in check pieces can capture or push to whereever they want 
        ulong captureMask, pushMask;  // for pieces that are checking our king these masks represent that paths that can be blocked and the checkers that should be captured 
        ulong pinningRays; // if a piece is pinned it can only move along the pinning rays
        
        var result = possibleKing(moveList, piecesBB, sideBB, nonCaptureBB, captureBB, emptyBB, Side.Black,castling);
        captureMask = result.captureMask; pushMask = result.pushMask; pinningRays = result.pinningRays; 

        possiblePawnBlack(moveList, piecesBB, sideBB, nonCaptureBB, captureBB, emptyBB, EP, captureMask, pushMask, pinningRays);
        possibleRook(moveList, piecesBB, sideBB, nonCaptureBB, captureBB, emptyBB, Side.Black, captureMask , pushMask, pinningRays);
        possibleBishop(moveList, piecesBB, sideBB, nonCaptureBB, captureBB, emptyBB, Side.Black, captureMask, pushMask, pinningRays);
        possibleQueen(moveList, piecesBB, sideBB, nonCaptureBB, captureBB, emptyBB, Side.Black, captureMask, pushMask, pinningRays); 
        possibleKnight(moveList, piecesBB, sideBB, nonCaptureBB, captureBB, emptyBB, Side.Black, captureMask, pushMask, pinningRays);

        return moveList; 
    }

    private static List<Move> possibleMovesWhite( ulong[][] piecesBB, ulong[] sideBB, ulong EP,int castling) {
        // Get all pieces white can and cannot capture 
        ulong nonCaptureBB = sideBB[(int)Side.White] | piecesBB[(int)Side.Black][(int)Piece.King]; // a bb that holds all white pieces and black king, because the player should never be able to cap. other king (illegal) 
        ulong captureBB = sideBB[(int)(Side.Black)] ^ piecesBB[(int)Side.Black][(int)Piece.King]; // every black piece except black king 

        // get all empty squares as well 
        ulong emptyBB = ~(sideBB[(int)(Side.White)] | sideBB[(int)Side.Black]); // bb of squares with no pieces on them 

        // get all the moves from each piece on this side 
        List<Move> moveList = new List<Move>();
        ulong captureMask, pushMask; // for pieces that are checking our king these masks represent that paths that can be blocked and the checkers that should be captured 
        ulong pinningRays; 

        var result = possibleKing(moveList, piecesBB, sideBB, nonCaptureBB, captureBB, emptyBB, Side.White, castling);
        captureMask = result.captureMask; pushMask = result.pushMask; pinningRays = result.pinningRays; 
        possiblePawnWhite(moveList, piecesBB, sideBB, nonCaptureBB, captureBB, emptyBB, EP , captureMask, pushMask, pinningRays);
        possibleRook(moveList, piecesBB, sideBB, nonCaptureBB, captureBB, emptyBB, Side.White, captureMask, pushMask, pinningRays);
        possibleBishop(moveList, piecesBB, sideBB, nonCaptureBB, captureBB, emptyBB, Side.White, captureMask, pushMask, pinningRays);
        possibleQueen(moveList, piecesBB, sideBB, nonCaptureBB, captureBB, emptyBB, Side.White, captureMask, pushMask, pinningRays);
        possibleKnight(moveList, piecesBB, sideBB, nonCaptureBB, captureBB, emptyBB, Side.White, captureMask, pushMask, pinningRays);

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
    private static void possiblePawnWhite(List<Move> moveList, ulong[][] piecesBB, ulong[] sideBB, ulong nonCaptureBB, ulong captureBB, ulong emptyBB, ulong EP, ulong captureMask , ulong pushMask, ulong pinningRays) {
        
        // capture right ;white pawn can't be on rank 7 because that'd be a promotion;  shift bits 9 to left ; make sure there is a caputarable piece there and make sure that piece is not on a file (left column wrap around)
        PAWN_MOVES = ((piecesBB[(int)Side.White][(int)Piece.Pawn] & ~RANKS[6]) << 9) & (captureBB & ~FILES[0]);
        PAWN_MOVES &= captureMask; // if there is a check on king should only be able to capture checking piece 

        // now if a bit is on in that bb convert into move notation
        //x1,y1,x2,y2 
        int currentIndex, origin;
        ulong mask;

        while (PAWN_MOVES > 0) {
            currentIndex = BitOperations.TrailingZeroCount(PAWN_MOVES);
            origin = currentIndex - 9;  // for capture right 
            if ((pinningRays & (1UL << origin)) != 0) { // piece is pinned ; only add move if it adheres to pin
                if ((pinningRays & (1UL << currentIndex)) != 0) { // if destination adheres to the pin ray then you can add the move 
                    moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.NONE, moveType = MoveType.CAPTURE });
                }
            } else { // piece is not pinned 
                moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.NONE, moveType = MoveType.CAPTURE });
            }
            mask = ~(1UL << currentIndex);
            PAWN_MOVES &= mask;
        }

        // left capture 
        //wp cant be on rank7; shift left 7; capturable piece has to be at destination and can't be on file h; 
        PAWN_MOVES = ((piecesBB[(int)Side.White][(int)Piece.Pawn] & ~RANKS[6]) << 7) & (captureBB & ~FILES[7]);
        PAWN_MOVES &= captureMask;

        while (PAWN_MOVES > 0) {
            currentIndex = BitOperations.TrailingZeroCount(PAWN_MOVES);
            origin = currentIndex -7;  // for capture left 
            if ((pinningRays & (1UL << origin)) != 0) { // piece is pinned ; only add move if it adheres to pin
                if ((pinningRays & (1UL << currentIndex)) != 0) { // if destination adheres to the pin ray then you can add the move 
                    moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.NONE, moveType = MoveType.CAPTURE });
                }
            } else { // piece is not pinned 
                moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.NONE, moveType = MoveType.CAPTURE });
            }
            mask = ~(1UL << currentIndex);
            PAWN_MOVES &= mask;
        }

        // push pawn 1 ; that spot has to be empty
        PAWN_MOVES = ((piecesBB[(int)Side.White][(int)Piece.Pawn] & ~RANKS[6]) << 8) & emptyBB;
        PAWN_MOVES &= pushMask;


        
        while (PAWN_MOVES > 0) {
            currentIndex = BitOperations.TrailingZeroCount(PAWN_MOVES);
            origin = currentIndex - 8;  // for push 1
            if ((pinningRays & (1UL << origin)) != 0) { // piece is pinned ; only add move if it adheres to pin
                if ((pinningRays & (1UL << currentIndex)) != 0) { // if destination adheres to the pin ray then you can add the move 
                    moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.NONE, moveType = MoveType.QUIET });
                }
            } else { // piece is not pinned 
                moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.NONE, moveType = MoveType.QUIET });
            }
            mask = ~(1UL << currentIndex);
            PAWN_MOVES &= mask;
        }

        //push pawn 2 ; both spot in front and destination has to be empty ; destination has to be on rank 4
        PAWN_MOVES = (piecesBB[(int)Side.White][(int)Piece.Pawn] << 16) & RANKS[3] & emptyBB & (emptyBB << 8);
        PAWN_MOVES &= pushMask;

        while (PAWN_MOVES > 0) {
            currentIndex = BitOperations.TrailingZeroCount(PAWN_MOVES);
            origin = currentIndex - 16;  // for push 2
            if ((pinningRays & (1UL << origin)) != 0) { // piece is pinned ; only add move if it adheres to pin
                if ((pinningRays & (1UL << currentIndex)) != 0) { // if destination adheres to the pin ray then you can add the move 
                    moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.NONE, moveType = MoveType.QUIET });
                }
            } else { // piece is not pinned 
                moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.NONE, moveType = MoveType.QUIET });
            }
            mask = ~(1UL << currentIndex);
            PAWN_MOVES &= mask;
        }

        //PROMOTIONS 


        // capture right promotion
        //destination has to be capturable, on rank 8, and can't be on file a (wrap around) 
        PAWN_MOVES = (piecesBB[(int)Side.White][(int)Piece.Pawn] << 9) & captureBB & RANKS[7] & (~FILES[0]);
        PAWN_MOVES &= captureMask; 

        // extract valid promotions 
        // in form of x1,x2,PromoType,'P'  ; Ex: 45QP: a pawn in col 4 captures right and promotes to queen

        while (PAWN_MOVES > 0) {
            currentIndex = BitOperations.TrailingZeroCount(PAWN_MOVES);
            origin = currentIndex - 9;  // for push 1
            if ((pinningRays & (1UL << origin)) != 0) { // piece is pinned ; only add move if it adheres to pin
                if ((pinningRays & (1UL << currentIndex)) != 0) { // if destination adheres to the pin ray then you can add the move 
                    moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.Queen, moveType = MoveType.CAPTURE });
                    moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.Rook, moveType = MoveType.CAPTURE });
                    moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.Bishop, moveType = MoveType.CAPTURE });
                    moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.Knight, moveType = MoveType.CAPTURE });
                }
            } else { // piece is not pinned 
                moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.Queen, moveType = MoveType.CAPTURE });
                moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.Rook, moveType = MoveType.CAPTURE });
                moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.Bishop, moveType = MoveType.CAPTURE });
                moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.Knight, moveType = MoveType.CAPTURE });
            }
            mask = ~(1UL << currentIndex);
            PAWN_MOVES &= mask;
        }


        // capture left promo 
        PAWN_MOVES = (piecesBB[(int)Side.White][(int)Piece.Pawn] << 7) & captureBB & RANKS[7] & (~FILES[7]);
        PAWN_MOVES &= captureMask;

        while (PAWN_MOVES > 0) {
            currentIndex = BitOperations.TrailingZeroCount(PAWN_MOVES);
            origin = currentIndex - 7;  // for push 1
            if ((pinningRays & (1UL << origin)) != 0) { // piece is pinned ; only add move if it adheres to pin
                if ((pinningRays & (1UL << currentIndex)) != 0) { // if destination adheres to the pin ray then you can add the move 
                    moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.Queen, moveType = MoveType.CAPTURE });
                    moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.Rook, moveType = MoveType.CAPTURE });
                    moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.Bishop, moveType = MoveType.CAPTURE });
                    moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.Knight, moveType = MoveType.CAPTURE });
                }
            } else { // piece is not pinned 
                moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.Queen, moveType = MoveType.CAPTURE });
                moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.Rook, moveType = MoveType.CAPTURE });
                moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.Bishop, moveType = MoveType.CAPTURE });
                moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.Knight, moveType = MoveType.CAPTURE });
            }
            mask = ~(1UL << currentIndex);
            PAWN_MOVES &= mask;
        }


        // push 1 promo 
        PAWN_MOVES = (piecesBB[(int)Side.White][(int)Piece.Pawn] << 8) & emptyBB & RANKS[7];
        PAWN_MOVES &= pushMask;

        // extract valid promos 
        while (PAWN_MOVES > 0) {
            currentIndex = BitOperations.TrailingZeroCount(PAWN_MOVES);
            origin = currentIndex - 9;  // for push 1
            if ((pinningRays & (1UL << origin)) != 0) { // piece is pinned ; only add move if it adheres to pin
                if ((pinningRays & (1UL << currentIndex)) != 0) { // if destination adheres to the pin ray then you can add the move 
                    moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.Queen, moveType = MoveType.QUIET });
                    moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.Rook, moveType = MoveType.QUIET });
                    moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.Bishop, moveType = MoveType.QUIET });
                    moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.Knight, moveType = MoveType.QUIET });
                }
            } else { // piece is not pinned 
                moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.Queen, moveType = MoveType.QUIET });
                moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.Rook, moveType = MoveType.QUIET });
                moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.Bishop, moveType = MoveType.QUIET });
                moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.Knight, moveType = MoveType.QUIET });
            }
            mask = ~(1UL << currentIndex);
            PAWN_MOVES &= mask;
        }


        //EN PASSANT 

        // right capture 
        // wp has to be left of bp, they both have to be on rank 5, can't wrap around to file a, and has to be on square where ep is possible 
        // gives the piece to remove, NOT THE DESTINATION 
        PAWN_MOVES = (piecesBB[(int)Side.White][(int)Piece.Pawn] << 1) & piecesBB[(int)Side.Black][(int)Piece.Pawn] & RANKS[4] & ~FILES[0] & EP;
        // should only add pawn move if pawnmoves and capturemask or pushbb and push mask 
        PAWN_MOVES = (PAWN_MOVES & captureMask) | (((PAWN_MOVES << 8) & pushMask) >> 8); // if the capture mask is on at that spot or the push is on on the destination add that move 

        int destination;
        // we know there is only going to be one 
        if (PAWN_MOVES > 0)
        {
            currentIndex = BitOperations.TrailingZeroCount(PAWN_MOVES);
            origin = currentIndex - 1;  // right en passant 
            destination = origin + 9;

            if ((pinningRays & (1UL << origin)) != 0) { // piece is pinned ; only add move if it adheres to pin
                if ((pinningRays & (1UL << destination)) != 0) { // if destination adheres to the pin ray then you can add the move
                    // now have to make sure this en passant move doesn't expose king to a discovered check 
                    //Make EP move on copy board 
                    Board board = new Board();
                    board.piecesBB = piecesBB;
                    board.sideBB = sideBB;
                    Move ep = new Move { origin = origin, destination = destination, promoPieceType = Piece.NONE, moveType = MoveType.ENPASSANT };
                    board.makeMove(ep);

                    //See if move put their king in check due to sliding pieces 
                    bool kingInCheck = isKingInCheckSliding(Side.White, board.piecesBB, board.sideBB);

                    // if not move is valid 
                    if (!kingInCheck)
                        moveList.Add(ep);
                }
            } else { // piece is not pinned 
                // now have to make sure this en passant move doesn't expose king to a discovered check 
                //Make EP move on copy board 
                Board board = new Board();
                board.piecesBB = piecesBB;
                board.sideBB = sideBB;
                Move ep = new Move { origin = origin, destination = destination, promoPieceType = Piece.NONE, moveType = MoveType.ENPASSANT };
                board.makeMove(ep);

                //See if move put their king in check due to sliding pieces 
                bool kingInCheck = isKingInCheckSliding(Side.White, board.piecesBB, board.sideBB);

                // if not move is valid 
                if (!kingInCheck)
                    moveList.Add(ep);
            }
            mask = ~(1UL << currentIndex);
            PAWN_MOVES &= mask;
        }

        //left capture 

        PAWN_MOVES = (piecesBB[(int)Side.White][(int)Piece.Pawn] >> 1) & piecesBB[(int)Side.Black][(int)Piece.Pawn] & RANKS[4] & ~FILES[7] & EP;
        PAWN_MOVES = (PAWN_MOVES & captureMask) | (((PAWN_MOVES << 8) & pushMask) >> 8);

        // we know there is only going to be one 
        if (PAWN_MOVES > 0)
        {
            currentIndex = BitOperations.TrailingZeroCount(PAWN_MOVES);
            origin = currentIndex + 1;   
            destination = origin + 7;
            if ((pinningRays & (1UL << origin)) != 0) { // piece is pinned ; only add move if it adheres to pin
                if ((pinningRays & (1UL << destination)) != 0) { // if destination adheres to the pin ray then you can add the move
                    // now have to make sure this en passant move doesn't expose king to a discovered check 
                    //Make EP move on copy board 
                    Board board = new Board();
                    board.piecesBB = piecesBB;
                    board.sideBB = sideBB;
                    Move ep = new Move { origin = origin, destination = destination, promoPieceType = Piece.NONE, moveType = MoveType.ENPASSANT };
                    board.makeMove(ep);

                    //See if move put their king in check due to sliding pieces 
                    bool kingInCheck = isKingInCheckSliding(Side.White, board.piecesBB, board.sideBB);

                    // if not move is valid 
                    if (!kingInCheck)
                        moveList.Add(ep);
                }
            } else { // piece is not pinned 
                // now have to make sure this en passant move doesn't expose king to a discovered check 
                //Make EP move on copy board 
                Board board = new Board();
                board.piecesBB = piecesBB;
                board.sideBB = sideBB;
                Move ep = new Move { origin = origin, destination = destination, promoPieceType = Piece.NONE, moveType = MoveType.ENPASSANT };
                board.makeMove(ep);

                //See if move put their king in check due to sliding pieces 
                bool kingInCheck = isKingInCheckSliding(Side.White, board.piecesBB, board.sideBB);

                // if not move is valid 
                if (!kingInCheck)
                    moveList.Add(ep);
            }
            mask = ~(1UL << currentIndex);
            PAWN_MOVES &= mask;
        }
        
    }

    // see if king is in check due to sliding pieces 
    private static bool isKingInCheckSliding(Side side, ulong[][] piecesBB, ulong[] sideBB) {
        // have the king pretend to be every other type of sliding piece, if it can see the same type from its perspective then it is in check due to sliding pieces 
        Side opp = (side == Side.White) ? Side.Black : Side.White;
        int originOfKing = BitOperations.TrailingZeroCount(piecesBB[(int)side][(int)Piece.King]);
        ulong attackers =0 ; 

        // sliding pieces 
        // blockers would be all pieces on the board removing the current square 
        ulong blockers = (sideBB[(int)side] | sideBB[(int)opp]) & ~(1UL << originOfKing);

        // bishop and rook 
       
        ulong rookMoves = (getRookMoves(blockers, originOfKing));
        ulong bishopMoves = (getBishopMoves(blockers, originOfKing));
        attackers |= (bishopMoves & piecesBB[(int)opp][(int)Piece.Bishop]);
        attackers |= (rookMoves & piecesBB[(int)opp][(int)Piece.Rook]);

        // queen just combine both then and with queens on board 
        attackers |= ((rookMoves | bishopMoves) & piecesBB[(int)opp][(int)Piece.Queen]);

        

        if (attackers > 0) // king is in check to a sliding piece
            return true; 
        return false; 
    }

    // captures have to be valid in the capture mask, all pushes have to be valid in the push mask 
    private static void possiblePawnBlack(List<Move> moveList, ulong[][] piecesBB, ulong[] sideBB, ulong nonCaptureBB, ulong captureBB, ulong emptyBB, ulong EP, ulong captureMask , ulong pushMask, ulong pinningRays)
    {
        
        // capture right ; current pawn can't be on rank 2 cus that just promo and result must be capturable and can't be on file a 
        PAWN_MOVES = ((piecesBB[(int)Side.Black][(int)Piece.Pawn] & ~RANKS[1]) >> 7) & (captureBB & ~FILES[0]);
        PAWN_MOVES &= captureMask; // capture has to be valid in capture mask in event of check (if no check all bits are set and nothing is changed 
        // now if a bit is on in that bb convert into move notation
        //x1,y1,x2,y2 
        int currentIndex;
        int origin, destination; 
        ulong mask;

        while (PAWN_MOVES > 0)
        {
            currentIndex = BitOperations.TrailingZeroCount(PAWN_MOVES);
            origin = currentIndex +7;  // for capture right 

            // check if pinned 
            if((pinningRays & (1UL<<origin) )!=0) { // piece is pinned ; only add move if it adheres to pin
                if((pinningRays & (1UL << currentIndex)) != 0) { // if destination adheres to the pin ray then you can add the move 
                    moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.NONE, moveType = MoveType.CAPTURE });
                }
            } else { // piece is not pinned 
                moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.NONE, moveType = MoveType.CAPTURE });
            }
            mask = ~(1UL << currentIndex);
            PAWN_MOVES &= mask;
        }

        // left capture 
        PAWN_MOVES = ((piecesBB[(int)Side.Black][(int)Piece.Pawn] & ~RANKS[1]) >> 9) & (captureBB & ~FILES[7]);
        PAWN_MOVES &= captureMask;

        while (PAWN_MOVES > 0)
        {
            currentIndex = BitOperations.TrailingZeroCount(PAWN_MOVES);
            origin = currentIndex + 9;
            // check if pinned 
            if ((pinningRays & (1UL << origin)) != 0) { // piece is pinned ; only add move if it adheres to pin
                if ((pinningRays & (1UL << currentIndex)) != 0) { // if destination adheres to the pin ray then you can add the move 
                    moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.NONE, moveType = MoveType.CAPTURE });
                }
            } else { // piece is not pinned 
                moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.NONE, moveType = MoveType.CAPTURE });
            }
            mask = ~(1UL << currentIndex);
            PAWN_MOVES &= mask;
        }

        // push pawn 1 ; that spot has to be empty
        PAWN_MOVES = ((piecesBB[(int)Side.Black][(int)Piece.Pawn] & ~RANKS[1]) >> 8) & emptyBB;
        PAWN_MOVES &= pushMask; 

        while (PAWN_MOVES > 0)
        {
            currentIndex = BitOperations.TrailingZeroCount(PAWN_MOVES);
            origin = currentIndex + 8;  //
                                        // check if pinned 
            if ((pinningRays & (1UL << origin)) != 0) { // piece is pinned ; only add move if it adheres to pin
                if ((pinningRays & (1UL << currentIndex)) != 0) { // if destination adheres to the pin ray then you can add the move 
                    moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.NONE, moveType = MoveType.QUIET });
                }
            } else { // piece is not pinned 
                moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.NONE, moveType = MoveType.QUIET });
            }
            mask = ~(1UL << currentIndex);
            PAWN_MOVES &= mask;
        }





        //push pawn 2 ; both spot in front and destination has to be empty ; destination has to be on rank 5
        PAWN_MOVES = (piecesBB[(int)Side.Black][(int)Piece.Pawn] >> 16) & RANKS[4] & emptyBB & (emptyBB >> 8);
        PAWN_MOVES &= pushMask; 

        while (PAWN_MOVES > 0)
        {
            currentIndex = BitOperations.TrailingZeroCount(PAWN_MOVES);
            origin = currentIndex + 16;  //
            if ((pinningRays & (1UL << origin)) != 0) { // piece is pinned ; only add move if it adheres to pin
                if ((pinningRays & (1UL << currentIndex)) != 0) { // if destination adheres to the pin ray then you can add the move 
                    moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.NONE, moveType = MoveType.QUIET });
                }
            } else { // piece is not pinned 
                moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.NONE, moveType = MoveType.QUIET });
            }
            mask = ~(1UL << currentIndex);
            PAWN_MOVES &= mask;
        }

        //PROMOTIONS 


        // capture right promotion
        //destination has to be capturable, on rank 1, and can't be on file a (wrap around) 
        PAWN_MOVES = (piecesBB[(int)Side.Black][(int)Piece.Pawn] >> 7) & captureBB & RANKS[0] & (~FILES[0]);
        PAWN_MOVES &= captureMask; 
        // extract valid promotions 
        // in form of x1,x2,PromoType,'P'  ; Ex: 45QP: a pawn in col 4 captures right and promotes to queen

        while (PAWN_MOVES > 0)
        {
            currentIndex = BitOperations.TrailingZeroCount(PAWN_MOVES);
            origin = currentIndex +7;  // for push 1
            if ((pinningRays & (1UL << origin)) != 0) { // piece is pinned ; only add move if it adheres to pin
                if ((pinningRays & (1UL << currentIndex)) != 0) { // if destination adheres to the pin ray then you can add the move 
                    moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.Queen, moveType = MoveType.CAPTURE });
                    moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.Rook, moveType = MoveType.CAPTURE });
                    moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.Bishop, moveType = MoveType.CAPTURE });
                    moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.Knight, moveType = MoveType.CAPTURE });
                }
            } else { // piece is not pinned 
                moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.Queen, moveType = MoveType.CAPTURE });
                moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.Rook, moveType = MoveType.CAPTURE });
                moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.Bishop, moveType = MoveType.CAPTURE });
                moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.Knight, moveType = MoveType.CAPTURE });
            }
            mask = ~(1UL << currentIndex);
            PAWN_MOVES &= mask;
        }


        // capture left promo 
        PAWN_MOVES = (piecesBB[(int)Side.Black][(int)Piece.Pawn] >> 9) & captureBB & RANKS[0] & (~FILES[7]);
        PAWN_MOVES &= captureMask;

        while (PAWN_MOVES > 0)
        {
            currentIndex = BitOperations.TrailingZeroCount(PAWN_MOVES);
            origin = currentIndex + 9;  // for push 1
            if ((pinningRays & (1UL << origin)) != 0) { // piece is pinned ; only add move if it adheres to pin
                if ((pinningRays & (1UL << currentIndex)) != 0) { // if destination adheres to the pin ray then you can add the move 
                    moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.Queen, moveType = MoveType.CAPTURE });
                    moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.Rook, moveType = MoveType.CAPTURE });
                    moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.Bishop, moveType = MoveType.CAPTURE });
                    moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.Knight, moveType = MoveType.CAPTURE });
                }
            } else { // piece is not pinned 
                moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.Queen, moveType = MoveType.CAPTURE });
                moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.Rook, moveType = MoveType.CAPTURE });
                moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.Bishop, moveType = MoveType.CAPTURE });
                moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.Knight, moveType = MoveType.CAPTURE });
            }
            mask = ~(1UL << currentIndex);
            PAWN_MOVES &= mask;
        }


        // push 1 promo 
        PAWN_MOVES = (piecesBB[(int)Side.Black][(int)Piece.Pawn] >> 8) & emptyBB & RANKS[0];
        PAWN_MOVES &= pushMask; 
        // extract valid promos 
        while (PAWN_MOVES > 0)
        {
            currentIndex = BitOperations.TrailingZeroCount(PAWN_MOVES);
            origin = currentIndex + 8;  // for push 1
            if ((pinningRays & (1UL << origin)) != 0) { // piece is pinned ; only add move if it adheres to pin
                if ((pinningRays & (1UL << currentIndex)) != 0) { // if destination adheres to the pin ray then you can add the move 
                    moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.Queen, moveType = MoveType.QUIET });
                    moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.Rook, moveType = MoveType.QUIET });
                    moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.Bishop, moveType = MoveType.QUIET });
                    moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.Knight, moveType = MoveType.QUIET });
                }
            } else { // piece is not pinned 
                moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.Queen, moveType = MoveType.QUIET });
                moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.Rook, moveType = MoveType.QUIET });
                moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.Bishop, moveType = MoveType.QUIET });
                moveList.Add(new Move { origin = origin, destination = currentIndex, promoPieceType = Piece.Knight, moveType = MoveType.QUIET });
            }
            mask = ~(1UL << currentIndex);
            PAWN_MOVES &= mask;
        }


        //EN PASSANT 

        // right capture 
        // bp has to be left of wp, both have to be on rank 4, can't wrap around to file a, has to be on a valid ep square 

        // give square of piece to remove NOT DESTINATION 
        // if check is present 
        // the piece to remove should be in capture mask if it is giving check since that piece is being captured 
        // or the destination can be in the push mask to block a check 


        PAWN_MOVES = (piecesBB[(int)Side.Black][(int)Piece.Pawn] << 1) & piecesBB[(int)Side.White][(int)Piece.Pawn] &  RANKS[3] & ~FILES[0] &EP;


        
        // should only add pawn move if pawnmoves and capturemask or pushbb and push mask 
        PAWN_MOVES = (PAWN_MOVES&captureMask)| (((PAWN_MOVES >> 8) & pushMask )<< 8); // if the capture mask is on at that spot or the push is on on the destination add that move 
        
        // we know there is only going to be one 
        if (PAWN_MOVES > 0)
        {
            currentIndex = BitOperations.TrailingZeroCount(PAWN_MOVES);
            origin = currentIndex - 1;  // right en passant 
            destination = origin - 7;
            if ((pinningRays & (1UL << origin)) != 0) { // piece is pinned ; only add move if it adheres to pin
                if ((pinningRays & (1UL << destination)) != 0) { // if destination adheres to the pin ray then you can add the move
                    // now have to make sure this en passant move doesn't expose king to a discovered check 
                    //Make EP move on copy board 
                    Board board = new Board();
                    board.piecesBB = piecesBB;
                    board.sideBB = sideBB;
                    Move ep = new Move { origin = origin, destination = destination, promoPieceType = Piece.NONE, moveType = MoveType.ENPASSANT };
                    board.makeMove(ep);

                    //See if move put their king in check due to sliding pieces 
                    bool kingInCheck = isKingInCheckSliding(Side.Black, board.piecesBB, board.sideBB);

                    // if not move is valid 
                    if (!kingInCheck)
                        moveList.Add(ep);
                }
            } else { // piece is not pinned 
                // now have to make sure this en passant move doesn't expose king to a discovered check 
                //Make EP move on copy board 
                Board board = new Board();
                board.piecesBB = piecesBB;
                board.sideBB = sideBB;
                Move ep = new Move { origin = origin, destination = destination, promoPieceType = Piece.NONE, moveType = MoveType.ENPASSANT };
                board.makeMove(ep);

                //See if move put their king in check due to sliding pieces 
                bool kingInCheck = isKingInCheckSliding(Side.Black, board.piecesBB, board.sideBB);

                // if not move is valid 
                if (!kingInCheck)
                    moveList.Add(ep);
            }
            mask = ~(1UL << currentIndex);
            PAWN_MOVES &= mask;
        }

        //left capture 

        PAWN_MOVES = (piecesBB[(int)Side.Black][(int)Piece.Pawn] >> 1) & piecesBB[(int)Side.White][(int)Piece.Pawn] & RANKS[3] & ~FILES[7] & EP;

        PAWN_MOVES = (PAWN_MOVES & captureMask) | (((PAWN_MOVES >> 8) & pushMask) << 8); 
        // we know there is only going to be one 
        if (PAWN_MOVES > 0)
        {
            currentIndex = BitOperations.TrailingZeroCount(PAWN_MOVES);
            origin = currentIndex + 1;  // right en passant 
            destination = origin - 9;
            if ((pinningRays & (1UL << origin)) != 0) { // piece is pinned ; only add move if it adheres to pin
                if ((pinningRays & (1UL << destination)) != 0) { // if destination adheres to the pin ray then you can add the move
                    // now have to make sure this en passant move doesn't expose king to a discovered check 
                    //Make EP move on copy board 
                    Board board = new Board();
                    board.piecesBB = piecesBB;
                    board.sideBB = sideBB;
                    Move ep = new Move { origin = origin, destination = destination, promoPieceType = Piece.NONE, moveType = MoveType.ENPASSANT };
                    board.makeMove(ep);

                    
                    //See if move put their king in check due to sliding pieces 
                    bool kingInCheck = isKingInCheckSliding(Side.Black, board.piecesBB, board.sideBB);

                    // if not move is valid 
                    if (!kingInCheck)
                        moveList.Add(ep);
                }
            } else { // piece is not pinned 
                // now have to make sure this en passant move doesn't expose king to a discovered check 
                //Make EP move on copy board 
                Board board = new Board();
                board.piecesBB = piecesBB;
                board.sideBB = sideBB;
                Move ep = new Move { origin = origin, destination = destination, promoPieceType = Piece.NONE, moveType = MoveType.ENPASSANT };
                board.makeMove(ep);

                
                //See if move put their king in check due to sliding pieces 
                bool kingInCheck = isKingInCheckSliding(Side.Black, board.piecesBB, board.sideBB);

                // if not move is valid 
                if (!kingInCheck)
                    moveList.Add(ep);
            }
            mask = ~(1UL << currentIndex);
            PAWN_MOVES &= mask;
        }




    }
    private static void possibleRook( List<Move> moveList, ulong[][] piecesBB, ulong[] sideBB, ulong nonCaptureBB, ulong captureBB, ulong emptyBB , Side side, ulong captureMask, ulong pushMask, ulong pinningRays ) {
   
        // iterate through all the rooks 
        ulong rookBB = piecesBB[(int)side][(int)Piece.Rook];
        ulong pinAdherence; 
        while (rookBB > 0 ) {// for every rook 
            int square = BitOperations.TrailingZeroCount(rookBB);

            // if this piece is pinned then it also has to adhere to the pinning rays 
             pinAdherence= ulong.MaxValue; // if not pinned should be able to move anywhere
            if((pinningRays & (1UL<<square)) != 0) { // piece is pinned 
                pinAdherence = pinningRays; // piece is pinned so has to adhere
            }

            // get sliding moves 
            //first or all piece boards then remove the current space so we can get blocker board 
            ROOK_MOVES = (~emptyBB) & ~(1UL << square) ; // get blocker board 

            ROOK_MOVES = getRookMoves(ROOK_MOVES, square); // get possible rook moves from function 

            ulong captures = ROOK_MOVES & captureBB& captureMask& pinAdherence; // a square that has a capturable piece is a capture 
            ulong pushes = ROOK_MOVES &emptyBB&  pushMask&pinAdherence;

            //for every capture 
            while(captures > 0) {
                int index = BitOperations.TrailingZeroCount(captures);
                moveList.Add(new Move { origin = square, destination = index, moveType = MoveType.CAPTURE, promoPieceType = Piece.NONE });
                captures &= ~(1UL << index);
            }
            //for every push 
            while (pushes  > 0) {
                int index = BitOperations.TrailingZeroCount(pushes);
                moveList.Add(new Move { origin = square, destination = index, moveType = MoveType.QUIET, promoPieceType = Piece.NONE });
                pushes &= ~(1UL << index);
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


    private static void possibleBishop(List<Move> moveList, ulong[][] piecesBB, ulong[] sideBB, ulong nonCaptureBB, ulong captureBB, ulong emptyBB, Side side,ulong captureMask, ulong pushMask, ulong pinningRays ) {
        

        // iterate through all the bishops 
        ulong bishopBB = piecesBB[(int)side][(int)Piece.Bishop];
        ulong pinAdherence; 
        while (bishopBB > 0) {// for every rook 
            int square = BitOperations.TrailingZeroCount(bishopBB);

            // if this piece is pinned then it also has to adhere to the pinning rays 
            pinAdherence = ulong.MaxValue; // if not pinned should be able to move anywhere
            if ((pinningRays & (1UL << square)) != 0) { // piece is pinned 
                pinAdherence = pinningRays; // piece is pinned so has to adhere
            }

            // get sliding moves 
            //first or all piece boards then remove the current space so we can get blocker board 
            BISHOP_MOVES = (~emptyBB) & ~(1UL << square); // get blocker board 

            BISHOP_MOVES = getBishopMoves(BISHOP_MOVES, square); // get possible bishop moves from function 
            ulong captures = BISHOP_MOVES & captureBB & captureMask& pinAdherence; // a square that has a capturable piece is a capture 
            ulong pushes = BISHOP_MOVES & emptyBB & pushMask & pinAdherence;

            //for every capture 
            while (captures > 0) {
                int index = BitOperations.TrailingZeroCount(captures);
                moveList.Add(new Move { origin = square, destination = index, moveType = MoveType.CAPTURE, promoPieceType = Piece.NONE });
                captures &= ~(1UL << index);
            }
            //for every push 
            while (pushes > 0) {
                int index = BitOperations.TrailingZeroCount(pushes);
                moveList.Add(new Move { origin = square, destination = index, moveType = MoveType.QUIET, promoPieceType = Piece.NONE });
                pushes &= ~(1UL << index);
            }
            // turn off the current index
            bishopBB &= ~(1UL << square);
        }
        
    }


    public static ulong getBishopMoves(ulong blockerConfig, int square) {
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
    private static void possibleQueen(List<Move> moveList, ulong[][] piecesBB, ulong[] sideBB, ulong nonCaptureBB, ulong captureBB, ulong emptyBB, Side side, ulong captureMask , ulong pushMask, ulong pinningRays) {

        // get all queen positions 
        ulong queenBB = piecesBB[(int)side][(int)Piece.Queen];
        ulong pinAdherence; 
        while (queenBB > 0) {// for every rook 
            int square = BitOperations.TrailingZeroCount(queenBB);

            // if this piece is pinned then it also has to adhere to the pinning rays 
            pinAdherence = ulong.MaxValue; // if not pinned should be able to move anywhere
            if ((pinningRays & (1UL << square)) != 0) { // piece is pinned 
                pinAdherence = pinningRays; // piece is pinned so has to adhere
            }
            // get sliding moves 
            //first or all piece boards then remove the current space so we can get blocker board 
            QUEEN_MOVES = (~emptyBB) & ~(1UL << square); // get blocker board 

            // or rook moves and bishop moves to get all possible queens moves 
            QUEEN_MOVES = getRookMoves(QUEEN_MOVES, square) | getBishopMoves(QUEEN_MOVES,square) ;

            ulong captures = QUEEN_MOVES & captureBB & captureMask& pinAdherence; // a square that has a capturable piece is a capture 
            ulong pushes = QUEEN_MOVES & emptyBB & pushMask&pinAdherence;

            //for every capture 
            while (captures > 0) {
                int index = BitOperations.TrailingZeroCount(captures);
                moveList.Add(new Move { origin = square, destination = index, moveType = MoveType.CAPTURE, promoPieceType = Piece.NONE });
                captures &= ~(1UL << index);
            }
            //for every push 
            while (pushes > 0) {
                int index = BitOperations.TrailingZeroCount(pushes);
                moveList.Add(new Move { origin = square, destination = index, moveType = MoveType.QUIET, promoPieceType = Piece.NONE });
                pushes &= ~(1UL << index);
            }
            // turn off the current index
            queenBB &= ~(1UL << square);
        }
    }

    //KNIGHT MOVES 
    static ulong northWestWest(ulong bb) {
        return (bb << 6) & ~(FILES[6] | FILES[7]);  //shift left 6 and make sure result isn't on file g or h (wrap around) 
    }
    static ulong northEastEast(ulong bb) {
        return (bb << 10) & ~(FILES[0] | FILES[1]); // make sure result not on a or b 
    }
    static ulong northNorthWest(ulong bb) {
        return (bb << 15) & ~(FILES[7]); // make sure result not on h
    }
    static ulong northNorthEast(ulong bb) {
        return (bb << 17) & ~(FILES[0]); // make sure result not on h
    }
    static ulong southEastEast(ulong bb) {
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

    private static void possibleKnight(List<Move> moveList, ulong[][] piecesBB, ulong[] sideBB, ulong nonCaptureBB, ulong captureBB, ulong emptyBB, Side side, ulong captureMask, ulong pushMask, ulong pinningRays) {
        // for every knight 
        // if knight pinned skip (can't move at all)
        // get pseudo legal knight moves 
        // captures and pushes have to be anded with respective masks 

        ulong knights = piecesBB[(int)side][(int)Piece.Knight];
        while (knights > 0 ) { // for every knight 
            int sq = BitOperations.TrailingZeroCount(knights); 
            ulong indexMask = (1UL<< sq);

            if((indexMask&pinningRays) ==0) {// if not pinned 
                ulong captures = KNIGHT_MOVES[sq] & captureBB & captureMask; 
                ulong pushes = KNIGHT_MOVES[sq] & emptyBB & pushMask;

                while (captures > 0) {
                    int dest = BitOperations.TrailingZeroCount(captures);
                    moveList.Add(new Move { origin = sq, destination = dest, moveType = MoveType.CAPTURE, promoPieceType = Piece.NONE });
                    captures ^= (1UL << dest); 
                }
                while (pushes > 0) {
                    int dest = BitOperations.TrailingZeroCount(pushes);
                    moveList.Add(new Move { origin = sq, destination = dest, moveType = MoveType.QUIET, promoPieceType = Piece.NONE });
                    pushes ^= (1UL << dest);
                }
            }

            knights ^= indexMask; 
        }

    }
    //KNIGHT MOVES 


    // there is always only one king for a side 
    // a king can only move to a place that won't put it into check 
    // kings can castle if the rook or king hasn't moved yet nd if the castling isn't blocked 
    // capture mask is for the pieces that are giving check 
    // push mask is the mask to push into to block check; both will be all squares initiall
    
    // since push mask and capture mask are primitives they could be returned as a tuple 
    private static (ulong captureMask, ulong pushMask, ulong pinningRays) possibleKing(List<Move> moveList, ulong[][] piecesBB, ulong[] sideBB, ulong nonCaptureBB, ulong captureBB, ulong emptyBB, Side side, int castling) {

        ulong captureMask = ulong.MaxValue, pushMask = ulong.MaxValue, pinningRays = 0; 
        
        // find the kings reg attack pattern
        ulong currentKing = piecesBB[(int)side][(int)Piece.King];
        int originOfKing= BitOperations.TrailingZeroCount(currentKing); // get current square of king; we know there's only ever one 
        ulong currentMoves = KING_MOVES[originOfKing]; // psuedo legal moves of the king at this square 

        // now we have to check that these moves are on empty or capturable squares
        currentMoves &= (emptyBB | captureBB);

        
        // a bb that holds all the square that are in sight of black pieces 
        ulong unsafeBB = getUnsafeSquares(piecesBB, sideBB, nonCaptureBB, captureBB, emptyBB, side, originOfKing);


        // then check that these moves don't put the king in check; king can only move where is safe for the king 
        currentMoves &= ~unsafeBB;


        // We have all the king's safe moves 
        //Now see if the king is already and check and if so return the moves that get hem out of check

        //Pretend the kings is every type of other pece on this square 
        // if king can see that piece type from it's sq then it is in check by that piece
        int opponent; 
        ulong attackers=0;
        ulong oppSlidingPiecesFromKing = 0;

        // add pawn moves from opponent perspective
        // can only check king from two diff positions 
        // if the king is white: if it can see a black pawn from up left or up right then a pawn is checking 
        // if black: then if it can see a white pawn from bottom right or bot left then it is checking 
        if (side == Side.White) {
            opponent = (int)Side.Black; 
            attackers |= (currentKing << 7) & piecesBB[opponent][(int)Piece.Pawn];   
            attackers |= (currentKing << 9) & piecesBB[opponent][(int)Piece.Pawn];
        } else {
            opponent = (int)Side.White;
            attackers |= (currentKing >> 7) & piecesBB[opponent][(int)Piece.Pawn];
            attackers |= (currentKing >> 9) & piecesBB[opponent][(int)Piece.Pawn];
        }
        
        // knight; and the knight moves bb from the king square and all the opponents knights to find which are checkin
        attackers |= KNIGHT_MOVES[originOfKing] & piecesBB[opponent][(int)Piece.Knight];


        // sliding pieces 
        // blockers would be all pieces on the board removing the current square 
        ulong blockers = (~emptyBB) & ~(1UL<<originOfKing);
        

        // bishop and rook 
        // get possible sliding moves then make sure they are on capturable pieces then make sure the correct piece can be seen 
        ulong rookMoves = (getRookMoves(blockers, originOfKing) );
        oppSlidingPiecesFromKing |= rookMoves;
        ulong bishopMoves = (getBishopMoves(blockers, originOfKing) );
        oppSlidingPiecesFromKing|= bishopMoves; // of use later 
        attackers |= (bishopMoves & piecesBB[opponent][(int) Piece.Bishop]); 
        attackers |= (rookMoves & piecesBB[opponent][(int) Piece.Rook]);

        // queen just combine both then and with queens on board 
        attackers |= ((rookMoves | bishopMoves) & piecesBB[opponent][(int)Piece.Queen]); 




        // now we have all the possible attackers

        // two possibilities : 
        int numCheckers = BitOperations.PopCount(attackers) ; 
        
        // more than one checker 
        MoveType captureOrNot;
        ulong indexMask;
       
        
        if(numCheckers >1) {
            // it is not possible to block or capture both attackers so just return the valid moves we already have 
            //set push and capture mask to 0 because the only thing that can save the king is a king move

            pushMask = 0;
            captureMask = 0;
            while (currentMoves > 0) {
                int index = BitOperations.TrailingZeroCount(currentMoves);
                indexMask = (1UL << index);
                moveList.Add(new Move { origin = originOfKing, destination = index, moveType = MoveType.EVASION, promoPieceType = Piece.NONE });
                currentMoves &= ~indexMask;
            }

        } else if(numCheckers ==1 ) {        // or single checker 
            // possible to capture this piece to prevent check or block it's path if it is a sliding piece 
            // set capture bb to attackers; if piece is a slider set the push bb to the spaces inbetween them and the king 
            captureMask = attackers;

            pushMask = 0; // in case of a non sliding checker 
            
            // THIS IS A SINGLE SLIDING CHECKER OPTIMIZE USING THE O^2R METHOD****
            if(((attackers & piecesBB[opponent][(int) Piece.Rook])>0) || ((attackers & piecesBB[opponent][(int)Piece.Bishop]) > 0) || ((attackers & piecesBB[opponent][(int)Piece.Queen]) > 0)) {
                // iteratively find the king; the max push length is 7 
                ulong north = attackers, south = attackers, east = attackers, west = attackers, nwest = attackers, swest = attackers, neast = attackers, seast = attackers;
                for (int i = 0; i < 7; i++) {
                    if (((north << 8) & currentKing) > 0) { // this  is the correct path 
                        pushMask = north ^ attackers; // attacker shouldn't be in 
                        break;
                    }
                    if (((south >> 8) & currentKing) > 0) { // this  is the correct path 
                        pushMask = south ^ attackers;
                        break;
                    }
                    if (((east << 1) & currentKing) > 0) { // this  is the correct path 
                        pushMask = east ^ attackers;
                        break;
                    }
                    if (((west >> 1) & currentKing) > 0) { // this  is the correct path 
                        pushMask = west ^ attackers;
                        break;
                    }
                    if (((nwest << 7) & currentKing) > 0) { // this  is the correct path 
                        pushMask = nwest ^ attackers;
                        break;
                    }
                    if (((neast << 9) & currentKing) > 0) { // this  is the correct path 
                        pushMask = neast ^ attackers;
                        break;
                    }
                    if (((swest >> 9) & currentKing) > 0) { // this  is the correct path 
                        pushMask = swest ^ attackers;
                        break;
                    }
                    if (((seast >> 7) & currentKing) > 0) { // this  is the correct path 
                        pushMask = seast ^ attackers;
                        break;
                    }
                    north = (north << 8) | north; south = (south >> 8) | south; east = (east << 1) | east; west = (west >> 1) | west; nwest = (nwest << 7) | nwest; neast = (neast << 9) | neast; swest = (swest >> 9) | swest; seast = (seast >> 7) | seast;
                }



            }
            // only valid king moves would also only be evasions 
            while (currentMoves > 0) {
                int index = BitOperations.TrailingZeroCount(currentMoves);
                indexMask = (1UL << index);
                moveList.Add(new Move { origin = originOfKing, destination = index, moveType = MoveType.EVASION, promoPieceType = Piece.NONE });
                currentMoves &= ~indexMask;
            }

        } else { // no checkers 

            while (currentMoves > 0) {
                int index = BitOperations.TrailingZeroCount(currentMoves);
                indexMask = (1UL << index);

                if ((indexMask & captureBB) > 0) {// something was captured 
                    captureOrNot = MoveType.CAPTURE;
                } else {
                    captureOrNot = MoveType.QUIET;
                }

                moveList.Add(new Move { origin = originOfKing, destination = index, moveType = captureOrNot, promoPieceType = Piece.NONE });
                currentMoves &= ~indexMask;
            }

            //CASTLING can only castle when not in check; 0000 no castling, 0001 white kingside ,0010 white queenside, 0100 black king, 1000 black queen 
            if(castling!= 0) {
                ulong kingInBetween = (side == Side.White) ? wKInBetween : bkInBetween; // the squares in between the rook and king 
                ulong queenInBetween = (side == Side.White) ? wQInBetween : bQInBetween;

                int kingDest = (side == Side.White) ? wKSideCastleDest : bKSideCastleDest; // destination on where you're castling 
                int queenDest = (side == Side.White) ? wQSideCastleDest : bQSideCastleDest;

                // and the in between squares with all pieces, if returns 0 there are no pieces in there
                bool kingSideIsVacant = (kingInBetween & ~emptyBB) == 0;
                bool queenSideIsVacant = (queenInBetween & ~emptyBB) == 0;

                // now all in between squares and the king squares have to be safe 
                // so and a combination of inbetween squares nd king bb with unsafe; if == 0 theres no overlap so the castle is safe 
                bool kingSideSafe = ((kingInBetween | currentKing) & unsafeBB) == 0;
                bool queenSideSafe = ((queenInBetween | currentKing) & unsafeBB) == 0;

                bool castleKingSide = false, castleQueenSide= false;
                if (side == Side.White) {
                    castleKingSide = (castling & 1)> 0 ;
                    castleQueenSide = (castling & (1 << 1)) > 0;
                } else {
                    castleKingSide = (castling & (1<<2)) > 0;
                    castleQueenSide = (castling & (1 << 3)) > 0;
                }
                // so now if the king has castling rights AND the spots are vacant AND they are all safe the king can castle on that side 
                if (castleKingSide && kingSideIsVacant && kingSideSafe)
                    moveList.Add(new Move { origin = originOfKing, destination = kingDest, moveType = MoveType.CASTLE, promoPieceType = Piece.NONE });
                if (castleQueenSide && queenSideIsVacant && queenSideSafe)
                    moveList.Add(new Move { origin = originOfKing, destination = queenDest, moveType = MoveType.CASTLE, promoPieceType = Piece.NONE });
            }

        }

        // PINNED PIECEs
        // To find calc in all directions : 
        // Moves from opponents sliding pcs 
        // Opponent sliding piece moves from kings posiition (king pretending to be every sliding piece) 
        // The overlap of these two will be all the pinned pieces 

        // Moves from opposing sliding pieces 
        ulong slidingPcs = 0;
        
        ulong rooks = piecesBB[(int)opponent][(int)Piece.Rook]; 
        while(rooks> 0) {// for every rook 
            int square = BitOperations.TrailingZeroCount(rooks);
            blockers = (~emptyBB) ^ (1UL << square); 
            slidingPcs |= getRookMoves(blockers, square);
            rooks ^= (1UL << square); 
        }
        

        ulong bishops = piecesBB[(int)opponent][(int)Piece.Bishop];
        while (bishops > 0) {// for every rook 
            int square = BitOperations.TrailingZeroCount(bishops);
            blockers = (~emptyBB) ^ (1UL << square);
            slidingPcs |= getBishopMoves(blockers, square);
            bishops ^= (1UL << square);
        }

        

        ulong queens = piecesBB[(int)opponent][(int)Piece.Queen];
        while (queens > 0) {// for every rook 
            int square = BitOperations.TrailingZeroCount(queens);
            blockers = (~emptyBB) ^ (1UL << square);
            slidingPcs |= getBishopMoves(blockers, square);
            slidingPcs |= getRookMoves(blockers, square);
            queens ^= (1UL << square);
        }

        
        // get opponent sliding pieces from kings perspective oppslidingmoves from king 

        // if there is a piece from the king's side that is seen by a opp sliding piece and sliding piece from king's perspective that piece is a pinned candidate 
        ulong pinnedPieces = oppSlidingPiecesFromKing & slidingPcs& sideBB[(int)side];

       

        // so now if there are any pinned pieces remove from board and develop pinning rays that that piece giving pin to king
        if (pinnedPieces != 0) {
            ulong removed = (~emptyBB) ^ pinnedPieces;
            // now if the king can see a sliding piece from it's position then it is pinned by that 
            ulong lineSliding = (piecesBB[opponent][(int)Piece.Rook] | piecesBB[opponent][(int)Piece.Queen]) & removed;
            ulong diagSliding = (piecesBB[opponent][(int)Piece.Bishop] | piecesBB[opponent][(int)Piece.Queen]) & removed;
            
            // to make sure we are not going through pieces 
            ulong nonSlidingBlockers = removed & (~lineSliding) & (~diagSliding); 
            ulong north = currentKing<<8, south = currentKing>>8, east = currentKing<<1, west = currentKing>>1, nwest = currentKing<<7, swest = currentKing>>9, neast = currentKing<<9, seast = currentKing>>7;
            int counter = 0; // to keep track of the directions; max it should go is 6; if 7 stop
            
            // only check cardinal directions if there are line sliding pieces
            if(lineSliding!= 0) {
                // should only go max of 7 times and if it wraps around stop
                // should also stop if you run into a nonsliding piece 
                while (counter < 7 && (north & RANKS[0]) == 0 && (north & nonSlidingBlockers) == 0) {
                    if ((north & lineSliding) != 0) { // ran into a valid sliding piece
                        pinningRays |= north; // add ray 
                        break;
                    }
                    north = (north << 8) | north; //iterate ray 
                    counter++;
                }
                counter = 0; // reset counter 
                while (counter < 7 && (south & RANKS[7]) == 0 && (south & nonSlidingBlockers) == 0) {
                    if ((south & lineSliding) != 0) { // ran into a valid sliding piece
                        pinningRays |= south; // add ray 
                        break;
                    }
                    south = (south >> 8) | south; //iterate ray 
                    counter++;
                }
                counter = 0; // reset counter 
                while (counter < 7 && (east & FILES[0]) == 0 && (east & nonSlidingBlockers) == 0) {
                    if ((east & lineSliding) != 0) { // ran into a valid sliding piece
                        pinningRays |= east; // add ray 
                        break;
                    }
                    east = (east << 1) | east; //iterate ray 
                    counter++;
                }
                counter = 0; // reset counter 
                while (counter < 7 && (west & FILES[7]) == 0 && (west & nonSlidingBlockers) == 0) {
                    if ((west & lineSliding) != 0) { // ran into a valid sliding piece
                        pinningRays |= west; // add ray 
                        break;
                    }
                    west = (west >> 1) | west; //iterate ray 
                    counter++;
                }
                counter = 0; // reset counter 

            }
            // only check cardinal directions if there are line sliding pieces
            if (diagSliding != 0) {
                // should only go max of 7 times and if it wraps around stop
                // should also stop if you run into a nonsliding piece 
                while (counter < 7 && (neast & (RANKS[0] | FILES[0])) == 0 && (neast & nonSlidingBlockers) == 0) {
                    if ((neast & diagSliding) != 0) { // ran into a valid sliding piece
                        pinningRays |= neast; // add ray 
                        break;
                    }
                    neast = (neast << 9) | neast; //iterate ray 
                    counter++;
                }
                counter = 0; // reset counter 
                while (counter < 7 && (nwest & (RANKS[0] | FILES[7])) == 0 && (nwest & nonSlidingBlockers) == 0) {
                    if ((nwest & diagSliding) != 0) { // ran into a valid sliding piece
                        pinningRays |= nwest; // add ray 
                        break;
                    }
                    nwest = (nwest << 7) | nwest; //iterate ray 
                    counter++;
                }
                counter = 0; // reset counter 
                while (counter < 7 && (seast & (RANKS[7] | FILES[0])) == 0 && (seast & nonSlidingBlockers) == 0) {
                    if ((seast & diagSliding) != 0) { // ran into a valid sliding piece
                        pinningRays |= seast; // add ray 
                        break;
                    }
                    seast = (seast >> 7) | seast; //iterate ray 
                    counter++;
                }
                counter = 0; // reset counter 
                while (counter < 7 && (swest & (RANKS[7] | FILES[7])) == 0 && (swest & nonSlidingBlockers) == 0) {
                    if ((swest & diagSliding) != 0) { // ran into a valid sliding piece
                        pinningRays |= swest; // add ray 
                        break;
                    }
                    swest = (swest >> 9) | swest; //iterate ray 
                    counter++;
                }
                counter = 0; // reset counter 

            }
        }
        return (captureMask, pushMask, pinningRays); 
    }

    private static int popCount(ulong attackers) {
        int setBits = 0; 
        while(attackers > 0) {
            int index = BitOperations.TrailingZeroCount(attackers);
            setBits++; 
            attackers &= ~(1UL << index);
        }
        return setBits; 
    }

    private static ulong getMovesAtSquareKnight(Square currSquare, Side side) {
        ulong knightBB = (1UL << (int)currSquare);
        return northEastEast(knightBB)
            | northNorthEast(knightBB)
            | northWestWest(knightBB)
            | northNorthWest(knightBB)
            | southEastEast(knightBB)
            | southSouthEast(knightBB)
            | southWestWest(knightBB)
            | southSouthWest(knightBB);

    }

    //this would return the unsafe squares for this sides to help with king safe moves 
    //*MIGHT HAVE TO CHECK IF PIECES ARE PINNED BEFORE COUNTING THEIR UNSAFE SQUARES
    private static ulong getUnsafeSquares(ulong[][] piecesBB, ulong[] sideBB, ulong nonCaptureBB, ulong captureBB, ulong emptyBB, Side side, int kingsCurrentSquare) {

        ulong unsafeBB = 0;
        // if we looking unsafe squares for the white side the opponent would be black 
        int opponent; 

        // find the squares protected by black squares 

        //for pawn moves 
        if (side == Side.White) {
            opponent = (int) Side.Black; 
            // get black pawns right and left attacks 
            unsafeBB |= (piecesBB[opponent][(int)Piece.Pawn]>>7) & ~FILES[0]; // black pawns right attack; make sure result not on file a  
            unsafeBB |= (piecesBB[opponent][(int)Piece.Pawn]>>9) & ~FILES[7];
        }
        else{
            opponent = (int)Side.White; 
            unsafeBB |= (piecesBB[opponent][(int)Piece.Pawn] << 9) & ~FILES[0]; // white pawns right attack; make sure result not on file a  
            unsafeBB |= (piecesBB[opponent][(int)Piece.Pawn] << 7) & ~FILES[7];
        }

        //check for knights all at the same time 
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
        ulong destMask; 

        //OPTIMIZE***
        //could use koggstone generators to optimize for multiple sliding pieces at a time 
        while (slidingPieceBB > 0)
        {
            int square = BitOperations.TrailingZeroCount(slidingPieceBB);
            destMask = (1UL << square); 
            // get bishop move from current spot; remove current bishop from blocker bb  
            // you want to also remove current side king from blocker so that it doesn't allow king to make illegal moves 
            blockerBB = ~(emptyBB | piecesBB[(int) side][(int)Piece.King]) & ~destMask;
            unsafeBB |= getBishopMoves(blockerBB, square);// extract squares protected by bishop 

            slidingPieceBB &= ~destMask;
        }

        // Black rooks 

        slidingPieceBB = piecesBB[opponent][(int)Piece.Rook];
        while (slidingPieceBB > 0)
        {
            int square = BitOperations.TrailingZeroCount(slidingPieceBB);
            destMask = (1UL << square);

            blockerBB = ~(emptyBB | piecesBB[(int)side][(int)Piece.King]) & ~destMask;
            unsafeBB |= getRookMoves(blockerBB, square);


            slidingPieceBB &= ~destMask;
        }
        // Black queens 

        slidingPieceBB = piecesBB[opponent][(int)Piece.Queen];
        while (slidingPieceBB > 0)
        {
            int square = BitOperations.TrailingZeroCount(slidingPieceBB);
            destMask = (1UL << square); 

            blockerBB = ~(emptyBB | piecesBB[(int)side][(int)Piece.King]) & ~destMask;
            unsafeBB |= getBishopMoves(blockerBB, square);
            unsafeBB |= getRookMoves(blockerBB, square);

            slidingPieceBB &= ~destMask;
        }

        // black kings attacks 
        int enemyKing = BitOperations.TrailingZeroCount(piecesBB[opponent][(int)Piece.King]);
        unsafeBB |= KING_MOVES[enemyKing]; 



        return unsafeBB; 
    }

    /*// to make move generation more efficient, pseudo legal knight moves are going to be looked up instead of calculated at runtime 
    public static void generateKnightMoveTable() {
        string result = "new ulong { ";
        ulong moveBB, indexMask; 
        // for each square 
        for (int i = 0; i< 64; i++) {
            // on every square make all the knight moves possible
            indexMask = (1UL << i);
            moveBB = northEastEast(indexMask) |
                northNorthEast(indexMask) |
                northWestWest(indexMask) |
                northNorthWest(indexMask) |
                southEastEast(indexMask) |
                southSouthEast(indexMask) |
                southWestWest(indexMask) |
                southSouthWest(indexMask);
            Board.printBitBoard(moveBB); 
            result += moveBB+", ";
        }
        result += "};"; 
        Console.WriteLine(result); 
    }
    public static void generateKingMoveTable() {
        string result = "new ulong[] { ";
        ulong moveBB, indexMask;
        // for each square 
        for (int i = 0; i < 64; i++) {
            // on every square make all the knight moves possible
            indexMask = (1UL << i);
            moveBB = indexMask << 8  & ~RANKS[0]|
                indexMask >> 8 & ~RANKS[7] |
                indexMask << 9 & ~(FILES[0] | RANKS[0]) |
                indexMask >> 9 & ~(FILES[7] | RANKS[7]) |
                indexMask << 7 & ~(FILES[7] | RANKS[0]) |
                indexMask >> 7 & ~(FILES[0] | RANKS[7]) |
                indexMask << 1 & ~FILES[0] |
                indexMask >> 1 & ~FILES[7]; 
            Board.printBitBoard(moveBB);
            result += moveBB + ", ";
        }
        result += "};";
        Console.WriteLine(result);
    }

*/
    // using hyperbola quitenssence can get the moves of a single sliding piece using the index, all pieces , and mask of the movement allowed (have to do per line : vertical and horizontal etc ) 
    static ulong hypQuint (int sq , ulong occ, ulong mask)
    {
        return (((mask & occ) - (1UL<<sq) * 2) ^
        reverseUlong(reverseUlong(mask & occ) - reverseUlong((1UL << sq)) * 2)) & mask;
    }

    public static ulong reverseUlong(ulong n)
    {
        n = (n >> 1) & 0x5555555555555555UL | (n & 0x5555555555555555UL) << 1;
        n = (n >> 2) & 0x3333333333333333UL | (n & 0x3333333333333333UL) << 2;
        n = (n >> 4) & 0x0F0F0F0F0F0F0F0FUL | (n & 0x0F0F0F0F0F0F0F0FUL) << 4;
        n = (n >> 8) & 0x00FF00FF00FF00FFUL | (n & 0x00FF00FF00FF00FFUL) << 8;
        n = (n >> 16) & 0x0000FFFF0000FFFFUL | (n & 0x0000FFFF0000FFFFUL) << 16;
        n = (n >> 32) | (n << 32);
        return n;
    }

    public static ulong getSingleRookMoves(int sq , ulong occ)
    {
        return hypQuint(sq, occ, RANKS[(sq/8)]) | hypQuint(sq, occ, FILES[(sq % 8)]) ;
    }
    public static ulong getSingleBishopMoves(int sq, ulong occ)
    {
        return hypQuint(sq, occ, DiagMasks[sq/8 +sq%8]) | hypQuint(sq, occ, AntiDiagMasks[sq/8 +7 -(sq%8)]);
    }
    public static void generateDiag()
    {
        string result = "new ulong[]{";
        ulong start; 
        // start from bot left 
        for (int i = 7; i >=0; i--)
        {
            ulong ray = 1UL << i;

            
            while ((ray & FILES[7]) == 0)
            {
                ray = ray | (ray << 9);

            }
            Board.printBitBoard(ray); 
            result += ray + ", ";
            

        }
        //now start from h2
        for (int i = 1; i < 8; i++)
        {
            // start at h2 then do same 
            ulong ray = 1UL<< ((8*i));
            
            while ((ray & RANKS[7])==0)
            {
                ray = ray | (ray << 9);
            }
            Board.printBitBoard(ray);
            result += ray + ", ";
        }
        result += "}; "; 
        Console.WriteLine(result);
    }
    
}

