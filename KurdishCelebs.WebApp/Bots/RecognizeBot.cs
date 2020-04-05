using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System;
using System.IO;
using KurdishCelebs.WebApp.Helpers;
using KurdishCelebs.WebApp.Services;
using System.Net.Http;

namespace KurdishCelebs.WebApp.Bots
{
    public class RecognizeBot : ActivityHandler
    {
        private readonly FacialRecognitionService _recognitionService;
        private readonly HttpClient _httpClient;

        public RecognizeBot(FacialRecognitionService recognitionService, HttpClient httpClient)
        {
            _recognitionService = recognitionService;
            _httpClient = httpClient;
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var attachment = turnContext.Activity.Attachments?.FirstOrDefault(a => a.ContentType.StartsWith("image/"));
            if (attachment is null)
            {
                var replyText = $"تکایە وێنەی کەسێک بنێرە 😒";
                await turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);
            }
            else
            {
                try
                {
                    var original = await DownloadImage(attachment.ContentUrl);

                    var result = _recognitionService.Search(original);
                    var top = result.Matches.First();

                    using var memoryStream = ImageProcessingHelper.CreateImage(original, result);

                    var base64 = $"data:image/jpeg;base64,{memoryStream.ConvertToBase64()}";

                    var responseAttachment = new Attachment("image/jpeg", contentUrl: base64);

                    var text = $"تۆ {top.Confidence:p1} لە {top.Name} دەچیت!";
                    var message = MessageFactory.Attachment(responseAttachment, text, text, text);
                    await turnContext.SendActivityAsync(message);
                }
                catch (NoFaceFoundException)
                {
                    var replyText = $"ئەو وێنەیە هیچ دەموچاوێکی تێدا نییە 😢";
                    await turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);
                }
                catch (Exception)
                {
                    var replyText = $"هەڵەیەک ڕوویدا لە کاتی جێبەجێکردنی داواکارییەکەت 🤷‍♂️";
                    await turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);
                }
            }
        }

        private async Task<string> DownloadImage(string url)
        {
            try
            {
                var fileName = Path.GetTempFileName() + ".jpg";
                using (var stream = await _httpClient.GetStreamAsync(url))
                using (var file = File.OpenWrite(fileName))
                {
                    await stream.CopyToAsync(file);
                }

                return fileName;
            }
            catch (Exception)
            {
                return null;
            }
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeText = "بەخێربێی! وێنەی کەسێک بنێرە و پێت دەڵێم بە کام هونەرمەند دەچێت!";
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);
                }
            }
        }
    }
}
