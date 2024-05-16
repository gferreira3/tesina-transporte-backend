using MongoDB.Driver;
using TransporteApiMono.Model;
using TransporteApiMono.Workers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IMongoClient>(sp => new MongoClient("mongodb://mongo:27017"));
builder.Services.AddHostedService<SubtesWorker>();
builder.Services.AddHostedService<BicisStatusWorker>();
builder.Services.AddHostedService<BicisInfoWorker>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            policy.AllowAnyOrigin();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/subtes/alertas", (IMongoClient client) =>
{
    Console.WriteLine("Request recibido: /subtes/alertas");

    var mongoDatabase = client.GetDatabase("transporte");

    var alertaCollection = mongoDatabase.GetCollection<Alerta>("alertas");

    return alertaCollection.Find(_ => true).ToList();
});

app.MapGet("/bicis/info", (IMongoClient client) =>
{
    Console.WriteLine("Request recibido: /bicis/info");

    var mongoDatabase = client.GetDatabase("transporte");

    var stationCollection = mongoDatabase.GetCollection<StationInfo>("stationinfo");

    return stationCollection.Find(_ => true).ToList();

});

app.MapGet("/bicis/status", (IMongoClient client) =>
{
    Console.WriteLine("Request recibido: /bicis/status");

    var mongoDatabase = client.GetDatabase("transporte");

    var stationCollection = mongoDatabase.GetCollection<StationStatus>("stationstatus");

    return stationCollection.Find(_ => true).ToList();
});

app.UseCors();
app.Run();