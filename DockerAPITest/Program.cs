
namespace DockerAPITest
{
    public class Program
    {
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Add services
			builder.Services.AddControllers();
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen();

			var app = builder.Build();

			// Configure HTTP request pipeline
			if (app.Environment.IsDevelopment())
			{
				app.UseSwagger();
				app.UseSwaggerUI();
			}

			app.UseAuthorization();

			app.MapControllers();

			// Lyt på alle netværksinterfaces på port 8080
			app.Urls.Clear();
			app.Urls.Add("http://+:8080");

			app.Run();
		}
	}
}
