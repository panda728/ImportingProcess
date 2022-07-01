# ImportingProcess
Benchmark program to study the efficiency of file ingestion processes that may be found in Japanese business systems.

|             Method |     Mean |    Error |   StdDev | Ratio | RatioSD |      Gen 0 |     Gen 1 |     Gen 2 | Allocated |
|------------------- |---------:|---------:|---------:|------:|--------:|-----------:|----------:|----------:|----------:|
|  FirstVersionAsync | 94.71 ms | 71.47 ms | 3.918 ms |  1.00 |    0.00 |  5666.6667 | 3000.0000 | 1500.0000 |     41 MB |
|      SpanCharAsync | 72.40 ms | 32.76 ms | 1.795 ms |  0.76 |    0.02 |  5285.7143 | 2714.2857 | 1571.4286 |     39 MB |
|   YieldReturnAsync | 34.21 ms | 14.05 ms | 0.770 ms |  0.36 |    0.02 |  7866.6667 | 1666.6667 | 1600.0000 |     39 MB |
| StringBuilderAsync | 38.95 ms | 13.74 ms | 0.753 ms |  0.41 |    0.01 | 10500.0000 | 2071.4286 | 2000.0000 |     48 MB |
