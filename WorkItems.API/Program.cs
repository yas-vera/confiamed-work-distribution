using WorkItems.API.Clients;
using WorkItems.API.Repositories;
using WorkItems.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

// Inyeccion de dependencias se registra cada interfaz con su implementacion
builder.Services.AddSingleton<IWorkItemRepository, WorkItemRepository>();
builder.Services.AddScoped<IDistributionService, DistributionService>();
builder.Services.AddScoped<IWorkItemService, WorkItemService>();

// Configura el cliente HTTP para comunicarse con el microservicio de  UserManagement.API
// La URL es configurable vía appsettings.json 
var userApiUrl = builder.Configuration["Services:UserManagementApi"]
    ?? throw new InvalidOperationException("Falta configurar Services:UserManagementApi en appsettings.json");

builder.Services.AddHttpClient<IUserApiClient, UserApiClient>(client =>
{
    client.BaseAddress = new Uri(userApiUrl);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
