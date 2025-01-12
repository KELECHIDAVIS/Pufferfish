using Microsoft.VisualBasic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

public class Perft {


    // return the number of legal moves that can be made from this board state based on depth 
    // moves refers to the moves possible in current position 
    public static long  perft(Board board, int depth,  bool firstCall , Dictionary<string,long> map) {
        long tot = 0;
        

        if (depth == 0)
            return 1; 

       
        Move[] moves = new Move[Moves.MAX_POSSIBLE_MOVES];
        int moveCount = Moves.possibleMoves(board, moves);

        for (int i = 0; i < moveCount; i++)
        {
            Board child = Board.initCopy(board);
            
            child.makeMove(moves[i]);
            long childResponses = perft(child, depth - 1, false, map); 
            

            if (firstCall)
                map.Add(((Square)moves[i].origin + "" + (Square)moves[i].destination).ToLower() ,  childResponses);

            tot += childResponses; // account for child as well 
        }
        return tot; 

    }

    internal static void runTest()
    {
        Board board = new Board();
        board.initStandardChess(); 
        string uciPath = "D:\\Downloads\\stockfish-windows-x86-64-avx2\\stockfish\\stockfish-windows-x86-64-avx2.exe";
        // Start the Stockfish process


        // Start the Stockfish process
        var stockfish = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = uciPath,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        stockfish.Start();

        // Access the input and output streams
        using StreamWriter input = stockfish.StandardInput;
        using StreamReader output = stockfish.StandardOutput;

        // Ensure Stockfish is running
        if (stockfish.HasExited)
        {
            Console.WriteLine("Failed to start Stockfish.");
            return;
        }

        // Initialize Stockfish with the UCI protocol
        input.WriteLine("uci");
        input.Flush();
        while (output.ReadLine() != "uciok") { } // Wait for uciok

        // Call the command processing function
        CommandLoop(input, output,board );


        // Close Stockfish
        stockfish.Close();
    }

