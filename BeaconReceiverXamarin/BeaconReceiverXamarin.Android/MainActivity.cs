using Acr.UserDialogs;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Prism;
using Prism.Ioc;
using Xamarin.Forms;

namespace BeaconReceiverXamarin.Droid
{
    [Activity(Label = "すれ違い基盤 受信機 ver.2", Icon = "@drawable/donbiki_neko", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);


            global::Xamarin.Forms.Forms.Init(this, bundle);
            UserDialogs.Init(() => (Activity)Forms.Context);
            //Android6 Marshmallow以降を対象
            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                PowerManager pm = (PowerManager)this.GetSystemService(Context.PowerService);
                if (!pm.IsIgnoringBatteryOptimizations(this.PackageName))
                {
                    //Dozeホワイトリストに追加 -> ActyG1ではAndroidManifest.xmlにREQUEST_IGNORE_BATTERY_OPTIMIZATIONS権限を追加するだけで、自動的にホワイトリストに追加される模様。
                    Intent intent = new Intent(Android.Provider.Settings.ActionRequestIgnoreBatteryOptimizations);
                    intent.SetData(Android.Net.Uri.Parse("package:" + this.PackageName));
                    this.StartActivity(intent);
                }
            }
            LoadApplication(new App(new AndroidInitializer()));
        }
    }

    public class AndroidInitializer : IPlatformInitializer
    {
        public void RegisterTypes(IContainerRegistry container)
        {
            // Register any platform specific implementations
        }
    }
}

