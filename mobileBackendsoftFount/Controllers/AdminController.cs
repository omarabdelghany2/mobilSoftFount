// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;

// namespace mobileBackendsoftFount.Controllers
// {
//     [Route("api/admin")]
//     [ApiController]
//     [Authorize(Roles = "Admin")] // 🔹 Only Admins Can Access This Controller
//     public class AdminController : ControllerBase
//     {
//         [HttpGet("dashboard")]
//         public IActionResult GetAdminDashboard()
//         {
//             return Ok(new { message = "Welcome Admin! This is a protected dashboard." });
//         }
//     }
// }
