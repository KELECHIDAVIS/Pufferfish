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

    public static ulong[][][] randPieces = initRandPiecesArray(); // sides , pieces ,squares
    public static ulong[] randCastling = new ulong[NUM_CASTLING_PERMISSIONS]; 
    public static ulong[] randSides = new ulong[(int)Side.BOTH]; 
    public static ulong[] randEnPassant = new ulong[(int)Board.NUM_SQUARES +1 ]; // en passant rules are 65


    private static ulong[][][] initRandPiecesArray()
    {
        ulong[][][] temp = new ulong[(int) Side.BOTH][][] ;  

        // for each side 
        for(int i =0; i< (int ) Side.BOTH; i++)
        {
            
        }
        throw new NotImplementedException();
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
        for(int i =0; i< bbWhite.Length; i++)
        {
            Piece pieceType = (Piece)i; 
            ulong whitePieces = bbWhite[(int) pieceType]; //white bitBoard of type Piece type 
            ulong blackPieces = bbBlack[(int) pieceType]; // black bitBoard of type Piece type 

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
