# ASP.Net Core
컴투스 서버 캠퍼스 2기 1 ~ 2주차에 진행하였던 ASP.Net Core에 대해 학습한 내용을 정리한 문서입니다.




## 목차
1. [ASP.Net Core?]
2. [ASP.Net Core에 대하여]
   * [Program 시작]
   * [Builder]
   * [Routing]
   * [Configuration]
   * [미들웨어]
3. DI(의존성 주입)
4. [비동기 제어 Async, Await]
5. sqlKata
6. CloudStructures




---
# ASP.Net Core?

ASP.Net Core는 .Net 플랫폼에서 웹 애플리케이션을 구축하기 위한 최신 고성능 웹 개발 프레임워크.
C#으로 API 서버를 개발하기위해 주로 사용되며 아래와 같은 특징을 가졌다.


* 오픈 소스 소프트웨어, 멀티플랫폼 지원
* .Net Core 프레임워크 기반으로 하여 빠르고 효율적인 웹 애플리케이션 개발 지원
* 모듈식 아키텍처를 가지고 있어 필요한 기능한 선택하여 사용가능
* 인증, 캐싱, 라우팅, 데이터 액세스 등 웹 애플리케이션 개발에 필요한 다양한 기능 제공
---
# ASP.Net Core에 대하여

## Program 시작

* ASP.Net Core의 프로그램 진입점
* ASP.Net Core 6버전 부터 main 함수를 선언할 필요 없이 '최상위 문' 기능을 사용하여 스크립트 언어처럼 사용할 수 있다.
* DI를 위한 인터페이스 및 클래스 등록
* 라우팅 및 사용자 정의 미들웨어 등록


-> ASP.Net Core 빈 프로젝트 program.cs
```
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
```

1. WebApplication.CreateBuilder(args)
   WebApplicationBuilder 인스턴스를 생성한다. 해당 인스턴스를 통해 ASP.Net Core 애플리케이션을 구성한다.
2. builder.Build()
   앞에서 생성한 builder 인스턴스를 통해 ASP.Net Core 애플리케이션을 빌드한다.  
3. app.MapGet("/", () => "Hello World!")
   지정된 경로(URL)에 따라 GET 요청을 처리하는 엔드포인트를 매핑한다. 해당 애플리케이션에 루트 경로("/")로 요청을 보내면
   "Hello World!" 문자열을 반환한다.
4. app.Run()
   앞의 과정에서 설정된 ASP.Net Core 애플리케이션을 실제로 실행한다.



## Builder

ASP.Net Core에서 웹 애플리케이션을 구성하고 빌드하는 데 사용되는 주요 클래스. Builder 클래스를 사용하여 서비스, 미들웨어, 라우팅 등을 등록하고 애플리케이션의 동작 방식을 정의 할 수 있다.

-> builder를 통해 애플리케이션 구성하기
```
var builder = WebApplication.CreateBuilder(args);

// 서비스 등록
builder.Services.AddSingleton<IMyService, MyService>();

// 미들웨어 등록
builder.UseMiddleware<MyMiddleware>();

// 라우팅 설정
builder.AddControllers();

// 호스트 구성
builder.Host.UseEnvironment("Development");

// 구성이 완료된 builder 객체를 통해 Build
var app = builder.Build();
```

* 서비스 등록: Services 속성을 사용하여 애플리케이션에서 사용할 서비스를 등록할 수 있다. 서비스는 인터페이스와 구현 클래스를 사용하여 등록한다.
* 미들웨어 등록: UseMiddleware() 메서드를 사용하여 HTTP 요청 파이프라인에 사용자 정의 미들웨어를 추가 할 수 있다.
* 라우팅 설정: AddControllers() 또는 MapGet() 등의 메서드를 사용하여 엔드포인트를 라우팅할 수 있다.
* 호스트 구성: Host 속성을 사용하여 호스트 설정을 구성할 수 있다. 여기에는 포트번호 환경 변수 등이 포함된다.
* 애플리케이션 빌드: Build() 메서드를 사용하여 WebApplication 인스턴스를 빌드한다.



## Routing

라우팅은 들어오는 HTTP 요청을 적절한 컨트롤러 작업이나 미들웨어에 매핑하는 프로세스이다. 이를 통해 애플리케이션은 다양한 요청을 처리하고 사용자에게 올바른 응답을 제공 할 수 있다.

-> 컨트롤러 선언 후 연결하기 MyContoller.cs
```
[Route("[controller]")]
[ApiController]
public class LoginController : ControllerBase
{
    readonly ILogger<LoginController> _logger;
    readonly IGameService _gameService;

    public LoginController() 
    {
        _logger = logger;
        _gameService = gameService;
    }

    // 유저 정보가 있다면 같이 동봉해서 보내고, 없다면 신규 유저이므로 새로 만들어서 전달
    [HttpPost]
    public async Task<LoginRes> Login([FromBody] LoginReq req)
    {
        // Login Logic ~


        // 로그인 성공 시 게임 데이터 로드
        res.UserGameData = await _gameService.LoadGameData(req.AccountId);

        return res;
    }

}


```

