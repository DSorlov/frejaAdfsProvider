namespace com.sorlov.frejaadfsprovider
{
    using System;
    using System.Globalization;
    using com.sorlov.eidprovider;
    using Microsoft.IdentityServer.Web.Authentication.External;

    /// <summary>
    /// Implementation of the <see cref="IAdapterPresentation"/> and <see cref="IAdapterPresentationForm"/>.  
    /// </summary>
    public class AdapterPresentation : IAdapterPresentation, IAdapterPresentationForm
    {
        /// <summary>
        /// The users UPN.
        /// </summary>
        private string upn = null;

        /// <summary>
        /// The authref
        /// </summary>
        private EIDResult initResult = null;

        /// <summary>
        /// Indicates whether the authentication adapter ran into an exception.
        /// </summary>
        private ExternalAuthenticationException error = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdapterPresentation"/> class, when trying to verify the proof data. 
        /// </summary>
        /// <param name="attempts">The number of attempts the user has gone through entering the proper code.</param>
        /// <param name="locked">Whether or not the user has been locked out of this Authentication Provider.</param>
        /// <remarks>This constructor is called when the users has entered an invalid code.</remarks>
        public AdapterPresentation(string upn, EIDResult initResult)
        {
            this.upn = upn;
            this.initResult = initResult;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdapterPresentation"/> class, whenever an error has occurred.
        /// </summary>
        /// <param name="ex">The exception that needs to be handled.</param>
        /// <remarks>Called by the OnError method of the <see cref="AuthenticationAdapter"/>.</remarks>
        public AdapterPresentation(ExternalAuthenticationException ex)
        {
            this.error = ex;
        }

        /// <summary>
        /// Gets or sets the name of your organization as used in the QR code.
        /// </summary>
        public static string CompanyName { get; set; }

        /// <summary>
        /// Gets or sets the support email address of your organization.
        /// </summary>
        public static string SupportEmail { get; set; }

        /// <summary>
        /// Gets the title of the page to be displayed in the browser for this Authentication Provider, given a language culture identifier.
        /// </summary>
        /// <param name="lcid">The language culture identifier of the language.</param>
        /// <returns>The title of the page to be displayed in the users browser.</returns>
        /// <remarks>
        /// Check <see href="https://msdn.microsoft.com/en-us/library/ee825488(v=cs.20).aspx">MSDN</see> for all language culture identifiers.
        /// </remarks>
        public string GetPageTitle(int lcid)
        {
            resources.strings.Culture = new CultureInfo(lcid);
            return resources.strings.PageTitle;
        }

        /// <summary>
        /// Gets the HTML to be displayed within the AD FS MFA page for a user, given a language culture identifier.
        /// </summary>
        /// <param name="lcid">The language culture identifier of the language.</param>
        /// <returns>The HTML to be displayed in the AD FS MFA page.</returns>
        /// <remarks>
        /// Check <see href="https://msdn.microsoft.com/en-us/library/ee825488(v=cs.20).aspx">MSDN</see> for all language culture identifiers.
        /// </remarks>
        public string GetFormHtml(int lcid)
        {
            CultureInfo cultureInfo = new CultureInfo(lcid);
            resources.strings.Culture = cultureInfo;

            string htmlData = string.Empty;
            if (this.error != null)
            {
                htmlData = FrejaEID.GetResourceFile("com.sorlov.frejaadfsprovider.resources.html.ErrorForm.html");
                htmlData = htmlData.Replace("$ERROR$", "Exception: " + error.Message);
                return htmlData;
            }
            if (initResult.Status == EIDResult.ResultStatus.cancelled || initResult.Status == EIDResult.ResultStatus.error)
            {
                htmlData = FrejaEID.GetResourceFile("com.sorlov.frejaadfsprovider.resources.html.ErrorForm.html");
                htmlData = htmlData.Replace("$ERROR$", "API-Error: " + (string)initResult["code"]);
                return htmlData;
            }

            htmlData = FrejaEID.GetResourceFile("com.sorlov.frejaadfsprovider.resources.html.AuthForm.html");
            htmlData = htmlData.Replace("$CODE$", (string)initResult["code"]);
            htmlData = htmlData.Replace("$STATUS$", initResult.Status.ToString());
            htmlData = htmlData.Replace("$QRDATA$", (string)initResult["extra"]["autostart_url"]);
            return htmlData;
        }

        /// <summary>
        /// Gets the HTML code that needs to be inserted into the AD FS MFA page before the body of the page, given a language culture identifier.
        /// </summary>
        /// <param name="lcid">The language code identifier of the language.</param>
        /// <returns>The HTML code that needs to be inserted before the BODY part of the AD FS MFA page.</returns>
        /// <remarks>
        /// Check <see href="https://msdn.microsoft.com/en-us/library/ee825488(v=cs.20).aspx">MSDN</see> for all language culture identifiers.
        /// </remarks>
        public string GetFormPreRenderHtml(int lcid)
        {
            return string.Empty;
        }
    }
}