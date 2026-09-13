using System;
using System.Linq;
using Avalonia;
using Avalonia.Platform.Storage;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.VisualTree;
using NyxAssetsEditor.ViewModels.ArchiveLoaders;

namespace NyxAssetsEditor.Views.ArchiveLoaders;

public partial class FloatingThingFinderControl : UserControl
{
	private FloatingThingFinderViewModel? _viewModel;
	private int _lastPage = 1;

	public FloatingThingFinderControl()
	{
		InitializeComponent();

		ResultListListBox.PointerWheelChanged += OnListBoxPointerWheelChanged;
		ResultGridListBox.PointerWheelChanged += OnListBoxPointerWheelChanged;

		DataContextChanged += (_, _) =>
		{
			if (_viewModel != null)
			{
				_viewModel.PropertyChanged -= OnViewModelPropertyChanged;
			}

			_viewModel = DataContext as FloatingThingFinderViewModel;
			if (_viewModel != null)
			{
				_viewModel.PropertyChanged += OnViewModelPropertyChanged;
				_lastPage = _viewModel.CurrentPage;
			}
		};

		var titleBar = this.FindControl<Border>("TitleBar");
		if (titleBar == null) return;
		var interaction = new FloatingPanelInteraction(this, titleBar, minWidth: 860, minHeight: 430);
		Register(interaction, "ResizeLeft", 4);
		Register(interaction, "ResizeRight", 1);
		Register(interaction, "ResizeBottom", 2);
		Register(interaction, "ResizeCorner", 3);
	}

	private void Register(FloatingPanelInteraction interaction, string name, int direction)
	{
		var border = this.FindControl<Border>(name);
		if (border != null) interaction.RegisterResizeHandle(border, direction);
	}

	protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
	{
		base.OnAttachedToVisualTree(e);
		if (DataContext is not FloatingThingFinderViewModel vm || !vm.IsDefaultPosition)
			return;

		var canvas = GetParentCanvas();
		if (canvas == null) return;

		void CenterPanel()
		{
			if (canvas.Bounds.Width <= 0 || canvas.Bounds.Height <= 0) return;
			vm.DockState = "Floating";
			vm.PositionX = Math.Max(0, (canvas.Bounds.Width - vm.PanelWidth) / 2);
			vm.PositionY = Math.Max(0, (canvas.Bounds.Height - vm.ContentHeight) / 2);
			vm.IsDefaultPosition = false;
		}

		if (canvas.Bounds.Width > 0 && canvas.Bounds.Height > 0)
			CenterPanel();
		else
		{
			canvas.SizeChanged += OnCanvasSizeChanged;
			void OnCanvasSizeChanged(object? sender, SizeChangedEventArgs args)
			{
				if (args.NewSize.Width <= 0 || args.NewSize.Height <= 0) return;
				canvas.SizeChanged -= OnCanvasSizeChanged;
				CenterPanel();
			}
		}
	}

	private Canvas? GetParentCanvas()
	{
		Visual? visual = this;
		while (visual != null && visual is not Canvas)
			visual = visual.GetVisualParent();
		return visual as Canvas;
	}

	private void OnResultPointerPressed(object? sender, PointerPressedEventArgs e)
	{
		if (sender is not Control control || control.DataContext is not ThingFinderResultViewModel result || DataContext is not FloatingThingFinderViewModel vm)
			return;

		if (e.GetCurrentPoint(control).Properties.IsRightButtonPressed)
		{
			if (!result.IsSelected)
				vm.SelectResult(result, shift: false, ctrl: false);
			return;
		}

		var shift = e.KeyModifiers.HasFlag(KeyModifiers.Shift);
		var ctrl = e.KeyModifiers.HasFlag(KeyModifiers.Control);
		vm.SelectResult(result, shift, ctrl);

		if (e.GetCurrentPoint(control).Properties.IsLeftButtonPressed)
			e.Handled = true;
	}

