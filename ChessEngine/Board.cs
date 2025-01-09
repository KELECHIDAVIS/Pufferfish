using System.Numerics;
using System.Xml.Schema;

enum Square
{
    A1, B1, C1, D1, E1, F1, G1, H1,
    A2, B2, C2, D2, E2, F2, G2, H2,
    A3, B3, C3, D3, E3, F3, G3, H3,
    A4, B4, C4, D4, E4, F4, G4, H4,
    A5, B5, C5, D5, E5, F5, G5, H5,
    A6, B6, C6, D6, E6, F6, G6, H6,
    A7, B7, C7, D7, E7, F7, G7, H7,
    A8, B8, C8, D8, E8, F8, G8, H8
};
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
public class Board {
   

    public static readonly int NUM_PIECE_TYPES = 6;
    public static readonly int NUM_SIDES = 2;
    public static readonly int NUM_SQUARES = 64;

    public ulong[][] piecesBB = initPiecesBitBoard(); // row is color, col is piece type Ex: [white][king]
    public ulong[] sideBB = new ulong[NUM_SIDES]; // hold all pieces from a certain color Ex: [white] 
    
    //PIECE LIST IS LEAST SIG BIT TO MOST SIG; LSB->MSB; 0->63
    public int[] pieceList = new int[NUM_SQUARES]; // rep of the board that holds piece info so we can tell which piece is on which square


    public GameState state;
    public List<GameState> gameHistory= new(); // list of states; all the states taken so far 

    // need zobrist randoms 
    static readonly ZobristRandoms zobristRandom= new();  // UNCOMMENT WHEN DONE DEBUGGING 
    

    

