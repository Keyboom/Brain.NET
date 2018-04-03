using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Brain.NET
{
    public sealed class Program
    {
        // Global variables
        public static string code;
        // This represents the memory as a whole, each index being a memory cell
        public static short[] memory;
        public static int ptr;
        public static int codePos;
        public static Stack<int> stack;
        // Branch table is used for, yes, branching. This essencially means taking a part to another part, and is used for the [ ] operators (loop-like)
        public static Dictionary<int, int> branchTable;
        // This represents the number of memory cells. I specially picked this number so I could have 16 bits and I like 16 bits
        public const int MEMORY_CELLS = 65536;

        static void Main(string[] args)
        {
            var source = new FileInfo(args[0]);
            if (!source.Exists)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[ERROR] This file doesn't exists");
            }
            Parse(source);
        }

        // Although this isn't really a parser, but the whole interpreter itself, I used this name because
        // I originally planned this as an overkill Brainfuck Interpreter, using Token and Lex and everything
        // like that, but I wanted to learn Brainfuck + code this in less than one hour (C# lecture time)
        static void Parse(FileInfo f)
        {
            memory = new short[MEMORY_CELLS];
            ptr = 0;
            codePos = 0;
            stack = new Stack<int>();
            branchTable = new Dictionary<int, int>();

            try
            {
                using (StreamReader r = f.OpenText())
                {
                    code = r.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e);
            }

            // This loop deals with branching, explained above
            for (var i = 0; i < code.Length; i++)
            {
                switch (code[i])
                {
                    case '[':
                        stack.Push(i);
                        break;
                    case ']':
                        if (stack.Count == 0)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("[ERROR] Missing opening chevron \"[\"");
                            Console.ResetColor();
                        }
                        else
                        {
                            var openingBracketPosition = stack.Pop();
                            branchTable.Add(openingBracketPosition, i + 1);
                            branchTable.Add(i, openingBracketPosition + 1);
                        }
                        break;
                    default:
                        break;

                }
            }

            // This is also part of the branching, and is related to the closing chevron
            // TBH, this [chevron] seems like an unusual word, but I really like it
            if (stack.Count > 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[ERROR] Missing closing chevron \"]\"");
                return;
            }
            while (codePos < code.Length)
            {
                switch (code[codePos])
                {
                    // Pointer value increment operator
                    case '+':
                        memory[ptr]++;
                        codePos++;
                        break;
                    // Pointer value decrement operator
                    case '-':
                        memory[ptr]--;
                        codePos++;
                        break;
                    // Pointer position increment operator
                    case '>':
                        ptr++;
                        if (ptr == MEMORY_CELLS)
                            ptr = 0;
                        codePos++;
                        break;
                    // Pointer position decrement operator
                    case '<':
                        ptr--;
                        if (ptr == -1)
                            ptr = MEMORY_CELLS - 1;
                        codePos++;
                        break;
                    // Current pointer print operator
                    case '.':
                        Console.Write(Convert.ToChar(memory[ptr]));
                        codePos++;
                        break;
                    // Readkey, saves at the current pointer
                    case ',':
                        var r = (short)Console.Read();
                        if (r != 13)
                        {
                            if (r != -1)
                                memory[ptr] = r;
                            codePos++;
                        }
                        break;
                    // I like to call this Looper, since I really like the movie and I don't know
                    // the real name. This repeats *while* current pointer != 0
                    case '[':
                        if (memory[ptr] == 0)
                            codePos = branchTable[codePos];
                        else
                            codePos++;
                        break;
                    // This guy here ends the loop
                    case ']':
                        if (memory[ptr] == 0)
                            codePos++;
                        else
                            codePos = branchTable[codePos];
                        break;
                    // In Brainfuck, anything that's not one of the former operators is a comment. Thus, it should be ignored 
                    // by the interpreter
                    default:
                        codePos++;
                        break;
                }
            }
        }
    }
}
