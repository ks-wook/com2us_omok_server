﻿using HiveServer;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace HiveServer.Model.DAO
{
    public class CreateAccountReq
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }

    public class CreateAccountRes
    {
        ErrorCode _result = ErrorCode.None;
        public ErrorCode result
        {
            get { return _result; }
            set
            {
                _result = value;
                this.message = ErrorMessage.GetErrorMsg(_result);
            }
        }
        public string? message { get; set; }
    }


}
