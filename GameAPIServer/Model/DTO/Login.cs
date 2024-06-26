﻿using GameAPIServer.Model.DAO.GameDb;
using System.ComponentModel.DataAnnotations;

namespace GameAPIServer.Model.DTO
{
    public class LoginReq
    {
        [Required]
        public Int64 Uid { get; set; }

        [Required]
        public string Token { get; set; } = string.Empty;
    }

    public class LoginRes
    {
        public ErrorCode Result { get; set; } = ErrorCode.None;
        public UserGameData? UserGameData { get; set; }
    }
}
