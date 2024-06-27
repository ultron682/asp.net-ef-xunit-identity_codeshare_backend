namespace CodeShareBackend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin",
                    builder =>
                    {
                        builder.WithOrigins("http://localhost:3000")
                               .AllowAnyMethod()
                               .AllowAnyHeader()
                               .AllowCredentials();
                    });
            });

            builder.Services.AddControllers();
            builder.Services.AddSignalR();

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            //app.UseAuthorization();

            app.UseCors("AllowSpecificOrigin");
            app.MapControllers();
            app.MapHub<CodeShareHub>("/codesharehub");
            app.Run();
        }
    }
}
