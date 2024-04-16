# 하이브 게정 생성

```mermaid
sequenceDiagram
    
    participant Client
    participant APIGameServer
    participant RedisCache
    participant APIAccountServer
    participant Database


    Client             ->> APIAccountServer:  하이브 계정 생성 요청
    APIAccountServer   ->> Database:          이미 가입된 정보가 있는 지 확인
    Database          -->> APIAccountServer:  하이브 계정 검색 결과 응답
    alt 하이브 계정 정보가 이미 존재하는 경우
    APIAccountServer  -->> Client:             계정 생성 실패 응답
    end

    APIAccountServer   ->> 
    APIAccountServer  -->> Client:            계정 생성 성공 응답

```


---


# 하이브 계정이 있는 유저의 게임 로그인

```mermaid
sequenceDiagram
    
    participant Client
    participant APIGameServer
    participant RedisCache
    participant APIAccountServer
    participant Database


    Client             ->> APIAccountServer:  로그인 요청
    APIAccountServer   ->> Database:          아이디와 패스워드를 통해 하이브 계정 정보 검색
    alt 하이브 계정 정보가 존재하지 않는 경우
    APIAccountServer -->> Client:             로그인 실패
    end

    APIAccountServer  -->> Client:            로그인전용 토큰 발급(유효기간 자정까지)

    Client ->> APIGameServer:                 계정 ID와 토큰을 이용해 로그인 요청
    APIGameServer ->> APIAccountServer:       토큰 유효성 검증
    alt 토큰이 유효하지 않은 경우
    APIGameServer -->> Client:                토큰 유효성 검증 실패
    end

    APIAccountServer -->> APIGameServer:      토큰 유효성 검증 성공
    APIGameServer ->> Database:               유저 아이디를 통해 유저의 게임 데이터 검색
    Database -->> APIGameServer:              하이브 계정에 묶인 유저의 게임 데이터 조회 결과
    alt 유저의 게임 데이터가 존재하는 경우
        APIGameServer -->> Client:            유저 데이터 전달 및 로그인 성공 응답
    else 유저의 게임 데이터가 존재하지 않는 경우
        APIGameServer -->> Database:          게임을 처음 시작한 유저의 기본 데이터 생성
        APIGameServer -->> Client:            기본 유저 데이터 전달 및 로그인 성공 응답
    end
    APIGameServer ->> RedisCache:             Redis에 로그인 성공한 토큰을 저장, 이후의 요청은 Redis를 통해 처리
    
```

---




