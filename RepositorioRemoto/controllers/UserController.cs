using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("/api/users")]
public class UserControllers(
    IUserService _service
) : ControllerBase
{
    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserDto>> GetUserByiD([FromRoute] int id)
    {
        var response = await _service.GetUserByIdAsync(id);
        if(response.IsFailure)
            return StatusCode((int)response.Error.StatusCode, response.Error.message);
        return Ok(response.Value);
    }

    [HttpGet]
    public async Task<ActionResult<List<UserDto>>> GetAllUser()
    {
        var response = await _service.GetAllUsersAsync();
        var dtos = response.Value.Select(u => u.ToDto()).ToList();
        if(response.IsFailure)
            return StatusCode((int)response.Error.StatusCode, response.Error.message);
        return Ok(
            response.Value);
    }
    [HttpPost]
    public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserDto dto)
    {
        var response = await _service.CreateUserAsync(dto);
        if(response.IsFailure)
            return StatusCode((int)response.Error.StatusCode, response.Error.message);
        return CreatedAtAction(
            nameof(GetUserByiD),
            new {id = response.Value.id},
            response.Value
        );
    }
    [HttpPut("{id:int}")]
    public async Task<ActionResult<UserDto>> UpdateUser(
        [FromBody] UpdateUserRequest dto,
        [FromRoute] int id)
    {
        var response = await _service.UpdateUserAsync(id, dto);
        if(response.IsFailure)
            return StatusCode((int)response.Error.StatusCode, response.Error.message);
        return Ok(
            response.Value);
    }
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var response = await _service.DeleteUserAsync(id);
        if(response.IsFailure)
            return StatusCode((int)response.Error.StatusCode, response.Error.message);
        return NoContent();
    }
}