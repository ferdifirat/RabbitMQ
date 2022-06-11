using RabbitMQ.Client;
using RabbitMQ.EventBus;
using RabbitMQ.EventBus.Producer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

#region RabbitMQ Configuration
builder.Services.AddSingleton<IRabbitMQPersistentConnection>(sp =>
{
    var factory = new ConnectionFactory()
    {
        HostName = builder.Configuration["EventBus:HostName"]
    };
    factory.AutomaticRecoveryEnabled = true;
    factory.NetworkRecoveryInterval = TimeSpan.FromSeconds(5);
    factory.TopologyRecoveryEnabled = true;

    if (!string.IsNullOrWhiteSpace(builder.Configuration["EventBus:UserName"]))
        factory.UserName = builder.Configuration["EventBus:UserName"];

    if (!string.IsNullOrWhiteSpace(builder.Configuration["EventBus:Password"]))
        factory.Password = builder.Configuration["EventBus:Password"];

    var retryCount = 3;

    if (!string.IsNullOrWhiteSpace(builder.Configuration["EventBus:RetryCount"]))
        retryCount = int.Parse(builder.Configuration["EventBus:RetryCount"]);

    return new DefaultRabbitMQPersistentConnection(factory, retryCount);
});
builder.Services.AddScoped<EventBusRabbitMQProducer>();

#endregion

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
