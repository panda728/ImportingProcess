﻿using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImportingProcess
{
    [MarkdownExporterAttribute.GitHub]
    [ShortRunJob]
    [MemoryDiagnoser]
    public class Import01
    {
        const string INPUT_FILE = @"data01.txt";
        const int HEADER_LEN = 39;
        const int DETAIL_LEN = 10;
        const int DETAIL_COUNT = 10;
        const int FOOTER_LEN = 39;

        readonly int _footerOffset = HEADER_LEN + DETAIL_LEN * DETAIL_COUNT;
        readonly int _totalLength = HEADER_LEN + DETAIL_LEN * DETAIL_COUNT + FOOTER_LEN;
        readonly Encoding _enc;
        readonly byte[] _input;
        BaseLine _baseLine;

        public Import01()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            _enc = Encoding.GetEncoding("shift-jis");
            //_input = File.ReadAllBytes(INPUT_FILE);
            _input = Enumerable.Repeat(File.ReadAllBytes(INPUT_FILE), 1000).SelectMany(x => x).ToArray();
            _baseLine = new BaseLine(_input);
        }

        #region Benchmark
        [Benchmark(Baseline = true)]
        public async Task BaseLineAsync()
        {
            await _baseLine.RunAsync();
        }
        #endregion

        #region ReadOnlySpan<char>
        [Benchmark]
        public async Task SpanCharAsync()
        {
            var details = ImportSpan();
            await WriteFileAsync(details);
            //Console.WriteLine(JsonConvert.SerializeObject(details));
        }

        private IEnumerable<Row> ImportSpan()
        {
            var details = new List<Row>();
            var lineNum = 1;
            using (var sr = new StreamReader(new MemoryStream(_input), _enc))
            {
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    if (line.Length != _totalLength)
                        throw new ApplicationException($"Data length differs line:{lineNum}");

                    if (string.IsNullOrEmpty(line))
                        break;

                    var header = line.AsSpan()[0..HEADER_LEN];
                    var footer = line.AsSpan().Slice(_footerOffset, FOOTER_LEN);

                    var offset = HEADER_LEN;
                    for (int i = 0; i < DETAIL_COUNT; i++)
                    {
                        var detail = line.AsSpan().Slice(offset, DETAIL_LEN);
                        offset += DETAIL_LEN;
                        details.Add(ConvertSpan(i + 1, header, detail, footer));
                    }
                }
            }
            return details;
        }

        private async Task WriteFileAsync(IEnumerable<Row> details)
        {
            using (var sw = new StreamWriter(new MemoryStream(), _enc))
            {
                foreach (var d in details)
                {
                    var line = $"{d.HeaderID},{d.DetailID:000},{d.Data},{d.Header01}{d.Header02}{d.Header03}{d.Header04}{d.Header05}{d.Header06}{d.Header07}{d.Footer01}{d.Footer02}{d.Footer03}{d.Footer04}{d.Footer05}{d.Footer06}{d.Footer07}{d.Footer08}";
                    await sw.WriteLineAsync(line);
                }
            }
        }

        private Row ConvertSpan(int detailID, ReadOnlySpan<char> header, ReadOnlySpan<char> detail, ReadOnlySpan<char> footer)
        {
            if (!int.TryParse(header.Slice(0, 9), out var headerID))
                throw new ApplicationException("Could not be converted to int.");

            return new Row()
            {
                HeaderID = headerID,
                DetailID = detailID,
                Header01 = header.Slice(9, 4).TrimEnd().ToString(),
                Header02 = header.Slice(13, 7).TrimEnd().ToString(),
                Header03 = header.Slice(20, 5).TrimEnd().ToString(),
                Header04 = header.Slice(25, 2).TrimEnd().ToString(),
                Header05 = header.Slice(27, 3).TrimEnd().ToString(),
                Header06 = header.Slice(30, 3).TrimEnd().ToString(),
                Header07 = header.Slice(33, 6).TrimEnd().ToString(),
                Data = detail.ToString(),
                Footer01 = footer.Slice(0, 5).TrimEnd().ToString(),
                Footer02 = footer.Slice(5, 9).TrimEnd().ToString(),
                Footer03 = footer.Slice(14, 2).TrimEnd().ToString(),
                Footer04 = footer.Slice(16, 3).TrimEnd().ToString(),
                Footer05 = footer.Slice(19, 8).TrimEnd().ToString(),
                Footer06 = footer.Slice(27, 5).TrimEnd().ToString(),
                Footer07 = footer.Slice(32, 3).TrimEnd().ToString(),
                Footer08 = footer.Slice(35, 4).TrimEnd().ToString(),
            };
        }
        #endregion

        #region yield
        [Benchmark]
        public async Task YieldReturnAsync()
        {
            var details = ImportYield();
            await WriteFileAsync(details);
        }

        private IEnumerable<Row> ImportYield()
        {
            var lineNum = 1;
            using (var sr = new StreamReader(new MemoryStream(_input), _enc))
            {
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    if (line.Length != _totalLength)
                        throw new ApplicationException($"Data length differs line:{lineNum}");

                    if (string.IsNullOrEmpty(line))
                        break;

                    var offset = HEADER_LEN;
                    for (int i = 0; i < DETAIL_COUNT; i++)
                    {
                        var span = line.AsSpan();
                        yield return ConvertSpan(
                            i + 1,
                            span[..HEADER_LEN],
                            span.Slice(offset, DETAIL_LEN),
                            span.Slice(_footerOffset, FOOTER_LEN)
                        );
                        offset += DETAIL_LEN;
                    }
                }
            }
        }
        #endregion

        #region StringBuilder
        [Benchmark]
        public async Task StringBuilderAsync()
        {
            var details = ImportYield();
            await WriteFileStringBuilderAsync(details);
        }

        private async Task WriteFileStringBuilderAsync(IEnumerable<Row> details)
        {
            using (var sw = new StreamWriter(new MemoryStream(), _enc))
            {
                foreach (var d in details)
                {
                    var sb = new StringBuilder();
                    sb.Append(d.HeaderID);
                    sb.Append(',');
                    sb.AppendFormat("000", d.DetailID);
                    sb.Append(',');
                    sb.Append(d.Data);
                    sb.Append(',');
                    sb.Append(d.Header01);
                    sb.Append(d.Header02);
                    sb.Append(d.Header03);
                    sb.Append(d.Header04);
                    sb.Append(d.Header05);
                    sb.Append(d.Header06);
                    sb.Append(d.Header07);
                    sb.Append(d.Footer01);
                    sb.Append(d.Footer02);
                    sb.Append(d.Footer03);
                    sb.Append(d.Footer04);
                    sb.Append(d.Footer05);
                    sb.Append(d.Footer06);
                    sb.Append(d.Footer07);
                    sb.Append(d.Footer08);
                    sb.AppendLine();
                    await sw.WriteLineAsync(sb);
                }
            }
        }
        #endregion

    }

}
