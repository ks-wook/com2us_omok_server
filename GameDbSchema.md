# __GameDb__
### schema
![스크린샷(154)](https://github.com/ks-wook/com2us_omok_server/assets/76806695/23804df1-9ea8-4735-bd6b-96bc86685049)
---


## *account*

__create query__
```
CREATE TABLE account
(
    `account_id`        BIGINT          NOT NULL    COMMENT '계정 아이디',
    `email`             VARCHAR(30)     NOT NULL    COMMENT '이메일',
    `password`          VARCHAR(30)     NOT NULL    COMMENT '해싱된 비밀번호',
    `created_at`        DATETIME        NOT NULL    DEFAULT CURRENT_TIMESTAMP COMMENT '계정 생성 일시',
    `recent_login_at`   DATETIME        NOT NULL    DEFAULT CURRENT_TIMESTAMP COMMENT '가장 최근 로그인 일시',
     PRIMARY KEY (account_id),
     UNIQUE KEY (email)
);
```

하이브 계정과 관련된 정보를 저장하는 테이블.


---


## *user_game_data*

__create query__
```
CREATE TABLE user_game_data
(
    `uid`                       BIGINT         NOT NULL    AUTO_INCREMENT COMMENT '유저 게임 데이터 아이디',
    `account_id`                BIGINT         NOT NULL    COMMENT '유저가 속한 계정 아이디', 
    `nickname`                  VARCHAR(16)    NOT NULL    COMMENT '닉네임',
    `user_money`                INT            NOT NULL    COMMENT '유저 게임 돈',
    `created_at`                DATETIME       NOT NULL    DEFAULT CURRENT_TIMESTAMP COMMENT '유저 생성 일시', 
    `recent_login_at`           DATETIME       NOT NULL    DEFAULT CURRENT_TIMESTAMP COMMENT '가장 최근 로그인 일시',
    `user_level`                INT            NOT NULL    DEFAULT 0 COMMENT '유저 레벨', 
    `user_exp`                  INT            NOT NULL    DEFAULT 0 COMMENT '유저 경험치',  
    `total_win_cnt`             INT            NOT NULL    DEFAULT 0 COMMENT '현재 시즌 승수', 
    `total_lose_cnt`            INT            NOT NULL    DEFAULT 0 COMMENT '현재 시즌 패수',
     PRIMARY KEY (uid),
     UNIQUE KEY (nickname),
     FOREIGN KEY (`account_id`) REFERENCES `account` (`account_id`)
);
```
하이브 계정에 묶인 유저의 정보에 대해 저장하는 테이블.


---



## *item*

__create query__
```
CREATE TABLE item
(
    `item_id`                    BIGINT            NOT NULL    AUTO_INCREMENT COMMENT '아이템 아이디',
    `item_template_id`           BIGINT            NOT NULL    COMMENT '아이템 템플릿 아이디',
    `owner_id`                   BIGINT            NOT NULL    COMMENT '아이템 주인의 user_id', 
    `item_count`                 INT               NOT NULL    COMMENT '아이템의 개수',
     PRIMARY KEY (item_id),
     FOREIGN KEY (`owner_id`) REFERENCES `user` (`user_id`)
);
```
유저가 획득한 아이템의 정보에 대해 관리하는 테이블. 유저가 소유한 모든 아이템은 고유한 아이디와 함께 아이템의 종류를 확인하기 위한 템플릿 아이디를 가진다.


---


## *mailbox*

__create query__
```
CREATE TABLE mail
(
    `mail_id`           BIGINT          NOT NULL    AUTO_INCREMENT COMMENT '메일 아이디', 
    `user_id`           BIGINT          NOT NULL    COMMENT '수신자의 유저 아이디', 
    `mail_template_id`  VARCHAR(20)     NOT NULL    COMMENT '우편 템플릿 아이디',
    `created_at`        DATETIME        NOT NULL    COMMENT '우편 생성 일시', 
    `expired_at`        DATETIME        NOT NULL    COMMENT '우편 만료 일시', 
    `received_at`       DATETIME        NOT NULL    DEFAULT CURRENT_TIMESTAMP COMMENT '수령 일시', 
    `receive_yn`        TINYINT         NOT NULL    DEFAULT 0 COMMENT '수령 유무',
     PRIMARY KEY (mail_seq),
     FOREIGN KEY (`user_id`) REFERENCES `user` (`user_id`)
);
```
유저의 우편함을 관리하는 테이블. 메일의 템플릿 아이디를 통해 외부에서 관리되는 메일의 제목과 내용을 주입하여 유저에게 표시해준다. 수령 완료 시 바로 삭제하지 않고
매일 자정에 수령이 완료된 메일 데이터에 대해 일괄 삭제를 실시한다. 혹은 만료 일시가 지난 보상에 대해 삭제를 실시한다.


---


## *mailbox_item*

__create query__
```
CREATE TABLE mail_item
(
    `mail_item_id`      BIGINT          NOT NULL    AUTO_INCREMENT COMMENT '메일 아이템 아이디', 
    `mail_id`           BIGINT          NOT NULL    COMMENT '어떤 메일의 보상인지 나타내는 mail_id', 
    `item_template_id`  BIGINT          NOT NULL    COMMENT '아이템 템플릿 아이디', 
    `item_count`        INT             NOT NULL    COMMENT '아이템 개수', 
    `receive_yn`        TINYINT         NOT NULL    DEFAULT 0 COMMENT '수령 유무',
     PRIMARY KEY (mail_seq),
     FOREIGN KEY (`mail_id`) REFERENCES `mailbox` (`mail_id`)
);
```
우편의 보상에 대한 정보를 기록하는 테이블. mailbox의 mail_id를 참조하여 어떤 메일에 대한 보상인지를 나타낸다. 마찬가지로 수령 유무를 표시하며
수령 완료 시 바로 삭제하는 것이 아닌 매일 자정에 수령이 완료된 메일 데이터에 대해 일괄 삭제를 실시한다.


---


## *fridend*

__create query__
```
CREATE TABLE friend
(
    `uid`         BIGINT      NOT NULL    COMMENT '유저 아이디', 
    `friend_uid`  BIGINT      NOT NULL    COMMENT '친구 유저아이디', 
    `friend_yn`   TINYINT     NOT NULL    DEFAULT 0  COMMENT '친구요청 수락여부(친구상태 여부)', 
    `created_at`   DATETIME    NOT NULL    DEFAULT CURRENT_TIMESTAMP COMMENT '생성 일시', 
     PRIMARY KEY (uid, friend_uid),
     FOREIGN KEY (`uid`) REFERENCES `user_game_data` (`uid`),
     FOREIGN KEY (`friend_uid`) REFERENCES `user_game_data` (`uid`)
);
```
친구 관계를 저장하는 테이블, uid와 friend_uid에 대해 한번만 삽입하여 친구관계를 하나의 데이터로만 나타낸다. 쌍으로 삽입하지 않는다.
친구 데이터 검색 시 uid혹은 friend_uid와 일치하는 지 모두 검색해야함에 주의한다.

친구 여부에 따라 수락된 친구요청인지 아직 미수락된 친구 요청인지 파악한다.
친구 요청이 거부된 경우 해당 데이터를 삭제한다.

---

