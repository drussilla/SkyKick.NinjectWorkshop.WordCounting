﻿using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Rhino.Mocks;
using Should;
using SkyKick.Bcl.Extensions.Reflection;
using SkyKick.Bcl.Logging.ConsoleTestLogger;
using SkyKick.Bcl.Logging.Infrastructure;
using SkyKick.NinjectWorkshop.WordCounting.Http;

namespace SkyKick.NinjectWorkshop.WordCounting.Tests
{
    /// <summary>
    /// Tests for <see cref="WordCountingEngineTests"/>
    /// </summary>
    [TestFixture]
    public class WordCountingEngineTests
    {
        /// <summary>
        /// Cross Component test that tests the happy path of 
        /// <see cref="WordCountingEngine"/> counting the correct
        /// number of words on a web page using mocked
        /// Web Content
        /// </summary>
        [Test]
        [TestCase(
            "SkyKick.NinjectWorkshop.WordCounting.Tests.SampleFiles.TwoWordsHtml.txt",
            2)]
        public async Task CountsWordsInSampleFilesCorrectly(string embeddedHtmlResourceName, int expectedCount)
        {
            // ARRANGE
            var fakeUrl = "http://testing.com/";
            var fakeToken = new CancellationTokenSource().Token;

            var fakeWebContent = GetType().Assembly.GetEmbeddedResourceAsString(embeddedHtmlResourceName);

            var mockWebClient = MockRepository.GenerateMock<IWebClient>();
            mockWebClient
                .Stub(x => x.GetHtmlAsync(
                    Arg.Is(fakeUrl),
                    Arg.Is(fakeToken)))
                .Return(Task.FromResult(fakeWebContent));

            var wordCountingEngine =
                new WordCountingEngine(
                    new WebTextSource(
                        mockWebClient),
                    new WordCountingAlgorithm(),
                    new ConsoleTestLogger(typeof(WordCountingEngine), new LoggerImplementationHelper()));

            // ACT
            var count = await wordCountingEngine.CountWordsOnUrlAsync(fakeUrl, fakeToken);

            // ASSERT
            count.ShouldEqual(expectedCount);
        }
    }
}