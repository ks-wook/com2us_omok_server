using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


[ApiController]
[Route("[controller]")]
public class LoginController : ControllerBase
{
    public LoginController() { }


    [HttpPost]
    public string TestAPI([FromHeader] string message)
    {
        Console.WriteLine(message);
        return "hello client";
    }
}

