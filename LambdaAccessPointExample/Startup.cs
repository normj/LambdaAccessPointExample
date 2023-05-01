using Microsoft.Extensions.DependencyInjection;
using Amazon.S3;

namespace LambdaAccessPointExample
{
    [Amazon.Lambda.Annotations.LambdaStartup]
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAWSService<IAmazonS3>();
        }
    }
}