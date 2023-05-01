using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.Translate;
using Amazon.Translate.Model;

namespace LambdaAccessPointExample;

public class S3LambdaAccessPoint
{
    private readonly HttpClient _httpClient;
    private readonly IAmazonS3 _s3Client;
    private readonly IAmazonTranslate _translateClient;

    public S3LambdaAccessPoint(HttpClient httpClient, IAmazonS3 s3Client, IAmazonTranslate translateClient)
    {
        _httpClient = httpClient;
        _s3Client = s3Client;
        _translateClient = translateClient;
    }

    [LambdaFunction(ResourceName = "LambdaTranslate", Policies = "AWSLambdaBasicExecutionRole,TranslateReadOnly,AmazonS3ObjectLambdaExecutionRolePolicy")]
    public async Task TranslateS3Object(S3ObjectLambdaEvent evnt, ILambdaContext context)
    {
        var originalRequestUri = new Uri(evnt.UserRequest.Url);
        var language = HttpUtility.ParseQueryString(originalRequestUri.Query).Get("response-content-language");
        context.Logger.LogInformation($"Translating {originalRequestUri.LocalPath} to language {language}");

        var request = new HttpRequestMessage(HttpMethod.Get, evnt.GetObjectContext.InputS3Url);

        using var response = await _httpClient.SendAsync(request);
        

        if(!response.IsSuccessStatusCode)
        {
            context.Logger.LogError($"Failed making presigned URL request to S3. Status code {response.StatusCode}");
            return;    
        }

        context.Logger.LogInformation("Reading content from S3");

        var content = await response.Content.ReadAsStringAsync();

        var translateRequest = new TranslateTextRequest
        {
            SourceLanguageCode = "en-US",
            TargetLanguageCode = language,
            Text = content
        };
        var translateResponse = await _translateClient.TranslateTextAsync(translateRequest);

        var outputStream = new MemoryStream(Encoding.UTF8.GetBytes(translateResponse.TranslatedText), false);

        context.Logger.LogInformation("Sending transformed content back to S3");
        var s3Request = new WriteGetObjectResponseRequest
        {
            RequestRoute = evnt.GetObjectContext.OutputRoute,
            RequestToken = evnt.GetObjectContext.OutputToken,
            ContentType = response.Content.Headers.ContentType?.MediaType,
            StatusCode = 200,
            Body = outputStream
        };
        await _s3Client.WriteGetObjectResponseAsync(s3Request);
    }
}