# ImportingProcess
Benchmark program to study the efficiency of file ingestion processes that may be found in Japanese business systems.

ver2. Result
|         Method |     Mean |       Error |   StdDev | Ratio | RatioSD |      Gen 0 |      Gen 1 |     Gen 2 | Allocated |
|--------------- |---------:|------------:|---------:|------:|--------:|-----------:|-----------:|----------:|----------:|
|  BaseLineAsync | 736.8 ms | 1,001.49 ms | 54.89 ms |  1.00 |    0.00 | 51000.0000 | 17000.0000 | 4000.0000 |    375 MB |
|   RowByteAsync | 185.5 ms |   169.70 ms |  9.30 ms |  0.25 |    0.03 | 31000.0000 |  2000.0000 | 2000.0000 |    244 MB |
| RowMemoryAsync | 180.3 ms |    87.94 ms |  4.82 ms |  0.25 |    0.02 | 17000.0000 |  2000.0000 | 2000.0000 |    188 MB |


ver1.　Result
|             Method |     Mean |    Error |  StdDev | Ratio | RatioSD |      Gen 0 |      Gen 1 |     Gen 2 | Allocated |
|------------------- |---------:|---------:|--------:|------:|--------:|-----------:|-----------:|----------:|----------:|
|      BaselineAsync | 733.4 ms | 177.0 ms | 9.70 ms |  1.00 |    0.00 | 51000.0000 | 17000.0000 | 4000.0000 |    375 MB |
|      SpanCharAsync | 727.9 ms | 178.0 ms | 9.76 ms |  0.99 |    0.02 | 50000.0000 | 16000.0000 | 4000.0000 |    370 MB |
|   YieldReturnAsync | 325.7 ms | 160.3 ms | 8.79 ms |  0.44 |    0.01 | 65000.0000 |  2000.0000 | 2000.0000 |    362 MB |
| StringBuilderAsync | 366.8 ms | 124.9 ms | 6.84 ms |  0.50 |    0.01 | 88000.0000 |  2000.0000 | 2000.0000 |    452 MB |
