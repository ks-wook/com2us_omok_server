
# __HiveDb__
### schema
![스크린샷(155)](https://github.com/ks-wook/com2us_omok_server/assets/76806695/5d801330-5eab-4b78-8504-f6295b00ffc5)
---


## *account*

__create query__
```
CREATE TABLE account
(
    `account_id`        BIGINT          NOT NULL    AUTO_INCREMENT COMMENT '계정 아이디',
    `email`             VARCHAR(30)     NOT NULL    COMMENT '이메일',
    `password`          VARCHAR(100)    NOT NULL    COMMENT '해싱된 비밀번호',
    `saltValue`         VARCHAR(100)    NOT NULL    COMMENT 'password salting에 사용된 값',
    `created_at`        DATETIME        NOT NULL    DEFAULT CURRENT_TIMESTAMP COMMENT '계정 생성 일시',
    `recent_login_at`   DATETIME        NOT NULL    DEFAULT CURRENT_TIMESTAMP COMMENT '가장 최근 로그인 일시',
     PRIMARY KEY (account_id),
     UNIQUE KEY (email)
);
```

하이브 계정과 관련된 정보를 저장하는 테이블.


---
