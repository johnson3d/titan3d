using Android.App;
using Android.Content.PM;
using Android.OS;

namespace AloneGame.Maui;

// 类名写死，避免 R8/反射在真机上按程序集名猜测 Activity。
[Activity(Name = "com.titan3d.alonegame.MainActivity", Theme = "@style/Maui.SplashTheme",
    MainLauncher = true, LaunchMode = LaunchMode.SingleTop,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation |
    ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize |
    ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        // 引擎初始化放到后台线程，UI 线程只负责显示结果。
        _ = TitanSmokeRunner.Start(FilesDir!.AbsolutePath);
    }
}
