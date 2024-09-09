# 코딩 스타일 
- [세종대왕 프로젝트 (한글 코딩 컨벤션)](https://tosspayments-dev.oopy.io/chapters/frontend/posts/hangul-coding-convention )  
- [dotnet/runtime 코딩 스타일](https://github.com/dotnet/runtime/blob/main/docs/coding-guidelines/coding-style.md)을 따라간다  
- [(일어) C# CODING GUIDELINES 2024](https://qiita.com/Ted-HM/items/1d4ecdc2a252fe745871)
  
---    
## 필수
- 헝가리어 표기법을 사용하지 않는다.
- 쉽게 읽을 수 있는 식별자 이름을 선택한다. 
    - 예를 들어, 영어에서는 `AlignmentHorizontal` 라는 속성 이름보다 `HorizontalAlignment` 라는 이름이 읽기가 더 쉽다.
- 가독성에 중점을 둔다. 
    - 예를 들어, 전자의 경우 X 축에 대한 참조가 명확하지 않은 ScrollableX라는 속성 이름보다는 CanScrollHorizontally라는 이름이 더 좋다.
- 간결하게 할 수 있다면 간결하게 한다  
    ```
	if(isJump)   // if(isJump == true) 보다 좋다

	// 아래와 같은 주석은 달지 않는다
	// 1 + 1 을 하고 있다
	int a = 1 + 1;
	```  
- 밑줄, 하이픈 또는 기타 영숫자가 아닌 문자를 사용하지 않는다.
- 변수, 함수, 인스턴스 등을 하드 프린트하는 대신 nameof("...")을 사용한다
- 형식 참조를 위한 프레임워크 형식 이름 대신 언어 키워드 사용 (예 String(닷넷 기본 타입) -> string(String의 별칭)
    - 단 정수 타입은 비트의 크기가 붙은 것을 사용한다. int -> Int32
- 널리 사용되는 프로그래밍 언어의 키워드와 충돌하는 식별자를 사용하지 않는다.
- **예외적으로 IDE에서 자동으로 포맷팅하여 코딩룰이 바뀌는 경우는 이것에 따라도 괜찮다. 단 이 경우 사용하는 IDE를 일치시킨다**
- 매직넘버를 사용하지 않는다.   
    
### 클래스, 구조체
- 이름은 파스칼식 대/소문자를 사용한다.  
	```
	public class PlayerMove
	```  
- Private, Internal 필드:	 
    - `_`로 시작해서 `_camelCase` 식을 사용한다.  `int _currentHP;`
	- 필드 이름은 명사 또는 명사구를 사용하여 지정합니다.
	- 정적 필드는 `s_`를 붙인다.   `static int s_num;`
	- thread static 필드는 `t_`를 붙인다,   `[ThreadStatic] static int t_num;`
- Public 필드
	- 최대한 쓰지 않는다. 
	- 파스칼식 대/소문자 사용    
  

### 함수
- 함수의 이름은 파스칼식 대/소문자를 사용한다. 
    - `void Move()`
- 지역 변수, 매개 변수 이름에는 카멜식 대/소문자를 사용합니다. 
    - `int moveSpeed;`
- 설명이 포함된 매개 변수 이름을 사용합니다.
- 접근 제한자 중 `private`은 기본 적용이므로 별도로 표기하지 않는다. 
    - 예) `public string UserID` (0), `private string UserID` (X)
- 매개 변수 타입을 기반으로 하는 이름이 아닌, 매개 변수의 의미를 기반으로 하는 이름을 사용할 수 있다.  	
  

### 정수는 파스칼식 대/소문자를 사용한다.
```
enum Action
{
	Idle,
	Move,
}

const int MaxHp = 100;
readonly int MaxSpeed = 5;
```   
    
  
### 코드 블락 정리
- [올맨 스타일](http://en.wikipedia.org/wiki/Indent_style#Allman_style) 의 각 중괄호는 새로운 줄에서 시작한다. **조건문이 한줄이라도 중괄호**를 사용한다
- 인덴트는 4개의 빈칸을 사용한다
- 빈줄은 최대 1개만 사용한다. (가독성용)
- 코드 끝자락에 빈칸은 생략한다. 

  
<br>  
  
---    
## 권고(가능하면 지켜야됨) 
  
### 표준적으로 사용되는 접미사
- 인터페이스는 `I-`를 붙인다.
- 기능을 나타내는 인터페이스는 `-able`을 붙인다.
- 추상 클래스는 `-Base`를 붙인다.
- 파생 클래스는 원래의 클래스 이름을 포함한다.
- 속성 클래스는 `-Attribute`를 붙인다.
- 처리 중심 클래스는 `-er`를 붙인다.
- 비동기 메서드는 `-Async`를 붙인다.
- 이벤트 핸들러 메서드는 `On-` 또는 `ControlName_On-`으로 시작한다.
- 유형 매개변수는 `T` 또는 `T-`로 시작한다.
  

### 메서드 이름은 동사구로 한다.
- 인스턴스를 <주어>라고 하면, 메서드 이름은 <동사>가 된다.
- 변환을 나타내는 메서드는 예외적으로 `To-`나 `As-`를 사용한다.
- 자연스러운 영문으로 읽는 것이 이상적입니다.
  
- ✔️DO 자연스러운 메서드 이름: `server.Restart()`, `enemy.target)`, `enemy.Attack(target)`
- ✔️DO 변환을 나타내는 메서드 이름: `obj.ToString()`, `obj.AsSpan()`
- ❌DO NOT 단일 책임이 아님: `server.StopAndStart()`, `enemy.MoveToAndAttack(target)`
- ❌DO NOT 동사가 아님: `server.Name()`, `enemy.Hp()`  
  

### 논리값을 나타내는 이름은 is-, has-, can- 를 붙인다
논리값을 나타내는 변수나 속성의 경우 `is-< 과거형>`, `has-< 과거분사>`, `can-< 현재형>`을 붙인다.  
<동사>로 시작하는 것은 메서드 이름이지만, 논리값을 나타내는 이름에 대해서는 예외로 사용한다.    
- ✔️DO 변수 이름: `bool isItemExist, hasChanges, isChanged, isChanged, canSave, isSaved, isSaved.canSave, isSaved, hasTransaction`
- ✔️CONSIDER 변수 이름: `bool shouldUpdate, needsCommit`
❌DO NOT 비추천: `bool doesItemExist, itemsExists, existed, doesSave, saved, saves`  
   

속성명의 경우 인스턴스를 <주어>로, Is-<명사>로 상태를 나타내는 표현이 많다.  
과거 MSDN에서는 속성의 경우 과거형을 사용하는 것이 권장되었으나, 이벤트 이름과 겹치기 때문에 현재는 권장하지 않는다.    
- ✔️DO 프로퍼티 이름: `bool IsRefreshing { get; }, HasError { get; }, CanExecute { get; }`
- ❌DO NOT 비권장: `bool Refreshing { get; }, Refreshed { get; }`
  

메서드 이름의 경우 Is-Is- 명사도 있지만, 동사s로 삼단현(삼인칭 단수 현재형)으로 표현한다.  
- ✔️DO 메서드 이름: `bool IsNullOrEmpty(...), Exists(...), Contains(...)`
- ❌DO NOT 비추천: `boolIsExisted(...), IsContained(...)`
  
  
### 컬렉션이나 목록의 변수 이름은 복수형으로 한다.
컬렉션이나 리스트의 변수 이름은 복수형으로 한다.  
  
단수형: File file, Item item, byte singleByte
- ✔️DO 복수형: `List<File> files, IEnumerable<Item> items, byte[] bytes`
- ❌DO NOT 비추천: `List<File> fileList, IEnumerable<Item> itemList`
  

단수형과 복수형이 다른 용어에 주의가 필요하다.  
- Child - Children
- Person - People
- Man - Men
- Woman - Women
- Index - Indexes(색인) - Indices(지수)
- Axis - Axes(축)
- Analysis - Analyses(분해)
  

셀 수 없는 명사 등 복수형이 없는 것은 예외적으로 `-s`를 강제로 붙일 수 있다.    
- Series - Serieses
- Information - Informations
- Knowledge - Knowledges
- Software - Softwares
- Logic - Logics
  

Data는 Datum의 복수형이지만 단수형으로 사용된다.  
`-Data`를 붙이면 중복되므로 가급적 사용하지 않는다.  
`Dataset`은 Data의 집합을 의미한다.  
- Data - Dataset, DataSet 
  

영어의 관례로 단위에 `-s`를 붙여서 표현한다.   
배열 표현과 혼동할 수 있으므로 가급적 사용하지 않는다.   
- Milliseconds - Msec
- Retries - RetryCount
- Bytes - ByteCount
  
 
### 네임스페이스와 클래스 이름 중복 피하기
네임스페이스와 클래스명이 같으면 문제가 생긴다.  
네임스페이스는 표기가 흔들리지 않도록 약어를 피하고, 중복되지 않도록 복수형을 사용한다.  
- ✔️DO 중복되지 않음: `Xxx.Logging.Logger, Xxx.Configuration`
- ❌DO NOT 비추천: `Xxx.Logger.Logger, Xxx.Config.Config`
  

### 열거형 비트 필드는 복수형으로 한다.
열거체, 열거값 모두 파스칼 케이스이다.  
열거체 이름은 기본적으로 단수이지만 [Flags](비트 필드)를 나타낼 때는 복수형으로 한다.  
열거형을 정의할 때는 사용하지 않아도 `None = 0` 을 정의한다.    
```
[Flags]
public enum Permissions
{
	None = 0,
	Read = 1 << 0, // 1
	Write = 1 << 1, // 2
	Execute = 1 << 2, // 4
	ReadWrite = Read | Write, // 3
	All = Read | Write | Execute, // 7
	// 시프트 연산자도 표현할 수 있다
	//All = ~(-1 << 3),
}
```  
  
C#7.0(.NET4.6/Core1.0)부터 이진수 리터럴을 사용할 수 있다.  
시각적으로 비트 표현을 확인할 수 있어 계산이 불필요하다.  
동등한 위치를 정렬하려면 VS 확장을 사용한다.  
#pragma 지시문으로 문서 서식을 비활성화하여 정렬을 위한 공백을 남겨둔다.    
```
#pragma warning disable format
[Flags]
public enum Permissions
{
	None      = 0b0000,
	Read      = 0b0001,
	Write     = 0b0010,
	Execute   = 0b0100,
	ReadWrite = 0b0011,
	All       = 0b0111,
}
#pragma warning restore format
```  
  

### 메서드 길이는 한 화면에 들어갈 정도로만 작성한다
긴 메소드는 읽는 데 시간이 오래 걸리므로 적절한 길이의 메소드로 나눈다.  
단일 책임 원칙을 의식하면 메소드를 짧게 유지할 수 있다.  
일반적으로 30줄 정도, 길어도 100줄 정도를 기준으로 한다.  
  

### 긴 줄은 줄바꿈하기
가로 스크롤이 표시되면 가독성이 떨어지므로 피하는 것이 좋다.  
모바일은 65자, 데스크톱은 80자, 와이드 디스플레이는 100자에서 120자, 최대 140자 정도를 기준으로 한다.  
```
// 인수가 많은 경우는 `,`(콤마)  뒤로 걔항한다
public class Person(
	int id,
	string name,
	DateTime? birthDate,
	Sex sex
)
{
	public int Id { get; set; } = id;
	public string Name { get; set; } = name;
	public DateTime? BirthDate { get; set; } = birthDate;
	public Sex Sex { get; set; } = sex;
}

// 타입 파라미터가 긴 경우는 `where` 앞에서 개행한다
public abstract class DbRepositoryBase<TEntity> : IDbRepository<TEntity>
	where TEntity : class
{
	// 메서드의 람다식이 긴 경우는 `=>`(람다 연산자, 애로우 연산자) 뒤로 개행한다public virtual TEntity? Get(Func<TEntity, bool> predicate) =>
		this._dbSet.FirstOrDefault(predicate);
}

// 승계가 긴 경우는  `:`(콜론) 앞에서 개행한다
public class PersonDbRepository(DbContext Context)
	: DbRepositoryBase<Person>(Context);

// 선언이 긴 경우는 `.`(피리오드) 뒤에서 개행한다
var currentPerformanceCounterCategory = new global::System.
	Diagnostics.PerformanceCounterCategory();

// 람다식의 `{}` 은 메서드 선언행 개시 위치에 맞춘다
Action<string> printMessage = (msg) =>
{
	Console.WriteLine(msg);
};~
```
  
## 연산자 개행 위치
연산자가 많거나 1행이 긴 경우 연산자의 앞뒤에서 개행한다.  
일기 어려운 경우는 임시 변수를 이용하여 설명적인 이름을 붙이는 것을 검토한다
```
// C#7.0(.NET4.6/Core1.0) 에서는  is 연산자를 사용할 수 있다
// if 의 경우는 연산자 뒤에서 개행하면 괄호의 위치를 앞으로 한다
if ((data is null) ||
	(data == DBNull.Value) ||
	(data is string s && s.Length == 0))

// return 경우는 연산자 앞에서 개행하면 괄호 위치를 앞으로 한다
return (data is null)
	|| (data == DBNull.Value)
	|| (data is string s && s.Length == 0);

// [✔️CONSIDER] 설명 변수를 이용하면 가독성이 올라간다
var isNullOrDBNullOrEmpty = (data is null)
	|| (data == DBNull.Value)
	|| (data is string s && s.Length == 0);

// 삼항 연산자를 개행하는 경우는 연산자를 앞에 둔다
var result = condition
	? "success"
	: "failure";
```
  
  
### 범위를 나타내는 비교 연산자 방향
비교 연산자는 왼쪽을 주로 사용하는 것이 일반적이다.  
범위를 다룰 때는 수학에서와 마찬가지로 비교 연산자의 방향을 정렬하면 가독성이 높아진다.    
```
// 비교 연산자의 왼쪽을 기준으로 한 경우
if ((a >= 90 && a <= 180) ||
	(a >= 270 && a <= 360))

// [✔️CONSIDER] 비교 연산자의 방향으로 앞으로 한 경우
if ((90 <= a && a <= 180) ||
	(270 <= a && a <= 360))

// [✔️DO] 인덱스를 다루는 경우는 포함 배타를 의식해서 `<` 으로 한다
for (var i = 0; i < length; i++)
```
  

### 빈 생성자는 한 줄로 작성한다.
빈 생성자는 한 줄로 작성한다.  
코드를 접어도 식별이 가능하기 때문에 전개할 필요가 없다.  
```
// ctor -> tab -> tab 으로 보완한다.
public MyConstructor() : base() { }

// [❌AVOID] 내용이 있는 것처럼 보인다
public MyConstructor() : base()
{
}
```
  
  
### 줄 끝 주석을 사용하지 않는다
줄 끝 주석은 편집에 방해가 되므로 피한다.  
```
// [✔️DO] 주석은 다른 행에 기술한다
var a = 0;

var abcde = 0; // [❌AVOID] 
var a = 0;     // [❌DO NOT] 
```
  
  
### 로컬 변수의 범위를 작게 한다.
로컬 변수는 사용할 때 선언하고, 생존 기간을 최대한 짧게 유지한다.
범위를 최소화하면 가독성이 향상되고 불필요한 처리가 없어진다.
  
  
### 증분은 별도의 줄에 넣는다.
인크리멘트를 수식 안에 사용하면 고려해야 할 사항이 늘어난다.
별도의 행으로 만들어서 앞뒤 구분을 의식할 필요가 없다.  
```
// [✔️CONSIDER] 증분은 다른 행에서 한다
while (i < numbers.Length)
{
	numbers[i] = 0;
	i++;
}

// [❌AVOID] 전치와 후치를 착각하면 IndexOutOfRangeException 가 발생한다
while (i < numbers.Length)
{
	numbers[++i] = 0; // 예외발생
}

// 루프 하지 않은 경우는 식 안에서 증분하기도 한다
messages[i++] = "Hello, world!";
messages[i++] = "Welcome to the system.";
messages[i++] = "Error: Invalid input.";
messages[i++] = "Goodbye!";
messages[i++] = "Thank you for visiting.";
```
  
  
### 흐름 제어에 예외를 사용하지 않는다.
예외는 관리되는 goto 이다.
따라서 흐름 제어에 예외를 사용하면 가독성과 유지보수성이 떨어진다.
그러나 비동기 취소 예외는 흐름 제어에 사용된다.  
```
static async Task Main()
{
	try
	{
		var cts = new CancellationTokenSource();
		await Task.Run(() => DoWork(cts.Token), cts.Token);
	}
	catch (OperationCanceledException ex)
	{
		// 취소 예외처리
	}
}

// C#7.1(Core2.0) 에서는 default 식을 사용할 수 있다
// default 는 컴파일 시에 결정되는 값이다
static void DoWork(CancellationToken token = default)
{
	// 예외로 종료하는 패턴
	while (true)
	{
		token.ThrowIfCancellationRequested();
		...
	}

	// 예외를 발생시키지 않는 패턴
	while (token != default && !token.IsCancellationRequested)
	{
		...
	}
}
```
 
  
### Dispose() 후에는 null을 설정한다.
객체는 Dispose() 후 바로 메모리에서 해제되지 않는다.  
타이밍에 따라서는 문제없이 접근할 수 있다.  
null을 설정하면 위험한 접근을 피할 수 있다.  
?(Null 조건 연산자)와 함께 사용하면 Dispose()를 두 번 수행하는 것을 방지할 수 있다.  
  

### 파이널라이저를 사용하지 않는다.
Dispose()가 제대로 된 경우, 파이널라이저는 필요하지 않다.  
파이널라이저가 있으면 파이널라이저가 파이널라이즈 큐에 추가되어 Gen 1이 되므로 메모리 해제가 늦어진다.  
  
높은 신뢰성이 요구되는 경우 Dispose()를 잊어버리는 안전장치로 파이널라이저를 사용하기도 한다.  
Dispose() 한 경우에는 파이널라이저가 필요 없다는 GC.SuppressFinalize(this)을 호출한다  
  
NET5 이상에서는 애플리케이션 종료 시 파이널라이저가 호출되지 않는다.  
using을 사용하여 외부 리소스의 소켓을 닫는 것을 잊어버려 연결 수가 부족하지 않도록 주의한다.  

  
### 예외 처리는 가급적 사용하지 않는다.
예외 처리, 스택 트레이싱은 부하가 많이 걸리므로 가급적 피해야 한다.  
예외로 감지할 수밖에 없는 부분도 있지만, 가급적 예외를 제어에 사용하지 않도록 한다.  예를 들어, 예외가 발생하는 `Parse()` 보다 `TryParse()`를 사용합니다.  