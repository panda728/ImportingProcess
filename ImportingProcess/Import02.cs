using BenchmarkDotNet.Attributes;
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

        readonly byte[] _crlf = Encoding.ASCII.GetBytes("\r\n");
        readonly byte[] _comma = Encoding.ASCII.GetBytes(",");

        readonly int _footerOffsetByte = HEADER_BYTE_LEN + DETAIL_BYTE_LEN * DETAIL_COUNT;
        readonly int _totalLengthByte = HEADER_BYTE_LEN + DETAIL_BYTE_LEN * DETAIL_COUNT + FOOTER_BYTE_LEN + 2;
        readonly byte[] _input;
        readonly BaseLine _baseLine;
        readonly Encoding _enc;

        public Import02()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            //_input = File.ReadAllBytes(INPUT_FILE);
            _input = Enumerable.Repeat(File.ReadAllBytes(INPUT_FILE), 1000).SelectMany(x => x).ToArray();
            _baseLine = new BaseLine(_input);
            _enc = Encoding.GetEncoding("shift-jis");
        }

        #region Benchmark
        [Benchmark(Baseline = true)]
        public async Task ListAsync()
        {
            await _baseLine.RunAsync();
        }
        #endregion

        #region ReadStream
        [Benchmark]
        public Task ReadStream()
        {
            var input = new MemoryStream(_input);
            var output = new MemoryStream();
            foreach (var r in ProcessRowClass(input))
                r.Export(output, _comma, _crlf);
            return Task.CompletedTask;
        }

        private IEnumerable<RowClass> ProcessRowClass(Stream st)
        {
            for (; ; )
            {
                var buffer = new byte[_totalLengthByte];
                var len = st.Read(buffer);
                if (len == 0)
                    break;

                if (buffer.Last() == (byte)'\r')
                    throw new ApplicationException("\r not found");

                if (!int.TryParse(_enc.GetString(buffer.AsSpan(0, 9)), out var headerID))
                    throw new ApplicationException("Could not be converted to int.");

                var offset = HEADER_BYTE_LEN;
                for (int i = 0; i < DETAIL_COUNT; i++)
                {
                    yield return new RowClass(
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
        #endregion

        #region Row Class ver.
        public class RowClass
        {
            readonly ReadOnlyMemory<byte> _header;
            readonly ReadOnlyMemory<byte> _detail;
            readonly ReadOnlyMemory<byte> _footer;

            readonly int _headerID;
            readonly int _detailID;

            public RowClass(int headerID, int detailID, ReadOnlyMemory<byte> header, ReadOnlyMemory<byte> detail, ReadOnlyMemory<byte> footer)
            {
                _headerID = headerID;
                _detailID = detailID;
                _header = header;
                _detail = detail;
                _footer = footer;
            }

            ReadOnlySpan<byte> HeaderID => Encoding.ASCII.GetBytes($"{_headerID}");
            ReadOnlySpan<byte> DetailID => Encoding.ASCII.GetBytes($"{_detailID:000}");
            ReadOnlySpan<byte> Header01 => _header.Slice(9, 8).Span;
            ReadOnlySpan<byte> Header02 => _header.Slice(17, 14).Span;
            ReadOnlySpan<byte> Header03 => _header.Slice(31, 10).Span;
            ReadOnlySpan<byte> Header04 => _header.Slice(41, 4).Span;
            ReadOnlySpan<byte> Header05 => _header.Slice(45, 6).Span;
            ReadOnlySpan<byte> Header06 => _header.Slice(51, 6).Span;
            ReadOnlySpan<byte> Header07 => _header.Slice(57, 12).Span;
            ReadOnlySpan<byte> Data => _detail[..DETAIL_BYTE_LEN].Span;
            ReadOnlySpan<byte> Footer01 => _footer[..10].Span;
            ReadOnlySpan<byte> Footer02 => _footer.Slice(10, 18).Span;
            ReadOnlySpan<byte> Footer03 => _footer.Slice(28, 4).Span;
            ReadOnlySpan<byte> Footer04 => _footer.Slice(32, 6).Span;
            ReadOnlySpan<byte> Footer05 => _footer.Slice(38, 16).Span;
            ReadOnlySpan<byte> Footer06 => _footer.Slice(54, 10).Span;
            ReadOnlySpan<byte> Footer07 => _footer.Slice(64, 6).Span;
            ReadOnlySpan<byte> Footer08 => _footer.Slice(70, 8).Span;

            public void Export(Stream output, in ReadOnlySpan<byte> comma, in ReadOnlySpan<byte> crlf)
            {
                output.Write(HeaderID);
                output.Write(comma);
                output.Write(DetailID);
                output.Write(comma);
                output.Write(Data);
                output.Write(comma);
                output.Write(Header01);
                output.Write(Header02);
                output.Write(Header03);
                output.Write(Header04);
                output.Write(Header05);
                output.Write(Header06);
                output.Write(Header07);
                output.Write(Footer01);
                output.Write(Footer02);
                output.Write(Footer03);
                output.Write(Footer04);
                output.Write(Footer05);
                output.Write(Footer06);
                output.Write(Footer07);
                output.Write(Footer08);
                output.Write(crlf);
            }
        }
        #endregion

        public static async Task FillPipeAsync(Stream stream, PipeWriter writer)
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

        #region PipeLine-SequencePosition
        [Benchmark]
        public async Task Pipe_SeqPos_ClassAsync()
        {
            var input = new MemoryStream(_input);
            var output = new MemoryStream();

            var pipe = new Pipe();
            var writing = FillPipeAsync(input, pipe.Writer);
            var reading = ReadSeqPositionClassAsync(output, pipe.Reader);
            await Task.WhenAll(writing, reading);
#if DEBUG
            Console.WriteLine(_enc.GetString(output.ToArray()));
            File.WriteAllBytes("output.txt", output.ToArray());
            Console.WriteLine();
#endif
        }

        private async Task ReadSeqPositionClassAsync(Stream output, PipeReader reader)
        {
            while (true)
            {
                var result = await reader.ReadAsync();
                var buffer = result.Buffer;
                SequencePosition? position;
                do
                {
                    position = buffer.PositionOf(_crlf.Last());
                    if (position != null)
                    {
                        try
                        {
                            ParseReadOnlySequenceClass(output, buffer.Slice(0, position.Value));
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

        private void ParseReadOnlySequenceClass(Stream output, in ReadOnlySequence<byte> lineSegment)
        {
            var line = lineSegment.IsSingleSegment
                ? lineSegment.First
                : lineSegment.ToArray();

            if (!int.TryParse(_enc.GetString(line[..9].Span), out var headerID))
                throw new ApplicationException("Could not be converted to int.");

            var offset = HEADER_BYTE_LEN;
            for (int i = 0; i < DETAIL_COUNT; i++)
            {
                var r = new RowClass(
                    headerID,
                    i,
                    line[..HEADER_BYTE_LEN],
                    line.Slice(offset, DETAIL_BYTE_LEN),
                    line.Slice(_footerOffsetByte, FOOTER_BYTE_LEN)
                );
                r.Export(output, _comma, _crlf);
                offset += DETAIL_BYTE_LEN;
            }
        }
        #endregion

        #region PipeLine-SequenceReader
        [Benchmark]
        public async Task Pipe_SeqReader_ClassAsync()
        {
            var input = new MemoryStream(_input);
            var output = new MemoryStream();

            var pipe = new Pipe();
            var writing = FillPipeAsync(input, pipe.Writer);
            var reading = ReadPipeSeqReaderClassAsync(output, pipe.Reader);
            await Task.WhenAll(writing, reading);
#if DEBUG
            Console.WriteLine(_enc.GetString(output.ToArray()));
            File.WriteAllBytes("output.txt", output.ToArray());
            Console.WriteLine();
#endif
            // https://docs.microsoft.com/ja-jp/dotnet/standard/io/pipelines
            // https://qiita.com/skitoy4321/items/0fc4dc72bc50dabba92b
        }

        private async Task ReadPipeSeqReaderClassAsync(Stream output, PipeReader reader)
        {
            while (true)
            {
                var result = await reader.ReadAsync();
                var buffer = result.Buffer;
                ProcessSeqReaderClass(output, ref buffer);
                reader.AdvanceTo(buffer.Start, buffer.End);
                if (result.IsCompleted)
                    break;
            }
            reader.Complete();
        }

        private void ProcessSeqReaderClass(Stream output, ref ReadOnlySequence<byte> buffer)
        {
            var sequenceReader = new SequenceReader<byte>(buffer);
            while (!sequenceReader.End)
            {
                while (sequenceReader.TryReadTo(out ReadOnlySpan<byte> line, _crlf))
                    ParseReadOnlySpanClass(output, line);

                buffer = buffer.Slice(sequenceReader.Position);
                sequenceReader.Advance(buffer.Length);
            }
        }

        private void ParseReadOnlySpanClass(Stream output, in ReadOnlySpan<byte> line)
        {
            if (!int.TryParse(_enc.GetString(line[..9]), out var headerID))
                throw new ApplicationException("Could not be converted to int.");

            Span<byte> header = stackalloc byte[HEADER_BYTE_LEN];
            Span<byte> detail = stackalloc byte[DETAIL_BYTE_LEN];
            Span<byte> footer = stackalloc byte[FOOTER_BYTE_LEN];

            line[..HEADER_BYTE_LEN].CopyTo(header);
            line.Slice(_footerOffsetByte, FOOTER_BYTE_LEN).CopyTo(footer);

            var offset = HEADER_BYTE_LEN;
            for (int i = 0; i < DETAIL_COUNT; i++)
            {
                line.Slice(offset, DETAIL_BYTE_LEN).CopyTo(detail);
                var r = new RowStruct(
                    headerID,
                    i,
                    header,
                    detail,
                    footer
                );
                r.Export(output, _comma, _crlf);

                offset += DETAIL_BYTE_LEN;

            }
        }
        #endregion

        #region PipeLine-SequencePosition
        [Benchmark]
        public async Task Pipe_SeqPos_StructAsync()
        {
            var input = new MemoryStream(_input);
            var output = new MemoryStream();

            var pipe = new Pipe();
            var writing = FillPipeAsync(input, pipe.Writer);
            var reading = ReadSeqPositionStructAsync(output, pipe.Reader);
            await Task.WhenAll(writing, reading);
#if DEBUG
            Console.WriteLine(_enc.GetString(output.ToArray()));
            File.WriteAllBytes("output.txt", output.ToArray());
            Console.WriteLine();
#endif
        }

        private async Task ReadSeqPositionStructAsync(Stream output, PipeReader reader)
        {
            while (true)
            {
                var result = await reader.ReadAsync();
                var buffer = result.Buffer;
                SequencePosition? position;
                do
                {
                    position = buffer.PositionOf(_crlf.Last());
                    if (position != null)
                    {
                        try
                        {
                            ParseReadOnlySequenceStruct(output, buffer.Slice(0, position.Value));
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

        private void ParseReadOnlySequenceStruct(Stream output, in ReadOnlySequence<byte> lineSegment)
        {
            var line = lineSegment.IsSingleSegment
                ? lineSegment.First.Span
                : lineSegment.ToArray().AsSpan();

            if (!int.TryParse(_enc.GetString(line[..9]), out var headerID))
                throw new ApplicationException("Could not be converted to int.");

            var offset = HEADER_BYTE_LEN;
            for (int i = 0; i < DETAIL_COUNT; i++)
            {
                var r = new RowStruct(
                    headerID,
                    i,
                    line[..HEADER_BYTE_LEN],
                    line.Slice(offset, DETAIL_BYTE_LEN),
                    line.Slice(_footerOffsetByte, FOOTER_BYTE_LEN)
                );
                r.Export(output, _comma, _crlf);
                offset += DETAIL_BYTE_LEN;
            }
        }
        #endregion

        #region PipeLine-SequenceReader
        [Benchmark]
        public async Task Pipe_SeqReader_StructAsync()
        {
            var input = new MemoryStream(_input);
            var output = new MemoryStream();

            var pipe = new Pipe();
            var writing = FillPipeAsync(input, pipe.Writer);
            var reading = ReadPipeSeqReaderAsync(output, pipe.Reader);
            await Task.WhenAll(writing, reading);
#if DEBUG
            Console.WriteLine(_enc.GetString(output.ToArray()));
            File.WriteAllBytes("output.txt", output.ToArray());
            Console.WriteLine();
#endif
            // https://docs.microsoft.com/ja-jp/dotnet/standard/io/pipelines
            // https://qiita.com/skitoy4321/items/0fc4dc72bc50dabba92b
        }

        private async Task ReadPipeSeqReaderAsync(Stream output, PipeReader reader)
        {
            while (true)
            {
                var result = await reader.ReadAsync();
                var buffer = result.Buffer;
                ProcessSeqReaderStruct(output, ref buffer);
                reader.AdvanceTo(buffer.Start, buffer.End);
                if (result.IsCompleted)
                    break;
            }
            reader.Complete();
        }

        private void ProcessSeqReaderStruct(Stream output, ref ReadOnlySequence<byte> buffer)
        {
            var sequenceReader = new SequenceReader<byte>(buffer);
            while (!sequenceReader.End)
            {
                while (sequenceReader.TryReadTo(out ReadOnlySpan<byte> line, _crlf))
                    ParseReadOnlySpanStruct(output, line);

                buffer = buffer.Slice(sequenceReader.Position);
                sequenceReader.Advance(buffer.Length);
            }
        }

        private void ParseReadOnlySpanStruct(Stream output, in ReadOnlySpan<byte> line)
        {
            if (!int.TryParse(_enc.GetString(line[..9]), out var headerID))
                throw new ApplicationException("Could not be converted to int.");

            var offset = HEADER_BYTE_LEN;
            for (int i = 0; i < DETAIL_COUNT; i++)
            {
                var r = new RowStruct(
                    headerID,
                    i,
                    line[..HEADER_BYTE_LEN],
                    line.Slice(offset, DETAIL_BYTE_LEN),
                    line.Slice(_footerOffsetByte, FOOTER_BYTE_LEN)
                );
                r.Export(output, _comma, _crlf);

                offset += DETAIL_BYTE_LEN;

            }
        }
        #endregion

        #region Row struct ver.
        public readonly ref struct RowStruct
        {
            readonly ReadOnlySpan<byte> _header;
            readonly ReadOnlySpan<byte> _detail;
            readonly ReadOnlySpan<byte> _footer;

            readonly int _headerID;
            readonly int _detailID;

            public RowStruct(int headerID, int detailID, ReadOnlySpan<byte> header, ReadOnlySpan<byte> detail, ReadOnlySpan<byte> footer)
            {
                _headerID = headerID;
                _detailID = detailID;
                _header = header;
                _detail = detail;
                _footer = footer;
            }

            ReadOnlySpan<byte> HeaderID => Encoding.ASCII.GetBytes($"{_headerID}");
            ReadOnlySpan<byte> DetailID => Encoding.ASCII.GetBytes($"{_detailID:000}");
            ReadOnlySpan<byte> Header01 => _header.Slice(9, 8);
            ReadOnlySpan<byte> Header02 => _header.Slice(17, 14);
            ReadOnlySpan<byte> Header03 => _header.Slice(31, 10);
            ReadOnlySpan<byte> Header04 => _header.Slice(41, 4);
            ReadOnlySpan<byte> Header05 => _header.Slice(45, 6);
            ReadOnlySpan<byte> Header06 => _header.Slice(51, 6);
            ReadOnlySpan<byte> Header07 => _header.Slice(57, 12);
            ReadOnlySpan<byte> Data => _detail[..DETAIL_BYTE_LEN];
            ReadOnlySpan<byte> Footer01 => _footer[..10];
            ReadOnlySpan<byte> Footer02 => _footer.Slice(10, 18);
            ReadOnlySpan<byte> Footer03 => _footer.Slice(28, 4);
            ReadOnlySpan<byte> Footer04 => _footer.Slice(32, 6);
            ReadOnlySpan<byte> Footer05 => _footer.Slice(38, 16);
            ReadOnlySpan<byte> Footer06 => _footer.Slice(54, 10);
            ReadOnlySpan<byte> Footer07 => _footer.Slice(64, 6);
            ReadOnlySpan<byte> Footer08 => _footer.Slice(70, 8);

            public void Export(Stream output, in ReadOnlySpan<byte> comma, in ReadOnlySpan<byte> crlf)
            {
                output.Write(HeaderID);
                output.Write(comma);
                output.Write(DetailID);
                output.Write(comma);
                output.Write(Data);
                output.Write(comma);
                output.Write(Header01);
                output.Write(Header02);
                output.Write(Header03);
                output.Write(Header04);
                output.Write(Header05);
                output.Write(Header06);
                output.Write(Header07);
                output.Write(Footer01);
                output.Write(Footer02);
                output.Write(Footer03);
                output.Write(Footer04);
                output.Write(Footer05);
                output.Write(Footer06);
                output.Write(Footer07);
                output.Write(Footer08);
                output.Write(crlf);
            }
        }
        #endregion
    }
}
