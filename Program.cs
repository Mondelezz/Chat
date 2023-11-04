using Quantum.Interfaces;
using Quantum.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IWebSocket, WebSocketServices>();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

WebSocketOptions webSocketOptions = new WebSocketOptions()
{
    KeepAliveInterval = TimeSpan.FromMinutes(1)
};
app.UseWebSockets(webSocketOptions);



app.UseHttpsRedirection();

app.UseAuthorization();
app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.MapControllers();

app.Run();
