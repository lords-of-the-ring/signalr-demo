using Web.Auth;
using Web.Signalr;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuth(builder.Configuration);
builder.Services.AddSignalR();
builder.Services.AddLogging();

var app = builder.Build();

app.UseAuthorization();

var api = app.MapGroup("/api").RequireAuthorization();
api.MapPost("/login", Login.Handle).AllowAnonymous();
api.MapGet("/secret", () => "Welcome, Tsanov! You are now in!");

var hubsGroup = app.MapGroup(SignalrConstants.HubsBasePrefix).RequireAuthorization();
app.MapHub<ChatHub>("/chat").AllowAnonymous();

app.Run();
