using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;


namespace InmobiliariaDTO.Models.DTO
{
    public class ApiResponse<T>
    {
        public HttpStatusCode StatusCode { get; set; }
        public bool IsSuccess { get; set; } = true;
        public List<string> ErrorMessages { get; set; } = new();
        public T Result { get; set; }
    }
}
