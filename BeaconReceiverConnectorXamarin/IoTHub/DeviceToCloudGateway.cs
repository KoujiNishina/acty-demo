using BeaconReceiverConnectorXamarin.Interface;
using BeaconReceiverConnectorXamarin.IoTHub.Data;
using BeaconReceiverConnectorXamarin.Utils;
using IoTHubJavaClientRewrittenInDotNet.Transport.Https;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace BeaconReceiverConnectorXamarin.IoTHub
{
    /**
     * device-to-cloud メッセージ処理.
     * <p>本クラスでは、Microsoft Azure IoT Hubに対しメッセージ処理を行うための、以下の機能を提供する。
     * </p>
     * <ul>
     *     <li> <a href="#SendToCloud">接触データの送信</a>
     * </ul>
     *
     * <a name="SendToCloud"></a>
     * <h3> 接触データの送信 </h3>
     * <p>
     *     実装するアプリで生成及び構築した接触データをIoT Hubへ送信する機能である。
     * </p>
     * <ul>
     *     <li>リスト(java,util.List)上に構築された接触データ{@link TouchParamBase}は本ライブラリで
     *     JSON文字列に変換した上、Iot Hubに送信する。
     *     <li>変換したJSON文字列のサイズが、IoT Hubで一度に送信出来る最大サイズ(256KB)を超えた場合、
     *     自動的に分割して送信する。
     *     <li>IoT Hub側で処理が完了すると、{@link SendCallback}がコールバックされる。
     *     <li>IoT Hub側で何らかの障害が発生し、接触データの送信に失敗した場合は、
     *     <a href="#Failover">送信データのフェイルオーバー処理</a>が自動的に行われる。
     * </ul>
     *
     * <a name="Failover"></a>
     * <h3>送信データのフェイルオーバー処理 </h3>
     * <p>接触データの送信時に、IoT Hub側で何らかの障害が発生し、IoT Hubからエラーレスポンスを受信した
     * 際に、接触データを送信する接続先（稼働系／待機系）を、自動的に切り替える機能である。</p>
     * <p>以下のシーケンスに従い、IoT Hubの接続を切り替える。</p>
     * <ol>
     *     <li> 【通常送信】通常送信時は、コンストラクタで指定した稼働系接続文字列で設定された接続先へ
     *     送信を行う。
     *     <li> 【送信リトライ】稼働系接続先への送信に失敗した場合、コンストラクタのフェイルオーバー条件
     *     で指定した回数に達するまで再送信を試行する。
     *     <li> 【フェイルオーバー】送信失敗回数がフェイルオーバー条件で指定した回数に達した場合、送信先
     *     をコンストラクタで指定した待機系接続文字列で設定された接続先へ切り替える。
     *     <li> 【フェイルバック】待機系接続先への送信中にフェイルバック条件の設定時間に達した場合、
     *     稼働系送信先への送信を試行する。送信が成功した場合は【通常送信】へ、失敗した場合は
     *     【フェイルオーバー】へ、それぞれ遷移する。
     * </ol>
     * <img src="./failover.png" alt="failover">
     *
     */
    public class DeviceToCloudGateway
    {

        private const String TAG = "DeviceToCloudGateway";

        // D2C最大メッセージサイズ( from Microsoft IoT Hub SDK.)
        private const int IOTHUB_D2C_MAX_MESSAGE_SIZE = HttpsBatchMessage.SERVICEBOUND_MESSAGE_MAX_SIZE_BYTES;

        private IotHubTransaction mIotHubTransaction;

        /**
         * Iot Hubに接触データを送信するために必要な初期情報を設定する.
         * <p></p>
         * <ul>
         *     <li> Iot Hubに接続するための接続文字列（稼働系・待機系）は、
         *     Microsoft Azure IoT Hub services のデバイス接続文字列(Connection String)を指定する。
         *     <li> フェイルオーバー条件では、通常モードで何回送信を失敗した場合にフェイルオーバーモード
         *     に送信モードを遷移させるかを、送信失敗回数の数値で設定する。
         *     <li> フェイルバック条件では、フェイルオーバーモードに遷移してから、通常モードに復帰する
         *     ための試行（フェイルバック）を行うまでの間隔を秒数で設定する。
         * </ul>
         *
         * @param connectionStringActiveHost    IoT Hub接続文字列（稼働系）
         * @param connectionStringStandbyHost   IoT Hub接続文字列（待機系）
         * @param failoverCondition     フェイルオーバー条件（送信失敗回数）
         * @param failbackCondition     フェイルバック条件（稼働系送信復帰秒数）
         * @throws URISyntaxException   接続文字列のフォーマット異常時にthrowされる
         * @throws IllegalArgumentException 接続文字列がnullの場合等、フォーマット異常以外に接続情報
         * として不適切な場合にthrowされる
         */
        public DeviceToCloudGateway(String connectionStringActiveHost, String connectionStringStandbyHost, int failoverCondition, int failbackCondition)
        {

            mIotHubTransaction = new IotHubTransaction(connectionStringActiveHost, connectionStringStandbyHost, failoverCondition, failbackCondition);
        }

        /**
         * 接触データをIot Hubに送信する.
         *
         * <p> パラメータとして、送信する接触データ型のリストと、送信結果を受け取るための、接触データ
         * 送信コールバックを指定する。</p>
         * <ul>
         *     <li> リスト(java,util.List)上に構築された接触データ{@link TouchParamBase}をJSON文字列に
         *     変換して送信する。
         *     <li> IoT Hub側で処理が完了すると、{@link SendCallback}がコールバックされ、Iot Hubに送信
         *     した接触データがIot Hub側で正常に処理されたかを知ることができる。
         * </ul>
         * <p>※{@link TouchParamBase}のクラスメンバーがnullの場合は、そのメンバーはJSON文字列に変換
         * されず、送信も行わない。
         * </p>
         * <p>本メソッドは非同期で処理されるため、実行は即座に終了する。戻り値として送信受付結果が返却
         * される。通常はtrueが返却されるが、IoT Hubへの送信処理中等でライブラリがBusy状態のため、
         * 送信処理が受付けられない場合はfalseが返却される。</p>
         *
         * @param touchList           送信する接触データのリスト
         * @param sendCallback        接触データ送信コールバック
         * @return 送信受付結果
         * @throws IOException IoT Hubとの接続が確立しない場合にthrowされる -> メッセージをキューに追加するだけなので発生しません
         */
        public bool send(List<TouchParamBase> touchList, SendCallback sendCallback, SendSingleMessageCallback sendSingleMessageCallback)
        {
            DebugMessageUtils.GetInstance().WriteLog(TAG, "send start touchList.Count:" + touchList.Count, LogLevel.I);
            if (touchList == null) throw new ArgumentException("touchList cannot be null.");

            if (mIotHubTransaction.isTransaction()) return false;

            mIotHubTransaction.beginTransaction();

            mIotHubTransaction.setSendCallback(sendCallback);
            mIotHubTransaction.setSendSingleMessageCallback(sendSingleMessageCallback);
            mIotHubTransaction.setTouchList(touchList);

            // D2C最大メッセージサイズ単位で、接触データをJSONに変換・結合
            int messageSize = 0;
            StringBuilder message = new StringBuilder();
            int retryIndex = 0;
            for (int i = 0; i < touchList.Count; i++)
            {

                String json;
                try {

                    json = JsonConvert.SerializeObject(touchList[i], new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    });
                } catch (JsonReaderException e) {

                    throw new ArgumentException(e.Message);
                }
                int jsonSize = getByteLength(json);
                DebugMessageUtils.GetInstance().WriteLog(TAG, "@@@@@ json:" + json, LogLevel.I);
                // JSON配列の始め括弧または終わり括弧または連結子（カンマ）のサイズ（1バイト）をインクリメント
                messageSize++;

                if (messageSize + jsonSize <= IOTHUB_D2C_MAX_MESSAGE_SIZE)
                    {

                    if (messageSize == 1) {
                        // JSON配列の始め括弧を追加
                        message.Append('[');
                    } else if (messageSize > 1) {
                        // JSON配列の連結子（カンマ）を追加
                        message.Append(',');
                    }
                    message.Append(json);
                    messageSize += jsonSize;
                    DebugMessageUtils.GetInstance().WriteLog(TAG, "@@@@@ message messageSize:" + messageSize + " / sbByteLength:" + getByteLength(message.ToString()), LogLevel.D);
                } else {
                    // JSON配列の終わり括弧を追加
                    message.Append(']');
                    mIotHubTransaction.add(message.ToString(), retryIndex);
                    DebugMessageUtils.GetInstance().WriteLog(TAG, "@@@@@ IotHubTransaction added  messageSize:" + messageSize, LogLevel.I);
                    DebugMessageUtils.GetInstance().WriteLog(TAG, "@@@@@ message divided", LogLevel.I);
                    retryIndex = i + 1;
                    messageSize = 0;
                    message.Clear();
                    i--;
                }
            }
            if (message.Length > 0) {
                // JSON配列の終わり括弧を追加
                message.Append(']');
                mIotHubTransaction.add(message.ToString(), retryIndex);
                DebugMessageUtils.GetInstance().WriteLog(TAG, "@@@@@ IotHubTransaction added  messageSize:" + messageSize, LogLevel.I);
            }
            DebugMessageUtils.GetInstance().WriteLog(TAG, "@@@@@ IotHubTransaction confirmed.", LogLevel.I);

            //try {

            mIotHubTransaction.commit();    //メッセージをキューに入れるだけなので、I/O例外は発生しえない
            //} catch (IOException e) {

            //    mIotHubTransaction.fail();
            //    DebugMessageUtils.GetInstance().WriteLog(TAG, e.Message, LogLevel.E);
            //    throw new IOException("IotHub client open failed.", e);
            //}
            DebugMessageUtils.GetInstance().WriteLog(TAG, "send end", LogLevel.D);
            return true;
        }

        private int getByteLength(String str)
        {

            int size = 0;
            size = Encoding.UTF8.GetBytes(str).Length;

            return size;
        }
    }
}
