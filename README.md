# ImportingProcess
Benchmark program to study the efficiency of file ingestion processes that may be found in Japanese business systems.

|               Method |      Mean |     Error |    StdDev | Ratio |      Gen 0 |      Gen 1 |     Gen 2 | Allocated |
|--------------------- |----------:|----------:|----------:|------:|-----------:|-----------:|----------:|----------:|
| UseStreamReaderAsync | 625.29 ms | 446.15 ms | 24.455 ms |  1.00 | 46000.0000 | 16000.0000 | 4000.0000 |    364 MB |
|     Pipe_MemoryAsync |  94.27 ms |  57.08 ms |  3.129 ms |  0.15 |  6200.0000 |  2000.0000 | 2000.0000 |    145 MB |
|     Pipe_StructAsync |  85.98 ms |  18.84 ms |  1.032 ms |  0.14 |  1714.2857 |  1571.4286 | 1571.4286 |    128 MB |
|  Pipe_SeqReaderAsync |  83.83 ms |  23.99 ms |  1.315 ms |  0.13 |  1714.2857 |  1571.4286 | 1571.4286 |    128 MB |
|           StreamRead |  76.82 ms |  39.54 ms |  2.167 ms |  0.12 |  1571.4286 |  1571.4286 | 1571.4286 |    128 MB |

