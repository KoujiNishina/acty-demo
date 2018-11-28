using BeaconReceiverXamarin.Interface;
using BeaconReceiverXamarin.Resource;
using BeaconReceiverXamarin.Store;
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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace BeaconReceiverXamarin.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {

        public MainPageViewModel(INavigationService navigationService, IPageDialogService pageDialogService)
            : base(navigationService, pageDialogService)
        {
            Title = "トップ画面";
            SettingsCommand = new DelegateCommand(async () =>
            {
                await _navigationService.NavigateAsync("SettingsPage");
            }
            ,
            () =>
            {
                return true;
            });
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
        private string _UniqueID;
        public string UniqueID
        {
            get { return _UniqueID; }
            set { SetProperty(ref _UniqueID, value); }
        }
        private string _Nickname;
        public string Nickname
        {
            get { return _Nickname; }
            set { SetProperty(ref _Nickname, value); }
        }
        public override async void OnNavigatedTo(NavigationParameters parameters)
        {
            var checkPermResult = await CheckPermissionsAsync();
            var message = "必要な権限がすべて付与されています";
            if (!checkPermResult)
            {
                message = "必要な権限の内、付与されていないものがあります";
                MessageFontColor = Color.Red;
            }
            Message = message;
            UniqueID = SetupDataStore.getIothubAuthInfo().name;
            Nickname = SetupDataStore.getString(AppResource.setting_receiver_nickname_key, null);
            //SetupDataStore.updateWebSettingsPreferences(CrossSettings.Current, null);
            Debug.WriteLine("CheckPermissionsAsync result:" + checkPermResult);
        }
        //サービス開始ボタン押下
        public DelegateCommand StartServiceCommand { get; set; } = new DelegateCommand(() =>
        {
            Xamarin.Forms.DependencyService.Get<IBackgroundService>().StartMainSerivce();
        }
        ,
        () =>
        {
            return true;
        });
        //サービス停止ボタン押下
        public DelegateCommand StopServiceCommand { get; set; } = new DelegateCommand(() =>
        {
            Xamarin.Forms.DependencyService.Get<IBackgroundService>().StopMainService();
        }
        ,
        () =>
        {
            return true;
        });
        //設定委ボタン押下
        public DelegateCommand SettingsCommand { get; set; }

        /// <summary>
        /// ランタイムパーミッションチェック
        /// </summary>
        /// <returns>許可されている場合のみtrue</returns>
        private async Task<bool> CheckPermissionsAsync()
        {
            Debug.WriteLine("CheckGpsPermissionAsync start");
            PermissionStatus status = await GetPermission();
            return status == PermissionStatus.Granted;
        }

        #region Permission取得
        /// <summary>
        ///  位置情報のPermissionの状態を取得、確認します。
        /// </summary>
        public async Task<PermissionStatus> GetPermission()
        {
            var reqPermissions = new List<Permission>();
            var statusResult = PermissionStatus.Granted;
            var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Location);
            if (status != PermissionStatus.Granted)
            {
                reqPermissions.Add(Permission.Location);
            }
            status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Storage);
            if (status != PermissionStatus.Granted)
            {
                reqPermissions.Add(Permission.Storage);
            }
            if (reqPermissions.Count > 0)
            {
                // ユーザーに許可してもらうためにPermissionのリクエストを行う。
                var permissionResults = await CrossPermissions.Current.RequestPermissionsAsync(reqPermissions.ToArray());
                foreach (var perm in reqPermissions)
                {
                    if (permissionResults[perm] != PermissionStatus.Granted)
                    {
                        statusResult = permissionResults[perm];
                        break;
                    }
                }
            }
            return statusResult;
        }
        #endregion
    }
}