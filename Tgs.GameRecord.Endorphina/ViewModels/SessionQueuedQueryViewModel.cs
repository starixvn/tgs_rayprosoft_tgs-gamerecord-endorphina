using System.ComponentModel;

using System.ComponentModel.DataAnnotations;

namespace Tgs.GameRecord.Endorphina.ViewModels;

public record SessionQueuedQueryViewModel
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
