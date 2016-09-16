using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Preferences;

namespace MessageClient
{
    [Activity(Label = "MessageClient", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            PreferenceManager.SetDefaultValues(this, Resource.Xml.Preferences, false);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            FragmentManager.BeginTransaction().Replace(Resource.Id.Content, new SettingsFragment()).Commit();

        }
    }
}

