namespace BlazorAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Value?.Split(';') ?? [];

            builder.Services.AddCors(p => p.AddPolicy("LivethereCMSPublicCors", builder =>
            {
                builder.WithOrigins(allowedOrigins).SetIsOriginAllowedToAllowWildcardSubdomains().AllowAnyHeader().AllowAnyMethod();
            }));

            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();
            app.UseCors("LivethereCMSPublicCors");

            app.MapControllers();

            app.Run();
        }
    }
}
