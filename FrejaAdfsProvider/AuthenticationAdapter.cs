namespace com.sorlov.frejaadfsprovider
{
    using System;
    using System.Net;
    using System.Security.Claims;
    using System.Xml.Serialization;
    using com.sorlov.eidprovider;
    using com.sorlov.eidprovider.frejaeid;
    using Microsoft.IdentityServer.Web.Authentication.External;

    /// <summary>
    /// Implementation of the <see cref="IAuthenticationAdapter"/>.
    /// </summary>
    public class AuthenticationAdapter : IAuthenticationAdapter
    {
        /// <summary>
        /// Gets the metadata describing the authentication adapter.
        /// </summary>
        public IAuthenticationAdapterMetadata Metadata => new AuthenticationAdapterMetadata();

        /// <summary>
        /// Begins the authentication process for the authentication adapter.
        /// </summary>
        /// <param name="identityClaim">The claim identifying the user.</param>
        /// <param name="request">The actual AD FS request.</param>
        /// <param name="context">The AD FS authentication context.</param>
        /// <returns>The <see cref="AdapterPresentation"/> the be shown in the AD FS dialog.</returns>
        public IAdapterPresentation BeginAuthentication(Claim identityClaim, HttpListenerRequest request, IAuthenticationContext context)
        {
            IAdapterPresentation result;
            var upn = identityClaim.Value;
            context.Data.Add("upn", upn);

            EIDResult eidResult = FrejaEID.Client.InitAuthRequest(upn);

            if (eidResult.Status == EIDResult.ResultStatus.initialized)
                context.Data.Add("authref", eidResult["id"]);
            else
                context.Data.Add("authref", string.Empty);

            result = new AdapterPresentation(upn, eidResult);

            return result;
        }

        /// <summary>
        /// Determines whether this authentication adapter is available for the user.
        /// </summary>
        /// <param name="identityClaim">The claim identifying the user.</param>
        /// <param name="context">The AD FS authentication context.</param>
        /// <returns>always return true as this is kind of just a pass to the remote service.</returns>
        public bool IsAvailableForUser(Claim identityClaim, IAuthenticationContext context)
        {
            return true;
        }

        /// <summary>
        /// Initializes the authentication adapter.
        /// </summary>
        /// <param name="configData">A stream for reading the configuration data.</param>
        /// <remarks>
        /// Called when AD FS starts and loads the authentication adapters. 
        /// The configuration is read from the AD FS configuration database.
        /// </remarks>
        public void OnAuthenticationPipelineLoad(IAuthenticationMethodConfigData configData)
        {
            FrejaEIDConfiguration configurationData;
            InitializationData defaultInitData;

            if (configData == null || configData.Data == null)
            {
                defaultInitData = new InitializationData(eidprovider.EIDEnvironment.Testing);
                configurationData = new FrejaEIDConfiguration();
                configurationData.Ca_cert = defaultInitData["ca_cert"];
                configurationData.Attribute_list = defaultInitData["attribute_list"];
                configurationData.Client_cert = defaultInitData["client_cert"];
                configurationData.Endpoint = defaultInitData["endpoint"];
                configurationData.Id_type = defaultInitData["id_type"];
                configurationData.Minimum_level = defaultInitData["minimum_level"];
                configurationData.Password = defaultInitData["password"];
                configurationData.Enviroment = defaultInitData["testing"];
            }

            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(FrejaEIDConfiguration));
                configurationData = (FrejaEIDConfiguration)serializer.Deserialize(configData.Data);

                EIDEnvironment enviroment = (EIDEnvironment)Enum.Parse(typeof(EIDEnvironment), configurationData.Enviroment, true);
                defaultInitData = new InitializationData(enviroment);
                defaultInitData["ca_cert"] = configurationData.Ca_cert;
                defaultInitData["attribute_list"] = configurationData.Attribute_list;
                defaultInitData["client_cert"] = configurationData.Client_cert;
                defaultInitData["endpoint"] = configurationData.Endpoint;
                defaultInitData["id_type"] = configurationData.Id_type;
                defaultInitData["minimum_level"] = configurationData.Minimum_level;
                defaultInitData["password"] = configurationData.Password;

                FrejaEID.Client = new eidprovider.frejaeid.Client(defaultInitData);

                AdapterPresentation.CompanyName = configurationData.CompanyName;
                AdapterPresentation.SupportEmail = configurationData.SupportEmail;
            }
            catch (Exception error)
            {
                throw new Exception("Invalid configuration data.", error);
            }
        }

        /// <summary>
        /// Allows the authentication adapter to dispose resources when the adapter is unloaded.
        /// </summary>
        /// <remarks>Called when AD FS stops.</remarks>
        public void OnAuthenticationPipelineUnload()
        {
        }

        /// <summary>
        /// This is called whenever something goes wrong in the authentication process. 
        /// </summary>
        /// <param name="request">The actual AD FS request.</param>
        /// <param name="ex">The exception raised</param>
        /// <returns>An <see cref="AdapterPresentation"/> web form to be shown in the AD FS dialog.</returns>
        /// <remarks>
        /// This is called whenever something goes wrong in the authentication process.
        /// To be more precise; if anything goes wrong in the BeginAuthentication or TryEndAuthentication
        /// methods of this authentication adapter, and either of these methods throw an ExternalAuthenticationException,
        /// the OnError method is called.
        /// </remarks>
        public IAdapterPresentation OnError(HttpListenerRequest request, ExternalAuthenticationException ex)
        {
            return new AdapterPresentation(ex);
        }

        /// <summary>
        /// Tries to complete the MFA request by validating the user input.
        /// </summary>
        /// <param name="context">The AD FS authentication context.</param>
        /// <param name="proofData">The proof provided by the client.</param>
        /// <param name="request">The actual AD FS request.</param>
        /// <param name="claims">If the validation was successful, this contains the authentication method claim.</param>
        /// <returns>'null' if successful, an <see cref="AdapterPresentation"/> to be shown in the AD FS dialog otherwise.</returns>
        public IAdapterPresentation TryEndAuthentication(IAuthenticationContext context, IProofData proofData, HttpListenerRequest request, out Claim[] claims)
        {
            object authref = null;
            object upn = null;
            context?.Data?.TryGetValue("upn", out upn);
            context?.Data?.TryGetValue("authref", out authref);

            if (authref == null || upn == null)
            {
                throw new ExternalAuthenticationException("Corrupted context.", context);
            }

            EIDResult eidResult = FrejaEID.Client.PollAuthRequest((string)authref);

            if (eidResult.Status== EIDResult.ResultStatus.completed)
            {
                var claim = new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/authenticationmethod", "http://schemas.microsoft.com/ws/2012/12/authmethod/otp");
                //TODO should send all the data along?
                //Should handle more claims as we get many datapoints from freja
                claims = new Claim[] { claim };
                return null;
            }

            claims = null;
            return new AdapterPresentation((string)upn, eidResult);
        }
    }
}
