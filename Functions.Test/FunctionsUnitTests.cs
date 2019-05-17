using Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using WAMS.Functions;
using Xunit;


namespace Functions.Tests
{
    public class FunctionsUnitTests
    {
        private readonly ILogger logger = TestFactory.CreateLogger();

        [Theory]
        [MemberData(nameof(TestFactory.MediaJobInput), MemberType = typeof(TestFactory))]
        public async void HttpTrigger_SubmitJob(string name, Job job)
        {
            // Set AMS Credentials
            var launchSettings = new LaunchSettingsFixture();

            var request = TestFactory.CreateHttpRequest((Job)job);
            var response =
                (OkObjectResult) await SubmitJob.Run(request, logger, new ExecutionContext());
            Assert.NotNull(response);
        }

    }
}
