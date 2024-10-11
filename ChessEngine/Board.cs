public enum Piece
{
   King, 
   Queen, 
   Rook, 
   Bishop,
   Knight, 
   Pawn,
   NONE,
}
public enum  Side
{
    White,
    Black,
    BOTH,
}

// Rands for the zobrist tables 
// for each side , for each piece , on each square, generate a random number 
// these nums are stored here so that they can be used for lookup later
public class ZobristRandoms 
{
    public const int NUM_CASTLING_PERMISSIONS = 16;
    public const int RAND_SEED = 1; 

    private static ulong[][][] randPieces = initRandPiecesArray(); // sides , pieces ,squares
    private static ulong[] randCastling = initRandCastlingArray();
    private static ulong[] randSides = initRandSidesArray();
    private static ulong[] randEnPassant = initRandEnPassantArray();  // en passant rules are 65

    private static Random random = new Random(RAND_SEED);

    private static ulong[][][] initRandPiecesArray()
    {
        ulong[][][] temp = new ulong[(int) Side.BOTH][][] ;
        
        // for each side 
        for (int side =0; side < (int ) Side.BOTH; side ++)
        {
            temp[side]= new ulong[Board.NUM_PIECE_TYPES][]; 
            //for each piece
            for (int piece = 0; piece < Board.NUM_PIECE_TYPES; piece++)
            {
                temp[side][piece] = new ulong[Board.NUM_SQUARES]; 
               // for each square
               for(int square = 0; square < Board.NUM_SQUARES; square ++)
                {
                    temp[side][piece][square] = (ulong) random.NextInt64(); 
                }
            }
        }
        return temp; 
    }
    private static ulong[] initRandSidesArray()
    {
        ulong[] temp = new ulong[(int)Side.BOTH]; 
        for(int i = 0; i< (int)Side.BOTH; i++)
        {
            temp[i] = (ulong) random.NextInt64();
        }
        return temp; 
    }
    private static ulong[] initRandCastlingArray()
    {
        ulong[] temp = new ulong[NUM_CASTLING_PERMISSIONS];
        for (int i = 0; i < NUM_CASTLING_PERMISSIONS; i++)
        {
            temp[i] = (ulong)random.NextInt64();
        }
        return temp; 
    }
    private static ulong[] initRandEnPassantArray()
    {
        ulong[] temp = new ulong[Board.NUM_SQUARES +1];
        for (int i = 0; i < Board.NUM_SQUARES + 1; i++)
        {
            temp[i] = (ulong)random.NextInt64();
        }
        return temp; 
    }

    public static ulong piece (Side side , int Piece, int Square)
    {
        return randPieces[(int)side][Piece][Square]; 
    }

    public static ulong castling(int castlingPermission)
    {
        return randCastling[castlingPermission]; 
    }
    public static ulong side(int Side)
    {
        return randSides[Side];
    }
    public static ulong enPassant(int? ep)
    {
        if(ep != null)
            return randEnPassant[(int) ep];

        return randEnPassant[Board.NUM_SQUARES];
    }
}
class Board
{
    public const int NUM_SQUARES = 64; // number of squares on a chess board 
    public const int NUM_PIECE_TYPES = 6; // there are six distinct pieces in chess 

    // the row is the side , the columns is the type of piece 
    // The bb pieces carries all bit boards for pieces  ex: (white , king); row is side, col is piece 
    // bbside carries all piece info for that side: 0 is white , 1 is black 
    public ulong[][] bbPieces = initBitBoardPiecesArray(); // inits the jagged array
    public ulong[] bbSide = new ulong[(int) Side.BOTH];

