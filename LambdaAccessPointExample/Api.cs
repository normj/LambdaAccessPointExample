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
        private readonly ApiConfig _apiConfig;

        public Api(ApiConfig apiConfig, IAmazonS3 s3Client)
        {
            if(string.IsNullOrEmpty(apiConfig.DataFilesBucket))
            {
                throw new ArgumentException("Missing configuration for data files bucket");
            }

            _apiConfig = apiConfig;
            _s3Client = s3Client;
        }

        [LambdaFunction(Policies = "AWSLambdaBasicExecutionRole,AmazonS3ReadOnlyAccess,AWSLambdaRole")]
        [HttpApi(LambdaHttpMethod.Get, "/{filename}")]
        public async Task<IHttpResult> GetTranslatedText(string filename, [FromHeader(Name="content-language")] string language, ILambdaContext context)
        {
            if (string.IsNullOrEmpty(language))
                return HttpResults.BadRequest("Missing required header Content-Language");
            
            context.Logger.LogInformation($"Getting file {filename} in language {language}");
            var getRequest = new GetObjectRequest()
            {
                BucketName = _apiConfig.DataFilesBucket,
                Key = ConstructS3Key(filename),
                ResponseHeaderOverrides = new ResponseHeaderOverrides
                {
                    ContentLanguage = language
                }
            };

            try
            {
                var response = await _s3Client.GetObjectAsync(getRequest);
                var transformedText = (new StreamReader(response.ResponseStream)).ReadToEnd();
                return HttpResults.Ok(transformedText);
            }
            catch (KeyNotFoundException)
            {
                context.Logger.LogWarning($"Failed to find file {filename}");
                return HttpResults.NotFound($"No file found with name {filename}");
            }
            catch(Exception e)
            {
                context.Logger.LogError($"S3 Exception getting file {filename}: {e.Message}");
                context.Logger.LogError(e.StackTrace);
                return HttpResults.InternalServerError($"Internal service error getting file {filename}: {e.Message}");
            }
        }

        private string ConstructS3Key(string filename)
        {
            return $"{filename}";
        }
    }
}