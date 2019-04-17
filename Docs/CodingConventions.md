# Именования

- **Всегда**: Используй CamelCase в названиях разныхПеременных, МетодовКлассов и т.д.

- **Всегда**: Используй заглавные буквы согласно договорённости:

| Сущность                                       | Написание         |
|------------------------------------------------|-------------------|
| Namespace                                      | С большой буквы   |
| Type (class, struct)                           | С большой буквы   |
| Method (private, public)                       | С большой буквы   |
| Property (private, protected, public)          | С большой буквы   |
| Event (private, protected, public)             | С большой буквы   |
| Delegate (private, protected, public)          | С большой буквы   |
| Field (private, protected)                     | С маленькой буквы |
| Field (public)                                 | С большой буквы   |
| Enum value                                     | С большой буквы   |
| Parameter                                      | С маленькой буквы |
| Variable                                       | С маленькой буквы |
| Constant (private, protected, public, in-code) | С большой буквы   |

- **Предпочтительно**: Используй для bool такие названия, чтобы название представляло собой вопрос. Например: `IsWidgetHidden`, `AreTasksComplete`, `CanPlayerExit`.

- **Нежелательно**: Использовать Венгерскую нотацию или любой другой способ определения типа в идентификаторах.

```csharp
// Неправильно:
int iCounter;
string strName;

// Правильно:
int counter;
string name;
```

- **Никогда**: Никогда не используй Screaming Caps

```csharp
// Неправильно:
public static const string SHIPPINGTYPE = "DropShip";

// Правильно:
public static const string ShippingType = "DropShip";
```

- **Нежелательно**: Использовать сокращения, кроме общепринятых аббревиатур.

- **Всегда**: Капитализируй аббревиатуры так же, как и обычные слова в названиях. Исключение: аббревиатуры, состоящие из 2 букв

```csharp
// Правильные названия:
public string Html;
public object UI;
private int id;
```

- **Нежелательно**: Использовать символ подчёркивания и другие неалфавитные символы

```csharp
// Неправильно:
public DateTime client_Appointment;

// Правильно:
public DateTime clientAppointment;
```

- **Никогда**: Никогда не используй символ подчёркивания в начале имени

```csharp
// Неправильно:
private DateTime _registrationDate;

// Правильно:
private DateTime registrationDate;
```

- **Всегда**: Начинай название интерфейса с буквы `I`

```csharp
// Правильно:
public interface IShape
{
}
```

- **Предпочтительно**: Использовать названия файлов, соответствующие названию главного класса в них.

- **Всегда**: Синхронизируй структуру файлов и структуру неймспесов

```csharp
// ProjectFolder/Main.cs:
namespace Project
{
  class Main { ... }
}

// ProjectFolder/Module/ClassA.cs:
namespace Project.Module
{
  class ClassA { ... }
}

// ProjectFolder/Module/Submodule/ClassB.cs:
namespace Project.Module.Submodule
{
  class ClassB { ... }
}
```

- **Всегда**: Используй названия в единственном числе для `enum` и в множественном числе для `Flags`

```csharp
public enum Color
{
  Red,
  Green,
  Blue,
}

[Flags]
public enum Dockings
{
  None = 0,
  Top = 1,
  Right = 2,
  Bottom = 4,
  Left = 8,
}
```

- **Никогда**: Никогда не используй обрезанные слова в названиях

```csharp
// Неправильно
GetWin

// Правильно:
GetWindow
```

- **Всегда**: Используй следующие стандартные имена аргументов:
    + Унарный оператор - `value`
    + Equals-подобный метод - `other`
    + Бинарный оператор - `lhs`, `rhs`
    + Метод с N параметрами одинакового типа: `value1`, `value2`, ..., `valueN`

- **Всегда**: Называй ивенты следующим образом:
    + Заканчивай название на `-ing`, если ивент вызывается до или во время главного действия.
    + Заканчивай название на `-ed`, если ивент вызывается после главного действия.
    + Заканчивай название метода на `_EventName`, если он присваивается ивенту с названием `EventName`.
    + Называй метод `OnEventName`, если он вызывает ивент с названием `EventName`.

```csharp
class A
{
  public delegate void UpdateDelegate();
  public event UpdateDelegate Updating;
  public event UpdateDelegate Updated;
  
  private void Update()
  {
    OnUpdating();
    // ...
    OnUpdated();
  }

  private void OnUpdating()
  {
    Updating();
  }

  private void OnUpdated()
  {
    Updated();
  }
}

class MyWidget
{
  private void Method()
  {
    a.Updating += MyWidget_Updating;
    a.Updated += MyWidget_Updated;
  }
}
```

