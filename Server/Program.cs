using Server.Auth;
using Server.Gameplay;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddAuth(builder.Configuration);
builder.Services.AddLogging();
builder.Services.AddSingleton<IWaitingQueue, InMemoryWaitingQueue>();
builder.Services.AddSingleton<IPlayerStore, InMemoryPlayerStore>();
builder.Services.AddSingleton<IGameRoomStore, InMemoryGameRoomStore>();
builder.Services.AddHostedService<MatchmakingBackgroundService>();

var app = builder.Build();

app.UseAuthorization();

var api = app.MapGroup("/api").RequireAuthorization();
api.MapPost("/login", Login.Handle).AllowAnonymous();
api.MapGet("/secret", () => "Welcome, Tsanov! You are now in!");
api.MapGet("/waiting-queue",
    async (
        IWaitingQueue queue,
        CancellationToken cancellationToken) => await queue.GetAllPlayers(cancellationToken));

api.MapGet("/all-connections",
    async (
        IPlayerStore store,
        CancellationToken cancellationToken) => (await store.GetAllConnections(cancellationToken))
        .Select(x => new
        {
            key = x.Key.Value,
            value = x.Value.Value
        })
        .ToList());

api.MapGet("/game-rooms",
    async (
        IPlayerStore store,
        CancellationToken cancellationToken) => (await store.GetAllGameRooms(cancellationToken))
        .Select(x => new
        {
            key = x.Key.Value,
            value = x.Value.Value
        })
        .ToList());

var hubs = app.MapGroup(HubConstants.HubsBasePrefix).RequireAuthorization();
hubs.MapHub<GameHub>("/game");

app.Run();
