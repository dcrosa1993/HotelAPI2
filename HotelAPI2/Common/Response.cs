namespace HotelAPI2.Common
{
	public class Response<T>
	{
		public Response() { }
		public Response(T result)
		{
			Success = result;
		}
		public T? Success { get; set; }
		public bool Error { get; set; } = false;
		public string? Message { get; set; } = string.Empty;
	}
}
