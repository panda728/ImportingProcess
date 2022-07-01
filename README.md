# ImportingProcess
Benchmark program to study the efficiency of file ingestion processes that may be found in Japanese business systems.



|             Method |     Mean |    Error |   StdDev | Ratio | RatioSD |      Gen 0 |     Gen 1 |     Gen 2 | Allocated |
|------------------- |---------:|---------:|---------:|------:|--------:|-----------:|----------:|----------:|----------:|
|  FirstVersionAsync | 80.60 ms | 45.55 ms | 2.497 ms |  1.00 |    0.00 |  5714.2857 | 3000.0000 | 1571.4286 |     40 MB |
|      SpanCharAsync | 77.49 ms | 55.34 ms | 3.033 ms |  0.96 |    0.02 |  5285.7143 | 2714.2857 | 1285.7143 |     39 MB |
|   YieldReturnAsync | 33.71 ms | 22.55 ms | 1.236 ms |  0.42 |    0.03 |  7875.0000 | 1687.5000 | 1625.0000 |     39 MB |
| StringBuilderAsync | 38.25 ms | 13.50 ms | 0.740 ms |  0.47 |    0.02 | 10500.0000 | 2071.4286 | 2000.0000 |     48 MB |
