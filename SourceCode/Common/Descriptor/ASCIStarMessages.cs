﻿using PX.Common;

namespace ASCISTARCustom.Common.Descriptor
{
    public class ASCIStarMessages
    {
        #region Plugin
        [PXLocalizable]
        public class Plugin
        {
            public const string PluginStart = "INFO:......ASC Jewelshop Module Plugin start working...";
            public const string PluginEnd = "INFO:......ASC Jewelshop Module Plugin work completed.";
            public const string PluginCreateConnectionPref = "INFO:......Creating Connection preferences...";
            public const string PluginCreateConnectionPrefSuccess = "SUCCESS:...Connection Preferences created successfully!";
            public const string PluginCreateConnectionPrefError = "ERROR:.....{0}";
        }
        #endregion

        #region StatusCode
        [PXLocalizable]
        public class StatusCode
        {
            public const string StatusCodeError = "Error: Received a {0} status code. Content: {1}";
            public const string RemoteServerError = "The remote server returned an error. For more details open trace.";
        }
        #endregion

        #region Connection
        [PXLocalizable]
        public class Connection
        {
            public const string TestConnectionSuccess = "The connection to the Metals-API was successful.";
            public const string TestConnectionFailed = "Test connection failed. For more details, please refer to the trace log.";
        }
        #endregion
    }
}
