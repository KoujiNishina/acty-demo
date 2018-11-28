using Acr.UserDialogs;
using BeaconReceiverConnectorXamarin.Utils;
using BeaconReceiverXamarin.Interface;
using BeaconReceiverXamarin.Resource;
using BeaconReceiverXamarin.Store;
using BeaconReceiverXamarin.Utils;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Plugin.Settings;
using Plugin.Settings.Abstractions;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace BeaconReceiverXamarin.ViewModels
{
    public class SettingsPageViewModel : ViewModelBase
    {
        private IBackgroundService backgroundService = Xamarin.Forms.DependencyService.Get<IBackgroundService>();
        private IMessageWriter messageWriter = Xamarin.Forms.DependencyService.Get<IMessageWriter>();

        public class Setting
        {
            public string Name { get; set; }
            public string Value { get; set; }

            public bool IsEditable { get; set; } = false;

            public InputType InputType { get; set; } = InputType.Default;

            public FontAttributes FontAttributes { get { return IsEditable ? FontAttributes.Bold : FontAttributes.None; } }
        }

        private List<Setting> settings = new List<Setting>();
        public List<Setting> Settings
        {
            get { return settings; }
            set { SetProperty(ref settings, value); }
        }
        public Setting SelectedSetting
        {
            get { return null; }
            set
            {
                if (value != null && value.IsEditable)
                {
                    HandleSelectedSetting(value);
                }
                RaisePropertyChanged();
            }
        }
        public SettingsPageViewModel(INavigationService navigationService, IPageDialogService pageDialogService)
            : base(navigationService, pageDialogService)
        {
            Title = "設定画面";
        }
        private string _message;
        public string Message
        {
            get { return _message; }
            set { SetProperty(ref _message, value); }
        }
        private Color messageFontColor = Color.Gray;
        public Color MessageFontColor
        {
            get { return messageFontColor; }
            set { SetProperty(ref messageFontColor, value); }
        }

        private bool isSettingChanged = false;

        private async void HandleSelectedSetting(Setting setting)
        {
            ISettings settings = CrossSettings.Current;
            PromptResult r;
            if (setting.Name == "分析間隔 [秒]" || setting.Name == "送信間隔 [秒]")
                r = await UserDialogs.Instance.PromptAsync(setting.Name + "を1～86400の整数で入力してください", inputType: setting.InputType);
            else if (setting.Name == "RSSI値の種別")
                r = await UserDialogs.Instance.PromptAsync(setting.Name + "を0～2の整数で入力してください(0:最大値、1:中央値、2:生値)", inputType: setting.InputType);
            else if (setting.Name == "受信許可電波強度")
                r = await UserDialogs.Instance.PromptAsync(setting.Name + "を-120～-40の整数で入力してください", inputType: setting.InputType);
            else
                r = await UserDialogs.Instance.PromptAsync(setting.Name + "をカンマ区切り文字列で入力してください", inputType: setting.InputType);
            if (r.Ok)
            {
                if (setting.Name == "UUID対象ホワイトリスト")
                {
                    bool isUuidsValid = true;
                    if (!string.IsNullOrEmpty(r.Text))
                    { 
                        var uuids = r.Text.Split(","[0]);
                        foreach (var uuid in uuids)
                        {
                            if (!UuidUtils.IsValidUuid(uuid))
                            {
                                isUuidsValid = false;
                                break;
                            }
                        }
                    }
                    if (isUuidsValid)
                    {
                        SetupDataStore.editorPut(settings, AppResource.setting_uuid_white_list_key, r.Text);
                        onSettingChanged();
                    }
                    else
                        UserDialogs.Instance.Toast("値の書式が不正です");
                    return;
                }
                int val = int.MinValue;
                int.TryParse(r.Text, out val);
                if (setting.Name == "分析間隔 [秒]")
                {
                    if (1 <= val && val <= 86400)
                    {
                        SetupDataStore.editorPut(settings, AppResource.setting_analysis_interval_key, r.Text);
                        onSettingChanged();
                    }
                    else
                        UserDialogs.Instance.Toast("値の範囲が不正です");
                }
                else if (setting.Name == "送信間隔 [秒]")
                {
                    if (1 <= val && val <= 86400)
                    {
                        SetupDataStore.editorPut(settings, AppResource.setting_send_interval_key, r.Text);
                        onSettingChanged();
                    }
                    else
                        UserDialogs.Instance.Toast("値の範囲が不正です");
                }
                else if (setting.Name == "受信許可電波強度")
                {
                    if (-120 <= val && val <= -40)
                    {
                        SetupDataStore.editorPut(settings, AppResource.setting_allowed_min_rssi_key, r.Text);
                        onSettingChanged();
                    }
                    else
                        UserDialogs.Instance.Toast("値の範囲または書式が不正です");
                }
                else if (setting.Name == "RSSI値の種別")
                {
                    if (0 <= val && val <= 2)
                    {
                        SetupDataStore.editorPut(settings, AppResource.setting_rssi_type_key, r.Text);
                        onSettingChanged();
                    }
                    else
                        UserDialogs.Instance.Toast("値の範囲が不正です");
                }
            }
        }
        public override void OnNavigatedTo(NavigationParameters parameters)
        {
            isSettingChanged = false;
            updateSettngs();
        }
        public override void OnNavigatedFrom(NavigationParameters parameters)
        {
            base.OnNavigatedFrom(parameters);
            if (isSettingChanged)
                messageWriter.ShowToast("設定変更を確実にサービスに反映させるためには、一度アプリを強制終了させてから再起動し、「サービス手動開始」を押してください");
        }

        private void onSettingChanged()
        {
            isSettingChanged = true;
            updateSettngs();
        }

        private void updateSettngs()
        {
            var isServiceRunning = backgroundService.IsMainServiceRunning();
            Settings = new List<Setting>();
            SettingsData settings = SetupDataStore.getSettingsDataFromPreferences();
            Settings.Add(new Setting() { Name = "稼働系IoTHubの接続先ホスト名", Value = settings.active_host });
            Settings.Add(new Setting() { Name = "待機系IoTHubの接続先ホスト名", Value = settings.standby_host });
            Settings.Add(new Setting() { Name = "IoTHub接続名", Value = settings.iothub_auth_info.name });
            Settings.Add(new Setting() { Name = "IoTHub接続パスワード", Value = settings.iothub_auth_info.password });
            Settings.Add(new Setting() { Name = "分析間隔 [秒]", Value = ((int)settings.analysis_interval).ToString(), IsEditable = !isServiceRunning, InputType = InputType.Number });
            Settings.Add(new Setting() { Name = "送信間隔 [秒]", Value = ((int)settings.send_interval).ToString(), IsEditable = !isServiceRunning, InputType = InputType.Number });
            Settings.Add(new Setting() { Name = "フェイルオーバー条件 [送信回数]", Value = ((int)settings.failover).ToString() });
            Settings.Add(new Setting() { Name = "フェイルバック条件 [秒]", Value = ((int)settings.failback).ToString() });
            Settings.Add(new Setting() { Name = "受信許可電波強度", Value = ((int)settings.allowed_min_rssi).ToString(), IsEditable = !isServiceRunning, InputType = InputType.Default });
            Settings.Add(new Setting() { Name = "Web設定ファイルダウンロードURL", Value = settings.web_settings_url });
            Settings.Add(new Setting() { Name = "カスタマーフロントURL", Value = settings.customer_front_url });
            Settings.Add(new Setting() { Name = "カスタマーフロントID", Value = settings.customer_front_auth_info.id });
            Settings.Add(new Setting() { Name = "カスタマーフロントパスワード", Value = settings.customer_front_auth_info.password });
            Settings.Add(new Setting() { Name = "UUID対象ホワイトリスト", Value = SetupDataStore.getUuidWhiteListRaw(), IsEditable = !isServiceRunning, InputType = InputType.Default });
            Settings.Add(new Setting() { Name = "検索条件とする受信機識別子", Value = settings.receiver_nickname });
            var rssi_type = SetupDataStore.getString(AppResource.setting_rssi_type_key, null);
            Settings.Add(new Setting() { Name = "RSSI値の種別", Value = rssi_type == "0" ? "最高値" : rssi_type == "1" ? "中央値" : "生値", IsEditable = !isServiceRunning, InputType = InputType.Number });

        }
    }
}