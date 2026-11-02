using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace PackFit.Engineering.Desktop;

/// <summary>A minimal native fallback when the main Fluent XAML screen fails to initialize.</summary>
internal sealed class RecoveryWindow : Window
{
    internal RecoveryWindow(Exception failure)
    {
        Title = "PackFit Engineering — Recovery";
        var panel = new StackPanel
        {
            Padding = new Thickness(28),
            Spacing = 14,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        panel.Children.Add(new TextBlock
        {
            Text = "PackFit 無法載入主工作台",
            FontSize = 26,
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
        });
        panel.Children.Add(new TextBlock
        {
            Text = "主視窗發生錯誤，但診斷模式已成功啟動。請先使用完整解壓的 ZIP 版本。",
            TextWrapping = TextWrapping.Wrap
        });
        panel.Children.Add(new TextBlock
        {
            Text = failure.GetType().Name + ": " + failure.Message,
            TextWrapping = TextWrapping.Wrap
        });
        panel.Children.Add(new TextBlock
        {
            Text = "啟動診斷紀錄：\n" + StartupDiagnostics.LogPath,
            IsTextSelectionEnabled = true,
            TextWrapping = TextWrapping.Wrap
        });
        panel.Children.Add(new TextBlock
        {
            Text = "紀錄只在本機；若要回報問題，請先刪除其中的使用者名稱與私人路徑。",
            TextWrapping = TextWrapping.Wrap
        });
        Content = new ScrollViewer { Content = panel };
    }
}
