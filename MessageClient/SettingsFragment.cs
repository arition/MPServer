using System.Linq;
using Android.Content;
using Android.OS;
using Android.Preferences;
using Android.Text;

namespace MessageClient
{
    public class SettingsFragment : PreferenceFragment, ISharedPreferencesOnSharedPreferenceChangeListener
    {
        protected Intent HeartBeatServiceIntent { get; set; }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            AddPreferencesFromResource(Resource.Xml.Preferences);
            foreach (var key in PreferenceManager.GetDefaultSharedPreferences(Activity).All.Keys)
            {
                var pref = FindPreference(key) as EditTextPreference;
                if (pref != null)
                {
                    AddSummary(pref);
                }
            }

            HeartBeatServiceIntent = new Intent(Activity, typeof(HeartBeatService));
            Activity.StartService(HeartBeatServiceIntent);
        }

        public override void OnResume()
        {
            base.OnResume();
            PreferenceManager.GetDefaultSharedPreferences(Activity).
                RegisterOnSharedPreferenceChangeListener(this);
        }

        public override void OnPause()
        {
            base.OnPause();
            PreferenceManager.GetDefaultSharedPreferences(Activity).
                UnregisterOnSharedPreferenceChangeListener(this);
        }

        public void OnSharedPreferenceChanged(ISharedPreferences sharedPreferences, string key)
        {
            var pref = FindPreference(key) as EditTextPreference;
            if (pref != null)
            {
                AddSummary(pref);
            }
            if (HeartBeatServiceIntent != null)
            {
                Activity.StopService(HeartBeatServiceIntent);
                Activity.StartService(HeartBeatServiceIntent);
            }
        }

        public void AddSummary(EditTextPreference editTextPreference)
        {
            editTextPreference.Summary = editTextPreference.EditText.InputType != (InputTypes.ClassText | InputTypes.TextVariationPassword) ? editTextPreference.Text : editTextPreference.Text.Aggregate("", (all, next) => all + "*");
        }
    }
}