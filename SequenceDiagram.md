
# http://localhost:port/createHiveAccount

```mermaid
sequenceDiagram
    
    participant Client
    participant APIAccountServer
    participant APIGameServer
    participant Database

    Client             ->> APIAccountServer: 계정생성 요청
    APIAccountServer   ->> Database:         계정 정보 삽입, 유저 게임 정보 생성
    APIAccountServer  -->> Client:           로그인전용 토큰 발급(유효기간 자정까지)
