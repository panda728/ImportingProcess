# ImportingProcess
Benchmark program to study the efficiency of file ingestion processes that may be found in Japanese business systems.

ver2. Result
|         Method |     Mean |    Error |   StdDev | Ratio | RatioSD |      Gen 0 |      Gen 1 |     Gen 2 | Allocated |
|--------------- |---------:|---------:|---------:|------:|--------:|-----------:|-----------:|----------:|----------:|
|      ListAsync | 715.5 ms | 336.0 ms | 18.42 ms |  1.00 |    0.00 | 51000.0000 | 17000.0000 | 4000.0000 |    375 MB |
|   RowByteAsync | 171.4 ms | 705.9 ms | 38.69 ms |  0.24 |    0.06 | 29000.0000 |          - |         - |    116 MB |
| RowMemoryAsync | 166.9 ms | 778.2 ms | 42.66 ms |  0.23 |    0.06 | 15000.0000 |          - |         - |     61 MB |
| PipelinesAsync | 169.1 ms | 695.2 ms | 38.11 ms |  0.24 |    0.06 | 14000.0000 |          - |         - |     56 MB |


ver1.　Result
|             Method |     Mean |    Error |  StdDev | Ratio | RatioSD |      Gen 0 |      Gen 1 |     Gen 2 | Allocated |
|------------------- |---------:|---------:|--------:|------:|--------:|-----------:|-----------:|----------:|----------:|
|      BaselineAsync | 733.4 ms | 177.0 ms | 9.70 ms |  1.00 |    0.00 | 51000.0000 | 17000.0000 | 4000.0000 |    375 MB |
|      SpanCharAsync | 727.9 ms | 178.0 ms | 9.76 ms |  0.99 |    0.02 | 50000.0000 | 16000.0000 | 4000.0000 |    370 MB |
|   YieldReturnAsync | 325.7 ms | 160.3 ms | 8.79 ms |  0.44 |    0.01 | 65000.0000 |  2000.0000 | 2000.0000 |    362 MB |
| StringBuilderAsync | 366.8 ms | 124.9 ms | 6.84 ms |  0.50 |    0.01 | 88000.0000 |  2000.0000 | 2000.0000 |    452 MB |
