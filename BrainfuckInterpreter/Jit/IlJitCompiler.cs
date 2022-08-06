using System.Reflection.Emit;

namespace Brainfuck.Jit
{
    public sealed class IlJitCompiler
    {
        string _code;
        public IlJitCompiler(string code)
            => _code = code;

        public Action Compile()
        {
            DynamicMethod dynamicMethod = new($"brainfuck_{DateTime.Now.ToFileTime()}", typeof(void), new Type[0], typeof(IlJitCompiler).Module);

            List<(Label start, Label check)> loops = new();
            var generator = dynamicMethod.GetILGenerator();

            var stackRef = generator.DeclareLocal(typeof(List<int>));
            var ptrRef = generator.DeclareLocal(typeof(int));
            var cache = generator.DeclareLocal(typeof(int));
            var cache2 = generator.DeclareLocal(typeof(bool));

            generator.Emit(OpCodes.Ldc_I4, 0);
            generator.Emit(OpCodes.Stloc, cache2);

            generator.Emit(OpCodes.Ldc_I4, 0);
            generator.Emit(OpCodes.Stloc, cache);

            generator.Emit(OpCodes.Ldc_I4, 0);
            generator.Emit(OpCodes.Stloc, ptrRef);

            generator.Emit(OpCodes.Newobj, typeof(List<int>).GetConstructors()[0]);
            generator.Emit(OpCodes.Stloc, stackRef);

            var get_Item = stackRef.LocalType.GetProperty("Item")?.GetMethod;
            var set_Item = stackRef.LocalType.GetProperty("Item")?.SetMethod;
            var get_Count = stackRef.LocalType.GetProperty("Count")?.GetMethod;
            var add = stackRef.LocalType.GetMethod("Add");

            var write = typeof(Console).GetMethod("Write", new[] { typeof(char) });
            var read = typeof(Console).GetMethod("Read");

            // First slot
            generator.Emit(OpCodes.Ldloc, stackRef);
            generator.Emit(OpCodes.Ldc_I4, 0);
            generator.Emit(OpCodes.Callvirt, add);

            for (int i = 0; i < _code.Length; i++)
            {
                char c = _code[i];
                switch (c)
                {
                    case '>':
                        generator.Emit(OpCodes.Ldloc, ptrRef);
                        generator.Emit(OpCodes.Ldc_I4, 1);
                        generator.Emit(OpCodes.Add);
                        generator.Emit(OpCodes.Stloc, ptrRef);

                        generator.Emit(OpCodes.Ldloc, ptrRef);
                        generator.Emit(OpCodes.Ldloc, stackRef);
                        generator.Emit(OpCodes.Callvirt, get_Count);
                        generator.Emit(OpCodes.Ldc_I4, 1);
                        generator.Emit(OpCodes.Sub);
                        generator.Emit(OpCodes.Cgt);
                        generator.Emit(OpCodes.Stloc, cache2);

                        // generator.Emit(OpCodes.Ldloc, cache2);
                        var skipLabel = generator.DefineLabel();
                        // generator.Emit(OpCodes.Brfalse, skipLabel);
                        generator.Emit(OpCodes.Ldloc, stackRef);
                        generator.Emit(OpCodes.Ldc_I4, 0);
                        generator.Emit(OpCodes.Callvirt, add);
                        generator.MarkLabel(skipLabel);
                        break;
                    case '<':
                        generator.Emit(OpCodes.Ldloc, ptrRef);
                        generator.Emit(OpCodes.Ldc_I4, 1);
                        generator.Emit(OpCodes.Sub);
                        generator.Emit(OpCodes.Stloc, ptrRef);
                        break;
                    case '+':
                        generator.Emit(OpCodes.Ldloc, stackRef);
                        generator.Emit(OpCodes.Ldloc, ptrRef);
                        generator.EmitCall(OpCodes.Callvirt, get_Item, null);
                        generator.Emit(OpCodes.Stloc, cache);
                        generator.Emit(OpCodes.Ldloc, stackRef);
                        generator.Emit(OpCodes.Ldloc, ptrRef);
                        generator.Emit(OpCodes.Ldloc, cache);
                        generator.Emit(OpCodes.Ldc_I4, 1);
                        generator.Emit(OpCodes.Add);
                        generator.EmitCall(OpCodes.Callvirt, set_Item, null);
                        break;
                    case '-':
                        generator.Emit(OpCodes.Ldloc, stackRef);
                        generator.Emit(OpCodes.Ldloc, ptrRef);
                        generator.EmitCall(OpCodes.Callvirt, get_Item, null);
                        generator.Emit(OpCodes.Stloc, cache);
                        generator.Emit(OpCodes.Ldloc, stackRef);
                        generator.Emit(OpCodes.Ldloc, ptrRef);
                        generator.Emit(OpCodes.Ldloc, cache);
                        generator.Emit(OpCodes.Ldc_I4, 1);
                        generator.Emit(OpCodes.Sub);
                        generator.EmitCall(OpCodes.Callvirt, set_Item, null);
                        break;
                    case '.':
                        generator.Emit(OpCodes.Ldloc, stackRef);
                        generator.Emit(OpCodes.Ldloc, ptrRef);
                        generator.EmitCall(OpCodes.Callvirt, get_Item, null);
                        generator.Emit(OpCodes.Conv_U2);
                        generator.EmitCall(OpCodes.Call, write, null);
                        break;
                    case ',':
                        generator.EmitCall(OpCodes.Call, read, null);
                        generator.Emit(OpCodes.Stloc, cache);
                        generator.Emit(OpCodes.Ldloc, stackRef);
                        generator.Emit(OpCodes.Ldloc, ptrRef);
                        generator.Emit(OpCodes.Ldloc, cache);
                        generator.EmitCall(OpCodes.Callvirt, set_Item, null);
                        break;
                    case '[':
                        {
                            var startLabel = generator.DefineLabel();
                            var checkLabel = generator.DefineLabel();
                            loops.Add((startLabel, checkLabel));

                            generator.Emit(OpCodes.Br, checkLabel);
                            generator.MarkLabel(startLabel);
                        }
                        break;
                    case ']':
                        {
                            if (loops.Count == 0)
                                throw new InvalidProgramException($"Unexpected \"]\" at {i}");
                            var loopInfo = loops.LastOrDefault();
                            generator.MarkLabel(loopInfo.check);
                            generator.Emit(OpCodes.Ldloc, stackRef);
                            generator.Emit(OpCodes.Ldloc, ptrRef);
                            generator.Emit(OpCodes.Callvirt, get_Item);
                            generator.Emit(OpCodes.Brtrue, loopInfo.start);
                            loops.RemoveAt(loops.Count - 1);
                        }
                        break;
                }
            }
            generator.Emit(OpCodes.Ret);
            return dynamicMethod.CreateDelegate<Action>();
        }
    }
}
