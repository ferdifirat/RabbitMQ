namespace RabbitMQ.Core.Model
{
    public class Response
    {
        public bool Success { get; set; }
        public string ErrorCode { get; set; }
        public string Message { get; set; }
        public Response()
        {
            Message = "";
            ErrorCode = "";
        }
    }
}
