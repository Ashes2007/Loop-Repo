using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
namespace CTIL_Interpreter
{
    class Program
    {
        //D:\Documents\vs\CTIL Interpreter\CTIL Interpreter\calculator.txt
        static int pCounter = 0;
        static bool run = true;
        static string[] texts;
        static void Main()
        {
        
            
            int[] registry = new int[15];
            Console.WriteLine("Input file:");
            string file = Console.ReadLine();
            string code = File.ReadAllText(file);
            int s = file.IndexOf('.');
            string textRead = file.Substring(0, s);
            int l = file.Length - textRead.Length;
            string extension = file.Substring(s, l);
            textRead = textRead + "_text" + extension;
            string text = File.ReadAllText(textRead);
            texts = text.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
            string[] lines =  code.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
            Console.Clear();
            while(run)
            {
                string iLine = lines[pCounter]; //Gets the instruction at the program counters position
                string[] parts = iLine.Split(' '); //Gets every word from the instruction
                int p = parts.Length; 
                string[] partsVerified = new string[p]; 
                //Remove all comments from the instruction
                for(int i = 0; i < parts.Length; i++)
                {
                   if(parts[i].Contains("//")) break;
                   partsVerified[i] = parts[i];         
                }
                string instruction = partsVerified[0];
                string[] args = new string[5];
                //Extract the arguments from the parts without extracting the operation
                for(int i = 0; i < partsVerified.Length; i++ )
                {
                    if(i == 0) continue;
                    args[i - 1] = partsVerified[i];


                }
                
                ExcecuteInstruction(registry, instruction, args); //Excecute the operation
                pCounter ++;
            }
        }
        static int[] ExcecuteInstruction(int[] registerBank, string operation, string[] unprocArgs) 
        {
            bool[] isRegister = new bool[15];
            int[] argRegs = new int[15];
            int[] args = new int[15];
            //Determine if an argument is a register or a value
            for(int i = 0; i < unprocArgs.Length; i++)
            {

                string iArg = unprocArgs[i];
                if(unprocArgs[i] == null) continue; //Error correction
                //Detects if the current arg is a register, and stores the registers identifier and value if it is.
                if (iArg.ToCharArray()[0] == 'r') 
                {
                    int valPos = Int32.Parse(iArg.Substring(1)); //Take the R off of the register value and store the number
                    args[i] = registerBank[valPos]; //Move the value from the register to the argument storage
                    argRegs[i] = valPos; //Stores the registers number for output specific cases
                    isRegister[i] = true;
                    continue;
                }
                //If the argument isn't a register we can store it for later
                args[i] = Int32.Parse(iArg); 
                isRegister[i] = false;
            }
            //Execute the command!
            switch (operation)
            {
                default:        //Unrecognized command
                    Console.WriteLine("[ERROR]: Invalid Command!");
                    break;

                //Logical operations:   
                #region
                case "and":     //Logical AND operation; AR0: Output Register, A1: Input A, A2 Input B
                    registerBank[argRegs[0]] = args[1] & args[2];
                    break;
                case "or":     //Logical OR operation; AR0: Output Register, A1: Input A, A2 Input B
                    registerBank[argRegs[0]] = args[1] | args[2];
                    break;
                case "nand":     //Logical NAND operation; AR0: Output Register, A1: Input A, A2 Input B
                    int iVal = args[1] & args[2];
                    if(iVal == 1)
                    {
                    registerBank[argRegs[0]] = 0;
                    break;
                    }
                    registerBank[argRegs[0]] = 1;
                    break;
                case "nor":     //Logical NOR operation; AR0: Output Register, A1: Input A, A2 Input B
                    iVal = args[1] | args[2];
                    if (iVal == 1)
                    {
                        registerBank[argRegs[0]] = 0;
                        break;
                    }
                    registerBank[argRegs[0]] = 1;
                    break;
                case "xor":
                    registerBank[argRegs[0]] = args[1] ^ args[2];
                    break;
                case "xnor":     //Logical XNOR operation; AR0: Output Register, A1: Input A, A2 Input B
                    iVal = args[1] ^ args[2];
                    if (iVal == 1)
                    {
                        registerBank[argRegs[0]] = 0;
                        break;
                    }
                    registerBank[argRegs[0]] = 1;
                    break;
                #endregion
                //Mathmatical operations:
                #region
                case "move": //Mathmatical equals operation, AR0: Out A1: In
                    registerBank[argRegs[0]] = args[1];
                    break;
                case "add": //AR0: Out A1: Input A A2: Input B
                    registerBank[argRegs[0]] = args[1] + args[2];
                    break;
                case "sub": //AR0: Out A1: Input A A2: Input B (A - B)
                    registerBank[argRegs[0]] = args[1] - args[2];
                    break;
                case "mul": //AR0: Out A1: Input A A2: Input B
                    registerBank[argRegs[0]] = args[1] * args[2];
                    break;
                case "div": //AR0: Out A1: Input A A2: Input B (A/B)
                    registerBank[argRegs[0]] = args[1] / args[2];
                    break;
                #endregion
                //Miscellanious operations:
                #region
                case "echo":    //Print to screen
                    Console.WriteLine(args[0]); //AR0: Num to print
                    break;
                case "halt":    //Halt for AR0 milliseconds
                    Thread.Sleep(args[0]);
                    break;
                case "read":    //Reads line from console
                    registerBank[argRegs[0]] = Int32.Parse(Console.ReadLine());
                    break;
                case "quit":    //Halts the program forever
                    run = false;
                    break;
                case "echt":
                    Console.WriteLine(texts[args[0]]);
                    break;
                #endregion
                //Branch operations
                #region
                case "j": //Jump to line specified by AR0
                    pCounter = args[0] - 2;
                    break;
                case "beq": //Jump to A2 if A0 = A1
                    if(args[0] == args[1])
                    {
                        pCounter = args[2] - 2;
                    }
                    break;
                case "bnq": //Jump to A2 if A0 != A1
                    if (args[0] != args[1])
                    {
                        pCounter = args[2] - 2;
                    }
                    break;
                case "beqz": //Jump to A2 if A0 = 0
                    if (args[0] == 0)
                    {
                        pCounter = args[1] - 2;
                    }
                    break;
                case "bnqz": //Jump to A2 if A0 != 0
                    if (args[0] != 0)
                    {
                        pCounter = args[1] - 2;
                    }
                    break;

                    #endregion
            }
            return registerBank;
        }

    }
}
