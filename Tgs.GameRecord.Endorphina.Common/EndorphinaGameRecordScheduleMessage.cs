using System.ComponentModel;

using System.ComponentModel.DataAnnotations;

namespace Tgs.GameRecord.Endorphina.Common;

public record EndorphinaGameRecordScheduleMessage
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
}
