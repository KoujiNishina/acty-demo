﻿using System;
using System.Collections.Generic;
using System.Text;

namespace BeaconReceiverXamarin.Data
{
    public interface IFileHelper
    {
        string GetLocalFilePath(string filename);
    }
}
