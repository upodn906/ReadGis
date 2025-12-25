using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace _Framework.Service._Common
{
    public class ServiceResult
    {
        public ServiceResult(bool isSuccess, string? message)
        {
            IsSuccess = isSuccess;
            Message = message;
        }

        public bool IsSuccess { get; }
        public string? Message { get; }

        public static ServiceResult Success(string? message = null) => new(true, message);

        public static ServiceResult Fail(string message) => new(false, message);
    }
}
