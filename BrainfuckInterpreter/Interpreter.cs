using System.Diagnostics;

namespace BrainfuckInterpreter
{
    public sealed class Interpreter
    {
        public bool ThrowOnUnkownChar { get; set; } = false;
        public OutputType OutputType { get; set; } = OutputType.Char;
        public uint MaxStackSize { get; set; } = 100;

        int index = 0;
        List<int> stack = new(new[] { 0 }); int ptr = 0;
        bool inLoop = false; int loopStartIndex = 0;
        public void Run(string code)
        {
            while (Process(
                code, ref index,
                stack, ref ptr,
                ref inLoop, ref loopStartIndex
            )) { }
        }


        public void RunSingle(string code)
        {
            int index = 0;
            List<int> stack = new(new[] { 0 }); int ptr = 0;
            bool inLoop = false; int loopStartIndex = 0;
            while (Process(
                code, ref index,
                stack, ref ptr,
                ref inLoop, ref loopStartIndex
            )) { }
        }

        bool Process(
            string code, ref int codeIndex,
            List<int> stack, ref int ptr,
            ref bool inLoop, ref int loopStartIndex
        )
        {
            if (codeIndex < 0 || codeIndex >= code.Length)
                return false;

            char c = code[codeIndex];
            switch (c)
            {
                case '>':
                    ptr++;
                    if (ptr >= stack.Count)
                    {
                        if (MaxStackSize > 0 && ptr > MaxStackSize)
                            throw new StackOverflowException($"Stack overflow; Current limit: {MaxStackSize}");
                        stack.Add((char)0);
                    }
                    break;
                case '<':
                    ptr--;
                    if (ptr < 0)
                        throw new AccessViolationException("Pointer out of Range: ptr < 0");
                    break;
                case '+':
                    stack[ptr]++;
                    break;
                case '-':
                    stack[ptr]--;
                    break;
                case '.':
                    if (OutputType == OutputType.Char)
                        Console.Write((char)stack[ptr]);
                    else
                        Console.Write(stack[ptr]);
                    break;
                case ',':
                    stack[ptr] = Console.Read();
                    break;
                case '[':
                    if (stack[ptr] != 0)
                    {
                        inLoop = true;
                        loopStartIndex = codeIndex;
                    }
                    else
                    {
                        int loopCount = 0;
                        for (int i = codeIndex + 1; i < code.Length; i++)
                        {
                            char cX = code[i];
                            if (cX == '[')
                            {
                                loopCount++;
                            }
                            else if (cX == ']')
                            {
                                if (loopCount <= 0)
                                {
                                    codeIndex = i + 1;
                                    return true;
                                }
                                loopCount--;
                            }
                        }
                        throw new InvalidProgramException("Expected \"]\"");
                    }
                    break;
                case ']':
                    {
                        int loopCount = 0;
                        for (int i = codeIndex - 1; i >= 0; i--)
                        {
                            char cX = code[i];
                            if (cX == ']')
                            {
                                loopCount++;
                            }
                            else if (cX == '[')
                            {
                                if (loopCount <= 0)
                                {
                                    codeIndex = i;
                                    return true;
                                }
                                loopCount--;
                            }
                        }
                        throw new InvalidProgramException("Expected \"]\"");
                    }
                case ' ':
                case '\r':
                case '\n':
                    break;
                default:
                    if (ThrowOnUnkownChar)
                        throw new InvalidProgramException($"Unexpected \"{c}\" at {codeIndex}");
                    break;
            }

            codeIndex++;
            return true;
        }
    }
}