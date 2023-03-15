using Microsoft.AspNetCore.Mvc;
using Tgs.GameRecord.EndorphinaService.Core;

namespace Tgs.GameRecord.EndorphinaService.Controllers;

[Route("[controller]")]
[ApiController]
public class InfoController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public InfoController(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    [HttpGet]
    public object Get()
    {
        var gameRecordInfo = _configuration.GetSection("GameRecordInfo").Get<GameRecordInfoOptions>();

        return new
        {
            gameRecordInfo,
        };
    }
}
