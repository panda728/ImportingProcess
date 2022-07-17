using System;
using BenchmarkDotNet.Running;
using ImportingProcess;
using System.Text;

#if DEBUG
var test = new PipeTest();
await test.ReadStreamStruct();
Console.WriteLine("Press any key...");
Console.ReadLine();
#else
var summary = BenchmarkRunner.Run<PipeTest>();
#endif
