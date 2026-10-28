using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace AloneGame.Maui;

// 冒烟宿主只做一件事：把引擎初始化结果显示在屏幕上；内容安装与 PreInitEngine
// 都在 TitanSmokeRunner 里单次执行，Activity 重建不会重复跑。
public sealed class App : Application
{
    protected override Window CreateWindow(IActivationState? state)
    {
        var label = new Label { Text = "Titan engine starting...", Margin = 24 };
        var page = new ContentPage { Content = label };
#if ANDROID
        page.Appearing += async (_, _) =>
        {
            label.Text = await TitanSmokeRunner.Start(Android.App.Application.Context.FilesDir!.AbsolutePath);
        };
#else
        label.Text = "Engine not available on iOS yet";
#endif
        return new Window(page);
    }
}
