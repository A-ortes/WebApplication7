using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using UserApiProject.Models;
using UserApiProject.Services;

namespace UserApiProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;

        public UsersController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public IActionResult GetAllUsers()
        {
            var users = _userService.GetAllUsers();

            if (users == null || !users.Any())
            {
                return NotFound(new { Message = "No users found." });
            }

            return Ok(users);
        }

        [HttpPost]
        public IActionResult CreateUser([FromBody] User newUser)
        {
            if (string.IsNullOrWhiteSpace(newUser.Name))
            {
                return BadRequest(new { Message = "User name cannot be empty." });
            }

            if (string.IsNullOrWhiteSpace(newUser.Email) || !IsValidEmail(newUser.Email))
            {
                return BadRequest(new { Message = "Invalid email address." });
            }

            _userService.AddUser(newUser);
            return CreatedAtAction(nameof(GetAllUsers), new { id = newUser.Id }, newUser);
        }

        private bool IsValidEmail(string email)
        {
            var emailRegex = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, emailRegex);
        }
    }
}