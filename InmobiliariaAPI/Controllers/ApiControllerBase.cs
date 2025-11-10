using InmobiliariaAPI.Models.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace InmobiliariaAPI.Controllers
{
    
    [ApiController]
    public class ApiControllerBase : ControllerBase
    {
        //Respuestas uniformes para API
        protected IActionResult ApiOk<T>(T result)
        {
            var r = new ApiResponse<T>
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Result = result
            };
            return Ok(r);
        }

        protected IActionResult ApiCreated<T>(string location, T result)
        {
            var r = new ApiResponse<T>
            {
                StatusCode = HttpStatusCode.Created,
                IsSuccess = true,
                Result = result
            };
            return Created(location, r);
        }

        protected IActionResult ApiError(HttpStatusCode code, params string[] errors)
        {
            var r = new ApiResponse<object>
            {
                StatusCode = code,
                IsSuccess = false,
                ErrorMessages = errors.ToList(),
                Result = null
            };
            return StatusCode((int)code, r);
        }
    }
}
