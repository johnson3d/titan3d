using Microsoft.Maui.Hosting;

namespace AloneGame.Maui;

public static class MauiProgram
{
    // 冒烟宿主不用字体/依赖注入：默认 Configuration 不含 XAML 资源字典。
    public static MauiApp CreateMauiApp() => MauiApp.CreateBuilder().UseMauiApp<App>().Build();
}
