using System.Net;

namespace InmobiliariaAPI.Models.DTO
{
    public class ApiResponse
    {
        public ApiResponse()
        {
            ErrorMessages = new List<string>();
        }

        public HttpStatusCode StatusCode { get; set; }
        public bool IsSuccess { get; set; } = true;
        public List<string> ErrorMessages { get; set; }
        public object Result { get; set; }
    }

    public class ApiResponse<T> : ApiResponse
    {
        public new T Result { get; set; }
    }
}
