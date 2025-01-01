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
   

    public const int NUM_PIECE_TYPES = 6;
    public const int NUM_SIDES = 2;
    public const int NUM_SQUARES = 64;

    public ulong[][] piecesBB = initPiecesBitBoard(); // row is color, col is piece type Ex: [white][king]
    public ulong[] sideBB = new ulong[NUM_SIDES]; // hold all pieces from a certain color Ex: [white] 
    
    //PIECE LIST IS LEAST SIG BIT TO MOST SIG; LSB->MSB; 0->63
    public int[] pieceList = new int[NUM_SQUARES]; // rep of the board that holds piece info so we can tell which piece is on which square


    public GameState state;
    public GameHistory history = new(); // list of states; all the states taken so far 

    // need zobrist randoms 
    ZobristRandoms zobristRandom= new();  // UNCOMMENT WHEN DONE DEBUGGING 
    

    

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
        // can't copy objects directly;
        // Two ways to copy: go through every relevant primitve Variable and just copy and paste
        // or just start with standard chess and make every move thats in the parent's history in order 
        Board newBoard = new Board();
        newBoard.initStandardChess(); 

        // for every parent move make same move on new board  
        for(int i =0; i< parent.history.length(); i++) {
            GameState currState = parent.history.getState(i);
            newBoard.makeMove(currState.nextMove); 
        }
        
        return newBoard;
    }


    // updates boards bbs and state to reflect move 

    public void makeMove(Move move ) {
        // first get the side that is moving 
        Side side = state.sideToMove; 
        Side opp = (Side.White == side) ? Side.Black : Side.White;
        ulong destMask = (1UL << (move.destination)), origMask = (1UL << (move.origin));
        // save the state into the history to prepare for other moves 
        state.nextMove = move; 
        history.push (state);
        state.sideToMove = opp; // opponents turn next

        // based on move the zobrist key should be updated accordingly 
        switch (move.moveType) {
            case MoveType.ENPASSANT:
                // en passant captures an enemy pawn so we have to update both their bb's seperately 
                // the pawn to remove is alway below the destination if pawn is white and above the destination if the pawn is black 
                int add;
                if (side == Side.White) {
                    add = -8; // if white its below if black its above
                } else {
                    add = 8;
                }
                // remove captured pawn 
                this.piecesBB[(int)opp][(int)Piece.Pawn] ^= (1UL << (move.destination + add));
                this.sideBB[(int)opp] ^= (1UL << (move.destination + add));
                this.pieceList[move.destination + add] = (int)Piece.NONE;
                // remove captured pawn from zobrist
                state.zobristKey ^= zobristRandom.pieceRandoms[(int)opp][(int)Piece.Pawn][move.destination + add]; 

                // move capturing pawn 
                // remove from origin , the remove from zobrist
                this.piecesBB[(int)side][(int)Piece.Pawn] ^= origMask;
                this.sideBB[(int)side] ^= origMask;
                this.pieceList[move.origin] = (int)Piece.NONE;
                state.zobristKey ^= zobristRandom.pieceRandoms[(int)side][(int)Piece.Pawn][move.origin];


                // then place at destination and update zobrist
                this.piecesBB[(int)side][(int)Piece.Pawn] |= destMask;
                this.sideBB[(int)side] |= destMask;
                this.pieceList[move.destination] = (int)Piece.Pawn;
                state.zobristKey ^= zobristRandom.pieceRandoms[(int)side][(int)Piece.Pawn][move.destination];



                // since EP was made have to turn off this EP in the state ;  piece to remove is at the same pos 
                this.state.EP ^= (1UL << (move.destination + add));
                state.zobristKey ^= zobristRandom.epRandoms[move.destination + add]; // toggle ep zobrist 
                break;
            case MoveType.CASTLE:
                // update castling rights based on where castled 
                // the king that moved is not allowed to castle again
                // wk 0001 wq 0010 bk 0100 bq 1000 all(initial): 1111 none: 0000
                int toRemove = 0b00000011; // if white remove 0011 if black shift twice to right and remove 1100
                if (side == Side.Black) toRemove <<=2;

                // toggle original castling rights off 
                state.zobristKey ^= zobristRandom.castlingRandoms[state.castling]; 

                state.castling &= ~toRemove; // gotta make sure both options are off 
                

                // if destination is greater than origin then it castled king side other wise it castled queen 
                // depending on which side was castled update zobrist 
                int rookOrigin, rookDestination; 
                if (move.destination> move.origin) { // king side 
                    rookOrigin = (side==Side.White) ? (int) Square.H1: (int)Square.H8;
                    rookDestination = move.destination - 1; // left of the king 

                    // zobrist : turn off queen side then toggle zobrist with the king side move 
                    toRemove &= ~0b00001010;

                } else {// queen side 
                    rookOrigin = (side == Side.White) ? (int)Square.A1 : (int)Square.A8;
                    rookDestination = move.destination +1; // right of the king 

                    toRemove &= ~0b00000101; // turn off king side
                }
                state.zobristKey ^= zobristRandom.castlingRandoms[toRemove]; //toggle that specific move

                // remove king and rook from origin 
                this.piecesBB[(int)side][(int)Piece.King] ^= origMask;
                this.sideBB[(int)side] ^= origMask;
                this.pieceList[move.origin] = (int)Piece.NONE;
                state.zobristKey ^= zobristRandom.pieceRandoms[(int)side][(int)Piece.King][move.origin];

                this.piecesBB[(int)side][(int)Piece.Rook] ^= (1UL << (rookOrigin));
                this.sideBB[(int)side] ^= (1UL << (rookOrigin));
                this.pieceList[rookOrigin] = (int)Piece.NONE;
                state.zobristKey ^= zobristRandom.pieceRandoms[(int)side][(int)Piece.Rook][rookOrigin];

                // now place rook and king at correct destinations 
                this.piecesBB[(int)side][(int)Piece.King] |= destMask;
                this.sideBB[(int)side] |= destMask;
                this.pieceList[move.destination] = (int)Piece.King;
                state.zobristKey ^= zobristRandom.pieceRandoms[(int)side][(int)Piece.King][move.destination];


                this.piecesBB[(int)side][(int)Piece.Rook] |= (1UL << (rookDestination));
                this.sideBB[(int)side] |= (1UL << (rookDestination));
                this.pieceList[rookDestination] = (int)Piece.Rook;
                state.zobristKey ^= zobristRandom.pieceRandoms[(int)side][(int)Piece.Rook][rookDestination];

                break;
            default:  // capture, evasion, or quiet 
                 
                if(MoveType.CAPTURE == move.moveType) {
                    int capturedPiece = pieceList[move.destination];
                    // update captured piece 
                    this.piecesBB[(int)opp][capturedPiece] ^= destMask;
                    this.sideBB[(int)opp] ^= destMask;
                    state.zobristKey ^= zobristRandom.pieceRandoms[(int)opp][capturedPiece][move.destination];

                }

                int piece =pieceList[move.origin];  // moving piece 

                // update piece list 
                pieceList[move.origin] = (int) Piece.NONE;
                pieceList[move.destination] = piece;

                // update moving piece: remove from origin the put at destination 
                this.piecesBB[(int)side][piece] ^= origMask ;
                this.sideBB[(int)side] ^= origMask;
                state.zobristKey ^= zobristRandom.pieceRandoms[(int)side][piece][move.origin];

                this.piecesBB[(int)side][piece] |= destMask;
                this.sideBB[(int)side] |= destMask;
                state.zobristKey ^= zobristRandom.pieceRandoms[(int)side][piece][move.destination];

                // if a pawn is pushed two spaces light up their destination in the ep
                if (piece == (int) Piece.Pawn && ((move.origin+2 == move.destination)||(move.origin-2== move.destination))) {
                    int epIndx = BitOperations.TrailingZeroCount(state.EP);
                    state.zobristKey ^= zobristRandom.epRandoms[epIndx]; // toggle original ep 
                    state.EP = destMask; //can only ever be one ep position at a time 
                    epIndx = BitOperations.TrailingZeroCount(state.EP);
                    state.zobristKey ^= zobristRandom.epRandoms[epIndx]; // final ep rights 

                }

                int kingRemove = 0b00000011; // if white remove 0011 if black shift twice to right and remove 1100
                if (side == Side.Black) kingRemove <<= 2;
                // if a king moves at all and still has castling rights 
                if (piece == (int)Piece.King && ((state.castling & kingRemove) > 0 )) {
                    state.zobristKey ^= zobristRandom.castlingRandoms[state.castling]; // toggle original castling rights

                    state.castling &= ~kingRemove; // can't xor bc it may be on or off 
                }

                //if rook moves for the first time and their side still has castling rights revoke rights 
                
                if (piece == (int) Piece.Rook ) {
                    int relevantBit = 0;
                    // if origin is 56 or 0  its queen side if 63 or 7 its king 
                    if (move.origin == 63 || move.origin == 7) {
                        int pos = BitOperations.TrailingZeroCount(kingRemove) ; // gives kings bit for this side
                        relevantBit = 1<<pos;
                    }
                    else if (move.origin == 56 || move.origin == 0) {
                        int pos = BitOperations.TrailingZeroCount(kingRemove) + 1;
                        relevantBit = 1<<pos; 
                    }
                    state.castling &= ~relevantBit; // turns off bit if rook hasn't moved 
                }
                break; 
        }
        

        


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