- **Всегда**: называй переменные означающие количество чего-либо в стиле `<something>Count`, где `<something>` в единственном числе.

- **Всегда**: называй переменные индекса как `index` или `<something>`Index, если это не сокращённая форма (i, j, k, etc)

- **Предпочитительно**: формируй имена переменных и методов в стиле естественных языков:

```csharp
// Правильно:
var shortName = "Leo";
var fullName = "Leonardo";

// Неправильно:
var nameFull = "Donatello";
var nameShort = "Don";
```

- **Всегда**: добавляй к методу префикс `try` если выполняются следующие пункты:
    1. обязательно: метод возвращает `bool`
    2. обязательно: если от метода ожидается возвращемое значение, то оно получается через `out`
    3. обязательно: метод не бросит исключений.
    4. желательно: есть такой же метод без `try`, в случае неудачи бросающий исключение.

# Оформление комментариев

- **Всегда**: Пиши комментарии на английском языке.
- **Предпочтительно**: Располагать комментарий на предыдущей строке, а не в конце строки комментируемого кода.
- **Всегда**: Начинай комментарий с заглавной буквы и заканчивай точкой.
- **Всегда**: Вставляй пробел между символами комментария `//` и текстом комментария

```csharp
// Правильно:
// The following declaration creates a query. It does not run 
// the query.
```

- **Никогда**: Никогда не создавай форматированные блоки звёздочек вокруг комментариев

```csharp
// Неправильно:
//****************************
//*  Very important comment  *
//****************************

// Правильно:
// Very important comment
```

- **Предпочтительно**: Указывать автора в тексте комментари

```csharp
// Правильно:
// Linus Torvalds: this method was not implemented
// because it would take too much time.
```

- **Всегда**: Оставляй комментарий, если вкручиваешь костыль или реализуешь какую-то неочевидную логику.
- **Никогда**: Никогда не используй комментарии в блоках `/**/`.

# Форматирование

- **Всегда**: Используй табуляцию вместо пробелов для отступов.
- **Всегда**: Используй пробелы вместо табуляции для выравнивания.
- **Всегда**: Используй вертикальное выравнивание скобочек вместо египетского выравнивания.
Исключения:
    - if
    - циклы
    - switch
    - object and collection initializers
    - lambda expressions


- **Всегда**: Вставляй пробелы между бинарными операторами и операндами.

- **Всегда**: Для однострочных лямбд, object and collection initializers отделяй внутреннюю часть от фигурных скобок одним пробелом.

- **Всегда**: Отделяй `[]` пробелом справа и не отделяй слева, в синтаксисе объявлений и инициализаций.

```csharp
// Правильно:
int[] dogs = new int[] { 1, 2, 3 };
// Не правильно:
int [] dogs = new int [] { 1, 2, 3 };
```

- **Предпочтительно**: Использовать LINQ в виде цепочек методов, а не sql-подобные LINQ вызовы

```csharp
// Нежелательно:
var names = from item in collection
            select item.Name;

// Желательно:
var names = collection
            .Select(item => item.Name);
```

- **Всегда**: Форматируй switch statement согласно следующим правилам:
  - однострочные `case` допустимы, если тело `case` из одной строчки считая `break`
  - вставляй отступ перед case
  - если тело case обрамляется в фигурные скобки, то откывающая скобка должна быть расположена на одной строчке с case, а закрывающая на отдельной строчке, на уровне вложенности соответствующего case; `break` или `return` в таком случае должен оставаться внутри фигурных скобок

Примеры правильного форматирования `switch` statement:

```csharp
public List<ChestBonusConfig> Config
{
  get
  {
    switch (Kind) {
      case SlotKind.StarterPack: return The.Config.StarterPackContents;
      case SlotKind.ProductPack: return The.Config.GetProductPackConfig(Index);
      case SlotKind.CoinPack: case SlotKind.ButtonMore: return null;
      default: throw new ArgumentOutOfRangeException();
    }
  }
}

private Widget GetBasicWidget()
{
  switch (ParticlesLinkage) {
    case ParticlesLinkage.Parent:
      return ParentWidget;
    case ParticlesLinkage.Other: {
      var widget = ParentWidget;
      while (widget != null) {
        if (widget.Id == LinkageWidgetName) {
            return widget;
        }
        widget = widget.ParentWidget;
      }
      return null;
    }
    case ParticlesLinkage.Root:
    default:
      return (Parent != null) ? WidgetContext.Current.Root : null;
}

switch (type) {
  case SignatureType.BinaryUnordered:
    yd = new BinaryDeserializer {
      Options = defaultYuzuCommonOptions,
      BinaryOptions = new BinarySerializeOptions { Unordered = true },
    };
    break;
  case SignatureType.BinaryOrdered:
    yd = new BinaryDeserializer {
      Options = defaultYuzuCommonOptions,
    };
    break;
  case SignatureType.Json:
    SkipBomIfPresent(ms);
    yd = new JsonDeserializer {
      JsonOptions = defaultYuzuJSONOptions,
      Options = defaultYuzuCommonOptions
    };
    break;
  default:
    throw new System.Exception("Unexpected Case");
}
```

