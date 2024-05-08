using Microsoft.AspNetCore.Mvc;
using Tgs.GameRecord.Endorphina.Core;

namespace Tgs.GameRecord.Endorphina.Controllers;

[Route("[controller]")]
[ApiController]
public class InfoController(
	IConfiguration configuration) : ControllerBase
{
	[HttpGet]
	public object Get()
	{
		var gameRecordInfo = configuration.GetSection("GameRecordInfo").Get<GameRecordInfoOptions>();

		return new
		{
			gameRecordInfo,
		};
	}
}
