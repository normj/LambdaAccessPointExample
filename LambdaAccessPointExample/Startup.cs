using Microsoft.Extensions.DependencyInjection;

namespace LambdaAccessPointExample
{
    [Amazon.Lambda.Annotations.LambdaStartup]
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAWSService<Amazon.Translate.IAmazonTranslate>();
            services.AddAWSService<Amazon.S3.IAmazonS3>();
        }
    }
}