    static void CommandLoop(StreamWriter input, StreamReader output, Board board )
    {
        string commands = "'stop' 'divide' 'newgame' 'move' 'clear' 'printfen' 'fen'"; 
        Console.WriteLine($"Enter commands ({commands}):");



        string userInput;
        while ((userInput = Console.ReadLine()) != null)
        {
            // Exit if the user enters "stop"
            if (userInput.Trim().ToLower() == "stop")
            {
                Console.WriteLine("Stopping...");
                
                break;
            }
            else if(userInput.Trim().ToLower() == "clear")
            {
                Console.Clear();
                Console.WriteLine("Enter commands ('stop' 'divide' 'newgame' 'move' 'clear'):");

            }
            else if(userInput.Trim().ToLower() == "printfen")
            {
                Console.WriteLine(board.toFEN()); 
            }
            else if (userInput.Trim().ToLower() == "fen")
            {
                Console.WriteLine("Enter fen string: "); 
                string fen = Console.ReadLine().Trim();

                board = new Board();
                try
                {
                    board.fromFEN(fen);
                }
                catch(Exception e )
                {
                    Console.WriteLine("Invalid fen string: " + e.Message); 
                }

                // restart stock as well 
                input.WriteLine("ucinewgame");
                input.Flush();

                // Send isready to ensure Stockfish responds
                input.WriteLine("isready");
                input.Flush();

                string line;
                while (output.ReadLine() != "readyok") { } // wait for ready okay; 

                input.WriteLine("position fen " + fen);
                input.Flush();

                Board.printBoard(board);
            }
            else if (userInput.Trim().ToLower() == "newgame")
            {
                Console.WriteLine("Starting new game... ");
                board = new Board(); 
                board.initStandardChess();
                // restart stock as well 
                input.WriteLine("ucinewgame");
                input.Flush();

                // Send isready to ensure Stockfish responds
                input.WriteLine("isready");
                input.Flush();

                string line;
                while (output.ReadLine() != "readyok") { } // wait for ready okay; 


                Console.Clear();
                Console.WriteLine($"Enter commands ({commands}):");

            }
            else if (userInput.StartsWith("move ", StringComparison.OrdinalIgnoreCase))
            {
                string move = userInput.ToLower().Split(" ")[1]; // get the move in string format 
                // make move on board 
                int startRank = move[1] - '0', startFile = move[0]-'a', endRank = move[3] - '0', endFile = move[2] - 'a';
                int origin = (startRank-1)* 8 + startFile; 
                int dest = (endRank-1)* 8 + endFile;

                // get possible moves 
                Move[] moves = new Move[Moves.MAX_POSSIBLE_MOVES];
                int moveCount = Moves.possibleMoves(board , moves);
                Move? userMove = null;
                
                for(int i = 0; i < moveCount; i++)
                {
                    if(origin == moves[i].origin&& dest== moves[i].destination)
                        userMove = moves[i];
                }
                if (userMove != null) // valid move 
                {
                    // make move on board 
                    board.makeMove(userMove.Value);
                    // translate board to fen string 
                    string fen = board.toFEN(); 
                    //set stock board to fen 
                    input.WriteLine("position fen "+fen);
                    input.Flush();

                    // print out current board 
                    Board.printBoard(board);
                }
                else
                {
                    Console.WriteLine("Invalid Move Try Again"); 
                }
            }
            // Check for "divide x" commands
            else if (userInput.StartsWith("divide ", StringComparison.OrdinalIgnoreCase))
            {
                // Extract the value of x
                string[] parts = userInput.Split(' ');
                if (parts.Length == 2 && int.TryParse(parts[1], out int depth))
                {
                    string command = $"go perft {depth}";
                    

                    // Send the command to Stockfish
                    input.WriteLine(command);
                    input.Flush();

                    // get my result 
                    Dictionary<string, long> myResults = new Dictionary<string, long>();
                    long myTotal = perft(board, depth, true, myResults) ;

                    // get stockfish result 
                    Dictionary<string, long> stockResults= new();
                    long stockTotal=0; 
                    // Read and display Stockfish output
                    string line;
                    while ((line = output.ReadLine()) != null)
                    {
                        if (line.Length>4 && line[4] == ':') // a move with result 
                        {
                            string[] move = line.Split(": ");
                          
                            long moveCount = long.Parse(move[1]);
                            stockTotal += moveCount; 
                            stockResults.Add(move[0], moveCount);
                        }

                        // Break if output indicates completion (example: bestmove or other end signal)
                        if (line.StartsWith("Nodes searched"))
                        {
                            break;
                        }
                    }

                    // for each move in stock : 
                    foreach (var pair in stockResults)
                    {
                        // if move not found in mine move was not generated 
                        if (!myResults.ContainsKey(pair.Key))
                        {
                            Console.WriteLine(pair.Key +" was not generated by my engine");
                        }else 
                        {
                            // if move has a diff val : move generated wrong val 
                            if(myResults[pair.Key] != stockResults[pair.Key])
                            {
                                Console.WriteLine(pair.Key +" generated wrong val.   Mine: "+myResults[pair.Key] +"  stock: " + stockResults[pair.Key]+ "   diff: "+ (myResults[pair.Key] - stockResults[pair.Key]) );
                            }
                            else
                            {
                                Console.WriteLine(pair.Key + " correct.   Mine: " + myResults[pair.Key] + "  stock: " + stockResults[pair.Key] );

                            }
                            // remove move from map 
                            myResults.Remove(pair.Key);
                        }

                    }
                    Console.WriteLine($"My total: {myTotal}  Stock Total: {stockTotal}"); 

                    // if there are any moves left theyre were extra generated by my engine 
                    foreach (var pair in myResults)
                    {
                        Console.WriteLine(pair.Key + " was an extra move"); 
                    }
                }
                else
                {
                    Console.WriteLine("Invalid command. Use 'divide x' where x is a valid integer.");
                }
            }
            else
            {
                Console.WriteLine("Invalid command try again"); 
            }
            
        }
    }


}
