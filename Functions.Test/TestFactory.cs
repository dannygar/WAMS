using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using System.IO;
using Data.Entities;
using Newtonsoft.Json;

namespace Functions.Tests
{
    public class TestFactory
    {
        public static IEnumerable<object[]> MediaJobInput()
        {
            var job = new Job()
            {
                AssetId = "nb:cid:UUID:28f40785-7bac-4337-af96-0957bb1bcb62",
                IndexV2 = new IndexV2()
                {
                    Language = "EnUs",
                    OutputStorage = "dannymedia"
                },
                Mes = new Mes()
                {
                    Preset = "H264 Multiple Bitrate 720p with thumbnail.json"
                },
                Ocr = new Ocr()
                {
                    Language = "AutoDetect",
                    OutputStorage = "dannymedia"
                },
                Summarization = new Summarization()
                {
                    Duration = 0.0,
                    OutputStorage = "dannymedia"
                },
                UseEncoderOutputForAnalytics = true,
                VideoAnnotation = new VideoAnnotation()
                {
                    Mode = "PreFaceEmotion",
                    OutputStorage = "dannymedia"
                }
            };

            return new List<object[]>
            {
                new object[] { "data", job }
            };
        }

        private static Dictionary<string, StringValues> CreateDictionary(string key, string value)
        {
            var qs = new Dictionary<string, StringValues>
            {
                { key, value }
            };
            return qs;
        }

        public static DefaultHttpRequest CreateHttpRequest(Job data)
        {
            var request = new DefaultHttpRequest(new DefaultHttpContext())
            {
                Body = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data)))
            };
            return request;
        }

        public static ILogger CreateLogger(LoggerTypes type = LoggerTypes.Null)
        {
            ILogger logger;

            if (type == LoggerTypes.List)
            {
                logger = new ListLogger();
            }
            else
            {
                logger = NullLoggerFactory.Instance.CreateLogger("Null Logger");
            }

            return logger;
        }
    }
}