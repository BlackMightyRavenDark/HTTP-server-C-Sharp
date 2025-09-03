
namespace GuiServer
{
	internal class HttpStatus
	{
		public int Code { get; }
		public string Message { get; }

		public HttpStatus(int code, string message)
		{
			Code = code;
			Message = message;
		}
	}
}
