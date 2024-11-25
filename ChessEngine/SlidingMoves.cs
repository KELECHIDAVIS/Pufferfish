using System.Numerics;

class SlidingMoves
{

    // starting on implementing the sliding moves 

    /// <summary>
    /// Returns the relevant blocker mask of a rook on the inputted square 
    /// </summary>
    /// <param name="square" > current square </param>
    public static ulong getRookMovementMask(int square)
    {
        ulong movementMask = 0;
        int rank = square / 8;
        int file = square % 8;

        // add all the rook relavant blocking squares  (possible moves on empty board excluding borders)
        for (int rankAbove = rank + 1; rankAbove < 7; rankAbove++)
        {
            movementMask |= 1UL << (rankAbove * 8 + file);
        }
        for (int fileRight = file + 1; fileRight < 7; fileRight++)
        {
            movementMask |= 1UL << (rank * 8 + fileRight);
        }

        for (int rankBelow = rank - 1; rankBelow >= 1; rankBelow--)
        {
            movementMask |= 1UL << (rankBelow * 8 + file);
        }
        for (int fileLeft = file - 1; fileLeft >= 1; fileLeft--)
        {
            movementMask |= 1UL << (rank * 8 + fileLeft);
        }
        return movementMask;
    }

    /// <summary>
    /// return relevant blocker masks for bishops on inputted square
    /// </summary>
    /// <param name="square">current square</param>
    /// <returns></returns>
    public static ulong getBishopMovementMasks(int square)
    {
        ulong movementMask = 0;

        int rankAbove = (square / 8), rankBelow = (square / 8);
        int fileRight = (square % 8), fileLeft = (square % 8);

        // should only go 7 because it shouldn't get the edge pieces 
        for (int it = 0; it < 7; it++)
        {
            rankAbove++; rankBelow--;
            fileRight++; fileLeft--;

            if (rankAbove < 7)
            {
                if (fileRight < 7)
                    movementMask |= 1UL << (rankAbove * 8 + fileRight);
                if (fileLeft >= 1)
                    movementMask |= 1UL << (rankAbove * 8 + fileLeft);
            }
            if (rankBelow >= 1)
            {
                if (fileRight < 7)
                    movementMask |= 1UL << (rankBelow * 8 + fileRight);
                if (fileLeft >= 1)
                    movementMask |= 1UL << (rankBelow * 8 + fileLeft);
            }
            if ((rankBelow < 1 && rankAbove > 7) || (fileLeft < 1 && fileRight > 7))
                break;
        }

        return movementMask;

    }


    // magic bitboards 

    /// <summary>
    /// Holds movement mask, magic num, and index shift wal for given square
    /// </summary>
    struct MagicInfo
    {
        public ulong movementMask;
        public ulong magicNum;
        public int indexShift; // amount to shift; apparently for bishop is just 9 
    }
    /// <summary>
    /// this is the lookup table for all sliding piece move patterns for each square 
    /// </summary>
    static ulong[][] RookMoveHashTable = new ulong[64][];  //  [64][4096]
    static ulong[][] BishopMoveHashTable = new ulong[64][]; //[64][512] 

    static MagicInfo[] RookInfoTable = initMoveTables(false, RookMoveHashTable);
    static MagicInfo[] BishopInfoTable = initMoveTables(true, BishopMoveHashTable);



    /// <summary>
    /// and occupied with the movementmask then multiply by magic num then shift to get key for table 
    /// </summary>
    /// <param name="entry"></param>
    /// <param name="occupied"></param>
    /// <returns>ulong but should be an int since perfect hash</returns>
    private static ulong getMagicIndex(MagicInfo entry, ulong occupied)
    {
        occupied &= entry.movementMask; // combines the actual occupied squares and the movement mask into a bb
        occupied *= entry.magicNum; // multiply blocking mask by magic num 
        occupied >>= 64 - entry.indexShift;// shift bits by index shift
        return occupied;
    }


    private static ulong getRookMoves(int square, ulong occupied)
    {
        int key = (int)getMagicIndex(RookInfoTable[square], occupied);
        return RookMoveHashTable[square][key];
    }

