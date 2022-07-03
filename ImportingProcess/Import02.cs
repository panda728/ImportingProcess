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

        const int HEADER_BYTE_LEN = 9 + 30 * 2;
        const int DETAIL_BYTE_LEN = 10;
        const int FOOTER_BYTE_LEN = 39 * 2;
        const int DETAIL_COUNT = 10;

        readonly int _footerOffsetByte = HEADER_BYTE_LEN + DETAIL_BYTE_LEN * DETAIL_COUNT;
        readonly int _totalLengthByte = HEADER_BYTE_LEN + DETAIL_BYTE_LEN * DETAIL_COUNT + FOOTER_BYTE_LEN + 2;
        readonly Encoding _enc;
        readonly byte[] _input;
        BaseLine _baseLine;

        public Import02()
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

        #region Byte
        [Benchmark]
        public async Task RowByteAsync()
        {
            var comma = _enc.GetBytes(",");
            var newLine = _enc.GetBytes("\n");

            var input = new MemoryStream(_input);
            var output = new MemoryStream();

            await foreach (var r in ImportRowByteAsync(input))
                Export(output, r, comma, newLine);
            //File.WriteAllBytes("output.txt", output.ToArray());
        }

        private async IAsyncEnumerable<RowByte> ImportRowByteAsync(Stream st)
        {
            var lineNum = 1;
            for (; ; )
            {
                var buffer = new byte[_totalLengthByte];
                var len = await st.ReadAsync(buffer.AsMemory(0, _totalLengthByte));
                if (len == 0)
                    break;

                if (buffer.Last() == (byte)'\r')
                    throw new ApplicationException("LF not found");

                var offset = HEADER_BYTE_LEN;
                for (int i = 0; i < DETAIL_COUNT; i++)
                {
                    yield return new RowByte(
                        lineNum++,
                        i,
                        buffer.AsSpan()[..HEADER_BYTE_LEN].ToArray(),
                        buffer.AsSpan().Slice(offset, DETAIL_BYTE_LEN).ToArray(),
                        buffer.AsSpan().Slice(_footerOffsetByte, FOOTER_BYTE_LEN).ToArray()
                    );
                    offset += DETAIL_BYTE_LEN;
                }
            }
        }

        private void Export(Stream st, RowByte r, ReadOnlySpan<byte> comma, ReadOnlySpan<byte> newLine)
        {
            st.Write(_enc.GetBytes($"{r.HeaderID}"));
            st.Write(comma);
            st.Write(_enc.GetBytes($"{r.DetailID:000}"));
            st.Write(comma);
            st.Write(r.Data);
            st.Write(comma);
            st.Write(r.Header01);
            st.Write(r.Header02);
            st.Write(r.Header03);
            st.Write(r.Header04);
            st.Write(r.Header05);
            st.Write(r.Header06);
            st.Write(r.Header07);
            st.Write(r.Footer01);
            st.Write(r.Footer02);
            st.Write(r.Footer03);
            st.Write(r.Footer04);
            st.Write(r.Footer05);
            st.Write(r.Footer06);
            st.Write(newLine);
        }

        public class RowByte
        {
            readonly int _headerID;
            readonly int _detailID;
            readonly byte[] _header;
            readonly byte[] _detail;
            readonly byte[] _footer;

            public RowByte(int headerID, int detailID, byte[] header, byte[] detail, byte[] footer)
            {
                _headerID = headerID;
                _detailID = detailID;
                _header = header;
                _detail = detail;
                _footer = footer;
            }

            public int HeaderID { get => _headerID; }
            public int DetailID { get => _detailID; }
            public ReadOnlySpan<byte> Header01 { get => _header.AsSpan().Slice(9, 8); }
            public ReadOnlySpan<byte> Header02 { get => _header.AsSpan().Slice(17, 14); }
            public ReadOnlySpan<byte> Header03 { get => _header.AsSpan().Slice(24, 10); }
            public ReadOnlySpan<byte> Header04 { get => _header.AsSpan().Slice(28, 4); }
            public ReadOnlySpan<byte> Header05 { get => _header.AsSpan().Slice(32, 6); }
            public ReadOnlySpan<byte> Header06 { get => _header.AsSpan().Slice(38, 6); }
            public ReadOnlySpan<byte> Header07 { get => _header.AsSpan().Slice(44, 12); }
            public ReadOnlySpan<byte> Data { get => _detail.AsSpan()[..DETAIL_BYTE_LEN]; }
            public ReadOnlySpan<byte> Footer01 { get => _footer.AsSpan().Slice(0, 10); }
            public ReadOnlySpan<byte> Footer02 { get => _footer.AsSpan().Slice(10, 18); }
            public ReadOnlySpan<byte> Footer03 { get => _footer.AsSpan().Slice(28, 4); }
            public ReadOnlySpan<byte> Footer04 { get => _footer.AsSpan().Slice(32, 6); }
            public ReadOnlySpan<byte> Footer05 { get => _footer.AsSpan().Slice(38, 16); }
            public ReadOnlySpan<byte> Footer06 { get => _footer.AsSpan().Slice(54, 10); }
            public ReadOnlySpan<byte> Footer07 { get => _footer.AsSpan().Slice(64, 6); }
            public ReadOnlySpan<byte> Footer08 { get => _footer.AsSpan().Slice(70, 8); }
        }
        #endregion

        #region Memroy
        [Benchmark]
        public async Task RowMemoryAsync()
        {
            var comma = _enc.GetBytes(",");
            var newLine = _enc.GetBytes("\n");

            var input = new MemoryStream(_input);
            var output = new MemoryStream();

            await foreach (var r in ImportRowMemoryAsync(input))
                Export(output, r, comma, newLine);
        }

        private async IAsyncEnumerable<RowMemory> ImportRowMemoryAsync(Stream st)
        {
            var lineNum = 1;
            for (; ; )
            {
                var buffer = new byte[_totalLengthByte];
                var len = await st.ReadAsync(buffer.AsMemory(0, _totalLengthByte));
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

        private void Export(Stream st, RowMemory r, ReadOnlySpan<byte> comma, ReadOnlySpan<byte> newLine)
        {
            st.Write(_enc.GetBytes($"{r.HeaderID}"));
            st.Write(comma);
            st.Write(_enc.GetBytes($"{r.DetailID:000}"));
            st.Write(comma);
            st.Write(r.Data);
            st.Write(comma);
            st.Write(r.Header01);
            st.Write(r.Header02);
            st.Write(r.Header03);
            st.Write(r.Header04);
            st.Write(r.Header05);
            st.Write(r.Header06);
            st.Write(r.Header07);
            st.Write(r.Footer01);
            st.Write(r.Footer02);
            st.Write(r.Footer03);
            st.Write(r.Footer04);
            st.Write(r.Footer05);
            st.Write(r.Footer06);
            st.Write(newLine);
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
            public ReadOnlySpan<byte> Header01 { get => _header.Slice(9, 8).Span; }
            public ReadOnlySpan<byte> Header02 { get => _header.Slice(17, 14).Span; }
            public ReadOnlySpan<byte> Header03 { get => _header.Slice(24, 10).Span; }
            public ReadOnlySpan<byte> Header04 { get => _header.Slice(28, 4).Span; }
            public ReadOnlySpan<byte> Header05 { get => _header.Slice(32, 6).Span; }
            public ReadOnlySpan<byte> Header06 { get => _header.Slice(38, 6).Span; }
            public ReadOnlySpan<byte> Header07 { get => _header.Slice(44, 12).Span; }
            public ReadOnlySpan<byte> Data { get => _detail[..DETAIL_BYTE_LEN].Span; }
            public ReadOnlySpan<byte> Footer01 { get => _footer.Slice(0, 10).Span; }
            public ReadOnlySpan<byte> Footer02 { get => _footer.Slice(10, 18).Span; }
            public ReadOnlySpan<byte> Footer03 { get => _footer.Slice(28, 4).Span; }
            public ReadOnlySpan<byte> Footer04 { get => _footer.Slice(32, 6).Span; }
            public ReadOnlySpan<byte> Footer05 { get => _footer.Slice(38, 16).Span; }
            public ReadOnlySpan<byte> Footer06 { get => _footer.Slice(54, 10).Span; }
            public ReadOnlySpan<byte> Footer07 { get => _footer.Slice(64, 6).Span; }
            public ReadOnlySpan<byte> Footer08 { get => _footer.Slice(70, 8).Span; }
        }
        #endregion

    }
}
