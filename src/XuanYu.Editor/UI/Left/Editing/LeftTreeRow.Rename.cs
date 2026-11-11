using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
namespace XuanYu.Editor.UI.Left;
internal sealed partial class LeftTreeRow
{
    private bool _renaming;
    public void BeginRename(Action<string> rename)
    {
        if (_renaming) return;
        _renaming = true;
        var grid = (Grid)Content!;
        var input = new TextBox
        {
            Text = _title.Text,
            FontFamily = _title.FontFamily, FontSize = _title.FontSize,
            Foreground = EditorTheme.TextMain, Background = EditorTheme.PanelBg,
            MinWidth = 0, MinHeight = 0,
            Padding = new Thickness(3, 0),
            BorderThickness = new Thickness(0),
            VerticalContentAlignment = VerticalAlignment.Center
        };
        input.Resources["TextControlBorderThemeThicknessFocused"] = new Thickness(0);
        input.Resources["TextControlBorderBrushFocused"] = Brushes.Transparent;
        input.Resources["TextControlBorderBrushPointerOver"] = Brushes.Transparent;
        var frame = new Border
        {
            Height = 23, Background = EditorTheme.PanelBg,
            BorderBrush = EditorTheme.Accent,
            BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(2),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Child = input
        };
        void FitWidth()
        {
            var text = new TextBlock
            {
                Text = input.Text ?? "",
                FontFamily = _title.FontFamily,
                FontSize = _title.FontSize,
                FontWeight = _normalWeight
            };
            text.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            double desired = Math.Max(62, text.DesiredSize.Width + 16);
            double available = grid.ColumnDefinitions[3].ActualWidth;
            frame.Width = available > 0
                ? Math.Min(desired, Math.Max(48, available - 4)) : desired;
        }
        Grid.SetColumn(frame, 3);
        grid.Children.Add(frame);
        _title.IsVisible = false;
        FitWidth();
        input.TextChanged += (_, _) =>
        {
            frame.BorderBrush = EditorTheme.Accent;
            FitWidth();
        };
        bool finished = false;
        void Finish(bool save)
        {
            if (finished)
                return;
            finished = true;
            if (save && input.Text != _title.Text)
            {
                try { rename(input.Text ?? ""); }
                catch (Exception error) when
                    (error is ArgumentException or InvalidOperationException)
                {
                    finished = false;
                    frame.BorderBrush = Brushes.IndianRed;
                    ToolTip.SetTip(frame, error.Message);
                    return;
                }
            }
            _renaming = false;
            grid.Children.Remove(frame);
            _title.IsVisible = true;
        }
        input.KeyDown += (_, e) =>
        {
            if (e.Key is Key.Enter or Key.Escape)
            {
                Finish(e.Key == Key.Enter);
                e.Handled = true;
            }
        };
        input.LostFocus += (_, _) => Finish(true);
        Dispatcher.UIThread.Post(() =>
        {
            input.Focus();
            input.SelectAll();
        });
    }
}