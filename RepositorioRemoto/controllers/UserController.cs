using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("/api/users")]
public class UserControllers(
    IUserService _service
)
{
    [HttpGet("/{id:int}")]
    public async Task<ActionResult<UserModel>> GetUserByiD([FromRoute] int id)
    {
        var response = await _service.GetUserByIdAsync(id);
        return response.IsSuccess 
        ? Results.Accepted(response.Value)
        : Results.NotFound();
    }
}