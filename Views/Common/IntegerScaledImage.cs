using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using NyxAssetsEditor.Services.Rendering;

namespace NyxAssetsEditor.Views.Common;

public sealed class IntegerScaledImage : Control
{
	public static readonly StyledProperty<IImage?> SourceProperty =
		AvaloniaProperty.Register<IntegerScaledImage, IImage?>(nameof(Source));

	static IntegerScaledImage()
	{
		AffectsMeasure<IntegerScaledImage>(SourceProperty);
		AffectsRender<IntegerScaledImage>(SourceProperty);
	}

	public IntegerScaledImage()
	{
		ClipToBounds = true;
		UseLayoutRounding = true;
		RenderOptions.SetBitmapInterpolationMode(this, BitmapInterpolationMode.None);
	}

	public IImage? Source
	{
		get => GetValue(SourceProperty);
		set => SetValue(SourceProperty, value);
	}

	protected override Size MeasureOverride(Size availableSize)
	{
		return Source?.Size ?? default;
	}

	public override void Render(DrawingContext context)
	{
		base.Render(context);

		var source = Source;
		if (source == null)
			return;

		var sourceSize = source.Size;
		var renderScaling = TopLevel.GetTopLevel(this)?.RenderScaling ?? 1;
		var bitmap = source as Bitmap;
		var sourcePixelWidth = bitmap != null
			? bitmap.PixelSize.Width
			: sourceSize.Width * renderScaling;
		var sourcePixelHeight = bitmap != null
			? bitmap.PixelSize.Height
			: sourceSize.Height * renderScaling;
		var scale = PixelArtScaling.CalculateFitScale(
			sourcePixelWidth,
			sourcePixelHeight,
			Bounds.Width * renderScaling,
			Bounds.Height * renderScaling);
		if (scale <= 0)
			return;

		var destinationPixelWidth = sourcePixelWidth * scale;
		var destinationPixelHeight = sourcePixelHeight * scale;
		var destination = new Rect(
			Math.Round((Bounds.Width * renderScaling - destinationPixelWidth) / 2) / renderScaling,
			Math.Round((Bounds.Height * renderScaling - destinationPixelHeight) / 2) / renderScaling,
			destinationPixelWidth / renderScaling,
			destinationPixelHeight / renderScaling);

		context.DrawImage(source, new Rect(sourceSize), destination);
	}
}
