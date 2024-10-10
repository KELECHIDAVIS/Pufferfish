using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class Engine
{
  
    public static void Main(string[] args)
    {
        int[] arr = { 1, 2, 3, 4, 5, 6, };
        Board board = new Board();
        
        Console.Write(board.getPieceBitBoard(Side.White,Piece.King));
    }
}

    
