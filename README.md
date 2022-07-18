# ImportingProcess
Benchmark program to study the efficiency of file ingestion processes that may be found in Japanese business systems.

|             Method |     Mean |     Error |   StdDev | Ratio |      Gen 0 |      Gen 1 |     Gen 2 | Allocated |
|------------------- |---------:|----------:|---------:|------:|-----------:|-----------:|----------:|----------:|
|  FirstVersionAsync | 845.9 ms | 520.17 ms | 28.51 ms |  1.00 | 53000.0000 | 17000.0000 | 4000.0000 |    386 MB |
|      SpanCharAsync | 713.5 ms | 488.48 ms | 26.78 ms |  0.84 | 50000.0000 | 16000.0000 | 4000.0000 |    370 MB |
|   YieldReturnAsync | 325.3 ms |  19.46 ms |  1.07 ms |  0.38 | 65000.0000 |  2000.0000 | 2000.0000 |    362 MB |
| StringBuilderAsync | 371.0 ms | 184.73 ms | 10.13 ms |  0.44 | 88000.0000 |  2000.0000 | 2000.0000 |    452 MB |
