using System.Numerics;
using System;
using System.IO; 

class SlidingMoves
{
    // magic bitboards 

    /// <summary>
    /// Holds movement mask, magic num, and index shift wal for given square
    /// </summary>
    public struct MagicInfo {
        public ulong relevantBlockerMask;
        public ulong magicNum;
        public int indexShift; // amount to shift; apparently for bishop is just 9 
    }
    /// <summary>
    /// this is the lookup table for all sliding piece move patterns for each square 
    /// </summary>
    public static ulong[][] RookMoveHashTable = new ulong[64][];  //  [64][4096]
    public static ulong[][] BishopMoveHashTable = new ulong[64][]; //[64][512] 

    public static MagicInfo[] RookInfoTable = initMoveTables(false, RookMoveHashTable);
    public static MagicInfo[] BishopInfoTable = initMoveTables(true, BishopMoveHashTable);

    // starting on implementing the sliding moves 
    private static MagicInfo[] initMoveTables(bool bishop, ulong[][] moveLookupTable) {
        MagicInfo[] table = new MagicInfo[64];
        for (int i = 0; i < table.Length; i++) {
            var magicData = findMagicNum(bishop, i);
            table[i] = magicData.entry;
            moveLookupTable[i] = magicData.hashTable;
        }
        return table;
    }

    /// <summary>
    /// Returns the relevant blocker mask of a rook on the inputted square 
    /// </summary>
    /// <param name="square" > current square </param>
    public static ulong getRookRelevantBlockerMask(int square)
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
    public static ulong getBishopRelevantBlockerMasks(int square)
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


   



