using System;
using System.Collections.Generic;
using System.Text;

namespace BeaconReceiverXamarin.Constants
{
    public class CommonConstants
    {
        // 共通
        /** モード引数名 */
        public const String ARG_MODE_TYPE = "modeType";

        // 設定
        /** Web設定ファイル名（ローカル） */
        public const String LOCAL_SETTINGS_FILE_NAME = "config.json";

        /** Web設定ファイル取得URL ※カスタマーフロントURL末尾に付与 */
        public const String WEB_SETTINGS_URL = "/config";

        /** 受信機情報取得URL ※カスタマーフロントURL末尾に付与 */
        public const String RECEIVERS_URL = "/receivers";

        /** Preferencesファイル項目_初回判定 */
        public const String FIRST_FLG = "first_flg";

        // IoTHub送信JSON
        /** code(1000固定) */
        public const String JSON_CODE_VALUE = "1000";
    }
}
