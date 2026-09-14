using Avalonia.Controls;
using Avalonia.Interactivity;

namespace NyxAssetsEditor.Views.Shell
{
	public partial class InfoDialog : Window
	{
		public InfoDialog() : this("Info", string.Empty)
		{
		}

		public InfoDialog(string title, string message)
		{
			InitializeComponent();
			TitleText.Text = title;
			Title = title;
			PopulateMessage(message);
		}

		private void PopulateMessage(string message)
		{
			MessageContainer.Children.Clear();
			if (string.IsNullOrEmpty(message)) return;

			var lines = message.Split('\n');
			foreach (var rawLine in lines)
			{
				var line = rawLine.TrimEnd('\r');
				var tb = new SelectableTextBlock
				{
					FontSize = 13,
					TextWrapping = Avalonia.Media.TextWrapping.Wrap,
					FontFamily = new Avalonia.Media.FontFamily("Consolas, Courier New, monospace"),
					Text = line
				};

				var trimmed = line.TrimStart();
				if (trimmed.StartsWith("Added", System.StringComparison.OrdinalIgnoreCase))
				{
					tb.Foreground = Avalonia.Media.Brushes.LightGreen;
				}
				else if (trimmed.StartsWith("Modified", System.StringComparison.OrdinalIgnoreCase))
				{
					tb.Foreground = Avalonia.Media.Brushes.Gold;
				}
				else if (trimmed.StartsWith("Removed", System.StringComparison.OrdinalIgnoreCase))
				{
					tb.Foreground = Avalonia.Media.Brushes.Salmon;
				}
				else if (line.StartsWith("---") || line.EndsWith(":"))
				{
					tb.Foreground = Avalonia.Media.Brushes.White;
					tb.FontWeight = Avalonia.Media.FontWeight.SemiBold;
				}
				else
				{
					tb.Foreground = Avalonia.Media.Brushes.LightGray;
				}

				MessageContainer.Children.Add(tb);
			}
		}

		private void OnOkClick(object? sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}
