﻿using System.Formats.Asn1;

namespace GameAPIServer.Services;

public interface IAuthenticationService
{
    public Task<ErrorCode> LoginTokenVerify(Int64 accuountId, string loginToken);


    public Task<ErrorCode> Login(Int64 accountId, string loginToken);
}
