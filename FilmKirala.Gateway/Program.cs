var builder = WebApplication.CreateBuilder(args);

// YARP'ý servislere ekle ve ayarlarý appsettings.json'dan okumasýný söyle
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

// YARP Middleware'ini (Yönlendiriciyi) devreye sok
app.MapReverseProxy();

app.Run();