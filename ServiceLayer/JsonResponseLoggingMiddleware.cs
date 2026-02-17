namespace ServiceLayer
{
    public class JsonResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public JsonResponseLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var originalBody = context.Response.Body;

            using var memStream = new MemoryStream();
            context.Response.Body = memStream;

            await _next(context); 

            if (context.Response.ContentType?.Contains("application/json") == true)
            {
                memStream.Seek(0, SeekOrigin.Begin);
                var json = await new StreamReader(memStream).ReadToEndAsync();

                File.AppendAllText("responses.log", json + Environment.NewLine);
            }

            memStream.Seek(0, SeekOrigin.Begin);
            await memStream.CopyToAsync(originalBody);
            context.Response.Body = originalBody;
        }
    }
}