- **Всегда**: Используй кодировку UTF-8.


# Расположение и структура кода

- **Всегда**: Пиши только одно выражение в строке

```csharp
// Неправильно:
a = 5; b = "kek"; c = new Example(a, b);

// Правильно:
a = 5; 
b = "kek"; 
c = new Example(a, b);
```


- **Всегда**: Пиши только одно объявление в строке

```csharp
// Неправильно:
int a, b; string c; char d;

// Правильно:
int a; 
int b; 
string c; 
char d;
```


- **Нежелательно**: Писать выражения на одной строке с if

```csharp
// Нежелательно:
if (true) return;

// Желательно:
if (true){
  return;
}
```

- **Предпочтительно**: Оборачивать даже однострочные выражения в фигурные скобки

```csharp
// Нежелательно:
if (true)
  return;

// Желательно:
if (true){
  return;
}
```

- **Всегда**: Добавляй одну пустую строку между объявлениями методов и свойств.

- **Нежелательно**: Писать строки длиннее 100 символов.

- **Никогда**: Никогда не пиши строк длиннее 120 символов.

- **Всегда**: Оставляй перенос на новую строку в конце файла.

- **Всегда**: Оставляй бинарный оператор на той же строке, что и первый операнд.

```csharp
// Неправильно:
var a = 
  someLongLongLongLongLongLongLongVariableName1 
  || someLongLongLongLongLongLongLongVariableName2;

// Правильно:
var a = 
  someLongLongLongLongLongLongLongVariableName1 ||
  someLongLongLongLongLongLongLongVariableName2;
```

- **Всегда**: Добавляй перенос строки между return и длинным выражением.

```csharp
// Неправильно:
return someLongLongLongLongLongLongLongVariableName1 ||
  someLongLongLongLongLongLongLongVariableName2;

// Правильно:
return 
  someLongLongLongLongLongLongLongVariableName1 ||
  someLongLongLongLongLongLongLongVariableName2;
```

- **Всегда**: Располагай длинные выражения отдельно от if и скобочек.

```csharp
// Неправильно:
if (someLongLongLongLongLongLongLongVariableName1 ||
  someLongLongLongLongLongLongLongVariableName2) {
  ...
}

// Правильно:
if (
  someLongLongLongLongLongLongLongVariableName1 ||
  someLongLongLongLongLongLongLongVariableName2
) {
  ...
}
```

# Использование языка

- **Всегда**: Используй обобщённые названия типов вместо системных типов, таких как `Int16`, `Single`, `UInt64` и т.д.

```csharp
// Неправильно:
String firstName;
Int32 lastIndex;
Boolean isSaved;

// Правильно:
string firstName;
int lastIndex;
bool isSaved;
```

- **Всегда**: Используй `var` для всех типов кроме простых.
- **Нежелательно**: Использовать var для простых типов.
- **Всегда**: Явно указывай модификатор доступа `private`.
- **Нежелательно**: Использовать директиву `#region`.
- **Предпочтительно**: Использовать авто-свойства (Auto-Implemented Properties) вместо public полей.
- **Всегда**: дополняй окончания блочных директив препроцессора комментарием, дублирующим выражение в первой директиве данного блока в той же строке.

```csharp
// Например
#if ANDROID && BFG_LIB
      Lime.Application.DiscardOpenGLObjects();
#endif // ANDROID && BFG_LIB
```

- **Желательно**: Указывать имя параметра в месте вызова, если параметр типа `bool`
```csharp
// Например
void Func(bool flag) {}
Func(flag: true);
```

# Документация

- **Предпочтительно**: Использовать следующие тэги:
    - `<param>`
    - `<see>`
    - `<summary>`

- **Никогда**: Никогда не используй тэг `<see>` в комментарии более одного раза.
