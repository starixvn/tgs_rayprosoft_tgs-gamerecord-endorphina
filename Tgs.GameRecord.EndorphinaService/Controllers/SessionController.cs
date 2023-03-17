using Google.Protobuf;
using Microsoft.AspNetCore.Mvc;
using Tgs.GameRecord.EndorphinaService.Core.Protos;
using Tgs.GameRecord.EndorphinaService.ViewModels;
using Tgs.MessageQueue;

namespace Tgs.GameRecord.EndorphinaService.Controllers;

[Route("[controller]")]
[ApiController]
public class SessionController : ControllerBase
{
    /// <summary>
    /// Sessions the process asynchronous.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="source">The source.</param>
    /// <returns></returns>
    /// {
//  "apiUrl": "string",
//  "merchantId": "string",
//  "secretKey": "string",
//  "token": "string"
//}
[HttpPost("SessionProcess")]
    public async ValueTask SessionProcessAsync(
        [FromServices] IMessageSender sender,
        SessionProcessQueryViewModel source)
    {
        var request = new SessionProcess()
        {
            ApiUrl = source.ApiUrl,
            MerchantId = source.MerchantId,
            SecretKey = source.SecretKey,
            Token = source.Token
        };

        await sender.PublishAsync(
            subject: "gp.gamerecord.endor.session.process",
            data: request.ToByteArray(),
            cancellationToken: HttpContext.RequestAborted)
        .ConfigureAwait(false);
    }

    [HttpPost("SessionQueued")]
    public async ValueTask SessionQueuedAsync(
        [FromServices] IMessageSender sender,
        SessionProcessQueryViewModel source)
    {
        var request = new SessionProcess()
        {
            ApiUrl = source.ApiUrl,
            MerchantId = source.MerchantId,
            SecretKey = source.SecretKey,
            Token = source.Token
        };

        await sender.PublishAsync(
            subject: "gp.gamerecord.endor.session.queued",
            data: request.ToByteArray(),
            cancellationToken: HttpContext.RequestAborted)
        .ConfigureAwait(false);
    }
}