	private void OnResultContextRequested(object? sender, ContextRequestedEventArgs e)
	{
		if (sender is not Control control || control.DataContext is not ThingFinderResultViewModel result || DataContext is not FloatingThingFinderViewModel vm) return;
		var selected = vm.GetSelectedResults();
		if (selected.Count == 0 || !selected.Contains(result))
		{
			vm.SelectResult(result, shift: false, ctrl: false);
			selected = vm.GetSelectedResults();
		}

		var menu = new ContextMenu();
		
		var copy = new MenuItem { Header = selected.Count > 1 ? $"Copy IDs ({selected.Count})" : "Copy ID" };
		copy.Click += async (_, _) =>
		{
			var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
			if (clipboard != null)
			{
				var ids = string.Join(", ", selected.Select(s => s.DisplayedId));
				await clipboard.SetTextAsync(ids);
			}
		};
		menu.Items.Add(copy);
		menu.Items.Add(new Separator());

		var export = new MenuItem { Header = selected.Count > 1 ? $"Export selected ({selected.Count})..." : "Export..." };
		export.Click += (_, _) =>
		{
			if (vm.SourcePanel != null)
			{
				var thingItems = selected.Select(s => new ThingItemViewModel(s.Thing.Id, vm.SourcePanel)).ToList();
				vm.SourcePanel.RequestExportThings(thingItems);
			}
		};

		var openNew = new MenuItem { Header = selected.Count > 1 ? $"Open in new windows ({selected.Count})" : "Open in new window" };
		openNew.Click += async (_, _) =>
		{
			if (vm.SourcePanel != null)
			{
				foreach (var sel in selected)
				{
					await vm.Parent.OpenThingEditor(vm.SourcePanel, sel.Thing.Id, newWindow: true, sel.Thing.Kind);
				}
			}
		};

		var edit = new MenuItem { Header = "Edit" };
		edit.Click += async (_, _) =>
		{
			if (vm.SourcePanel != null)
			{
				await vm.Parent.OpenThingEditor(vm.SourcePanel, result.Thing.Id, newWindow: false, result.Thing.Kind);
				await vm.SourcePanel.NavigateToThing(result.Thing.Id, result.Thing.Kind);
			}
		};

		menu.Items.Add(export);
		menu.Items.Add(new Separator());
		menu.Items.Add(openNew);
		menu.Items.Add(edit);

		var actions = result.GetContextActions();
		if (actions.Count > 0)
		{
			menu.Items.Add(new Separator());
			foreach (var action in actions)
			{
				var item = new MenuItem { Header = action.Label };
				item.Click += async (_, _) => await result.ExecuteContextActionAsync(action);
				menu.Items.Add(item);
			}
		}

		menu.Open(control);
		e.Handled = true;
	}

	private void OnDragOver(object? sender, DragEventArgs e)
	{
		var files = e.DataTransfer.TryGetFiles()?.ToList();
		var valid = files is { Count: 1 } && files[0].TryGetLocalPath() is { } path;
		e.DragEffects = valid ? DragDropEffects.Copy : DragDropEffects.None;
		e.Handled = true;
	}

	private async void OnDrop(object? sender, DragEventArgs e)
	{
		e.Handled = true;
		if (DataContext is not FloatingThingFinderViewModel vm) return;
		var files = e.DataTransfer.TryGetFiles()?.ToList();
		if (files is { Count: 1 } && files[0].TryGetLocalPath() is { } path)
		{
			try
			{
				using var stream = System.IO.File.OpenRead(path);
				var bitmap = new Avalonia.Media.Imaging.Bitmap(stream);
				await vm.LoadSpriteFromBitmapAsync(bitmap);
			}
			catch (Exception ex)
			{
				vm.SearchBySpriteStatus = $"Error loading file: {ex.Message}";
			}
		}
	}

	private async void OnResultDoubleTapped(object? sender, TappedEventArgs e)
	{
		if (DataContext is FloatingThingFinderViewModel vm && sender is Control control && control.DataContext is ThingFinderResultViewModel result)
		{
			if (vm.SourcePanel != null)
			{
				await vm.Parent.OpenThingEditor(vm.SourcePanel, result.Thing.Id, newWindow: false, result.Thing.Kind);
				await vm.SourcePanel.NavigateToThing(result.Thing.Id, result.Thing.Kind);
			}
		}
	}

	private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
	{
		if (_viewModel == null) return;

		if (e.PropertyName == nameof(FloatingThingFinderViewModel.CurrentPage))
		{
			int newPage = _viewModel.CurrentPage;
			if (newPage == _lastPage) return;
			bool isBackwards = newPage < _lastPage;
			_lastPage = newPage;

			var listBox = _viewModel.IsGridView ? ResultGridListBox : ResultListListBox;
			if (listBox != null)
			{
				Avalonia.Threading.Dispatcher.UIThread.Post(() =>
				{
					var scrollViewer = listBox.FindDescendantOfType<ScrollViewer>();
					if (scrollViewer != null)
					{
						if (isBackwards)
						{
							scrollViewer.Offset = new Vector(scrollViewer.Offset.X, scrollViewer.Extent.Height);
						}
						else
						{
							scrollViewer.Offset = new Vector(scrollViewer.Offset.X, 0);
						}
					}
				}, Avalonia.Threading.DispatcherPriority.Loaded);
			}
		}
	}

	private void OnListBoxPointerWheelChanged(object? sender, PointerWheelEventArgs e)
	{
		if (_viewModel == null || sender is not ListBox listBox) return;
		var scrollViewer = listBox.FindDescendantOfType<ScrollViewer>();
		if (scrollViewer == null) return;

		if (e.Delta.Y > 0) // Scrolling up
		{
			if (scrollViewer.Offset.Y <= 0.01)
			{
				if (_viewModel.HasPreviousPage)
				{
					_viewModel.CurrentPage--;
					e.Handled = true;
				}
			}
		}
		else if (e.Delta.Y < 0) // Scrolling down
		{
			double maxScroll = scrollViewer.Extent.Height - scrollViewer.Viewport.Height;
			if (scrollViewer.Offset.Y >= maxScroll - 0.01)
			{
				if (_viewModel.HasNextPage)
				{
					_viewModel.CurrentPage++;
					e.Handled = true;
				}
			}
		}
	}
}
