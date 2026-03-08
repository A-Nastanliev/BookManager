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

                using var fs = new FileStream("responses.log", FileMode.Append, FileAccess.Write, FileShare.ReadWrite);

                using var writer = new StreamWriter(fs);
                await writer.WriteLineAsync(json);
            }

            memStream.Seek(0, SeekOrigin.Begin);
            await memStream.CopyToAsync(originalBody);
            context.Response.Body = originalBody;
        }
    }
}
