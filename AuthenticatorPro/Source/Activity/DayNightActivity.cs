﻿using Android.Content.Res;
using Android.OS;
using AndroidX.AppCompat.App;
using AndroidX.Preference;

namespace AuthenticatorPro.Activity
{
    internal abstract class DayNightActivity : AppCompatActivity
    {
        protected bool IsDark { get; private set; }

        private bool _checkedOnCreate;
        private string _lastTheme;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            _checkedOnCreate = true;
            UpdateTheme();
            base.OnCreate(savedInstanceState);
        }

        private void UpdateTheme()
        {
            var sharedPrefs = PreferenceManager.GetDefaultSharedPreferences(this);
            var theme = sharedPrefs.GetString("pref_theme", "system");
            
            if(theme == _lastTheme)
                return; 
            
            switch(theme)
            {
                default:
                    IsDark = (Resources.Configuration.UiMode & UiMode.NightMask) == UiMode.NightYes;
                    AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightFollowSystem;
                    break;

                case "light":
                    IsDark = false;
                    AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightNo;
                    break;

                case "dark":
                    IsDark = true;
                    AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightYes;
                    break;
            }

            _lastTheme = theme;
        }

        protected override void OnResume()
        {
            base.OnResume();
            
            if(_checkedOnCreate)
            {
                _checkedOnCreate = false;
                return;
            }
            
            UpdateTheme();
        }
    }
}