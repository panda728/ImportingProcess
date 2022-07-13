# ImportingProcess
Benchmark program to study the efficiency of file ingestion processes that may be found in Japanese business systems.

ver2. Result

|          Method |     Mean |     Error |   StdDev | Ratio | RatioSD |      Gen 0 |      Gen 1 |     Gen 2 | Allocated |
|---------------- |---------:|----------:|---------:|------:|--------:|-----------:|-----------:|----------:|----------:|
|       ListAsync | 938.7 ms | 996.38 ms | 54.61 ms |  1.00 |    0.00 | 51000.0000 | 17000.0000 | 4000.0000 |    375 MB |
|    RowByteAsync | 232.3 ms | 147.99 ms |  8.11 ms |  0.25 |    0.02 | 29000.0000 |          - |         - |    247 MB |
|  RowMemoryAsync | 210.3 ms | 118.17 ms |  6.48 ms |  0.22 |    0.02 | 17000.0000 |  1000.0000 | 1000.0000 |    192 MB |
|  PipelinesAsync | 259.1 ms | 425.90 ms | 23.35 ms |  0.28 |    0.02 | 15000.0000 |          - |         - |    188 MB |
| Pipelines2Async | 249.7 ms | 437.05 ms | 23.96 ms |  0.27 |    0.04 | 18000.0000 |          - |         - |    201 MB |
| Pipelines3Async | 204.0 ms |  30.17 ms |  1.65 ms |  0.22 |    0.01 | 16333.3333 |  2000.0000 | 2000.0000 |    185 MB |
| Pipelines4Async | 210.6 ms | 233.72 ms | 12.81 ms |  0.23 |    0.02 | 11666.6667 |  2000.0000 | 2000.0000 |    167 MB |
| Pipelines5Async | 249.0 ms | 179.66 ms |  9.85 ms |  0.27 |    0.02 | 19000.0000 |  2000.0000 | 2000.0000 |    200 MB |
| Pipelines6Async | 197.3 ms |  20.97 ms |  1.15 ms |  0.21 |    0.01 | 11666.6667 |  2000.0000 | 2000.0000 |    167 MB |




ver1.　Result
|             Method |     Mean |    Error |  StdDev | Ratio | RatioSD |      Gen 0 |      Gen 1 |     Gen 2 | Allocated |
|------------------- |---------:|---------:|--------:|------:|--------:|-----------:|-----------:|----------:|----------:|
|          ListAsync | 733.4 ms | 177.0 ms | 9.70 ms |  1.00 |    0.00 | 51000.0000 | 17000.0000 | 4000.0000 |    375 MB |
|      SpanCharAsync | 727.9 ms | 178.0 ms | 9.76 ms |  0.99 |    0.02 | 50000.0000 | 16000.0000 | 4000.0000 |    370 MB |
|   YieldReturnAsync | 325.7 ms | 160.3 ms | 8.79 ms |  0.44 |    0.01 | 65000.0000 |  2000.0000 | 2000.0000 |    362 MB |
| StringBuilderAsync | 366.8 ms | 124.9 ms | 6.84 ms |  0.50 |    0.01 | 88000.0000 |  2000.0000 | 2000.0000 |    452 MB |

