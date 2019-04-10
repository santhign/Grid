using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models
{    /// <summary>
    /// 
    /// </summary>
    public class AuthTokenResponse
    {
        public AuthTokenResponse()
        {
            IsExpired = false;
        }

        /// <summary>
        /// Gets or sets the customer identifier.
        /// </summary>
        /// <value>
        /// The customer identifier.
        /// </value>
        public int CustomerID { get; set; }
        /// <summary>
        /// Gets or sets the created on.
        /// </summary>
        /// <value>
        /// The created on.
        /// </value>
        public DateTime CreatedOn { get; set; }

        public bool IsExpired { get; set; }
    }
    
}
