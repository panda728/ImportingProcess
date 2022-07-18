# ImportingProcess
Benchmark program to study the efficiency of file ingestion processes that may be found in Japanese business systems.

|                     Method |      Mean |     Error |   StdDev | Ratio |      Gen 0 |      Gen 1 |     Gen 2 | Allocated |
|--------------------------- |----------:|----------:|---------:|------:|-----------:|-----------:|----------:|----------:|
|       UseStreamReaderAsync | 640.16 ms | 181.30 ms | 9.937 ms |  1.00 | 46000.0000 | 16000.0000 | 4000.0000 |    364 MB |
|    Pipe_SeqPos_MemoryAsync |  88.21 ms |  22.57 ms | 1.237 ms |  0.14 |  1666.6667 |  1333.3333 | 1333.3333 |    129 MB |
|    Pipe_SeqPos_StructAsync |  84.97 ms |  24.00 ms | 1.316 ms |  0.13 |  1714.2857 |  1571.4286 | 1571.4286 |    128 MB |
| Pipe_SeqReader_StructAsync |  82.21 ms |  16.98 ms | 0.931 ms |  0.13 |  1714.2857 |  1571.4286 | 1571.4286 |    128 MB |
|           ReadStreamStruct |  76.13 ms |  48.46 ms | 2.657 ms |  0.12 |  1571.4286 |  1571.4286 | 1571.4286 |    128 MB |

