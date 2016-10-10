using System;
using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using TagTag.Backend;
using SQLite.Net.Platform.XamarinAndroid;
using SQLite.Net.Interop;
using System.IO;
using HockeyApp.Android;

namespace TagTag.Droid
{
    [Activity( Label = "TagTag", Icon = "@drawable/icon", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, WindowSoftInputMode = SoftInput.AdjustResize)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsApplicationActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            global::Xamarin.Forms.Forms.Init(this, bundle);
            LoadApplication(new App(new tplat(this)));
            CrashManager.Register(this, "940f01fcceac407b9ece6642ced711c0", new OurCrashListner());
        }
        class OurCrashListner : CrashManagerListener
        {
            public override bool IgnoreDefaultHandler()
            {
                return false;
            }
        }
    }
    class tplat : IPlatform
    {
        readonly Android.Content.Context c;
        public tplat(Android.Content.Context c)
        {
            this.c = c;
        }

        public string AppData
        {
            get
            {
                return c.FilesDir.AbsolutePath;
            }
        }

        SQLitePlatformAndroid sq = new SQLitePlatformAndroid();
        public ISQLitePlatform sqlite
        {
            get
            {
                return sq;
            }
        }

        public void DeleteFile(string path)
        {
            File.Delete(path);
        }


        public Stream ReadFile(string path)
        {
            throw new NotImplementedException();
        }

        public void WriteLine(string s)
        {
            throw new NotImplementedException();
        }
    }
}

