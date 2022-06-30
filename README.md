# ImportingProcess
Benchmark program to study the efficiency of file ingestion processes that may be found in Japanese business systems.
|             Method |        Mean |        Error |      StdDev | Ratio | RatioSD |     Gen 0 |     Gen 1 |     Gen 2 | Allocated |
|------------------- |------------:|-------------:|------------:|------:|--------:|----------:|----------:|----------:|----------:|
|  FirstVersionAsync | 75,189.4 us |  69,593.9 us | 3,814.67 us | 1.000 |    0.00 | 5571.4286 | 2857.1429 | 1571.4286 | 40,946 KB |
|      SpanCharAsync | 79,258.9 us |  42,706.4 us | 2,340.88 us | 1.056 |    0.06 | 5285.7143 | 2428.5714 | 1285.7143 | 40,398 KB |
|   YieldReturnAsync |    347.3 us |     200.7 us |    11.00 us | 0.005 |    0.00 |   92.2852 |    1.9531 |         - |    379 KB |
| StringBuilderAsync |    441.4 us |     277.1 us |    15.19 us | 0.006 |    0.00 |  114.7461 |    0.4883 |         - |    471 KB |
