using System;
using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;

namespace NyxAssetsEditor.Views.Pages;

public partial class AssetExportDialog : Window
{
	private static string _lastExportDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
	private readonly bool _showThingsFormats;

	public bool IsConfirmed { get; private set; }
	public string ExportPath => PathInput?.Text?.Trim() ?? string.Empty;
	public string ExportName => NameInput?.Text?.Trim() ?? "item";
	public bool SkipWestDirection => SkipWestCheckBox?.IsChecked == true && SkipWestCheckBox.IsVisible;
	public string ExportFormat
	{
		get
		{
			if (PngRadio?.IsChecked == true) return "png";
			if (BmpRadio?.IsChecked == true) return "bmp";
			if (JpgRadio?.IsChecked == true) return "jpg";
			if (ObdRadio?.IsChecked == true) return "obd";
			if (NyxRadio?.IsChecked == true) return "nyx-thing";
			return "png";
		}
	}

	public AssetExportDialog()
	{
		InitializeComponent();
	}

	public AssetExportDialog(string defaultName, bool showThingsFormats) : this()
	{
		_showThingsFormats = showThingsFormats;
		NameInput.Text = defaultName;
		PathInput.Text = _lastExportDirectory;

		if (!showThingsFormats)
		{
			ObdRadio.IsVisible = false;
			NyxRadio.IsVisible = false;
		}

		PathInput.TextChanged += (_, _) => UpdateExportEnabled();
		UpdateExportEnabled();
		SubscribeFormatChanges();
		UpdateSkipWestVisibility();
	}

	private void SubscribeFormatChanges()
	{
		if (PngRadio != null) PngRadio.IsCheckedChanged += OnFormatChanged;
		if (BmpRadio != null) BmpRadio.IsCheckedChanged += OnFormatChanged;
		if (JpgRadio != null) JpgRadio.IsCheckedChanged += OnFormatChanged;
		if (ObdRadio != null) ObdRadio.IsCheckedChanged += OnFormatChanged;
		if (NyxRadio != null) NyxRadio.IsCheckedChanged += OnFormatChanged;
	}

	private void OnFormatChanged(object? sender, RoutedEventArgs e)
	{
		UpdateSkipWestVisibility();
	}

	private void UpdateSkipWestVisibility()
	{
		if (SkipWestCheckBox == null)
			return;

		bool isGraphicalFormat = PngRadio?.IsChecked == true || BmpRadio?.IsChecked == true || JpgRadio?.IsChecked == true;
		SkipWestCheckBox.IsVisible = _showThingsFormats && isGraphicalFormat;
	}

	private void UpdateExportEnabled()
	{
		if (ExportButton != null)
			ExportButton.IsEnabled = !string.IsNullOrWhiteSpace(ExportPath);
	}

	private async void OnBrowseClick(object? sender, RoutedEventArgs e)
	{
		IStorageFolder? start = null;
		if (Directory.Exists(ExportPath))
			start = await StorageProvider.TryGetFolderFromPathAsync(ExportPath);

		var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
		{
			Title = "Export folder",
			AllowMultiple = false,
			SuggestedStartLocation = start
		});
		if (folders.Count > 0)
		{
			PathInput.Text = folders[0].Path.LocalPath;
			_lastExportDirectory = folders[0].Path.LocalPath;
			UpdateExportEnabled();
		}
	}

	private void OnExportClick(object? sender, RoutedEventArgs e)
	{
		if (string.IsNullOrWhiteSpace(ExportPath))
			return;

		IsConfirmed = true;
		_lastExportDirectory = ExportPath;
		Close();
	}

	private void OnCancelClick(object? sender, RoutedEventArgs e)
	{
		Close();
	}
}
