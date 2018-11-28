using BeaconReceiverConnectorXamarin.IoTHub.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static BeaconReceiverConnectorXamarin.IoTHub.IotHubTransaction;

namespace BeaconReceiverConnectorXamarin.IoTHub
{
    /**
     * 接触データ送信コールバックインターフェース.
     */
    public interface SendCallback
    {

        /**
         * 接触データ送信結果の受信コールバック
         *
         * @param statusCode       送信ステータスコード
         * @param retryData        IoT Hubに送信出来なかった接触データが格納されたリスト
         *                         （送信ステータスコードがエラーの場合のみ。正常終了した場合はnullが
         *                         返却される。）
         */
        void onSendResult(SendStatusCodeEnum statusCode, List<TouchParamBase> retryData);
    }
    /**
     * 接触データ送信コールバックインターフェース.
     */
    public interface SendSingleMessageCallback
    {

        /**
         * 接触データ送信結果の受信コールバック(メッセージ1件ごとに呼ばれる)
         *
         * @param statusCode       送信ステータスコード
         */
        void onSendSingleMessageResult(SendMode currentSendMode, SendStatusCodeEnum statusCode);
    }
}
