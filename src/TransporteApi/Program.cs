using MongoDB.Driver;
using TransporteApi.Model;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//DOCKER
builder.Services.AddSingleton<IMongoClient>(sp => new MongoClient("mongodb://mongo:27017"));

//LOCALHOST
//builder.Services.AddSingleton<IMongoClient>(sp => new MongoClient("mongodb://localhost:27017"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/colectivos/alertas", (IMongoClient client) =>
{
    var mongoDatabase = client.GetDatabase("transporte");

    var alertaCollection = mongoDatabase.GetCollection<Alerta>("alertas");

    return alertaCollection.Find(_ => true).ToList();
});

app.Run();