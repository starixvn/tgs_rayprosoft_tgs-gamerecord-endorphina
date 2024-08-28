using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Tgs.GameRecord.Endorphina.Common;
using Valhalla.MessageQueue;

namespace Tgs.GameRecord.Endorphina.Controllers;

[Route("Endor")]
[ApiController]
public class EndorphinaController : ControllerBase
{
	/// <summary>
	/// Games the record manual asynchronous.
	/// </summary>
	/// <param name="messageSender">The message sender.</param>
	/// <param name="source">The source.</param>
	/// <returns></returns>
	[HttpPost("manual")]
	public ValueTask GameRecordManualAsync(
		[FromServices] IMessageSender messageSender,
		EndorphinaGameRecordManualMessage source)
		=> messageSender.PublishAsync(
			subject: EndorphinaNatsConst.ManualPublish,
			data: JsonSerializer.SerializeToUtf8Bytes(source),
			cancellationToken: HttpContext.RequestAborted);

	/// <summary>
	/// Games the record schedule asynchronous.
	/// </summary>
	/// <param name="messageSender">The message sender.</param>
	/// <param name="source">The source.</param>
	/// <returns></returns>
	[HttpPost("schedule")]
	public ValueTask GameRecordScheduleAsync(
		[FromServices] IMessageSender messageSender,
		EndorphinaGameRecordScheduleMessage source)
		=> messageSender.PublishAsync(
			subject: EndorphinaNatsConst.SchedulePublish,
			data: JsonSerializer.SerializeToUtf8Bytes(source),
			cancellationToken: HttpContext.RequestAborted);
}