    /// <summary>
    /// Inits the pieces bitboard that holds piece bitboards like [side][piecetype]
    /// </summary>
    /// <returns> a 2d array of bitboards for all 12 color/piece types</returns>
    private static ulong[][] initPiecesBitBoard() {
        ulong[][] temp = new ulong[NUM_SIDES][]; 

        for(int i = 0; i< NUM_SIDES; i++) {
            temp[i] = new ulong[NUM_PIECE_TYPES];
            
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

        state.sideToMove = Side.White;
        state.castling = 15; // 1111: all castling rights 
        state.EP = 0;
        state.zobristKey = initZobristKey(); 
    }
    

    /// <summary>
    /// Print inputted bitboard into 8x8 chess board 
    /// </summary>
    /// <param name="board"></param>
    public static void printBitBoard(ulong board) {
        const int LAST_BIT = 63; // helps with calcs 
        string rankString = ""; 

        for (int rank =0;rank<=7; rank++) {
            rankString = (8 - rank) + " | ";
            for (int file= 7; file>=0; file--) {
                int currentBit = LAST_BIT - (rank * 8 + file);

                ulong mask = 1UL;
                mask <<= currentBit; // shift 1 to current bit 

                if((board & mask) > 0) { // check if there is a piece at the current piece 
                    rankString += "X ";
                } else { rankString += ". "; } 

            }
            Console.WriteLine(rankString); 
        }
        Console.WriteLine("    - - - - - - - - ");
        string fileNames = "abcdefgh";
        rankString = "  ";
        for (int i = 0; i < 8; i++) {
            rankString += fileNames[i] + " ";
        }
        Console.WriteLine("  " + rankString);

    }
    public static void printBoard(Board board)
    {
        printPieceList(board.pieceList, board.sideBB); 
    }
    /// <summary>
    /// Prints out piecelist that has all piece values into an 8x8 chess board form
    /// </summary>
    /// <param name="list"></param>
    public static void printPieceList(int[] list, ulong[] sideBB) {
        const int LAST_BIT = 63; // helps with calcs 
        string rankString = "";

        for (int rank = 0; rank <= 7; rank++) {
            rankString = (8 - rank) + " | ";
            for (int file = 7; file >= 0; file--) {
                int currentBit = LAST_BIT - (rank * 8 + file);

                if (list[currentBit ] != (int) Piece.NONE) { // check if there is a piece at the current piece 
                    char c = ' ';
                    Side side = (sideBB[(int) Side.White] & (1UL<<currentBit) )> 0 ? Side.White : Side.Black;
                    switch (list[currentBit]) {
                        case (int)Piece.Pawn: c = 'p'; break;
                        case (int)Piece.Knight: c = 'n'; break;
                        case (int)Piece.Bishop: c = 'b'; break;
                        case (int)Piece.Rook: c = 'r'; break;
                        case (int)Piece.Queen: c = 'q'; break;
                        case (int)Piece.King: c = 'k'; break;
                    }
                    if (side == Side.White)
                        c = char.ToUpper(c); 
                    rankString += c+" ";
                } else { rankString += ". "; }

            }
            Console.WriteLine(rankString);
        }
        Console.WriteLine("    - - - - - - - - "); 
        string fileNames = "abcdefgh";
        rankString = "  ";
        for (int i = 0; i < 8; i++) {
            rankString += fileNames[i] + " ";
        }
        Console.WriteLine("  " + rankString);
    }
    public static Board charArrayToBoard(char[][] chessBoard) {
        const int LAST_BIT = 63; // helps with calcs 

        Board board = new Board();

        for (int i = 0; i <= 7; i++) {

            for (int j = 0; j <= 7; j++) {

                int pieceVal = (int)Piece.NONE; // current piece val ; 
                Side side;  // current side of piece 

                switch (Char.ToLower(chessBoard[i][j])) { // find val of piece 
                    case 'r': pieceVal = (int)Piece.Rook; break;
                    case 'n': pieceVal = (int)Piece.Knight; break;
                    case 'b': pieceVal = (int)Piece.Bishop; break;
                    case 'q': pieceVal = (int)Piece.Queen; break;
                    case 'k': pieceVal = (int)Piece.King; break;
                    case 'p': pieceVal = (int)Piece.Pawn; break;
                }

                // from left -> right the indexes go 56 57 58... 63
                int currentIndex = LAST_BIT - (8 * i + (7 - j)); // also where bit is going to be 

                board.pieceList[currentIndex] = pieceVal; // info about currentPiece

                if (pieceVal == (int)Piece.NONE) {

                    continue; // skip if there is no piece there 
                }



                side = Char.IsUpper(chessBoard[i][j]) ? Side.White : Side.Black; // if uppercase its white 
                ulong mask = 1UL; // initialize mask 
                mask <<= currentIndex; // now shift the bit to currentPos


                board.piecesBB[(int)side][pieceVal] |= mask; // or the mask to bb to preserve data

                // or mask to side specific as well 
                board.sideBB[(int)side] |= mask;
            }
        }

        return board;
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
    public void printBoard() {
        Board.printBitBoard(sideBB[(int)Side.White] | sideBB[(int)Side.Black]); 
    }

    /// <summary>
    /// Translates fen string to our board representation
    /// uppercase white, lowercase black, k king, q queen, b bishop, r rook, n knight, p pawn, / new line ,
    /// fen is read right to left, top to bottom 
    /// </summary>
    /// <param name="fen"> </param>
    public static void initFEN (string fen) {
     
    }
    public static Board initCopy(Board parent) {
        Board newBoard = new Board();

        // copy parent's content by value not reference
        newBoard.piecesBB = CopyJagged(parent.piecesBB);
        Array.Copy(parent.sideBB, newBoard.sideBB, NUM_SIDES);
        Array.Copy(parent.pieceList, newBoard.pieceList, NUM_SQUARES); 
        
        //state 
        newBoard.state.halfMoveClock = parent.state.halfMoveClock;
        newBoard.state.fullMoveNum = parent.state.fullMoveNum;
        newBoard.state.nextMove = new Move { origin= parent.state.nextMove.origin,
            destination = parent.state.nextMove.destination,
            promoPieceType = parent.state.nextMove.promoPieceType,
            moveType = parent.state.nextMove.moveType,
        }; 
        newBoard.state.sideToMove = parent.state.sideToMove;
        newBoard.state.castling = parent.state.castling;
        newBoard.state.EP = parent.state.EP;
        newBoard.state.zobristKey = parent.state.zobristKey;


        // history
        foreach(GameState currState in parent.gameHistory) {
            GameState newState;
            newState.halfMoveClock = currState.halfMoveClock;
            newState.fullMoveNum = currState.fullMoveNum;
            newState.nextMove = new Move {
                origin = currState.nextMove.origin,
                destination = currState.nextMove.destination,
                promoPieceType = currState.nextMove.promoPieceType,
                moveType = currState.nextMove.moveType,
            };
            newState.sideToMove = currState.sideToMove;
            newState.castling = currState.castling;
            newState.EP = currState.EP;
            newState.zobristKey = currState.zobristKey;
            newBoard.gameHistory.Add(newState);
        }

        return newBoard; 
    }


    static ulong[][] CopyJagged(ulong[][] source) {
        return source.Select(s => s.ToArray()).ToArray();
    }
    // updates boards bbs and state to reflect move 

    public void makeMove(Move move ) {
        // first get the side that is moving 
        int side = (int) state.sideToMove;
        int opp;
        int epRemoveDest = -8; // remove pawn directly below destination if white; above if black 
        int castleMask = 0b11;  // if king castles turn of rights for this king 
        if (side == (int) Side.White){
            opp = (int)Side.Black; 
        }
        else{
            opp = (int) Side.White;
            epRemoveDest = 8;
            castleMask <<= 2; // black castle rights 
        }
        
        int pieceType= pieceList[move.origin]; 
        ulong destMask = (1UL << (move.destination)), origMask = (1UL << (move.origin));

        state.EP = 0; // clear ep ; if ep wasn't made this move not able to make next move 
        state.nextMove = move; // confirm move at state 
        gameHistory.Add(state); // add state to game history 
        state.sideToMove = (Side) opp; // side to move changes 

        // remove piece from origin
        pieceList[move.origin] = (int)Piece.NONE;
        piecesBB[side][pieceType] &= ~origMask;
        sideBB[side] &= ~origMask;

        // update based on movetype 
        switch (move.moveType)
        {
            case MoveType.CAPTURE:
                // update captured piece's bbs
                int capturedPiece = pieceList[move.destination]; 
                piecesBB[opp][capturedPiece] &= ~destMask;
                sideBB[opp] &= ~destMask;
                break;

            case MoveType.ENPASSANT:
                // remove captured pawn 
                capturedPiece = pieceList[move.destination+epRemoveDest];
                piecesBB[opp][capturedPiece] &= ~(1UL<<(move.destination+epRemoveDest));
                sideBB[opp] &= ~~(1UL << (move.destination + epRemoveDest));
                break;

            case MoveType.CASTLE: // if castling kingside, rook is on the h file otherwise its on the first file 
                // if dest is greater that origin it castled king side 
                int rookOrigin = (move.destination + 1);
                int rookDest = (move.destination - 1); // king side 

                if (move.destination < move.origin) // queen side 
                {
                    rookOrigin = (move.destination -2 );
                    rookDest = (move.destination +1);
                    
                }
                pieceList[rookOrigin] = (int)Piece.NONE;
                piecesBB[side][(int)Piece.Rook] &= ~(1UL << rookOrigin);
                sideBB[side] &= ~(1UL << rookOrigin);
                //place rook at dest 
                pieceList[rookDest] = (int)Piece.Rook;
                piecesBB[side][(int)Piece.Rook] |= (1UL << rookDest);
                sideBB[side] |= (1UL << rookDest);
                // turn of rights for this king 
                state.castling &= ~castleMask;
                break; 
        }


        // important edgecases based on piece type 
        switch (pieceType)
        {
            case (int)Piece.Pawn:
                // if move was a promo switch piece type to that promo piece 
                if (move.promoPieceType != Piece.NONE)
                    pieceType = (int)move.promoPieceType;
                else if (Math.Abs(move.destination - move.origin) == 16) // pawn move two square ; turn on ep at that square 
                    state.EP |= (destMask); 
                break; 
            case (int)Piece.King: // make sure castling rights are turned off it not already 
                state.castling &= ~castleMask;
                break;
            case (int)Piece.Rook:
                // if origin is on kingside make sure rights are off 
                if ((move.origin== 7 || move.origin == 63)){
                    state.castling &= ~(0b0101);
                }
                else if(move.origin==0||move.origin==56){ 
                    state.castling &= ~(0b1010);
                }
                break; 
        }
      

        // place at destination 
        pieceList[move.destination] = pieceType;
        piecesBB[side][pieceType] |= destMask;
        sideBB[side] |= destMask;

    }

    // creates a zobrist key based on this boards piece config and state 
    private ulong initZobristKey() {
        ulong key = 0; 
        
        // iterate through all piece types white and black and xor their zobrist random for that square and side to the key
        for(int side = 0; side< piecesBB.Length; side++) {

            for(int piece = 0; piece< piecesBB[side].Length; piece++) {
                ulong bb = piecesBB[side][piece]; // this is this current pieces bb for this side 

                // so for each piece that exists xor it's position onto the key
                while (bb> 0) {
                    int sq = BitOperations.TrailingZeroCount(bb);
                    key ^= zobristRandom.pieceRandoms[side][piece][sq]; 
                    bb &= ~(1UL << sq); 
                }

            }
        }

        // now do castling ep nd side to move 
        key ^= zobristRandom.castlingRandoms[state.castling];
        key ^= zobristRandom.sideRandoms[(int)state.sideToMove];


        int epIndx = BitOperations.TrailingZeroCount(state.EP); 
        key ^= zobristRandom.epRandoms[epIndx]; // the sq ep is valid on 
        return key; 
    }
}