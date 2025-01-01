
public class ZobristRandoms {
    public ulong[][][] pieceRandoms;//for each side, piece, and square; 768
    public ulong[] castlingRandoms;// 16 for castling since we are going by binary (0000 for no castilng, 0001 for w kingside)
    public ulong[] sideRandoms;// 2 sides 
    public ulong[] epRandoms;// 16 valid eps 1 for ep not possible, could be 17 but size 65 to match guide 
    int seed = 1; 

    public ZobristRandoms() {
        // should be seeded 
        Random rand = new Random(seed);

        // init all randoms 
        // piece rands: 2 sides , 6 piece types , 64 squares
        pieceRandoms = new ulong[Board.NUM_SIDES][][]; 
        for(int i =0; i< Board.NUM_SIDES; i++) {
            pieceRandoms[i] = new ulong[Board.NUM_PIECE_TYPES][];  
            for(int j = 0; j< Board.NUM_PIECE_TYPES; j++) {
                pieceRandoms[i][j] = new ulong[Board.NUM_SQUARES]; 
                for(int k = 0; k< Board.NUM_SQUARES; k++) {
                    pieceRandoms[i][j][k] = (ulong)rand.NextInt64();
                }
            }
        }

        // castling : 16 
        castlingRandoms = new ulong[16]; 
        for(int i = 0; i<16; i++) { castlingRandoms[i] = (ulong)rand.NextInt64(); }

        // side : 2 
        sideRandoms = new ulong[Board.NUM_SIDES];
        for (int i = 0; i < Board.NUM_SIDES; i++) { sideRandoms[i] = (ulong)rand.NextInt64(); }

        // ep : 65 : all squares plus1 because if ep is 0 itll have 64 leading 0's so it'll be the last position 
        epRandoms = new ulong[Board.NUM_SQUARES+1];
        for (int i = 0; i < Board.NUM_SQUARES+1; i++) { epRandoms[i] = (ulong)rand.NextInt64(); }
    }
    
}