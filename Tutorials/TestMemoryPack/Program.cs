// See https://aka.ms/new-console-template for more information
using System.Xml.Linq;
using MemoryPack;
using TestMemoryPack;

Console.WriteLine("Hello, MemoryPack!");

Test1();
Console.WriteLine("");

Test2();
Console.WriteLine("");

Test3();
Console.WriteLine("");

Test6();


// 기본적인 직렬화가 잘 되는지 테스트
void Test1()
{
    Console.WriteLine("[ Test 1 ] 기능 검증");

    // 직렬화 하면 앞에 1 바이트는 갯수, 이후는 데이터 순서대로 직렬화한다
    var bin = MemoryPackSerializer.Serialize(new TestClass
    {
        Id = 100,
        Name = "Hello"
    });

    
    var obj = MemoryPackSerializer.Deserialize<TestClass>(bin);

    if (obj != null)
    {
        Console.WriteLine($"{obj.Id}:{obj.Name}");

        if (obj.Id == 100 && obj.Name == "Hello")
        {
            Console.WriteLine("OK - Test1");
        }   
    }
}

// 클래스를 직렬화 했을 때의 바이너리 크기 확인
void Test2()
{
    Console.WriteLine("[ Test 2 ] 직렬화 했을 때의 바이너리 크기");

    var bin = MemoryPackSerializer.Serialize(new TestClassSize
    {
        Id = 100,
        Speed = 12.14f,
        Money = 1000
    });

    Console.WriteLine($"Test2 - TestClassSize size: {bin.Length}"); 
}

// 클래스를 직렬화 할 때 1개의 클래스가 가질 수 있는 최대 멤버 개수 확인
void Test3()
{
    Console.WriteLine("[ Test 3 ] 직렬화 할 클래스의 데이터 멤버 개수");

    // 직렬화 할 클래스의 데이터 멤버 개수는 최대 251개까지 이다.
    var bin = MemoryPackSerializer.Serialize(new TestClassTooDataCount());

    Console.WriteLine($"Test3 - TestClassTooDataCount size: {bin.Length}");
}

// 네트워크 프로그래밍에서 사용될 목적으로 헤더+보디 구조로된 패킷을 직렬화 할 때의 방법을 확인
// 패킷의 보디 부분만 직렬화 하면 간단하게 패킷을 직렬화 할 수 있지만 이러면 메모리 할당과 복사가 추가로 발생한다. 그래서 1번에 헤더+보디를 직렬화 하려고 한다.
void Test6()
{
    Console.WriteLine("[ Test 6 ] 패킷 데이터 직렬화");

    var reqPkt = new PKTReqLogin
    {
        TotalSize = 0, // 여기에서는 패킷의 전체 크기를 알 수 없다
        Id = 22,
        Type = 0,
        UserID = "jacking75",
        AuthToken = "jacking75",
    };
    // 직렬화 하면 앞에 1 바이트는 갯수, 이후는 데이터 순서대로 직렬화한다
    var bin = MemoryPackSerializer.Serialize(reqPkt);
    var totalSize = (UInt16)bin.Length;
    Console.WriteLine($"[Test6] Packet bin Size: {totalSize}");

    // PKTReqLogin 초기화에서 패킷의 전체 크기를 0으로 했기 때문에 올바르게 수정한다
    FastBinaryWrite.UInt16(bin, 1, totalSize);
    

    // 패킷 헤더 정보 읽기
    var headerInfo = new MemoryPackPacketHeadInfo();
    headerInfo.Read(bin);
    headerInfo.DebugConsolOutHeaderInfo();


    var obj = MemoryPackSerializer.Deserialize<PKTReqLogin>(bin);

    if (obj != null)
    {
        Console.WriteLine($"{obj.UserID}:{obj.AuthToken}");

        if (obj.Id == reqPkt.Id && obj.AuthToken == reqPkt.AuthToken)
        {
            Console.WriteLine("OK - Test6");
        }
    }
}



