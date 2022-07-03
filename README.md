# ImportingProcess
Benchmark program to study the efficiency of file ingestion processes that may be found in Japanese business systems.

ver2. Result
|         Method |     Mean |    Error |   StdDev | Ratio | RatioSD |      Gen 0 |      Gen 1 |     Gen 2 | Allocated |
|--------------- |---------:|---------:|---------:|------:|--------:|-----------:|-----------:|----------:|----------:|
|      ListAsync | 699.0 ms | 108.6 ms |  5.95 ms |  1.00 |    0.00 | 51000.0000 | 17000.0000 | 4000.0000 |    375 MB |
|   RowByteAsync | 178.6 ms | 815.7 ms | 44.71 ms |  0.26 |    0.07 | 29750.0000 |          - |         - |    119 MB |
| RowMemoryAsync | 174.7 ms | 711.5 ms | 39.00 ms |  0.25 |    0.06 | 16000.0000 |          - |         - |     64 MB |
| PipelinesAsync | 171.5 ms | 850.5 ms | 46.62 ms |  0.25 |    0.07 | 14750.0000 |          - |         - |     59 MB |



ver1.　Result
|             Method |     Mean |    Error |  StdDev | Ratio | RatioSD |      Gen 0 |      Gen 1 |     Gen 2 | Allocated |
|------------------- |---------:|---------:|--------:|------:|--------:|-----------:|-----------:|----------:|----------:|
|          ListAsync | 733.4 ms | 177.0 ms | 9.70 ms |  1.00 |    0.00 | 51000.0000 | 17000.0000 | 4000.0000 |    375 MB |
|      SpanCharAsync | 727.9 ms | 178.0 ms | 9.76 ms |  0.99 |    0.02 | 50000.0000 | 16000.0000 | 4000.0000 |    370 MB |
|   YieldReturnAsync | 325.7 ms | 160.3 ms | 8.79 ms |  0.44 |    0.01 | 65000.0000 |  2000.0000 | 2000.0000 |    362 MB |
| StringBuilderAsync | 366.8 ms | 124.9 ms | 6.84 ms |  0.50 |    0.01 | 88000.0000 |  2000.0000 | 2000.0000 |    452 MB |

