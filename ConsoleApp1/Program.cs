using System;
using BinarySerializer;
namespace BinarySerializer_v5.Test
{
    class Program
    {
        static void Main(string[] args)
        {

            Test t = new Test();

            t.Run(2, "Integer Serialize", Structs.IntegerStruct.bsSerializeAction);
            t.Run(2, "Integer Desserialize", Structs.IntegerStruct.bsDesserilize);

            t.Run(2, "Float Serialize", Structs.FloatStruct.bsSerializeAction);
            t.Run(2, "Float Desserialize", Structs.FloatStruct.bsDesserilize);

            t.Run(2, "String Serialize", Structs.StringStruct.bsSerializeAction);
            t.Run(2, "String Desserialize", Structs.StringStruct.bsDesserilize);

            t.Run(1000, "Array Serialize", Structs.ArrayStruct.bsSerializeAction);
            t.Run(1000, "Array Desserialize", Structs.ArrayStruct.bsDesserilize);

            t.Run(1000, "List Serialize", Structs.ListStruct.bsSerializeAction);
            t.Run(1000, "List Desserialize", Structs.ListStruct.bsDesserilize);

            t.Run(1000, "Dictionary Serialize", Structs.DictionaryStruct.bsSerializeAction);
            t.Run(1000, "Dictionary Desserialize", Structs.DictionaryStruct.bsDesserilize);

            Console.ReadKey();
        }
    }
}