    private static ulong[][] initBitBoardPiecesArray()
    {
        ulong[][] temp = new ulong[(int) Side.BOTH][];
        for(int i = 0; i< (int) Side.BOTH; i++)
        {
            temp[i] = new ulong[NUM_PIECE_TYPES] { 0UL, 0UL, 0UL, 0UL, 0UL, 0UL };  // 1 for each piece 

        }
        return temp;
    }

    
    public int[] initPieceList ()
    {
        ulong[] bbWhite = bbPieces[(int) Side.White];  // all 6 bitboards for white pieces 
        ulong[] bbBlack= bbPieces[(int)Side.Black];

        int[] pieceList = new int[NUM_SQUARES]; // 64 squares in a chess board

        for (int i = 0; i < NUM_SQUARES; i++)
        {
            pieceList[i] = (int) Piece.NONE;  // to represent theres nothing in that square 
        }

        // now for each piece put the corresponding piece type into the piece list 
        for(int pieceType = 0; pieceType < bbWhite.Length; pieceType++)
        {
            ulong whitePieces = bbWhite[pieceType]; //white bitBoard of type Piece type 
            ulong blackPieces = bbBlack[pieceType]; // black bitBoard of type Piece type 

            // now for each of this piece in the pieces bitboard put piece of that type in pieces list at position 
            // get the next bit that is turned on to find at what position the piece is located 
            int currIndex = 0; 
            while(whitePieces >0)
            {
                while((whitePieces & 1UL) == 0)
                {
                    currIndex++;
                    whitePieces >>= 1; 
                }
                // now set that current index to the piece type 
                pieceList[currIndex] = (int)pieceType;
                whitePieces >>= 1;
                currIndex++;

            }

            // get the next bit that is turned on to find at what position the piece is located 
            currIndex = 0;
            // now for each of this piece in the pieces bitboard put piece of that type in pieces list at position 
            while (blackPieces > 0)
            {
                while ((blackPieces & 1UL) == 0) // while were looking at a zero
                {
                    currIndex++;
                    blackPieces >>= 1;
                }
                // now set that current index to the piece type and go to next bit 
                pieceList[currIndex] = (int)pieceType;
                blackPieces >>= 1;
                currIndex++;
            }

        }

        return pieceList; 
    }

    public ulong initZobristKey ()
    {
        ulong key = 0UL;

        //get bitboards for both side 
        ulong[] bbWhite = bbPieces[(int) Side.White]; 
        ulong[] bbBlack = bbPieces[(int) Side.Black];

        // iterate through all piece types' bitboards , for both white and black ( like done in initializing the piece list) 
        for (int pieceType = 0; pieceType < bbWhite.Length; pieceType++)
        {
            ulong whitePieces = bbWhite[pieceType]; //white bitBoard of type Piece type 
            ulong blackPieces = bbBlack[pieceType]; // black bitBoard of type Piece type 

            // iterate through all locations of current piece type (like we did in last step) get that square then look up in our zobrist tables; 
            // then xor that value onto key 
            int currSquare = 0;
            while (whitePieces > 0)
            {
                while ((whitePieces & 1UL) == 0)
                {
                    currSquare++;
                    whitePieces >>= 1;
                }
                // now we have the square use that and all other info to get specific rand num from zorbist and xor it to key
                key ^= ZobristRandoms.piece(Side.White, pieceType, currSquare); 

                whitePieces >>= 1;
                currSquare++;

            }
            // do same for black 
            currSquare = 0;
            while (blackPieces > 0)
            {
                while ((blackPieces & 1UL) == 0)
                {
                    currSquare++;
                    blackPieces >>= 1;
                }
                // now we have the square use that and all other info to get specific rand num from zorbist and xor it to key
                key ^= ZobristRandoms.piece(Side.Black , pieceType , currSquare);

                blackPieces >>= 1;
                currSquare++;

            }
            
        }

        // hash castling, side to move, and en-passant permissions 
        //TBD
        /*key ^= ZobristRandoms.castling(GameState.castling);
        key ^= ZobristRandoms.side((int) GameState.activeColor);
        key ^= ZobristRandoms.enPassant(GameState.enPassant); */

        return key; 
    }
    // takes in a bit board and prints out its representation
    public void printBitBoard(ulong bb) 
    {
        const int LAST_BIT = 63; // we want to look at the 63rd bit 

        for (int rank = 0; rank < 8; rank++)
        {
            for (int file = 7; file >= 0; file--)
            {
                int amountShifted = (LAST_BIT - (rank * 8) - file);
                ulong mask = 1UL << amountShifted;

                // basically now we have to check if there is a bit at the current position (and) 
                char bit = (bb & mask) != 0 ? '1' : '0';

                Console.Write(bit + " ");
            }
            Console.WriteLine();
        }
    }

    //Returns the corresponding piece's bitboard which contains the postiion of all pieces of that certain type (white pawns for example) 
    public ulong getPieceBitBoard ( Side side , Piece piece)
    {
        return bbPieces[(int)side] [(int) piece];
    }

    //returns the bitboard for all pieces of a certain color ( all black pieces for ex) 
    public ulong getSideBitBoard (Side side)
    {
        return bbSide[(int) side]; 
    }
}
