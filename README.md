# Brainfuck-Interpreter
Simple app to interpret or jit compile the [brainfuck](https://en.wikipedia.org/wiki/Brainfuck) language.

## Jit-Compilation (IL)
The compiler goes through every character one by one and emits [IL](https://en.wikipedia.org/wiki/Common_Intermediate_Language) for the [CLR](https://docs.microsoft.com/en-us/dotnet/standard/clr) using [`DynamicMethod`](https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.dynamicmethod).    
The compiler does *not* optimize the output in any way (memory size, performance, ...).

## Jit-Compilation (Asm)
> **Note**   
> Not supported yet
