using System;

namespace BeaconReceiverConnectorXamarin
{
    public class CustomerFrontAuthorization
    {
        public String id = null;
        public String password = null;

        /**
         * コンストラクタ
         *
         * @param id        BASIC認証ID
         * @param password  BASIC認証パスワード
         */
        public CustomerFrontAuthorization(String id, String password)
        {

            this.id = id;
            this.password = password;
        }
    }
}
