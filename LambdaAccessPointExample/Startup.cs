using Microsoft.Extensions.DependencyInjection;

namespace LambdaAccessPointExample
{
    [Amazon.Lambda.Annotations.LambdaStartup]
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            var config = new ApiConfig
            {
                DataFilesBucket = Environment.GetEnvironmentVariable("DATA_FILES_BUCKET")
            };

            services.AddSingleton<ApiConfig>(config);

            services.AddHttpClient();

            services.AddAWSService<Amazon.Translate.IAmazonTranslate>();
            services.AddAWSService<Amazon.S3.IAmazonS3>();
        }
    }
}