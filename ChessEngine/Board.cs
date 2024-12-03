public enum Piece
{
    King,
    Queen,
    Rook,
    Bishop,
    Knight,
    Pawn,
    NONE,
};
public enum Side
{
    White,
    Black
}
class Board {
   

    const int NUM_PIECE_TYPES = 6;
    const int NUM_SIDES = 2;
    const int NUM_SQUARES = 64;

    public ulong[][] piecesBB = initPiecesBitBoard(); // row is color, col is piece type Ex: [white][king]
    public ulong[] sideBB = new ulong[NUM_SIDES]; // hold all pieces from a certain color Ex: [white] 
    
    //PIECE LIST IS LEAST SIG BIT TO MOST SIG; LSB->MSB; 0->63
    public int[] pieceList = new int[NUM_SQUARES]; // rep of the board that holds piece info so we can tell which piece is on which square

    /// <summary>
    /// Inits the pieces bitboard that holds piece bitboards like [side][piecetype]
    /// </summary>
    /// <returns> a 2d array of bitboards for all 12 color/piece types</returns>
    private static ulong[][] initPiecesBitBoard() {
        ulong[][] temp = new ulong[NUM_SIDES][]; 

        for(int i = 0; i< NUM_SIDES; i++) {
            temp[i] = new ulong[NUM_PIECE_TYPES];
            for(int j = 0; j< NUM_PIECE_TYPES; j++) {
                temp[i][j] = 0; 
            }
        }
        return temp; 
    }

    /// <summary>
    /// init standard chess ; allows for easier debugging and inputting certain boards 
    /// </summary>
    public void initStandardChess () {
        char[][] chessBoard = new char[][]{
            new char[] { 'r', 'n', 'b', 'q', 'k', 'b', 'n', 'r' },
            new char[] { 'p', 'p', 'p', 'p', 'p', 'p', 'p', 'p' },
            new char[] { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            new char[] { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            new char[] { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            new char[] { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            new char[] { 'P', 'P', 'P', 'P', 'P', 'P', 'P', 'P' },
            new char[] { 'R', 'N', 'B', 'Q', 'K', 'B', 'N', 'R' },
        };
        charArrayToBitboards(chessBoard, pieceList, piecesBB, sideBB);

    }
    

    /// <summary>
    /// Print inputted bitboard into 8x8 chess board 
    /// </summary>
    /// <param name="board"></param>
    public static void printBitBoard(ulong board) {
        const int LAST_BIT = 63; // helps with calcs 
        string rankString = ""; 

        for (int rank =0;rank<=7; rank++) {
            rankString = (8-rank) + " "; 
            for(int file= 7; file>=0; file--) {
                int currentBit = LAST_BIT - (rank * 8 + file);

                ulong mask = 1UL;
                mask <<= currentBit; // shift 1 to current bit 

                if((board & mask) > 0) { // check if there is a piece at the current piece 
                    rankString += "X ";
                } else { rankString += ". "; } 

            }
            Console.WriteLine(rankString); 
        }
        string fileNames = "abcdefgh";
        rankString = "  "; 
        for (int i = 0; i < 8; i++)
        {
            rankString+= fileNames[i] + " ";
        }
        Console.WriteLine(rankString);

    }

    /// <summary>
    /// Prints out piecelist that has all piece values into an 8x8 chess board form
    /// </summary>
    /// <param name="list"></param>
    public static void printPieceList(int[] list) {
        const int LAST_BIT = 63; 
        for (int rank = 0; rank <= 7; rank++)
        {
            for (int file = 7; file >= 0; file--)
            {
                int currentIndx = LAST_BIT - (rank * 8 +file );

                Console.Write(list[currentIndx]+" ");
            }
            Console.WriteLine();
        }
    }
    public static void charArrayToBitboards(char[][] chessBoard, int[] pieceList, ulong[][] piecesBB, ulong[] sidesBB) {
        const int LAST_BIT = 63; // helps with calcs 

        for(int i=0; i<=7; i++) {
           
            for(int j =0; j<=7; j++) {
                
                int pieceVal = (int) Piece.NONE; // current piece val ; 
                Side side;  // current side of piece 

                switch (Char.ToLower(chessBoard[i][j])) { // find val of piece 
                    case 'r': pieceVal = (int)Piece.Rook;  break;
                    case 'n': pieceVal = (int) Piece.Knight; break;
                    case 'b': pieceVal = (int) Piece.Bishop; break;
                    case 'q': pieceVal = (int) Piece.Queen; break;
                    case 'k': pieceVal = (int) Piece.King; break;
                    case 'p': pieceVal = (int) Piece.Pawn; break;
                }

                // from left -> right the indexes go 56 57 58... 63
                int currentIndex = LAST_BIT - (8*i + (7-j) ); // also where bit is going to be 

                pieceList[currentIndex] = pieceVal; // info about currentPiece

                if (pieceVal == (int)Piece.NONE) {

                    continue; // skip if there is no piece there 
                }

                

                side = Char.IsUpper(chessBoard[i][j]) ? Side.White : Side.Black; // if uppercase its white 
                ulong mask  = 1UL; // initialize mask 
                mask <<= currentIndex; // now shift the bit to currentPos


                piecesBB[(int)side][pieceVal] |= mask; // or the mask to bb to preserve data

                // or mask to side specific as well 
                sidesBB[(int)side] |= mask; 
            }
        }
    }

    /// <summary>
    /// Translates fen string to our board representation
    /// uppercase white, lowercase black, k king, q queen, b bishop, r rook, n knight, p pawn, / new line ,
    /// fen is read right to left, top to bottom 
    /// </summary>
    /// <param name="fen"> </param>
    public void FEN_TO_BB (string fen) {
     
    }



    
}