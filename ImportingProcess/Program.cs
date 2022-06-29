using System;
using BenchmarkDotNet.Running;
using ImportingProcess;
using System.Text;

#if DEBUG
var test = new Import01();
test.FirstVersion();
Console.WriteLine("Press any key...");
Console.ReadLine();
#else
var summary = BenchmarkRunner.Run<Import01>();
#endif
