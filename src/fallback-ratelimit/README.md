# DotNet7-Worker-Polly-Fallback-RateLimit_ConsumoAPIContagem
Exemplo de consumo em um Worker Service criado com .NET 7 de uma API REST (contagem de acessos), utilizando para isto a biblioteca Polly com o padrão Fallback (retornando um valor default após se atingir um limite de requisições enviadas - Rate Limit).

API REST utilizada nos testes:

**https://github.com/renatogroffe/ASPNETCore7-WebApi-RateLimiting-Fixed-Concurrency-k6_ContagemAcessos**