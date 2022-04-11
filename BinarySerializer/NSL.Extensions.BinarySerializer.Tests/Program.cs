﻿using System;

namespace BinarySerializer_v5.Test
{
    class Program
    {
        static void Main(string[] args)
        {

            DebugTemp.TStruct.Debug();

            Utils.bs.BuildTypes();

            Test t = new Test();

            //BinarySerializer.TypeStorage.Instance.PreCompileBinaryStructs(Assembly.GetExecutingAssembly());

            Structs.StringStruct _ss = Structs.StringStruct.GetRndValue();
            Structs.IntegerStruct _is = Structs.IntegerStruct.GetRndValue();
            Structs.FloatStruct _fs = Structs.FloatStruct.GetRndValue();
            Structs.ArrayStruct _as = Structs.ArrayStruct.GetRndValue();
            Structs.ListStruct _ls = Structs.ListStruct.GetRndValue();
            Structs.DictionaryStruct _ds = Structs.DictionaryStruct.GetRndValue();
            Structs.OtherStruct _os = Structs.OtherStruct.GetRndValue();

            int iteration = 10000;

            Console.WriteLine("BinarySerializer");


            t.Run(iteration, "Integer Serialize", _is.bsSerializeAction);
            t.Run(iteration, "Integer Desserialize", _is.bsDesserializeAction);

            Console.WriteLine();
            Console.WriteLine("Stream");
            t.Run(iteration, "Integer Write", _is.streamWriteFunc);
            t.Run(iteration, "Integer Read", _is.streamReadFunc);

            _is.Compare();


            Console.WriteLine();
            Console.WriteLine("BinarySerializer");
            t.Run(iteration, "Float Serialize", _fs.bsSerializeAction);
            t.Run(iteration, "Float Desserialize", _fs.bsDesserializeAction);

            Console.WriteLine();
            Console.WriteLine("Stream");
            t.Run(iteration, "Float Write", _fs.streamWriteFunc);
            t.Run(iteration, "Float Read", _fs.streamReadFunc);

            _fs.Compare();

            Console.WriteLine();
            Console.WriteLine("BinarySerializer");
            t.Run(iteration, "String Serialize", _ss.bsSerializeAction);
            t.Run(iteration, "String Desserialize", _ss.bsDesserializeAction);

            Console.WriteLine();
            Console.WriteLine("Stream");
            t.Run(iteration, "String Write", _ss.streamWriteFunc);
            t.Run(iteration, "String Read", _ss.streamReadFunc);

            _ss.Compare();

            Console.WriteLine();
            Console.WriteLine("BinarySerializer");

            t.Run(iteration, "Array Serialize", _as.bsSerializeAction);
            t.Run(iteration, "Array Desserialize", _as.bsDesserializeAction);

            Console.WriteLine();
            Console.WriteLine("Stream");
            t.Run(iteration, "Array Write", _as.streamWriteFunc);
            t.Run(iteration, "Array Read", _as.streamReadFunc);

            _as.Compare();

            Console.WriteLine();
            Console.WriteLine("BinarySerializer");

            t.Run(iteration, "List Serialize", _ls.bsSerializeAction);
            t.Run(iteration, "List Desserialize", _ls.bsDesserializeAction);

            Console.WriteLine();
            Console.WriteLine("Stream");
            t.Run(iteration, "List Write", _ls.streamWriteFunc);
            t.Run(iteration, "List Read", _ls.streamReadFunc);

            _ls.Compare();

            Console.WriteLine();
            Console.WriteLine("BinarySerializer");

            t.Run(iteration, "Dictionary Serialize", _ds.bsSerializeAction);
            t.Run(iteration, "Dictionary Desserialize", _ds.bsDesserializeAction);

            Console.WriteLine();
            Console.WriteLine("Stream");
            t.Run(iteration, "Dictionary Write", _ds.streamWriteFunc);
            t.Run(iteration, "Dictionary Read", _ds.streamReadFunc);

            _ds.Compare();

            Console.WriteLine();
            Console.WriteLine("BinarySerializer");

            t.Run(iteration, "Other Serialize", _os.bsSerializeAction);
            t.Run(iteration, "Other Desserialize", _os.bsDesserializeAction);

            Console.WriteLine();
            Console.WriteLine("Stream");
            t.Run(iteration, "Other Write", _os.streamWriteFunc);
            t.Run(iteration, "Other Read", _os.streamReadFunc);

            _os.Compare();

            Console.ReadKey();
        }
    }
}
