Библиотека поставляеться как есть, и используеться только на свой страх и риск, являеться дополнением к проекту SocketNetworkFramework

О библиотеке, библиотека позволяет сериализировать данные в бинарный вид и десериализировать их по указанной структуре обратно 

По умолчанию для сериализации стурктуры используеться код вида

Пример стурктуры:

public class TempStruct
{
	[Binary(typeof(BinaryInt32))]
	[BinaryScheme("default")]
	[BinaryScheme("number")]
	public int TestNumber { get; set;}

	[Binary(typeof(BinaryFloat32))]
	[BinaryScheme("default")]
	[BinaryScheme("number")]
	public float TestNumber2 { get; set;}

	[Binary(typeof(BinaryFloat64))]
	[BinaryScheme("default")]
	[BinaryScheme("number")]
	public double TestNumber3 { get; set;}

	[Binary(typeof(BinaryString16))]
	[BinaryScheme("default")]
	[BinaryScheme("text")]
	public string TestText1 { get; set;}

	[Binary(typeof(BinaryString32))]
	[BinaryScheme("default")]
	[BinaryScheme("text")]
	public string TestText2 { get; set;}
}

Пример данных:

TempStruct ts1 = new TempStruct();

ts1.TestNumber = 15;

ts1.TestNumber2 = 25;

ts1.TestNumber3 = 75;

ts1.TestText1 = "Pro100Text";

ts1.TestText2 = "Pro200Text2";

Пример кода сериализации:

BinarySerializer bs = new BinarySerializer();

byte[] tempNumArr = bs.Serialize("number",ts1);

byte[] tempTextArr = bs.Serialize("text",ts1);

byte[] tempFullArr = bs.Serialize("default",ts1);

Пример кода десериализации:

TempStruct tempNumValue = bs.Deserialize<TempStruct>("number", tempNumArr);

TempStruct tempTextValue = bs.Deserialize<TempStruct>("text", tempTextArr);

TempStruct tempFullValue = bs.Deserialize<TempStruct>("default", tempFullArr);

Типы - используеться обозначения используемые в языке C, на примере целочисленных  BinaryInt32, BinaryInt64, или же в случае текста, BinaryString16, BinaryString32, где последние цифры означают кол-во байт хедера размера записываемых/читаемых в потоке, помимо поддержки базовых типов есть возможность серриализровать вложенные типы, заменив значение вида [Binary(typeof(BinaryString32))], на [Binary(typeof(MyType))], можно сериализовать по типам выше в иерархии наследования, так-же есть поддержка типов для List, Dictionary, Array, ConcurrentDictionary, где по аналогии с типами string есть типы с заголовками содержащие кол-во, типы которые являються кол-венными string, List,Array, etc имеют модели сериализации без хедеров, BinaryString, BinaryArray, etc, для них в BinaryAttribute есть 
TypeSize = Статический размер типа (пока актуально только для string)
TypeSizeName = Имя свойства которое содержит размер типа (если задано, TypeSize игнорируеться)
ArraySize = Статический размер массива (актуально для всех поддерживаемых IEnumerable типов)
ArraySizeName = Имя свойства которое содержит размер массива (если задано, ArraySize игнорируеться)

TypeStorage - (кэширование) хранилище структур и их вариаций которое обычно поставляеться в 1 экземпляре (TypeStorage.Instance), можно создавать отдельные экземпляры если это необходимо

Structorian - дополнение библиотеки позволяющее описывать стурктуру кодом за место атрибутов, 

Пример на основе TempStruct описанной выше

            StructBuilder<TempStruct>
                .GetStruct(TypeStorage.Instance) //TypeStorage.Instance можно не указывать, по умолчанию береться значение TypeStorage.Instance если не указано

                .SetSchemes("default") //Установка схем для всех свойств что будут описаны ниже, не обязательно

                .GetProperty(x => x.TestNumber) // Получение свойств по lambda выражению, есть перегрузка которая приминимает название в типе string, актуально для protected и других модификаторов доступа которые поддерживаються
                .SetBinaryType<BinaryInt32>() // Установка типа обработчика
                .AppendScheme("number") // Добавление схемы number к схеме описанной в SetSchemes()
                .SaveProperty() // сохранить настройки свойства, что-бы выйти из его редактирования

                .GetProperty(x => x.TestNumber2)
                .SetBinaryType<BinaryFloat32>()
                .AppendScheme("number")
                .SaveProperty()

                .GetProperty(x => x.TestNumber3)
                .SetBinaryType<BinaryFloat64>()
                .AppendScheme("number")
                .SaveProperty()

                .GetProperty(x => x.TestNumber3)
                .SetBinaryType<BinaryString16>()
                .AppendScheme("text")
                .SaveProperty()

                .GetProperty(x => x.TestNumber3)
                .SetBinaryType<BinaryString32>()
                .AppendScheme("text")
                .SaveProperty()

                .Compile(); // сохранение структуры в указанный TypeStorage


Помимо SetBinaryType нужно так-же заменить что есть метод SetPartialType, который позволяет подключать другие классы для обработки

                .GetProperty(x => x.MyClassVar)
                .SetPartialType<MyClass>()
				//тут важно заметить что можно здесь так-же описать свойства вложенного класса, но при этом вместо Back() нужно вызвать Compile() если этот класс описан где-то отдельно то этого делать не нужно и мы просто вызываем Back() возвращаясь к описанию предидущего класса
				.Back()
                .AppendScheme("text")
                .SaveProperty()
так-же Structorian содержит методы для установки таких значения как 
TypeSize
TypeSizeName
ArraySize
ArraySizeName

и возможность предкомпилировать структуру установив BinaryPreCompileAttribute

важно так-же заметить что 1 и тот же класс можно описывать бесконечное кол-во раз, все данные будут накладываться, только есть несколько условий
1. Тип класса в одном описании не должен отличаться в другом описании ибо словим Exception
2. TypeSize в одном описании не должен отличаться в другом описании ибо словим Exception
3. TypeSizeName в одном описании не должен отличаться в другом описании ибо словим Exception
4. ArraySize в одном описании не должен отличаться в другом описании ибо словим Exception
5. ArraySizeName в одном описании не должен отличаться в другом описании ибо словим Exception

BinaryPreCompileAttribute - атрибут которым помечаеться класс для предварительной компиляции в финальный вид и использование, что-бы не создавать задержку при первом использовании, должен указываться с названием схемы которую нужно скомпилировать, и начальным размером буффера, не может корректно работать с Structorian описанным выше, если вызвать метод PreCompileBinaryStructs(Assembly.GetExecutingAssembly()) в TypeStorage до того как Structorian опишет эти структуры


Начальный размер буффера - это инициализированный массив байт который не будет расширяться если расчитать и указать необходимый что приведет к ускорению работы