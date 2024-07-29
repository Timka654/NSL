﻿namespace NSL.Services.ServersHub.Shared.Client.DotNetIdentity
{
    /// <summary>
    /// Encapsulates an error from the identity subsystem.
    /// </summary>
    public class IdentityError
    {
        /// <summary>
        /// Gets or sets the code for this error.
        /// </summary>
        /// <value>
        /// The code for this error.
        /// </value>
        public string Code { get; set; } = default!;

        /// <summary>
        /// Gets or sets the description for this error.
        /// </summary>
        /// <value>
        /// The description for this error.
        /// </value>
        public string Description { get; set; } = default!;
    }
}
