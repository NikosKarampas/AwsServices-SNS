using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Customers.Api.Messaging
{
    public class SnsMessenger : ISnsMessenger
    {
        private readonly IAmazonSimpleNotificationService _sns;
        private readonly IOptions<TopicSettings> _topicSettings;
        private string? _topicArn;

        public SnsMessenger(IAmazonSimpleNotificationService sqs, IOptions<TopicSettings> topicSettings)
        {
            _sns = sqs;
            _topicSettings = topicSettings;
        }

        public async Task<PublishResponse> PublishMessageAsync<T>(T message)
        {
            var topicArn = await GetTopicArnAsync();

            var publishRequest = new PublishRequest
            {
                TopicArn = topicArn,
                Message = JsonSerializer.Serialize(message),
                MessageAttributes = new Dictionary<string, MessageAttributeValue>
                {
                    {
                        "MessageType", new MessageAttributeValue
                        {
                            DataType = "String",
                            StringValue = typeof(T).Name
                        }
                    }
                }
            };

            return await _sns.PublishAsync(publishRequest);
        }        

        private async ValueTask<string> GetTopicArnAsync()
        {
            if (_topicArn is not null)
                return _topicArn;

            var topicArnResponse = await _sns.FindTopicAsync(_topicSettings.Value.Name);
            _topicArn = topicArnResponse.TopicArn;
            return _topicArn;
        }
    }
}
