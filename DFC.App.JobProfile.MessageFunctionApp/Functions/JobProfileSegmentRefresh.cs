using DFC.App.JobProfile.MessageFunctionApp.Services;
using DFC.Functions.DI.Standard.Attributes;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Threading.Tasks;

namespace DFC.App.JobProfile.MessageFunctionApp.Functions
{
    public class JobProfileSegmentRefresh
    {
        private const string MessageAction = "ActionType";
        private const string MessageContentType = "CType";
        private const string MessageContentId = "Id";
        private static readonly string ThisClassPath = typeof(JobProfileSegmentRefresh).FullName;

        [FunctionName("JobProfileSegmentRefresh")]
        public async Task Run(
                                        [ServiceBusTrigger("%job-profiles-refresh-topic%", "%job-profiles-refresh-subscription%", Connection = "service-bus-connection-string")] Message segmentRefreshMessage,
                                        [ServiceBus("%job-profiles-refresh-topic%", EntityType = EntityType.Topic, Connection = "service-bus-connection-string")] IAsyncCollector<Message> outputTopic,
                                        ILogger log,
                                        [Inject] IMessageProcessor processor,
                                        MessageReceiver messageReceiver,
                                        string lockToken)
        {
            if (segmentRefreshMessage is null)
            {
                throw new System.ArgumentNullException(nameof(segmentRefreshMessage));
            }

            if (processor is null)
            {
                throw new System.ArgumentNullException(nameof(processor));
            }

            segmentRefreshMessage.UserProperties.TryGetValue(MessageAction, out var messageAction); // Parse to enum values
            segmentRefreshMessage.UserProperties.TryGetValue(MessageContentType, out var messageCtype);
            segmentRefreshMessage.UserProperties.TryGetValue(MessageContentId, out var messageContentId);

            // loggger should allow setting up correlation id and should be picked up from message
            log.LogInformation($"{ThisClassPath}: Received message action {messageAction} for type {messageCtype} with Id: {messageContentId}: Correlation id {segmentRefreshMessage.CorrelationId}");

            var message = Encoding.UTF8.GetString(segmentRefreshMessage.Body);

            //Check whether we need to defer failed messages?
            try
            {
                await processor.ProcessSegmentRefreshEventAsync(message, segmentRefreshMessage.SystemProperties.SequenceNumber).ConfigureAwait(false);
                await messageReceiver.CompleteAsync(lockToken);
            }
            //catch (BadRequestException)
            //{
            //    await messageReceiver.DeadLetterAsync(lockToken, "a-reason-code", "An error description");
            //}
            catch (Exception)
            {
                // we will retry...
                await RetryMessageAsync(segmentRefreshMessage, messageReceiver, lockToken, outputTopic);
            }
        }

        private async Task RetryMessageAsync(Message msg, MessageReceiver messageReceiver, string lockToken, IAsyncCollector<Message> outputTopic)
        {
            const string retryCountString = "RetryCount";
            // get our custom RetryCount property from the received message if it exists
            // if not, initiate it to 0
            var retryCount = msg.UserProperties.ContainsKey(retryCountString)
                ? (int)msg.UserProperties[retryCountString]
                : 0;

            // if we've tried 10 times or more, deadletter this message
            if (retryCount >= 10)
            {
                await messageReceiver.DeadLetterAsync(lockToken, "a-reason-code", "An error description");
                //return messageReceiver.DeadLetterAsync(lockToken, msg, "Retry count > 10");
                //await msg.DeadLetterAsync("too-many-retries", "Retry count > 10");
                return;
            }

            // create a copy of the received message
            var clonedMessage = msg.Clone();
            // set the ScheduledEnqueueTimeUtc to 30 seconds from now
            clonedMessage.ScheduledEnqueueTimeUtc = DateTime.UtcNow.AddSeconds(2 ^ retryCount);
            clonedMessage.UserProperties[retryCountString] = retryCount++;
            await outputTopic.AddAsync(clonedMessage);

            // IMPORTANT- Complete the original BrokeredMessage!
            await messageReceiver.CompleteAsync(lockToken);
        }
    }
}