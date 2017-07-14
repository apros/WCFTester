using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using IA.IAFG.UN.Contracts.Entities;
using System.Threading;


namespace UNWcfTester
{
    public class ClientSessionBehavior : BehaviorExtensionElement, IEndpointBehavior, IClientMessageInspector
    {

        /// <summary>
        /// Get current Session Id
        /// </summary>

        private static AppUserAccess _applicationProperties = new AppUserAccess();
        public static AppUserAccess ApplicationProperties
        {
           private get { return _applicationProperties; }
            set { _applicationProperties = value; }
        }

        /// <summary>
        /// Get current Session Id
        /// </summary>
        private static string SessionId
        {
            get
            {
               return string.IsNullOrEmpty(ApplicationProperties.Guid) ? string.Empty : ApplicationProperties.Guid;
            }
        }

        /// <summary>
        /// Get current culture
        /// </summary>
        private static string SessionCulture
        {
            get
            {
                if (string.IsNullOrEmpty(ApplicationProperties.Lang))
                    return Thread.CurrentThread.CurrentCulture.Name;
                return ApplicationProperties.Lang == IA.IAFG.UN.Contracts.Entities.Base.OptionCode.Language.English.ToString() ? IA.IAFG.UN.Contracts.Entities.Base.OptionCode.CultureName.CanadianEnglish : IA.IAFG.UN.Contracts.Entities.Base.OptionCode.CultureName.CanadianFrench;
            }
        }

        /// <summary>
        ///  Gets the type of behavior.
        /// </summary>
        /// <returns>A <see cref="T:System.Type"/></returns>
        public override Type BehaviorType
        {
            get { return this.GetType(); }
        }

        /// <summary>
        /// Creates a behavior extension based on the current configuration settings.
        /// </summary>
        /// <returns>The behavior extension.</returns>
        protected override object CreateBehavior()
        {
            return this;
        }

        /// <summary>
        /// Implement to pass data at runtime to bindings to support custom behavior.
        /// </summary>
        /// <param name="endpoint">The endpoint to modify.</param>
        /// <param name="bindingParameters">The objects that binding elements require to support the behavior.</param>

        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        /// <summary>
        /// Implements a modification or extension of the client across an endpoint.
        /// </summary>
        /// <param name="endpoint">The endpoint that is to be customized.</param>
        /// <param name="clientRuntime">The client runtime to be customized.</param>
        /// <remarks></remarks>
        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            clientRuntime.MessageInspectors.Add(this);
        }

        /// <summary>
        /// Implements a modification or extension of the service across an endpoint.
        /// </summary>
        /// <param name="endpoint">The endpoint that exposes the contract.</param>
        /// <param name="endpointDispatcher">The endpoint dispatcher to be modified or extended.</param>
        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
        }

        /// <summary>
        /// Implement to confirm that the endpoint meets some intended criteria.
        /// </summary>
        /// <param name="endpoint">The endpoint to validate.</param>

        public void Validate(ServiceEndpoint endpoint)
        {
        }

        /// <summary>
        /// Enables inspection or modification of a message after a reply message is received but prior to passing it back to the client application.
        /// </summary>
        /// <param name="reply">The message to be transformed into types and handed back to the client application.</param>
        /// <param name="correlationState">Correlation state data.</param>
        /// <remarks></remarks>

        public void AfterReceiveReply(ref Message reply, object correlationState)
        {
        }

        // ReSharper disable VBWarnings::BC42309
        /// <summary>
        /// Enables inspection or modification of a message before a request message is sent to a service.
        /// </summary>
        /// <param name="request">The message to be sent to the service.</param>
        /// <param name="channel">The WCF client object channel.</param>
        /// <returns>The object that is returned as the <paramref name="correlationState "/>argument of the:
        /// see "System.ServiceModel.Dispatcher.IClientMessageInspector.AfterReceiveReply(System.ServiceModel.Channels.Message@,System.Object)" method. This is null if no correlation state is used.The best practice is to make this a <see cref="T:System.Guid"/> to ensure that no two <paramref name="correlationState"/> objects are the same.</returns>
        /// <remarks></remarks>
        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            var header = new MessageHeader<string>(SessionId);
            dynamic sessionIdHeader = header.GetUntypedHeader("SessionId", "IA.IAFG.UN");
            request.Headers.Add(sessionIdHeader);

            dynamic cultureHeader = new MessageHeader<string>(SessionCulture).GetUntypedHeader("SessionCulture", "IA.IAFG.UN");
            request.Headers.Add(cultureHeader);

            dynamic clientIdHeader = new MessageHeader<string>("UN").GetUntypedHeader("ClientId", "IA.IAFG.UN");
            request.Headers.Add(clientIdHeader);

            return null;
        }
        // ReSharper restore VBWarnings::BC42309
    }

}

