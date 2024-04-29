# ASP.Net Core
*들어가며*
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
builder.MapControllers();

// 호스트 구성
builder.Host.UseEnvironment("Development");

// 구성이 완료된 builder 객체를 통해 Build
var app = builder.Build();
```

* 서비스 등록: Services 속성을 사용하여 애플리케이션에서 사용할 서비스를 등록할 수 있다. 서비스는 인터페이스와 구현 클래스를 사용하여 등록한다.
* 미들웨어 등록: UseMiddleware() 메서드를 사용하여 HTTP 요청 파이프라인에 사용자 정의 미들웨어를 추가 할 수 있다.
* 라우팅 설정: MapContollers() 또는 MapGet() 등의 메서드를 사용하여 엔드포인트를 라우팅할 수 있다.
* 호스트 구성: Host 속성을 사용하여 호스트 설정을 구성할 수 있다. 여기에는 포트번호 환경 변수 등이 포함된다.
* 애플리케이션 빌드: Build() 메서드를 사용하여 WebApplication 인스턴스를 빌드한다.






