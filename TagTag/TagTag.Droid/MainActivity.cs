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

namespace TagTag.Droid
{
    [Activity(Label = "TagTag", Icon = "@drawable/icon", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsApplicationActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            global::Xamarin.Forms.Forms.Init(this, bundle);
            LoadApplication(new App(new tplat()));
        }
    }
    class tplat : IPlatform
    {
        public string AppData
        {
            get
            {
                return System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
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
    }
}

