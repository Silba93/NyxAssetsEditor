using System;

namespace NyxAssetsEditor.Services.Rendering;

public static class PixelArtScaling
{
	/// <summary>
	/// Finds the largest uniform integer (or reciprocal-integer) scale that fits
	/// the source inside the available area.
	/// </summary>
	public static double CalculateFitScale(
		double sourceWidth,
		double sourceHeight,
		double availableWidth,
		double availableHeight)
	{
		if (sourceWidth <= 0 || sourceHeight <= 0 || availableWidth <= 0 || availableHeight <= 0)
			return 0;

		var fitScale = Math.Min(availableWidth / sourceWidth, availableHeight / sourceHeight);
		if (fitScale >= 1)
			return Math.Max(1, Math.Floor(fitScale + 1e-9));

		var divisor = Math.Ceiling((1 / fitScale) - 1e-9);
		return 1 / Math.Max(1, divisor);
	}
}
