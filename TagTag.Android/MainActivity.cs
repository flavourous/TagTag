using Android.App;
using Android.Content.PM;
using Android.OS;
using Avalonia.Android;

namespace TagTag.Android;

[Activity(
    Label = "TagTag",
    Theme = "@style/MyTheme.NoActionBar",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public sealed class MainActivity : AvaloniaMainActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        CompositionRoot.PlatformRegister(new Platform(FilesDir!.AbsolutePath));
        base.OnCreate(savedInstanceState);
    }
}