    /// <summary>
    /// and occupied with the movementmask then multiply by magic num then shift to get key for table 
    /// </summary>
    /// <param name="entry"></param>
    /// <param name="occupied"></param>
    /// <returns>ulong but should be an int since perfect hash</returns>
    public static ulong getMagicIndex(MagicInfo entry, ulong occupied)
    {
        occupied &= entry.relevantBlockerMask; // combines the actual occupied squares and the movement mask into a bb // might be unnessecary
        occupied *= entry.magicNum; // multiply blocking mask by magic num 
        occupied >>= 64 - entry.indexShift;// shift bits by index shift
        return occupied;
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
    public static (MagicInfo entry, ulong[] hashTable) findMagicNum(bool bishop, int square)
    {
        // get sliders relevant blocker mask 

        ulong relevantBlockerMask = bishop ? getBishopRelevantBlockerMasks(square) : getRookRelevantBlockerMask(square);


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
                relevantBlockerMask = relevantBlockerMask,
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

        for (int i = 0; i < hashTable.Length; i++)// init hash table as empty
            hashTable[i] = ulong.MaxValue; // what we will use to determine if it has been changed or not ; since we know this can't possibly be the moveset 

        // for each configuartion of blockers for this position and piece 
        foreach(ulong blocker in getBlockerSubsets(magicInfo.relevantBlockerMask, magicInfo.indexShift)) {
            // get moves possible bb from current blocker config
            ulong moveBB = bishop ? getMoveFromBlockerBishop(blocker, square):  getMoveFromBlockerRook(blocker,  square);

            // get key from hashing blocker config using magic num 
            int key = (int) getMagicIndex(magicInfo, blocker ); // max 4096 for rook 

            // if value at hashTable[key] is empty : set moves there 
            if (hashTable[key] == ulong.MaxValue)
            {
                hashTable[key] = moveBB;
            }// else if value == move : good do nothing (constructive collision
            // else if value != maxval or move: return null (magic num was a failure ; unconstructive collision) 
            else if (hashTable[key] != moveBB)
            {
                return null; 
            }
        }
        //this is a valid magic num and hash table if you reach end of loop
        return hashTable; // every possible move for this square and sliding piece is in this table

    }

    public static ulong getMoveFromBlockerBishop(ulong blockers, int square)
    {
        ulong moveBB = 0;
        ulong mask = 0;

        int rank = square / 8; int file = square % 8;

        // northeast diag
        for(int rankAbove =rank+1, fileRight = file+1; rankAbove<8 &&fileRight<8; rankAbove++, fileRight++) {
            // add move 
            mask = (1UL << (rankAbove * 8 + fileRight));

            moveBB |= mask;

            // check if current space is a blocker 
            if ((blockers & mask) != 0)
                break; // there is a blocker in this pos 
        }
        // southeast diag
        for (int rankBelow = rank -1, fileRight = file + 1; rankBelow >=0 && fileRight < 8; rankBelow--, fileRight++) {
            // add move 
            mask = (1UL << (rankBelow * 8 + fileRight));

            moveBB |= mask;

            // check if current space is a blocker 
            if ((blockers & mask) != 0)
                break; // there is a blocker in this pos 
        }
        //southwest
        for (int rankBelow = rank - 1, fileLeft = file -1; rankBelow >= 0 && fileLeft >=0; rankBelow--, fileLeft--) {
            // add move 
            mask = (1UL << (rankBelow * 8 + fileLeft));

            moveBB |= mask;

            // check if current space is a blocker 
            if ((blockers & mask) != 0)
                break; // there is a blocker in this pos 
        }
        for (int rankAbove = rank + 1, fileLeft = file - 1; rankAbove < 8 && fileLeft >= 0; rankAbove++, fileLeft--) {
            // add move 
            mask = (1UL << (rankAbove * 8 + fileLeft));

            moveBB |= mask;

            // check if current space is a blocker 
            if ((blockers & mask) != 0)
                break; // there is a blocker in this pos 
        }
        return moveBB; 
    }

    /// <summary>
    /// get possible moveBitBoard through iteration during magic number trials 
    /// </summary>
    /// <param name="blockers"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private static ulong getMoveFromBlockerRook(ulong blockers, int square)
    {
        ulong moveBB=0;
        ulong mask = 0; 
        // vertical above
        int rank = square / 8; int file = square % 8;

        for(int rankAbove =rank +1; rankAbove< 8; rankAbove ++) //north
        {
            mask = 1UL << (rankAbove * 8 + file);

            moveBB |= mask; // add move to moveBB

            if ((blockers & mask) != 0)// stop loop if current square has a blocker 
                break; 
        }
        for (int rankBelow = rank - 1; rankBelow >= 0; rankBelow--)//south
        {
            mask = 1UL << (rankBelow * 8 + file);

            moveBB |= mask; // add move to moveBB

            if ((blockers & mask) != 0)// stop loop if current square has a blocker 
                break;
        }
        for (int fileRight = file + 1; fileRight < 8; fileRight++) { // right
            mask = 1UL << (rank * 8 + fileRight);

            moveBB |= mask; // add move to moveBB

            if ((blockers & mask) != 0)// stop loop if current square has a blocker 
                break;
        }
        for (int fileLeft = file - 1; fileLeft >= 0; fileLeft--) {//left
            mask = 1UL << (rank * 8 + fileLeft);

            moveBB |= mask; // add move to moveBB

            if ((blockers & mask) != 0)// stop loop if current square has a blocker 
                break;
        }
        return moveBB; 
    }

    /// <summary>
    /// return all possible configurations of blockers within this movement mask
    /// (thank you Sebastian league :))
    /// </summary>
    /// <param name="movementMask"></param>
    /// <param name="relevantBits"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private static ulong[] getBlockerSubsets(ulong movementMask, int relevantBits)
    {
        int[] moveSquareIndices = new int[relevantBits] ;
        int it = 0; 
        //create a list of indicies of bits that are set in the movement mask 
        for(int i = 0; i< 64; i++)
        {
            if (((movementMask >> i) & 1) == 1) {
                moveSquareIndices[it] = i;
                it++; 
            }
        }
        //possible config of blocker bitboards 
        int possibleConfigurations = 1 << relevantBits; 

        ulong[] blockerBitBoards = new ulong[possibleConfigurations];

        for (int patternIndex = 0; patternIndex < possibleConfigurations; patternIndex++)
        {
            for(int bitIndex = 0; bitIndex<moveSquareIndices.Length; bitIndex++)
            {
                int bit = (patternIndex >> bitIndex) & 1;
                blockerBitBoards[patternIndex] |= (ulong)bit << moveSquareIndices[bitIndex]; 
            }
        }
        return blockerBitBoards; 
    }

    /// <summary>
    /// saves the info found through magic number generation for loading later to save on startup time 
    /// </summary>
    public static void SaveMagicInfoAndLookupTables() {

        
        string slidingFolderPath = System.IO.Path.GetFullPath(@"..\..\..\SlidingMoveLookupTables\SlidingPieceInfoandLookups.txt");

        string[] fileString = new string[64 * 4]; // each table has 64 lines 
        int iterator = 0; 
        // first is save rook magic info; file goes from square 0=>63 top to bottom 
        for (int i = 0; i < RookInfoTable.Length; i++) {
            fileString[iterator]= ("" + RookInfoTable[i].relevantBlockerMask + "," + RookInfoTable[i].magicNum + "," + RookInfoTable[i].indexShift);

            iterator++; 
        }

        // save bishop magic info 
        for (int i = 0; i < BishopInfoTable.Length; i++) {
            fileString[iterator] = ("" + BishopInfoTable[i].relevantBlockerMask + "," + BishopInfoTable[i].magicNum + "," + BishopInfoTable[i].indexShift);
            iterator++;
        }

        // save rook lookup table: square 0->63 top to bottom; each line has all moves 0->4096 in ulong form separated by commas 

        for (int i = 0; i < RookMoveHashTable.Length; i++) {
            fileString[iterator] = "";

            for (int j = 0; j < RookMoveHashTable[i].Length; j++) {
                fileString[iterator] += ("" + RookMoveHashTable[i][j] );

                if (i <=RookMoveHashTable[i].Length - 1) // for comma separation
                    fileString[iterator] += ","; 
            }
            iterator++; 
        }

        for (int i = 0; i < BishopMoveHashTable.Length; i++) {
            fileString[iterator] = "";

            for (int j = 0; j < BishopMoveHashTable[i].Length; j++) {
                fileString[iterator] += ("" + BishopMoveHashTable[i][j]);

                if (i <=BishopMoveHashTable[i].Length - 1) // for comma separation
                    fileString[iterator] += ",";
            }
            iterator++;
        }

        File.WriteAllLines(slidingFolderPath, fileString);
    }
    
}
