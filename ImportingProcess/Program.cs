using System;
using BenchmarkDotNet.Running;
using ImportingProcess;
using System.Text;

#if DEBUG
//var test = new Import01();
var test = new Import02();
await test.RowByteAsync();
Console.WriteLine("Press any key...");
Console.ReadLine();
#else
//var summary1 = BenchmarkRunner.Run<Import01>();
var summary2 = BenchmarkRunner.Run<Import02>();
#endif
