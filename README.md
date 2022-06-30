# ImportingProcess
Benchmark program to study the efficiency of file ingestion processes that may be found in Japanese business systems.
|             Method |       Mean |        Error |      StdDev |   Median | Ratio | RatioSD |   Gen 0 |   Gen 1 | Allocated |
|------------------- |-----------:|-------------:|------------:|---------:|------:|--------:|--------:|--------:|----------:|
|  FirstVersionAsync |   770.9 us |    212.91 us |    11.67 us | 768.5 us |  1.00 |    0.00 | 69.3359 | 27.3438 |    303 KB |
|      SpanCharAsync |   758.0 us |     89.20 us |     4.89 us | 755.6 us |  0.98 |    0.02 | 66.4063 | 25.3906 |    287 KB |
|   YieldReturnAsync |   887.0 us |  1,949.90 us |   106.88 us | 931.0 us |  1.15 |    0.12 | 68.3594 |  0.9766 |    279 KB |
| StringBuilderAsync |   996.7 us |    432.29 us |    23.70 us | 989.1 us |  1.29 |    0.04 | 89.8438 |  3.9063 |    373 KB |
|    ZStringVerAsync | 1,523.7 us | 19,234.73 us | 1,054.32 us | 923.4 us |  1.96 |    1.33 | 54.6875 | 23.4375 |    246 KB |
