# EZ.Job.Core

Job scheduler **fire-and-forget** de alta performance usando `System.Threading.Channels`, com suporte a jobs recorrentes e recuperação automática de jobs pendentes no startup.

> Este é o pacote principal do ecossistema [EZ.DotNet](https://github.com/ez-dotnet). Consulte também os stores oficiais: [MSSQL](https://github.com/ez-dotnet/ez-job-store-mssql), [MySQL](https://github.com/ez-dotnet/ez-job-store-mysql), [PostgreSQL](https://github.com/ez-dotnet/ez-job-store-postgresql), [SQLite](https://github.com/ez-dotnet/ez-job-store-sqlite), [MongoDB](https://github.com/ez-dotnet/ez-job-store-mongodb), [Redis](https://github.com/ez-dotnet/ez-job-store-redis), e o módulo [Recurring](https://github.com/ez-dotnet/ez-job-recurring).

## Performance

| Store         | Jobs | Workers | EZ.Job (ms) | Hangfire (ms) | Vezes mais rápido |
|---------------|------|---------|-------------|---------------|-------------------|
| **InMemory**  | 100  | 1       | 1.63        | 2.07          | 1.27×             |
| **InMemory**  | 1000 | 4       | 8.19        | 32.43         | 3.96×             |
| **MySQL**     | 100  | 1       | 18.77       | 56.95         | 3.03×             |
| **MySQL**     | 1000 | 4       | 115.64      | 299.05        | 2.59×             |
| **PostgreSQL**| 100  | 1       | 21.40       | 68.42         | 3.20×             |
| **PostgreSQL**| 1000 | 4       | 115.07      | 326.91        | 2.84×             |
| **SQLite**    | 100  | 1       | 59.45       | 145.06        | 2.44×             |
| **SQLite**    | 1000 | 4       | 372.50      | 865.33        | 2.32×             |
| **Redis**     | 100  | 1       | 21.64       | 76.86         | 3.55×             |
| **Redis**     | 1000 | 4       | 117.15      | 381.56        | 3.26×             |
| **MongoDB**   | 100  | 1       | 26.56       | 91.14         | 3.43×             |
| **MongoDB**   | 1000 | 4       | 128.55      | 455.13        | 3.54×             |
| **MSSQL**     | 100  | 1       | 1.17        | 1.04          | 0.89×             |
| **MSSQL**     | 1000 | 4       | 5.43        | 13.80         | 2.54×             |

## Instalação

```bash
dotnet add package EZ.Job.Core
```

Adicione um store (ex.: InMemory já incluso).

## Uso básico

```csharp
using EZ.Job.Core;

var builder = Host.CreateApplicationBuilder();

builder.Services.AddEZJob(options =>
{
    options.WorkerCount = 4;
});

// Injete e use
public class MeuService
{
    private readonly IJobDispatcher _dispatcher;

    public MeuService(IJobDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public async Task ExecutarAsync()
    {
        // Agendamento fire-and-forget
        await _dispatcher.EnqueueAsync<EmailService>(s => s.EnviarAsync("email@teste.com"));
    }
}

public class EmailService
{
    public async Task EnviarAsync(string email)
    {
        await Task.Delay(100);
    }
}
```

## Workers

| Canal      | Default Workers | Descrição                                      |
|------------|----------------|------------------------------------------------|
| FF         | 4              | Processa jobs fire-and-forget imediatamente    |
| Recovery   | 1              | Reprocessa jobs pendentes ao iniciar           |
| Recurring  | 1              | Processa jobs recorrentes agendados (via `EZ.Job.Recurring`) |

## Projetos relacionados

- [EZ.DotNet](https://github.com/ez-dotnet) — Organização
- [EZ.Redact](https://github.com/ez-dotnet/ez-redact) — Redação de dados
- [EZ.Lib](https://github.com/ez-dotnet/ez-lib) — Biblioteca base
