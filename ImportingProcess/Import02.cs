﻿using BenchmarkDotNet.Attributes;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Net.Sockets;
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
        readonly BaseLine _baseLine;
        MemoryStream _output;
        readonly byte[] _comma;
        readonly byte[] _newLine;
        int _lineNum;

        public Import02()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            _enc = Encoding.GetEncoding("shift-jis");
            _comma = _enc.GetBytes(",");
            _newLine = _enc.GetBytes("\r\n");
            //_input = File.ReadAllBytes(INPUT_FILE);
            _input = Enumerable.Repeat(File.ReadAllBytes(INPUT_FILE), 1000).SelectMany(x => x).ToArray();
            _baseLine = new BaseLine(_input);
            _output = new MemoryStream();
            _lineNum = 0;
        }

        #region Benchmark
        [Benchmark(Baseline = true)]
        public async Task ListAsync()
        {
            await _baseLine.RunAsync();
        }
        #endregion

        #region Byte
        [Benchmark]
        public async Task RowByteAsync()
        {
            _output = new MemoryStream();
            _lineNum = 0;
            var input = new MemoryStream(_input);
            await foreach (var r in ImportRowByteAsync(input))
                Export(_output, r, _comma, _newLine);
#if DEBUG
            Console.WriteLine(_enc.GetString(_output.ToArray()));
            File.WriteAllBytes("output.txt", _output.ToArray());
            Console.WriteLine();
            Console.WriteLine(_lineNum);
#endif
        }

        private async IAsyncEnumerable<RowByte> ImportRowByteAsync(Stream st)
        {
            for (; ; )
            {
                var buffer = new byte[_totalLengthByte];
                var len = await st.ReadAsync(buffer.AsMemory(0, _totalLengthByte));
                if (len == 0)
                    break;

                if (buffer.Last() == (byte)'\r')
                    throw new ApplicationException("\r not found");

                _lineNum++;
                if (!int.TryParse(_enc.GetString(buffer.AsSpan(0, 9)), out var headerID))
                    throw new ApplicationException("Could not be converted to int.");

                var offset = HEADER_BYTE_LEN;
                for (int i = 0; i < DETAIL_COUNT; i++)
                {
                    yield return new RowByte(
                        headerID,
                        i,
                        buffer.AsSpan(0, HEADER_BYTE_LEN).ToArray(),
                        buffer.AsSpan(offset, DETAIL_BYTE_LEN).ToArray(),
                        buffer.AsSpan(_footerOffsetByte, FOOTER_BYTE_LEN).ToArray()
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
            public ReadOnlySpan<byte> Header01 { get => _header.AsSpan(9, 8); }
            public ReadOnlySpan<byte> Header02 { get => _header.AsSpan(17, 14); }
            public ReadOnlySpan<byte> Header03 { get => _header.AsSpan(31, 10); }
            public ReadOnlySpan<byte> Header04 { get => _header.AsSpan(41, 4); }
            public ReadOnlySpan<byte> Header05 { get => _header.AsSpan(45, 6); }
            public ReadOnlySpan<byte> Header06 { get => _header.AsSpan(51, 6); }
            public ReadOnlySpan<byte> Header07 { get => _header.AsSpan(57, 12); }
            public ReadOnlySpan<byte> Data { get => _detail.AsSpan()[..DETAIL_BYTE_LEN]; }
            public ReadOnlySpan<byte> Footer01 { get => _footer.AsSpan(0, 10); }
            public ReadOnlySpan<byte> Footer02 { get => _footer.AsSpan(10, 18); }
            public ReadOnlySpan<byte> Footer03 { get => _footer.AsSpan(28, 4); }
            public ReadOnlySpan<byte> Footer04 { get => _footer.AsSpan(32, 6); }
            public ReadOnlySpan<byte> Footer05 { get => _footer.AsSpan(38, 16); }
            public ReadOnlySpan<byte> Footer06 { get => _footer.AsSpan(54, 10); }
            public ReadOnlySpan<byte> Footer07 { get => _footer.AsSpan(64, 6); }
            public ReadOnlySpan<byte> Footer08 { get => _footer.AsSpan(70, 8); }
        }
        #endregion

        #region Memroy
        [Benchmark]
        public async Task RowMemoryAsync()
        {
            _output = new MemoryStream();
            _lineNum = 0;
            var input = new MemoryStream(_input);
            await foreach (var r in ImportRowMemoryAsync(input))
                Export(_output, r, _comma, _newLine);
#if DEBUG
            Console.WriteLine(_enc.GetString(_output.ToArray()));
            File.WriteAllBytes("output.txt", _output.ToArray());
            Console.WriteLine();
            Console.WriteLine(_lineNum);
#endif
        }

        private async IAsyncEnumerable<RowMemory> ImportRowMemoryAsync(Stream st)
        {
            for (; ; )
            {
                var buffer = new byte[_totalLengthByte];
                var len = await st.ReadAsync(buffer.AsMemory(0, _totalLengthByte));
                if (len == 0)
                    break;

                if (buffer.Last() == (byte)'\r')
                    throw new ApplicationException("\r not found");

                _lineNum++;
                if (!int.TryParse(_enc.GetString(buffer.AsSpan(0, 9)), out var headerID))
                    throw new ApplicationException("Could not be converted to int.");
                var offset = HEADER_BYTE_LEN;
                for (int i = 0; i < DETAIL_COUNT; i++)
                {
                    yield return new RowMemory(
                        headerID,
                        i,
                        buffer.AsMemory(0,HEADER_BYTE_LEN),
                        buffer.AsMemory(offset, DETAIL_BYTE_LEN),
                        buffer.AsMemory(_footerOffsetByte, FOOTER_BYTE_LEN)
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
            readonly ReadOnlyMemory<byte> _header;
            readonly ReadOnlyMemory<byte> _detail;
            readonly ReadOnlyMemory<byte> _footer;

            public RowMemory(int headerID, int detailID, ReadOnlyMemory<byte> header, ReadOnlyMemory<byte> detail, ReadOnlyMemory<byte> footer)
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
            public ReadOnlySpan<byte> Header03 { get => _header.Slice(31, 10).Span; }
            public ReadOnlySpan<byte> Header04 { get => _header.Slice(41, 4).Span; }
            public ReadOnlySpan<byte> Header05 { get => _header.Slice(45, 6).Span; }
            public ReadOnlySpan<byte> Header06 { get => _header.Slice(51, 6).Span; }
            public ReadOnlySpan<byte> Header07 { get => _header.Slice(57, 12).Span; }
            public ReadOnlySpan<byte> Data { get => _detail[..DETAIL_BYTE_LEN].Span; }
            public ReadOnlySpan<byte> Footer01 { get => _footer[..10].Span; }
            public ReadOnlySpan<byte> Footer02 { get => _footer.Slice(10, 18).Span; }
            public ReadOnlySpan<byte> Footer03 { get => _footer.Slice(28, 4).Span; }
            public ReadOnlySpan<byte> Footer04 { get => _footer.Slice(32, 6).Span; }
            public ReadOnlySpan<byte> Footer05 { get => _footer.Slice(38, 16).Span; }
            public ReadOnlySpan<byte> Footer06 { get => _footer.Slice(54, 10).Span; }
            public ReadOnlySpan<byte> Footer07 { get => _footer.Slice(64, 6).Span; }
            public ReadOnlySpan<byte> Footer08 { get => _footer.Slice(70, 8).Span; }
        }
        #endregion

        #region
        [Benchmark]
        public async Task PipelinesAsync()
        {
            _output = new MemoryStream();
            _lineNum = 0;
            var input = new MemoryStream(_input);
            var pipe = new Pipe();
            var writing = FillPipeAsync(input, pipe.Writer);
            var reading = ReadPipeAsync(pipe.Reader);
            await Task.WhenAll(writing, reading);
#if DEBUG
            Console.WriteLine(_enc.GetString(_output.ToArray()));
            File.WriteAllBytes("output.txt", _output.ToArray());
            Console.WriteLine();
            Console.WriteLine(_lineNum);
#endif
            // https://docs.microsoft.com/ja-jp/dotnet/standard/io/pipelines
            // https://qiita.com/skitoy4321/items/0fc4dc72bc50dabba92b
        }

        public async Task FillPipeAsync(Stream stream, PipeWriter writer)
        {
            while (true)
            {
                try
                {
                    var memory = writer.GetMemory();
                    int byteRead = await stream.ReadAsync(memory);
                    if (byteRead == 0)
                        break;
                    writer.Advance(byteRead);
                }
                catch
                {
                    break;
                }
                var result = await writer.FlushAsync();
                if (result.IsCompleted)
                    break;
            }
            writer.Complete();
        }

        private async Task ReadPipeAsync(PipeReader reader)
        {
            while (true)
            {
                var result = await reader.ReadAsync();
                var buffer = result.Buffer;
                SequencePosition? position;
                do
                {
                    position = buffer.PositionOf((byte)'\n');
                    if (position != null)
                    {
                        try
                        {
                            ParseLine(buffer.Slice(0, position.Value));
                        }
                        catch (Exception ex)
                        {
                            reader.Complete(ex);
                            return;
                        }
                        buffer = buffer.Slice(buffer.GetPosition(1, position.Value));
                    }
                } while (position != null);
                reader.AdvanceTo(buffer.Start, buffer.End);
                if (result.IsCompleted)
                    break;
            }
            reader.Complete();
        }

        private void ParseLine(ReadOnlySequence<byte> seq)
        {
            var lineBuffer = seq.IsSingleSegment ? seq.First : seq.ToArray();
            foreach (var r in ParseRows(lineBuffer))
                Export(_output, r, _comma, _newLine);
        }

        private IEnumerable<RowMemory> ParseRows(ReadOnlyMemory<byte> segment)
        {
            _lineNum++;
            if (!int.TryParse(_enc.GetString(segment[..9].Span), out var headerID))
                throw new ApplicationException("Could not be converted to int.");

            var offset = HEADER_BYTE_LEN;
            for (int i = 0; i < DETAIL_COUNT; i++)
            {
                yield return new RowMemory(
                    headerID,
                    i,
                    segment[..HEADER_BYTE_LEN],
                    segment.Slice(offset, DETAIL_BYTE_LEN),
                    segment.Slice(_footerOffsetByte, FOOTER_BYTE_LEN)
                );
                offset += DETAIL_BYTE_LEN;
            }

        }
        #endregion
    }
}
