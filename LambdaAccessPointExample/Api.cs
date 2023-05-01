using Amazon.Lambda.Core;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using Amazon.S3;
using Amazon.S3.Model;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace LambdaAccessPointExample
{

    public class Api
    {
        private readonly IAmazonS3 _s3Client;
        private const string BUCKET_NAME = "lambda-access-point-example";
        
        public Api(IAmazonS3 s3Client)
        {
            _s3Client = s3Client;
        }

        [LambdaFunction]
        [HttpApi(LambdaHttpMethod.Get, "/{fileName}")]
        public async Task<IHttpResult> GetTranslatedText(string fileName, [FromHeader(Name="Content-Language")] string language, ILambdaContext context)
        {
            if (string.IsNullOrEmpty(language))
                return HttpResults.BadRequest("Missing required header Content-Language");
            
            context.Logger.LogInformation($"Getting file {fileName} in language {language}");
            var getRequest = new GetObjectRequest()
            {
                BucketName = BUCKET_NAME,
                Key = fileName,
                ResponseHeaderOverrides = new ResponseHeaderOverrides
                {
                    ContentLanguage = language
                }
            };

            try
            {
                var response = await _s3Client.GetObjectAsync(getRequest);
                return HttpResults.Ok(response.ResponseStream);
            }
            catch (KeyNotFoundException)
            {
                context.Logger.LogWarning($"Failed to find file {fileName}");
                return HttpResults.NotFound($"No file found with name {fileName}");
            }
            catch(Exception e)
            {
                context.Logger.LogError($"S3 Exception getting file {fileName}: {e.Message}");
                context.Logger.LogError(e.StackTrace);
                return HttpResults.InternalServerError($"Internal service error getting file {fileName}: {e.Message}");
            }
        }
    }
}