using BenchmarkDotNet.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImportingProcess
{
    [ShortRunJob]
    [MemoryDiagnoser]
    public class Import01
    {
        const string INPUT_FILE = @"data01.txt";
        const string OUTPUT_FILE = @"output.txt";
        const int HEADER_LEN = 39;
        const int DETAIL_LEN = 10;
        const int DETAIL_COUNT = 10;
        const int FOOTER_LEN = 39;

        readonly int _footerOffset = HEADER_LEN + DETAIL_LEN * DETAIL_COUNT;
        readonly int _totalLength = HEADER_LEN + DETAIL_LEN * DETAIL_COUNT + FOOTER_LEN;
        readonly Encoding _enc;

        public Import01()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            _enc = Encoding.GetEncoding("shift-jis");
        }

        #region Benchmark
        [Benchmark]
        public void FirstVersion()
        {
            var details = new List<Detail>();
            var lineNum = 1;
            using (var sr = new StreamReader(INPUT_FILE, _enc))
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
            WriteFile(details);
            //Console.WriteLine(JsonConvert.SerializeObject(details));
        }

        private Detail Convert(int detailID, string header, string detail, string footer)
        {
            if (!int.TryParse(header.Substring(0, 9), out var headerID))
                throw new ApplicationException("Could not be converted to int.");

            return new Detail()
            {
                HeaderID = headerID,
                DetailID = detailID,
                Header01 = header.Substring(9, 4),
                Header02 = header.Substring(13, 7),
                Header03 = header.Substring(20, 5),
                Header04 = header.Substring(25, 2),
                Header05 = header.Substring(27, 3),
                Header06 = header.Substring(30, 3),
                Header07 = header.Substring(33, 6),
                Data = detail,
                Footer01 = footer.Substring(0, 5),
                Footer02 = footer.Substring(5, 9),
                Footer03 = footer.Substring(14, 2),
                Footer04 = footer.Substring(16, 3),
                Footer05 = footer.Substring(19, 8),
                Footer06 = footer.Substring(27, 5),
                Footer07 = footer.Substring(32, 3),
                Footer08 = footer.Substring(35, 4),
            };
        }
        private void WriteFile(IEnumerable<Detail> details)
        {
            using (var sw = new StreamWriter(OUTPUT_FILE, false, _enc))
            {
                foreach (var d in details)
                {
                    sw.Write($"{d.HeaderID}");
                    sw.Write(",");
                    sw.Write($"{d.DetailID:000}");
                    sw.Write(",");
                    sw.Write($"{d.Header01}");
                    sw.Write($"{d.Header02}");
                    sw.Write($"{d.Header03}");
                    sw.Write($"{d.Header04}");
                    sw.Write($"{d.Footer01}");
                    sw.Write($"{d.Footer02}");
                    sw.Write(",");
                    sw.Write($"{d.Data}");
                    sw.WriteLine();
                }
            }
        }
        #endregion

        #region ReadOnlySpan<char>
        [Benchmark]
        public void SpanChar()
        {
            var details = new List<Detail>();
            var lineNum = 1;
            using (var sr = new StreamReader(INPUT_FILE, _enc))
            {
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    if (line.Length != _totalLength)
                        throw new ApplicationException($"Data length differs line:{lineNum}");

                    if (string.IsNullOrEmpty(line))
                        break;

                    var header = line.AsSpan().Slice(0, HEADER_LEN);
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
            WriteFile(details);
            //Console.WriteLine(JsonConvert.SerializeObject(details));
        }
        private Detail ConvertSpan(int detailID, ReadOnlySpan<char> header, ReadOnlySpan<char> detail, ReadOnlySpan<char> footer)
        {
            if (!int.TryParse(header.Slice(0, 9), out var headerID))
                throw new ApplicationException("Could not be converted to int.");

            return new Detail()
            {
                HeaderID = headerID,
                DetailID = detailID,
                Header01 = header.Slice(9, 4).ToString(),
                Header02 = header.Slice(13, 7).ToString(),
                Header03 = header.Slice(20, 5).ToString(),
                Header04 = header.Slice(25, 2).ToString(),
                Header05 = header.Slice(27, 3).ToString(),
                Header06 = header.Slice(30, 3).ToString(),
                Header07 = header.Slice(33, 6).ToString(),
                Data = detail.ToString(),
                Footer01 = footer.Slice(0, 5).ToString(),
                Footer02 = footer.Slice(5, 9).ToString(),
                Footer03 = footer.Slice(14, 2).ToString(),
                Footer04 = footer.Slice(16, 3).ToString(),
                Footer05 = footer.Slice(19, 8).ToString(),
                Footer06 = footer.Slice(27, 5).ToString(),
                Footer07 = footer.Slice(32, 3).ToString(),
                Footer08 = footer.Slice(35, 4).ToString(),
            };
        }
        #endregion

        #region yield
        [Benchmark]
        public void YieldReturn()
        {
            var details = Import();
            WriteFile(details);
        }

        private IEnumerable<Detail> Import()
        {
            var lineNum = 1;
            using (var sr = new StreamReader(INPUT_FILE, _enc))
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
                            span.Slice(0, HEADER_LEN),
                            span.Slice(offset, DETAIL_LEN),
                            span.Slice(_footerOffset, FOOTER_LEN)
                        );
                        offset += DETAIL_LEN;
                    }
                }
            }
        }
        #endregion
    }

    public class Detail
    {
        public int HeaderID { get; set; }
        public int DetailID { get; set; }
        public string Header01 { get; set; }
        public string Header02 { get; set; }
        public string Header03 { get; set; }
        public string Header04 { get; set; }
        public string Header05 { get; set; }
        public string Header06 { get; set; }
        public string Header07 { get; set; }
        public string Data { get; set; }
        public string Footer01 { get; set; }
        public string Footer02 { get; set; }
        public string Footer03 { get; set; }
        public string Footer04 { get; set; }
        public string Footer05 { get; set; }
        public string Footer06 { get; set; }
        public string Footer07 { get; set; }
        public string Footer08 { get; set; }
    }
}
