﻿using System;
using System.Collections;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Rhino.Mocks;
using Should;
using SkyKick.NinjectWorkshop.WordCounting.Http;
using SkyKick.NinjectWorkshop.WordCounting.Tests.Helpers;

namespace SkyKick.NinjectWorkshop.WordCounting.Tests.Http
{
    /// <summary>
    /// Tests for <see cref="WebTextSource"/>
    /// </summary>
    [TestFixture]
    public class WebTextSourceTests
    {
        public IEnumerable InvokesRetryPolicyExceptions()
        {
            yield return new object[]
            {
                new Exception("General Exception should be retried"),
                true
            };

            yield return new object[]
            {
                WebExceptionHelper.CreateWebExceptionWithStatusCode(HttpStatusCode.InternalServerError), 
                // retry on a 500
                true
            };

            yield return new object[]
            {
                WebExceptionHelper.CreateWebExceptionWithStatusCode(HttpStatusCode.NotFound), 
                // do not retry on 404
                false
            };
        }

        /// <summary>
        /// <see cref="WebTextSource"/> will retry on certain 
        /// exceptions but not others.  Verifies when <see cref="IWebClient"/>
        /// throws <paramref name="webClientException"/> that the 
        /// retry policy is invoked if <paramref name="expectRetry"/>.  This
        /// is verified by counting the number of times 
        /// <see cref="IWebClient.GetHtmlAsync"/> is called.
        /// </summary>
        [Test]
        [TestCaseSource(nameof(InvokesRetryPolicyExceptions))]
        public async Task InvokesRetryPolicyOnErrors(Exception webClientException, bool expectRetry)
        {
            // ARRANGE
            var fakeWebTextSourceOptions = new WebTextSourceOptions
            {
                RetryTimes = new[]
                {
                    TimeSpan.FromSeconds(0),
                    TimeSpan.FromSeconds(0),
                    TimeSpan.FromSeconds(0)
                }
            };

            var fakeUrl = "http://testing.com";
            var fakeToken = new CancellationTokenSource().Token;

            var mockWebClient = MockRepository.GenerateStrictMock<IWebClient>();
            mockWebClient
                .Expect(x => x.GetHtmlAsync(Arg.Is(fakeUrl), Arg.Is(fakeToken)))
                .Throw(webClientException)
                .Repeat.Times(
                    // 1 for initial call and then any retries
                    1 +
                    (expectRetry
                        ? fakeWebTextSourceOptions.RetryTimes.Length
                        : 0));

            var webTextSource = new WebTextSource(mockWebClient, fakeWebTextSourceOptions, fakeUrl);

            // ACT
            try
            {
                await webTextSource.GetTextAsync(fakeToken);

                Assert.Fail("Expected an exception to be thrown but was not.");
            }
            catch (Exception e)
            {
                // ASSERT
                e.ShouldEqual(webClientException);

                mockWebClient.VerifyAllExpectations();
            }
        }
    }
}