### 컨트롤러
컨트롤러는 MVC 패턴에서 사용자의 입력을 처리하는 로직을 실행한다. 이번 ASP.Net Core에서 MVC 패턴은 주된 학습 목표가 아니기 때문에 컨트롤러를 통해 사용자의 요청에 따라 다른 로직을 실행한다는 것 정도만 꼭 기억해두자.  


  
-> LoginController  

  
* '[Route("[controller]")]' : 이 속성은 컨트롤러가 라우팅되는 경로를 의미한다. 컨트롤러의 이름이 'LoginController'인 경우 앞 글자만 따서 '/Login' 으로 라우팅 경로를 지정한다.
* '[HttpPost]' : 이 속성은 해당 메서드가 HTTP POST 요청을 처리하도록 지정함을 의미한다. 위의 LoginContoller에는 'Login' 하나의 메서드만 존재하므로 '서버주소/Login'으로 들어오는 모든 HTTP POST 요청을 해당 메서드가 처리하게 된다.

  
  
이런식으로 선언한 컨트롤러는 '[ApiController]' 속성을 통해 ASP.Net Core에서 관리하는 컨트롤러로 취급되어 builder.AddController() 메서드를 호출하는 시점에 라우팅 경로가 매핑된다. 




  



# Configuration
Configuration을 통해 ASP.Net Core의 애플리케이션의 동작 방식을 구성하는 데 사용할 수 있는 설정 값들을 관리할 수 있다.

  
* Connection String - 데이터베이스, 메시지 큐 및 기타 외부 서비스에 대한 연결 정보
* 애플리케이션 설정 - 캐싱, 로깅, 보안 등과 같은 애플리케이션의 동작 방식을 제어하는 값
* 환경별 설정 값 - 개발, 테스트 및 프로덕션 환경에 따라 애플리케이션의 동작을 조정



-> appsettings.json
```
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=mydatabase;User Id=sa;Password=mypassword;"
  },
  "Logging": {
    "LogLevel": "Information"
  }
}
```


  

-> GetSection을 이용해 값 가져오기
```
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json");

var configuration = builder.Configuration;

var connectionString = configuration.GetSection("ConnectionStrings")["DefaultConnection"];
Console.WriteLine($"Connection string: {connectionString}");

var logLevel = configuration.GetSection("Logging")["LogLevel"];
Console.WriteLine($"Log level: {logLevel}");

```


DB 연결 문자열과 로그 레벨 정보를 가져와서 실제 ASP.Net Core 애플리케이션에 적용하고 사용할 수 있다.




 # 미들웨어



ASP.NET Core 미들웨어는 요청 처리 파이프라인에 연결되는 소프트웨어 구성 요소이다. 각 미들웨어는 요청을 처리하거나 파이프라인에서 다음 미들웨어로 전달하기 전에 작업을 수행할 수 있다.

미들웨어를 사용하면 다음과 같은 작업을 수행할 수 있다.

* 요청 인증 및 권한 부여: 사용자가 요청에 대한 권한이 있는지 확인합니다.
* 캐싱: 요청 결과를 캐싱하여 성능을 향상시킵니다.
* 로그 기록 및 추적: 요청 및 응답에 대한 정보를 기록합니다.
* 요청 및 응답 변환: 요청 및 응답 데이터를 원하는 형식으로 변환합니다.
* 정적 파일 제공: 정적 파일(HTML, CSS, JavaScript 등)을 제공합니다.
* 오류 처리: 오류가 발생하면 적절한 오류 메시지를 반환합니다.



![middleware-pipeline](https://github.com/ks-wook/com2us_omok_server/assets/76806695/9186d5cf-ec8b-4bf6-b022-6cca26e6480b)



ASP.Net Core의 미들웨어 실행 순서는 위의 이미지와 같다. 가장 마지막에 사용자 정의 미들웨어를 실행하게 되어있어 해당 부분에서 요청에 대한 로그나 사용자 인증 등의 기능을 수행하는 것이 바람직하다.


-> 사용자 정의 미들웨어
```
public class MyMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        // 요청 처리 전 작업 수행

        await next.InvokeAsync(context);

        // 요청 처리 후 작업 수행
    }
}
```

  
-> 미들웨어 등록
```
app.UseMiddleware<MyMiddleware>();
```






# DI(의존성 주입)

의존성 주입은 객체간 결합을 느슨하게 하고 코드 테스트를 용이하게 하기위해 사용하는 기능이다. 





