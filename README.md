# ImportingProcess
Benchmark program to study the efficiency of file ingestion processes that may be found in Japanese business systems.

ver2. Result
|          Method |     Mean |       Error |   StdDev | Ratio | RatioSD |      Gen 0 |      Gen 1 |     Gen 2 | Allocated |
|---------------- |---------:|------------:|---------:|------:|--------:|-----------:|-----------:|----------:|----------:|
|       ListAsync | 802.2 ms | 1,107.03 ms | 60.68 ms |  1.00 |    0.00 | 51000.0000 | 17000.0000 | 4000.0000 |    375 MB |
|    RowByteAsync | 222.8 ms |    82.79 ms |  4.54 ms |  0.28 |    0.02 | 30666.6667 |  1000.0000 | 1000.0000 |    247 MB |
|  RowMemoryAsync | 211.3 ms |    86.08 ms |  4.72 ms |  0.26 |    0.02 | 17000.0000 |  1000.0000 | 1000.0000 |    192 MB |
|  PipelinesAsync | 248.6 ms |   133.96 ms |  7.34 ms |  0.31 |    0.02 | 15000.0000 |          - |         - |    188 MB |
| Pipelines2Async | 239.3 ms |   266.72 ms | 14.62 ms |  0.30 |    0.04 | 18500.0000 |   500.0000 |  500.0000 |    201 MB |
| Pipelines3Async | 231.4 ms |   171.89 ms |  9.42 ms |  0.29 |    0.03 | 16333.3333 |  2000.0000 | 2000.0000 |    185 MB |
| Pipelines4Async | 206.5 ms |   251.37 ms | 13.78 ms |  0.26 |    0.03 | 11666.6667 |  2000.0000 | 2000.0000 |    167 MB |
| Pipelines5Async | 265.0 ms |   208.51 ms | 11.43 ms |  0.33 |    0.03 | 19000.0000 |  2000.0000 | 2000.0000 |    200 MB |
| Pipelines6Async | 212.4 ms |    92.08 ms |  5.05 ms |  0.27 |    0.02 | 11666.6667 |  2000.0000 | 2000.0000 |    167 MB |



ver1.　Result
|             Method |     Mean |    Error |  StdDev | Ratio | RatioSD |      Gen 0 |      Gen 1 |     Gen 2 | Allocated |
|------------------- |---------:|---------:|--------:|------:|--------:|-----------:|-----------:|----------:|----------:|
|          ListAsync | 733.4 ms | 177.0 ms | 9.70 ms |  1.00 |    0.00 | 51000.0000 | 17000.0000 | 4000.0000 |    375 MB |
|      SpanCharAsync | 727.9 ms | 178.0 ms | 9.76 ms |  0.99 |    0.02 | 50000.0000 | 16000.0000 | 4000.0000 |    370 MB |
|   YieldReturnAsync | 325.7 ms | 160.3 ms | 8.79 ms |  0.44 |    0.01 | 65000.0000 |  2000.0000 | 2000.0000 |    362 MB |
| StringBuilderAsync | 366.8 ms | 124.9 ms | 6.84 ms |  0.50 |    0.01 | 88000.0000 |  2000.0000 | 2000.0000 |    452 MB |

