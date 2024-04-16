using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


[ApiController]
[Route("[controller]")]
public class CreateAccountController : ControllerBase
{
    public CreateAccountController()
    {

    }


    [HttpPost]
    public string CtreateAccount()
    {
        // TODO DB 접근하여 계정생성

        // DB 최소 저장 데이터 이메일, 패스워드


        


        return "";
    }


}

