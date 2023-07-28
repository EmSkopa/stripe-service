namespace StripeApp.Service
{
    public class ServiceResponse<T>
    {
        public T Data { get; set; }

        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public object Message { get; set; }

        public ServiceResponse(T data)
        {
            Data = data;
            Success = true;
            Message = null;
        }
        
        public ServiceResponse(int statusCode, string message)
        {
            Message = new { errorMessage = message };
            StatusCode = statusCode;
            Success = false;
        }
    }
}