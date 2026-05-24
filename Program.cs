using HashSystem;
using Archives;
using DataBaseSystem;
using dotenv.net;
using System.Collections.Concurrent;

DotEnv.Load();
string? cs = Environment.GetEnvironmentVariable("DBConnection");

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options => {
  options.Limits.MinRequestBodyDataRate = null;
  options.Limits.MaxRequestBodySize = null;
  options.ListenAnyIP(3636);
});

builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options => {
  options.MultipartBodyLengthLimit = 150994944;
  options.ValueLengthLimit = int.MaxValue;
  options.MemoryBufferThreshold = 150994944;
  options.ValueCountLimit = 200;
  options.MultipartHeadersCountLimit = 200;
});

var app = builder.Build();

ConcurrentDictionary<string, int> OldHashs = new();

app.MapPost("/", async (HttpRequest request, string Name, string Team) => {
  var from_ = await request.ReadFormAsync();
  var files = from_.Files.ToList();
  if (files.Count == 0) {
    return Results.BadRequest("Nenhum arquivo enviado.");
  }
  HashMaker hash = new HashMaker(Name, Team);
  string teamhash = hash.Output;
  int currentCount = OldHashs.GetOrAdd(teamhash, 0);
  if (currentCount >= 3) {
    return Results.Ok("Este time já realizou 3 envios. Não é permitido mais.");
  }
  int codenumber = currentCount + 1;
  DBConnector dbConnector = new DBConnector(cs!);
  foreach (var file in files) {
    using var ms = new MemoryStream();
    await file.CopyToAsync(ms);
    byte[] content = ms.ToArray();
    await dbConnector.Connect(Name, Team, teamhash, codenumber, file.FileName, content);
  }
  OldHashs[teamhash] = codenumber;
  return Results.Ok(new {
    Message = $"Envio #{codenumber} realizado com sucesso.",
    EnvioNumero = codenumber,
    Files = files.Select(f => f.FileName).ToList(),
    Hash = teamhash
  });
}).DisableAntiforgery().WithName("/");

app.Run();
