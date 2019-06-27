using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace GRIDService
{
    /// <summary>
    /// 
    /// </summary>
    public static class Enum
    {

        /// <summary>
        /// The return success
        /// </summary>
        [Description("Success")]
        public const int ReturnSuccess = 1;

        /// <summary>
        /// The return failure
        /// </summary>
        [Description("Failure")]
        public const int ReturnFailure = -1;

        /// <summary>
        /// The return failure
        /// </summary>
        [Description("Failure")]
        public const string ConfigKeyForGridService = "GRIDServiceToken";
    }
}
