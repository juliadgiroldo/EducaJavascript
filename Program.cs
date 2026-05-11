var builder = WebApplication.CreateBuilder(args);

// 1. Adiciona o suporte para Controllers (Essencial para seu Webhook funcionar)
builder.Services.AddControllers();

var app = builder.Build();

// 2. Comenta a linha de redirecionamento HTTPS para não dar conflito com o Localtunnel
// app.UseHttpsRedirection();

// 3. Mapeia as rotas dos seus Controllers (Cria o caminho /api/dialogflow/...)
app.MapControllers();

app.Run();