[MemoryPackable]
public partial class TestClass
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
}

[MemoryPackable]
public partial class TestClassSize
{
    public int Id { get; set; }
    public float Speed { get; set; } = default!;
    public Int64 Money { get; set; } = default!;
}

[MemoryPackable]
public partial class TestClassTooDataCount
{
    public int Id1 { get; set; } = 0;
    public int Id2 { get; set; } = 0;
    public int Id3 { get; set; } = 0;
    public int Id4 { get; set; } = 0;
    public int Id5 { get; set; } = 0;
    public int Id6{ get; set; } = 0;
    public int Id7 { get; set; } = 0;
    public int Id8 { get; set; } = 0;
    public int Id9 { get; set; } = 0;
    public int Id10 { get; set; } = 0;
    public int Id11 { get; set; } = 0;
    public int Id12 { get; set; } = 0;
    public int Id13 { get; set; } = 0;
    public int Id14 { get; set; } = 0;
    public int Id15 { get; set; } = 0;
    public int Id16 { get; set; } = 0;
    public int Id17 { get; set; } = 0;
    public int Id18 { get; set; } = 0;
    public int Id19 { get; set; } = 0;
    public int Id20 { get; set; } = 0;
    public int Id21 { get; set; } = 0;
    public int Id22 { get; set; } = 0;
    public int Id23 { get; set; } = 0;
    public int Id24 { get; set; } = 0;
    public int Id25 { get; set; } = 0;
    public int Id26 { get; set; } = 0;
    public int Id27 { get; set; } = 0;
    public int Id28 { get; set; } = 0;
    public int Id29 { get; set; } = 0;
    public int Id30 { get; set; } = 0;
    public int Id31 { get; set; } = 0;
    public int Id32 { get; set; } = 0;
    public int Id33 { get; set; } = 0;
    public int Id34 { get; set; } = 0;
    public int Id35 { get; set; } = 0;
    public int Id36 { get; set; } = 0;
    public int Id37 { get; set; } = 0;
    public int Id38 { get; set; } = 0;
    public int Id39 { get; set; } = 0;
    public int Id40 { get; set; } = 0;
    public int Id41 { get; set; } = 0;
    public int Id42 { get; set; } = 0;
    public int Id43 { get; set; } = 0;
    public int Id44 { get; set; } = 0;
    public int Id45 { get; set; } = 0;
    public int Id46 { get; set; } = 0;
    public int Id47 { get; set; } = 0;
    public int Id48 { get; set; } = 0;
    public int Id49 { get; set; } = 0;
    public int Id50 { get; set; } = 0;
    public int Id51 { get; set; } = 0;
    public int Id52 { get; set; } = 0;
    public int Id53 { get; set; } = 0;
    public int Id54 { get; set; } = 0;
    public int Id55 { get; set; } = 0;
    public int Id56 { get; set; } = 0;
    public int Id57 { get; set; } = 0;
    public int Id58 { get; set; } = 0;
    public int Id59 { get; set; } = 0;
    public int Id60 { get; set; } = 0;
    public int Id61 { get; set; } = 0;
    public int Id62 { get; set; } = 0;
    public int Id63 { get; set; } = 0;
    public int Id64 { get; set; } = 0;
    public int Id65 { get; set; } = 0;
    public int Id66 { get; set; } = 0;
    public int Id67 { get; set; } = 0;
    public int Id68 { get; set; } = 0;
    public int Id69 { get; set; } = 0;
    public int Id70 { get; set; } = 0;
    public int Id71 { get; set; } = 0;
    public int Id72 { get; set; } = 0;
    public int Id73 { get; set; } = 0;
    public int Id74 { get; set; } = 0;
    public int Id75 { get; set; } = 0;
    public int Id76 { get; set; } = 0;
    public int Id77 { get; set; } = 0;
    public int Id78 { get; set; } = 0;
    public int Id79 { get; set; } = 0;
    public int Id80 { get; set; } = 0;
    public int Id81 { get; set; } = 0;
    public int Id82 { get; set; } = 0;
    public int Id83 { get; set; } = 0;
    public int Id84 { get; set; } = 0;
    public int Id85 { get; set; } = 0;
    public int Id86 { get; set; } = 0;
    public int Id87 { get; set; } = 0;
    public int Id88 { get; set; } = 0;
    public int Id89 { get; set; } = 0;
    public int Id90 { get; set; } = 0;
    public int Id91 { get; set; } = 0;
    public int Id92 { get; set; } = 0;
    public int Id93 { get; set; } = 0;
    public int Id94 { get; set; } = 0;
    public int Id95 { get; set; } = 0;
    public int Id96 { get; set; } = 0;
    public int Id97 { get; set; } = 0;
    public int Id98 { get; set; } = 0;
    public int Id99 { get; set; } = 0;
    public int Id100 { get; set; } = 0;
    public int Id101 { get; set; } = 0;
    public int Id102 { get; set; } = 0;
    public int Id103 { get; set; } = 0;
    public int Id104 { get; set; } = 0;
    public int Id105 { get; set; } = 0;
    public int Id106 { get; set; } = 0;
    public int Id107 { get; set; } = 0;
    public int Id108 { get; set; } = 0;
    public int Id109 { get; set; } = 0;
    public int Id110 { get; set; } = 0;
    public int Id111 { get; set; } = 0;
    public int Id112 { get; set; } = 0;
    public int Id113 { get; set; } = 0;
    public int Id114 { get; set; } = 0;
    public int Id115 { get; set; } = 0;
    public int Id116 { get; set; } = 0;
    public int Id117 { get; set; } = 0;
    public int Id118 { get; set; } = 0;
    public int Id119 { get; set; } = 0;
    public int Id120 { get; set; } = 0;
    public int Id121 { get; set; } = 0;
    public int Id122 { get; set; } = 0;
    public int Id123 { get; set; } = 0;
    public int Id124 { get; set; } = 0;
    public int Id125 { get; set; } = 0;
    public int Id126 { get; set; } = 0;
    public int Id127 { get; set; } = 0;
    public int Id128 { get; set; } = 0;
    public int Id129 { get; set; } = 0;
    public int Id130 { get; set; } = 0;
    public int Id131 { get; set; } = 0;
    public int Id132 { get; set; } = 0;
    public int Id133 { get; set; } = 0;
    public int Id134 { get; set; } = 0;
    public int Id135 { get; set; } = 0;
    public int Id136 { get; set; } = 0;
    public int Id137 { get; set; } = 0;
    public int Id138 { get; set; } = 0;
    public int Id139 { get; set; } = 0;
    public int Id140 { get; set; } = 0;
    public int Id141 { get; set; } = 0;
    public int Id142 { get; set; } = 0;
    public int Id143 { get; set; } = 0;
    public int Id144 { get; set; } = 0;
    public int Id145 { get; set; } = 0;
    public int Id146 { get; set; } = 0;
    public int Id147 { get; set; } = 0;
    public int Id148 { get; set; } = 0;
    public int Id149 { get; set; } = 0;
    public int Id150 { get; set; } = 0;
    public int Id151 { get; set; } = 0;
    public int Id152 { get; set; } = 0;
    public int Id153 { get; set; } = 0;
    public int Id154 { get; set; } = 0;
    public int Id155 { get; set; } = 0;
    public int Id156 { get; set; } = 0;
    public int Id157 { get; set; } = 0;
    public int Id158 { get; set; } = 0;
    public int Id159 { get; set; } = 0;
    public int Id160 { get; set; } = 0;
    public int Id161 { get; set; } = 0;
    public int Id162 { get; set; } = 0;
    public int Id163 { get; set; } = 0;
    public int Id164 { get; set; } = 0;
    public int Id165 { get; set; } = 0;
    public int Id166 { get; set; } = 0;
    public int Id167 { get; set; } = 0;
    public int Id168 { get; set; } = 0;
    public int Id169 { get; set; } = 0;
    public int Id170 { get; set; } = 0;
    public int Id171 { get; set; } = 0;
    public int Id172 { get; set; } = 0;
    public int Id173 { get; set; } = 0;
    public int Id174 { get; set; } = 0;
    public int Id175 { get; set; } = 0;
    public int Id176 { get; set; } = 0;
    public int Id177 { get; set; } = 0;
    public int Id178 { get; set; } = 0;
    public int Id179 { get; set; } = 0;
    public int Id180 { get; set; } = 0;
    public int Id181 { get; set; } = 0;
    public int Id182 { get; set; } = 0;
    public int Id183 { get; set; } = 0;
    public int Id184 { get; set; } = 0;
    public int Id185 { get; set; } = 0;
    public int Id186 { get; set; } = 0;
    public int Id187 { get; set; } = 0;
    public int Id188 { get; set; } = 0;
    public int Id189 { get; set; } = 0;
    public int Id190 { get; set; } = 0;
    public int Id199 { get; set; } = 0;
    public int Id200 { get; set; } = 0;
    public int Id201 { get; set; } = 0;
    public int Id202 { get; set; } = 0;
    public int Id203 { get; set; } = 0;
    public int Id204 { get; set; } = 0;
    public int Id205 { get; set; } = 0;
    public int Id206 { get; set; } = 0;
    public int Id207 { get; set; } = 0;
    public int Id208 { get; set; } = 0;
    public int Id209 { get; set; } = 0;
    public int Id210 { get; set; } = 0;
    public int Id211 { get; set; } = 0;
    public int Id212 { get; set; } = 0;
    public int Id213 { get; set; } = 0;
    public int Id214 { get; set; } = 0;
    public int Id215 { get; set; } = 0;
    public int Id216 { get; set; } = 0;
    public int Id217 { get; set; } = 0;
    public int Id218 { get; set; } = 0;
    public int Id219 { get; set; } = 0;
    public int Id220 { get; set; } = 0;
    public int Id221 { get; set; } = 0;
    public int Id222 { get; set; } = 0;
    public int Id223 { get; set; } = 0;
    public int Id224 { get; set; } = 0;
    public int Id225 { get; set; } = 0;
    public int Id226 { get; set; } = 0;
    public int Id227 { get; set; } = 0;
    public int Id228 { get; set; } = 0;
    public int Id229 { get; set; } = 0;
    public int Id230 { get; set; } = 0;
    public int Id231 { get; set; } = 0;
    public int Id232 { get; set; } = 0;
    public int Id233 { get; set; } = 0;
    public int Id234 { get; set; } = 0;
    public int Id235 { get; set; } = 0;
    public int Id236 { get; set; } = 0;
    public int Id237 { get; set; } = 0;
    public int Id238 { get; set; } = 0;
    public int Id239 { get; set; } = 0;
    public int Id240 { get; set; } = 0;
    public int Id241 { get; set; } = 0;
    public int Id242 { get; set; } = 0;
    public int Id243 { get; set; } = 0;
    public int Id244 { get; set; } = 0;
    public int Id245 { get; set; } = 0;
    public int Id246 { get; set; } = 0;
    public int Id247 { get; set; } = 0;
    public int Id248 { get; set; } = 0;
    public int Id249 { get; set; } = 0;
    public int Id250 { get; set; } = 0;
    public int Id251 { get; set; } = 0;
    public int Id252 { get; set; } = 0;
    public int Id253 { get; set; } = 0;
    public int Id254 { get; set; } = 0;
    public int Id255 { get; set; } = 0;
    public int Id256 { get; set; } = 0;
    public int Id257 { get; set; } = 0;
    //public int Id258 { get; set; } = 0;
    //public int Id259 { get; set; } = 0; 
    
}