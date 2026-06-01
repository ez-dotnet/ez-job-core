# Resultados — Benchmark EZ.Job.Core

## Ambiente

| Item           | Valor                         |
|----------------|-------------------------------|
| Hardware       | Intel i7-12700K, 64GB DDR5   |
| SO             | Ubuntu 24.04                  |
| .NET           | 10.0                          |
| Driver MySQL   | MySqlConnector 2.4.0          |
| Driver PG      | Npgsql 9.0.3                  |
| Driver Redis   | NRedisStack 0.13.1            |
| Driver MongoDB | MongoDB.Driver 3.2.1          |
| Driver MSSQL   | Microsoft.Data.SqlClient 6.0.1|
| Driver SQLite  | Microsoft.Data.Sqlite 9.0.3  |

## Metodologia

Cada job executa `INSERT INTO` (store) + `UPDATE` (status). 5 execuções por cenário, média aritmética. Hangfire usa `BackgroundJob.Enqueue` com filtro automático de serialização.

## Tabela completa

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

## Observações

- **MSSQL** é o único store onde Hangfire empata no cenário 100/1 (diferença de ~0.13ms). Em 1000/4, EZ.Job é 2.54× mais rápido.
- **SQLite** tem a maior latência absoluta devido ao locking de arquivo único.
- **Redis** e **MongoDB** mostram ganhos consistentes de 3.2–3.5×.
- **InMemory** é o cenário mais extremo: 3.96× mais rápido em 1000/4.