    // for bishop move table could just use wikipedia's version instead of having variable shift 
    private static ulong getBishopMove(int square, ulong occupied)
    {
        int key = (int)getMagicIndex(BishopInfoTable[square], occupied);
        return BishopMoveHashTable[square][key];
    }

    /// <summary>
    /// generate random ulong in a specific way for magic num generation 
    /// </summary>
    /// <returns></returns>
    private static ulong randomUlong()
    {
        ulong u1, u2, u3, u4;
        Random random = new Random();
        u1 = (ulong)(random.NextInt64()) & 0xFFFF; u2 = (ulong)(random.NextInt64()) & 0xFFFF;
        u3 = (ulong)(random.NextInt64()) & 0xFFFF; u4 = (ulong)(random.NextInt64()) & 0xFFFF;
        return u1 | (u2 << 16) | (u3 << 32) | (u4 << 48);
    }


    /// <summary>
    /// Return number of on bits in a given ulong
    /// Return the number of relevant bits in this movement mask; returns the number of bits that are on. 
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static int getNumberOnBits(ulong movementMask)
    {
        int shift = 0;
        while (movementMask > 0)
        {
            shift++;
            movementMask &= ~(1UL << BitOperations.TrailingZeroCount(movementMask));
        }
        return shift;
    }


    /// <summary>
    /// Return this squares magic entry and accompanying hashtable for move lookup 
    /// </summary>
    /// <param name="bishop"> whether or not the sliding piece is a bishop or not</param>
    /// <param name="square">current square</param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private static (MagicInfo entry, ulong[] hashTable) findMagicNum(bool bishop, int square)
    {
        // get sliders relevant blocker mask 
        ulong relevantBlockerMask = bishop ? getBishopMovementMasks(square) : getRookMovementMask(square);


        // while valid magic num hasn't been found (could do a for loop of 100000000 like wikipedia) 
        //and 3 random ulongs and set magic num to that 
        //create magic entry 
        //call trymaketable func
        //if returns a valid solution, then stop loop 

        while (true)
        {
            ulong magicNum = randomUlong() & randomUlong() & randomUlong();
            MagicInfo magicInfo = new MagicInfo()
            {
                indexShift = (int)getNumberOnBits(relevantBlockerMask),
                magicNum = magicNum,
                movementMask = relevantBlockerMask,
            };
            ulong[] hashTable = tryMakeTable(bishop, square, magicInfo);

            if (hashTable != null)  // successful magicNum has been found
                return (magicInfo, hashTable);
        }
    }

    /// <summary>
    /// tries to make a table based on the current squares generate magic num, if current magic num fails, return null 
    /// This funcion iterates throughtout all possible configurations of blockers for current square and attempts to use current magic num to hash these positions to accurate indices 
    /// Function fails if any non constructive collisions occur (two different blocker set patterns with different move sets hash to same index)
    /// </summary>
    /// <param name="bishop">whether piece is a bishop or not </param>
    /// <param name="square">current square</param>
    /// <param name="magicInfo">current squares magic info with magicNum candidate included </param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private static ulong[] tryMakeTable(bool bishop, int square, MagicInfo magicInfo)
    {
        ulong[] hashTable = new ulong[1 << magicInfo.indexShift]; // max for rooks: 4096 or 2^12 

        // for each configuartion of blockers for this position and piece 
        // get moves possible bb from current blocker config
        // get key from hashing blocker config using magic num 
        // if value at hashTable[key] is empty : set moves there 
        // else if value == move : good do nothing (constructive collision
        // else if value !=move: return null (magic num was a failure) 
        return null;
    }

    private static MagicInfo[] initMoveTables(bool bishop, ulong[][] moveLookupTable)
    {
        MagicInfo[] table = new MagicInfo[64];
        for (int i = 0; i < table.Length; i++)
        {
            var magicData = findMagicNum(bishop, i);
            table[i] = magicData.entry;
            moveLookupTable[i] = magicData.hashTable;
        }
        return table;
    }
}
