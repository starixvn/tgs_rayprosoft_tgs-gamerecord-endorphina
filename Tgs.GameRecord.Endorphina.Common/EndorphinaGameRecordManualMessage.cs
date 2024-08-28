using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tgs.GameRecord.Endorphina.Common;
public record EndorphinaGameRecordManualMessage
{
	[Required]
	[DataType(DataType.Url)]
	[DefaultValue(typeof(Uri), "https://test.endorphina.network")]
	public Uri ApiUrl { get; init; } = default!;
	[Required]
	[DefaultValue(typeof(string), "1494")]
	public string MerchantId { get; init; } = default!;
	[Required]
	[DefaultValue(typeof(string), "9BC9E97FFCD74B16B87BD6FA82C8C2AF")]
	public string SecretKey { get; init; } = default!;
	[Required]
	[DefaultValue(typeof(string), "AA8BB60F5A134264BE838CF6EBA7C8F1")]
	public string Token { get; init; } = default!;
}