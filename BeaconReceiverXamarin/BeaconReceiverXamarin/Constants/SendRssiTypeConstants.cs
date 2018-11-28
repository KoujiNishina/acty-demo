using System;
using System.Collections.Generic;
using System.Text;

namespace BeaconReceiverXamarin.Constants
{
    public class SendRssiTypeConstants
    {
        public class RssiType
        {
            public static RssiType MAX = new RssiType("0", "最高値");
            public static RssiType CENTER = new RssiType("1", "中央値");
            public static RssiType RAW = new RssiType("2", "生値");

            private String id;
            private String text;

            private RssiType(String id, String text)
            {
                this.id = id;
                this.text = text;
            }

            public String getId()
            {
                return this.id;
            }

            public String getText()
            {
                return this.text;
            }
        }
    }
}
