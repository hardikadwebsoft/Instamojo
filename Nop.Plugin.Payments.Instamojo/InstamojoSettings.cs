using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.Instamojo
{
   public class InstamojoSettings : ISettings
    {
        /// <summary>
        /// Gets or sets client identifier
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets client secret
        /// </summary>
        public string SecretKey { get; set; }

    }
}
