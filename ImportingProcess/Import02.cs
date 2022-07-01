using BenchmarkDotNet.Attributes;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImportingProcess
{
    [MarkdownExporterAttribute.GitHub]
    [ShortRunJob]
    [MemoryDiagnoser]
    public class Import02
    {
        const string INPUT_FILE = @"data01.txt";
        const int HEADER_LEN = 39;
        const int DETAIL_LEN = 10;
        const int DETAIL_COUNT = 10;
        const int FOOTER_LEN = 39;

        const int HEADER_BYTE_LEN = 9 + 30 * 2;
        const int DETAIL_BYTE_LEN = 10;
        const int FOOTER_BYTE_LEN = 39 * 2;

        readonly int _footerOffset = HEADER_LEN + DETAIL_LEN * DETAIL_COUNT;
        readonly int _totalLength = HEADER_LEN + DETAIL_LEN * DETAIL_COUNT + FOOTER_LEN;
        readonly int _footerOffsetByte = HEADER_BYTE_LEN + DETAIL_BYTE_LEN * DETAIL_COUNT;
        readonly int _totalLengthByte = HEADER_BYTE_LEN + DETAIL_BYTE_LEN * DETAIL_COUNT + FOOTER_BYTE_LEN + 2;
        readonly Encoding _enc;
        readonly byte[] _input;

        public Import02()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            _enc = Encoding.GetEncoding("shift-jis");
            //_input = File.ReadAllBytes(INPUT_FILE);
            _input = Enumerable.Repeat(File.ReadAllBytes(INPUT_FILE), 1000).SelectMany(x => x).ToArray();
        }
        #region Benchmark
        [Benchmark(Baseline = true)]
        public async Task Baseline02Async()
        {
            var details = Import();
            await WriteFileAsync(details);
            //Console.WriteLine(JsonConvert.SerializeObject(details));
        }

        private IEnumerable<Row> Import()
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

                    var header = line.Substring(0, HEADER_LEN);
                    var footer = line.Substring(_footerOffset, FOOTER_LEN);

                    var offset = HEADER_LEN;
                    for (int i = 0; i < DETAIL_COUNT; i++)
                    {
                        var detail = line.Substring(offset, DETAIL_LEN);
                        offset += DETAIL_LEN;
                        details.Add(Convert(i + 1, header, detail, footer));
                    }
                }
            }
            return details;
        }

        private Row Convert(int detailID, string header, string detail, string footer)
        {
            if (!int.TryParse(header.AsSpan(0, 9), out var headerID))
                throw new ApplicationException("Could not be converted to int.");

            return new Row()
            {
                HeaderID = headerID,
                DetailID = detailID,
                Header01 = header.Substring(9, 4).TrimEnd(),
                Header02 = header.Substring(13, 7).TrimEnd(),
                Header03 = header.Substring(20, 5).TrimEnd(),
                Header04 = header.Substring(25, 2).TrimEnd(),
                Header05 = header.Substring(27, 3).TrimEnd(),
                Header06 = header.Substring(30, 3).TrimEnd(),
                Header07 = header.Substring(33, 6).TrimEnd(),
                Data = detail,
                Footer01 = footer.Substring(0, 5).TrimEnd(),
                Footer02 = footer.Substring(5, 9).TrimEnd(),
                Footer03 = footer.Substring(14, 2).TrimEnd(),
                Footer04 = footer.Substring(16, 3).TrimEnd(),
                Footer05 = footer.Substring(19, 8).TrimEnd(),
                Footer06 = footer.Substring(27, 5).TrimEnd(),
                Footer07 = footer.Substring(32, 3).TrimEnd(),
                Footer08 = footer.Substring(35, 4).TrimEnd(),
            };
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
        #endregion

        [Benchmark]
        public async Task MemoryAsync()
        {
            var comma = _enc.GetBytes(",");
            var newLine = _enc.GetBytes("\n");
            using (var ms = new MemoryStream())
            {
                await foreach (var r in ImportMemoryAsync())
                {
                    ms.Write(_enc.GetBytes($"{r.HeaderID},"));
                    ms.Write(_enc.GetBytes($"{r.DetailID:000},"));
                    ms.Write(r.Data.Span);
                    ms.Write(comma);
                    ms.Write(r.Header01.Span);
                    ms.Write(r.Header02.Span);
                    ms.Write(r.Header03.Span);
                    ms.Write(r.Header04.Span);
                    ms.Write(r.Header05.Span);
                    ms.Write(r.Header06.Span);
                    ms.Write(r.Header07.Span);
                    ms.Write(r.Footer01.Span);
                    ms.Write(r.Footer02.Span);
                    ms.Write(r.Footer03.Span);
                    ms.Write(r.Footer04.Span);
                    ms.Write(r.Footer05.Span);
                    ms.Write(r.Footer06.Span);
                    ms.Write(newLine);
                }
            }
        }

        private async IAsyncEnumerable<RowMemory> ImportMemoryAsync()
        {
            var lineNum = 1;
            using (var ms = new MemoryStream(_input))
            {
                for (; ; )
                {
                    var buffer = new byte[_totalLengthByte];
                    var len = await ms.ReadAsync(buffer);
                    if (len == 0)
                        break;
                    if (buffer.Last() == (byte)'\r')
                        throw new ApplicationException("LF not found");

                    var offset = HEADER_BYTE_LEN;
                    for (int i = 0; i < DETAIL_COUNT; i++)
                    {
                        yield return new RowMemory(
                            lineNum++,
                            i,
                            buffer.AsMemory()[..HEADER_BYTE_LEN],
                            buffer.AsMemory().Slice(offset, DETAIL_BYTE_LEN),
                            buffer.AsMemory().Slice(_footerOffsetByte, FOOTER_BYTE_LEN)
                        );
                        offset += DETAIL_BYTE_LEN;
                    }
                }
            }
        }

        public class RowMemory
        {
            readonly int _headerID;
            readonly int _detailID;
            readonly Memory<byte> _header;
            readonly Memory<byte> _detail;
            readonly Memory<byte> _footer;

            public RowMemory(int headerID, int detailID, Memory<byte> header, Memory<byte> detail, Memory<byte> footer)
            {
                _headerID = headerID;
                _detailID = detailID;
                _header = header;
                _detail = detail;
                _footer = footer;
            }

            public int HeaderID { get => _headerID; }
            public int DetailID { get => _detailID; }
            public Memory<byte> Header01 { get => _header.Slice(9, 8); }
            public Memory<byte> Header02 { get => _header.Slice(17, 14); }
            public Memory<byte> Header03 { get => _header.Slice(24, 10); }
            public Memory<byte> Header04 { get => _header.Slice(28, 4); }
            public Memory<byte> Header05 { get => _header.Slice(32, 6); }
            public Memory<byte> Header06 { get => _header.Slice(38, 6); }
            public Memory<byte> Header07 { get => _header.Slice(44, 12); }
            public Memory<byte> Data { get => _detail[..DETAIL_BYTE_LEN]; }
            public Memory<byte> Footer01 { get => _footer.Slice(0, 10); }
            public Memory<byte> Footer02 { get => _footer.Slice(10, 18); }
            public Memory<byte> Footer03 { get => _footer.Slice(28, 4); }
            public Memory<byte> Footer04 { get => _footer.Slice(32, 6); }
            public Memory<byte> Footer05 { get => _footer.Slice(38, 16); }
            public Memory<byte> Footer06 { get => _footer.Slice(54, 10); }
            public Memory<byte> Footer07 { get => _footer.Slice(64, 6); }
            public Memory<byte> Footer08 { get => _footer.Slice(70, 8); }
        }
    }
}
