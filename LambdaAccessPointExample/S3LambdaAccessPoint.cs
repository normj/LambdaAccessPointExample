using Amazon.Lambda.Annotations;
using Amazon.Lambda.Core;
using Amazon.S3;
using Amazon.Translate;
using Amazon.Lambda.S3Events;

namespace LambdaAccessPointExample;

public class S3LambdaAccessPoint
{
    private readonly IAmazonS3 _s3Client;
    private readonly IAmazonTranslate _translateClient;

    public S3LambdaAccessPoint(IAmazonS3 s3Client, IAmazonTranslate translateClient)
    {
        _s3Client = s3Client;
        _translateClient = translateClient;
    }

    [LambdaFunction]
    public Task TranslateS3Object(S3ObjectLambdaEvent evnt, ILambdaContext context)
    {
        return Task.CompletedTask;
    }
}