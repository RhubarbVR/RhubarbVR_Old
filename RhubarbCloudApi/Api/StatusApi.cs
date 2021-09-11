/*
 * RhubarbVRApi
 *
 * No description provided (generated by Openapi Generator https://github.com/openapitools/openapi-generator)
 *
 * The version of the OpenAPI document: v1
 * Generated by: https://github.com/openapitools/openapi-generator.git
 */


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Mime;

using Org.OpenAPITools.Client;
using Org.OpenAPITools.Model;

namespace Org.OpenAPITools.Api
{

	/// <summary>
	/// Represents a collection of functions to interact with the API endpoints
	/// </summary>
	public interface IStatusApiSync : IApiAccessor
	{
		#region Synchronous Operations
		/// <summary>
		/// 
		/// </summary>
		/// <exception cref="Org.OpenAPITools.Client.ApiException">Thrown when fails to make API call</exception>
		/// <param name="authorization">For Authenticating the user (optional)</param>
		/// <returns>string</returns>
		string StatusClearstatusGet(string authorization = default(string));

		/// <summary>
		/// 
		/// </summary>
		/// <remarks>
		/// 
		/// </remarks>
		/// <exception cref="Org.OpenAPITools.Client.ApiException">Thrown when fails to make API call</exception>
		/// <param name="authorization">For Authenticating the user (optional)</param>
		/// <returns>ApiResponse of string</returns>
		ApiResponse<string> StatusClearstatusGetWithHttpInfo(string authorization = default(string));
		/// <summary>
		/// 
		/// </summary>
		/// <exception cref="Org.OpenAPITools.Client.ApiException">Thrown when fails to make API call</exception>
		/// <param name="uuid"> (optional)</param>
		/// <param name="authorization">For Authenticating the user (optional)</param>
		/// <returns>PublixStatus</returns>
		PublixStatus StatusFetchGet(string uuid = default(string), string authorization = default(string));

		/// <summary>
		/// 
		/// </summary>
		/// <remarks>
		/// 
		/// </remarks>
		/// <exception cref="Org.OpenAPITools.Client.ApiException">Thrown when fails to make API call</exception>
		/// <param name="uuid"> (optional)</param>
		/// <param name="authorization">For Authenticating the user (optional)</param>
		/// <returns>ApiResponse of PublixStatus</returns>
		ApiResponse<PublixStatus> StatusFetchGetWithHttpInfo(string uuid = default(string), string authorization = default(string));
		/// <summary>
		/// 
		/// </summary>
		/// <exception cref="Org.OpenAPITools.Client.ApiException">Thrown when fails to make API call</exception>
		/// <param name="authorization">For Authenticating the user (optional)</param>
		/// <returns>PrivateStatus</returns>
		PrivateStatus StatusMeGet(string authorization = default(string));

		/// <summary>
		/// 
		/// </summary>
		/// <remarks>
		/// 
		/// </remarks>
		/// <exception cref="Org.OpenAPITools.Client.ApiException">Thrown when fails to make API call</exception>
		/// <param name="authorization">For Authenticating the user (optional)</param>
		/// <returns>ApiResponse of PrivateStatus</returns>
		ApiResponse<PrivateStatus> StatusMeGetWithHttpInfo(string authorization = default(string));
		/// <summary>
		/// 
		/// </summary>
		/// <exception cref="Org.OpenAPITools.Client.ApiException">Thrown when fails to make API call</exception>
		/// <param name="statusUpdate"> (optional)</param>
		/// <param name="authorization">For Authenticating the user (optional)</param>
		/// <returns>string</returns>
		string StatusStatusupdatePost(StatusUpdate statusUpdate = default(StatusUpdate), string authorization = default(string));

		/// <summary>
		/// 
		/// </summary>
		/// <remarks>
		/// 
		/// </remarks>
		/// <exception cref="Org.OpenAPITools.Client.ApiException">Thrown when fails to make API call</exception>
		/// <param name="statusUpdate"> (optional)</param>
		/// <param name="authorization">For Authenticating the user (optional)</param>
		/// <returns>ApiResponse of string</returns>
		ApiResponse<string> StatusStatusupdatePostWithHttpInfo(StatusUpdate statusUpdate = default(StatusUpdate), string authorization = default(string));
		#endregion Synchronous Operations
	}

	/// <summary>
	/// Represents a collection of functions to interact with the API endpoints
	/// </summary>
	public interface IStatusApiAsync : IApiAccessor
	{
		#region Asynchronous Operations
		/// <summary>
		/// 
		/// </summary>
		/// <remarks>
		/// 
		/// </remarks>
		/// <exception cref="Org.OpenAPITools.Client.ApiException">Thrown when fails to make API call</exception>
		/// <param name="authorization">For Authenticating the user (optional)</param>
		/// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
		/// <returns>Task of string</returns>
		System.Threading.Tasks.Task<string> StatusClearstatusGetAsync(string authorization = default(string), System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

		/// <summary>
		/// 
		/// </summary>
		/// <remarks>
		/// 
		/// </remarks>
		/// <exception cref="Org.OpenAPITools.Client.ApiException">Thrown when fails to make API call</exception>
		/// <param name="authorization">For Authenticating the user (optional)</param>
		/// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
		/// <returns>Task of ApiResponse (string)</returns>
		System.Threading.Tasks.Task<ApiResponse<string>> StatusClearstatusGetWithHttpInfoAsync(string authorization = default(string), System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));
		/// <summary>
		/// 
		/// </summary>
		/// <remarks>
		/// 
		/// </remarks>
		/// <exception cref="Org.OpenAPITools.Client.ApiException">Thrown when fails to make API call</exception>
		/// <param name="uuid"> (optional)</param>
		/// <param name="authorization">For Authenticating the user (optional)</param>
		/// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
		/// <returns>Task of PublixStatus</returns>
		System.Threading.Tasks.Task<PublixStatus> StatusFetchGetAsync(string uuid = default(string), string authorization = default(string), System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

		/// <summary>
		/// 
		/// </summary>
		/// <remarks>
		/// 
		/// </remarks>
		/// <exception cref="Org.OpenAPITools.Client.ApiException">Thrown when fails to make API call</exception>
		/// <param name="uuid"> (optional)</param>
		/// <param name="authorization">For Authenticating the user (optional)</param>
		/// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
		/// <returns>Task of ApiResponse (PublixStatus)</returns>
		System.Threading.Tasks.Task<ApiResponse<PublixStatus>> StatusFetchGetWithHttpInfoAsync(string uuid = default(string), string authorization = default(string), System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));
		/// <summary>
		/// 
		/// </summary>
		/// <remarks>
		/// 
		/// </remarks>
		/// <exception cref="Org.OpenAPITools.Client.ApiException">Thrown when fails to make API call</exception>
		/// <param name="authorization">For Authenticating the user (optional)</param>
		/// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
		/// <returns>Task of PrivateStatus</returns>
		System.Threading.Tasks.Task<PrivateStatus> StatusMeGetAsync(string authorization = default(string), System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

		/// <summary>
		/// 
		/// </summary>
		/// <remarks>
		/// 
		/// </remarks>
		/// <exception cref="Org.OpenAPITools.Client.ApiException">Thrown when fails to make API call</exception>
		/// <param name="authorization">For Authenticating the user (optional)</param>
		/// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
		/// <returns>Task of ApiResponse (PrivateStatus)</returns>
		System.Threading.Tasks.Task<ApiResponse<PrivateStatus>> StatusMeGetWithHttpInfoAsync(string authorization = default(string), System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));
		/// <summary>
		/// 
		/// </summary>
		/// <remarks>
		/// 
		/// </remarks>
		/// <exception cref="Org.OpenAPITools.Client.ApiException">Thrown when fails to make API call</exception>
		/// <param name="statusUpdate"> (optional)</param>
		/// <param name="authorization">For Authenticating the user (optional)</param>
		/// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
		/// <returns>Task of string</returns>
		System.Threading.Tasks.Task<string> StatusStatusupdatePostAsync(StatusUpdate statusUpdate = default(StatusUpdate), string authorization = default(string), System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

		/// <summary>
		/// 
		/// </summary>
		/// <remarks>
		/// 
		/// </remarks>
		/// <exception cref="Org.OpenAPITools.Client.ApiException">Thrown when fails to make API call</exception>
		/// <param name="statusUpdate"> (optional)</param>
		/// <param name="authorization">For Authenticating the user (optional)</param>
		/// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
		/// <returns>Task of ApiResponse (string)</returns>
		System.Threading.Tasks.Task<ApiResponse<string>> StatusStatusupdatePostWithHttpInfoAsync(StatusUpdate statusUpdate = default(StatusUpdate), string authorization = default(string), System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));
		#endregion Asynchronous Operations
	}

	/// <summary>
	/// Represents a collection of functions to interact with the API endpoints
	/// </summary>
	public interface IStatusApi : IStatusApiSync, IStatusApiAsync
	{

	}

	/// <summary>
	/// Represents a collection of functions to interact with the API endpoints
	/// </summary>
	public partial class StatusApi : IStatusApi
	{
		private Org.OpenAPITools.Client.ExceptionFactory _exceptionFactory = (name, response) => null;

		/// <summary>
		/// Initializes a new instance of the <see cref="StatusApi"/> class.
		/// </summary>
		/// <returns></returns>
		public StatusApi() : this((string)null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="StatusApi"/> class.
		/// </summary>
		/// <returns></returns>
		public StatusApi(String basePath)
		{
			this.Configuration = Org.OpenAPITools.Client.Configuration.MergeConfigurations(
				Org.OpenAPITools.Client.GlobalConfiguration.Instance,
				new Org.OpenAPITools.Client.Configuration { BasePath = basePath }
			);
			this.Client = new Org.OpenAPITools.Client.ApiClient(this.Configuration.BasePath);
			this.AsynchronousClient = new Org.OpenAPITools.Client.ApiClient(this.Configuration.BasePath);
			this.ExceptionFactory = Org.OpenAPITools.Client.Configuration.DefaultExceptionFactory;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="StatusApi"/> class
		/// using Configuration object
		/// </summary>
		/// <param name="configuration">An instance of Configuration</param>
		/// <returns></returns>
		public StatusApi(Org.OpenAPITools.Client.Configuration configuration)
		{
			if (configuration == null)
				throw new ArgumentNullException("configuration");

			this.Configuration = Org.OpenAPITools.Client.Configuration.MergeConfigurations(
				Org.OpenAPITools.Client.GlobalConfiguration.Instance,
				configuration
			);
			this.Client = new Org.OpenAPITools.Client.ApiClient(this.Configuration.BasePath);
			this.AsynchronousClient = new Org.OpenAPITools.Client.ApiClient(this.Configuration.BasePath);
			ExceptionFactory = Org.OpenAPITools.Client.Configuration.DefaultExceptionFactory;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="StatusApi"/> class
		/// using a Configuration object and client instance.
		/// </summary>
		/// <param name="client">The client interface for synchronous API access.</param>
		/// <param name="asyncClient">The client interface for asynchronous API access.</param>
		/// <param name="configuration">The configuration object.</param>
		public StatusApi(Org.OpenAPITools.Client.ISynchronousClient client, Org.OpenAPITools.Client.IAsynchronousClient asyncClient, Org.OpenAPITools.Client.IReadableConfiguration configuration)
		{
			if (client == null)
				throw new ArgumentNullException("client");
			if (asyncClient == null)
				throw new ArgumentNullException("asyncClient");
			if (configuration == null)
				throw new ArgumentNullException("configuration");

			this.Client = client;
			this.AsynchronousClient = asyncClient;
			this.Configuration = configuration;
			this.ExceptionFactory = Org.OpenAPITools.Client.Configuration.DefaultExceptionFactory;
		}

		/// <summary>
		/// The client for accessing this underlying API asynchronously.
		/// </summary>
		public Org.OpenAPITools.Client.IAsynchronousClient AsynchronousClient { get; set; }

		/// <summary>
		/// The client for accessing this underlying API synchronously.
		/// </summary>
		public Org.OpenAPITools.Client.ISynchronousClient Client { get; set; }

		/// <summary>
		/// Gets the base path of the API client.
		/// </summary>
		/// <value>The base path</value>
		public String GetBasePath()
		{
			return this.Configuration.BasePath;
		}

		/// <summary>
		/// Gets or sets the configuration object
		/// </summary>
		/// <value>An instance of the Configuration</value>
		public Org.OpenAPITools.Client.IReadableConfiguration Configuration { get; set; }

		/// <summary>
		/// Provides a factory method hook for the creation of exceptions.
		/// </summary>
		public Org.OpenAPITools.Client.ExceptionFactory ExceptionFactory
		{
			get
			{
				if (_exceptionFactory != null && _exceptionFactory.GetInvocationList().Length > 1)
				{
					throw new InvalidOperationException("Multicast delegate for ExceptionFactory is unsupported.");
				}
				return _exceptionFactory;
			}
			set { _exceptionFactory = value; }
		}

		/// <summary>
		///  
		/// </summary>
		/// <exception cref="Org.OpenAPITools.Client.ApiException">Thrown when fails to make API call</exception>
		/// <param name="authorization">For Authenticating the user (optional)</param>
		/// <returns>string</returns>
		public string StatusClearstatusGet(string authorization = default(string))
		{
			Org.OpenAPITools.Client.ApiResponse<string> localVarResponse = StatusClearstatusGetWithHttpInfo(authorization);
			return localVarResponse.Data;
		}

		/// <summary>
		///  
		/// </summary>
		/// <exception cref="Org.OpenAPITools.Client.ApiException">Thrown when fails to make API call</exception>
		/// <param name="authorization">For Authenticating the user (optional)</param>
		/// <returns>ApiResponse of string</returns>
		public Org.OpenAPITools.Client.ApiResponse<string> StatusClearstatusGetWithHttpInfo(string authorization = default(string))
		{
			Org.OpenAPITools.Client.RequestOptions localVarRequestOptions = new Org.OpenAPITools.Client.RequestOptions();

			String[] _contentTypes = new String[] {
			};

			// to determine the Accept header
			String[] _accepts = new String[] {
				"text/plain",
				"application/json",
				"text/json"
			};

			var localVarContentType = Org.OpenAPITools.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
			if (localVarContentType != null)
				localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

			var localVarAccept = Org.OpenAPITools.Client.ClientUtils.SelectHeaderAccept(_accepts);
			if (localVarAccept != null)
				localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

			if (authorization != null)
			{
				localVarRequestOptions.HeaderParameters.Add("Authorization", Org.OpenAPITools.Client.ClientUtils.ParameterToString(authorization)); // header parameter
			}


			// make the HTTP request
			var localVarResponse = this.Client.Get<string>("/Status/clearstatus", localVarRequestOptions, this.Configuration);

			if (this.ExceptionFactory != null)
			{
				Exception _exception = this.ExceptionFactory("StatusClearstatusGet", localVarResponse);
				if (_exception != null)
					throw _exception;
			}

			return localVarResponse;
		}

		/// <summary>
		///  
		/// </summary>
		/// <exception cref="Org.OpenAPITools.Client.ApiException">Thrown when fails to make API call</exception>
		/// <param name="authorization">For Authenticating the user (optional)</param>
		/// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
		/// <returns>Task of string</returns>
		public async System.Threading.Tasks.Task<string> StatusClearstatusGetAsync(string authorization = default(string), System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
		{
			Org.OpenAPITools.Client.ApiResponse<string> localVarResponse = await StatusClearstatusGetWithHttpInfoAsync(authorization, cancellationToken).ConfigureAwait(false);
			return localVarResponse.Data;
		}

		/// <summary>
		///  
		/// </summary>
		/// <exception cref="Org.OpenAPITools.Client.ApiException">Thrown when fails to make API call</exception>
		/// <param name="authorization">For Authenticating the user (optional)</param>
		/// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
		/// <returns>Task of ApiResponse (string)</returns>
		public async System.Threading.Tasks.Task<Org.OpenAPITools.Client.ApiResponse<string>> StatusClearstatusGetWithHttpInfoAsync(string authorization = default(string), System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
		{

			Org.OpenAPITools.Client.RequestOptions localVarRequestOptions = new Org.OpenAPITools.Client.RequestOptions();

			String[] _contentTypes = new String[] {
			};

			// to determine the Accept header
			String[] _accepts = new String[] {
				"text/plain",
				"application/json",
				"text/json"
			};


			var localVarContentType = Org.OpenAPITools.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
			if (localVarContentType != null)
				localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

			var localVarAccept = Org.OpenAPITools.Client.ClientUtils.SelectHeaderAccept(_accepts);
			if (localVarAccept != null)
				localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

			if (authorization != null)
			{
				localVarRequestOptions.HeaderParameters.Add("Authorization", Org.OpenAPITools.Client.ClientUtils.ParameterToString(authorization)); // header parameter
			}


			// make the HTTP request

			var localVarResponse = await this.AsynchronousClient.GetAsync<string>("/Status/clearstatus", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

			if (this.ExceptionFactory != null)
			{
				Exception _exception = this.ExceptionFactory("StatusClearstatusGet", localVarResponse);
				if (_exception != null)
					throw _exception;
			}

			return localVarResponse;
		}

		/// <summary>
		///  
		/// </summary>
		/// <exception cref="Org.OpenAPITools.Client.ApiException">Thrown when fails to make API call</exception>
		/// <param name="uuid"> (optional)</param>
		/// <param name="authorization">For Authenticating the user (optional)</param>
		/// <returns>PublixStatus</returns>
		public PublixStatus StatusFetchGet(string uuid = default(string), string authorization = default(string))
		{
			Org.OpenAPITools.Client.ApiResponse<PublixStatus> localVarResponse = StatusFetchGetWithHttpInfo(uuid, authorization);
			return localVarResponse.Data;
		}

		/// <summary>
		///  
		/// </summary>
		/// <exception cref="Org.OpenAPITools.Client.ApiException">Thrown when fails to make API call</exception>
		/// <param name="uuid"> (optional)</param>
		/// <param name="authorization">For Authenticating the user (optional)</param>
		/// <returns>ApiResponse of PublixStatus</returns>
		public Org.OpenAPITools.Client.ApiResponse<PublixStatus> StatusFetchGetWithHttpInfo(string uuid = default(string), string authorization = default(string))
		{
			Org.OpenAPITools.Client.RequestOptions localVarRequestOptions = new Org.OpenAPITools.Client.RequestOptions();

			String[] _contentTypes = new String[] {
			};

			// to determine the Accept header
			String[] _accepts = new String[] {
				"text/plain",
				"application/json",
				"text/json"
			};

			var localVarContentType = Org.OpenAPITools.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
			if (localVarContentType != null)
				localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

			var localVarAccept = Org.OpenAPITools.Client.ClientUtils.SelectHeaderAccept(_accepts);
			if (localVarAccept != null)
				localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

			if (uuid != null)
			{
				localVarRequestOptions.QueryParameters.Add(Org.OpenAPITools.Client.ClientUtils.ParameterToMultiMap("", "uuid", uuid));
			}
			if (authorization != null)
			{
				localVarRequestOptions.HeaderParameters.Add("Authorization", Org.OpenAPITools.Client.ClientUtils.ParameterToString(authorization)); // header parameter
			}


			// make the HTTP request
			var localVarResponse = this.Client.Get<PublixStatus>("/Status/fetch", localVarRequestOptions, this.Configuration);

			if (this.ExceptionFactory != null)
			{
				Exception _exception = this.ExceptionFactory("StatusFetchGet", localVarResponse);
				if (_exception != null)
					throw _exception;
			}

			return localVarResponse;
		}

		/// <summary>
		///  
		/// </summary>
		/// <exception cref="Org.OpenAPITools.Client.ApiException">Thrown when fails to make API call</exception>
		/// <param name="uuid"> (optional)</param>
		/// <param name="authorization">For Authenticating the user (optional)</param>
		/// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
		/// <returns>Task of PublixStatus</returns>
		public async System.Threading.Tasks.Task<PublixStatus> StatusFetchGetAsync(string uuid = default(string), string authorization = default(string), System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
		{
			Org.OpenAPITools.Client.ApiResponse<PublixStatus> localVarResponse = await StatusFetchGetWithHttpInfoAsync(uuid, authorization, cancellationToken).ConfigureAwait(false);
			return localVarResponse.Data;
		}

		/// <summary>
		///  
		/// </summary>
		/// <exception cref="Org.OpenAPITools.Client.ApiException">Thrown when fails to make API call</exception>
		/// <param name="uuid"> (optional)</param>
		/// <param name="authorization">For Authenticating the user (optional)</param>
		/// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
		/// <returns>Task of ApiResponse (PublixStatus)</returns>
		public async System.Threading.Tasks.Task<Org.OpenAPITools.Client.ApiResponse<PublixStatus>> StatusFetchGetWithHttpInfoAsync(string uuid = default(string), string authorization = default(string), System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
		{

			Org.OpenAPITools.Client.RequestOptions localVarRequestOptions = new Org.OpenAPITools.Client.RequestOptions();

			String[] _contentTypes = new String[] {
			};

			// to determine the Accept header
			String[] _accepts = new String[] {
				"text/plain",
				"application/json",
				"text/json"
			};


			var localVarContentType = Org.OpenAPITools.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
			if (localVarContentType != null)
				localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

			var localVarAccept = Org.OpenAPITools.Client.ClientUtils.SelectHeaderAccept(_accepts);
			if (localVarAccept != null)
				localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

			if (uuid != null)
			{
				localVarRequestOptions.QueryParameters.Add(Org.OpenAPITools.Client.ClientUtils.ParameterToMultiMap("", "uuid", uuid));
			}
			if (authorization != null)
			{
				localVarRequestOptions.HeaderParameters.Add("Authorization", Org.OpenAPITools.Client.ClientUtils.ParameterToString(authorization)); // header parameter
			}


			// make the HTTP request

			var localVarResponse = await this.AsynchronousClient.GetAsync<PublixStatus>("/Status/fetch", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

			if (this.ExceptionFactory != null)
			{
				Exception _exception = this.ExceptionFactory("StatusFetchGet", localVarResponse);
				if (_exception != null)
					throw _exception;
			}

			return localVarResponse;
		}

		/// <summary>
		///  
		/// </summary>
		/// <exception cref="Org.OpenAPITools.Client.ApiException">Thrown when fails to make API call</exception>
		/// <param name="authorization">For Authenticating the user (optional)</param>
		/// <returns>PrivateStatus</returns>
		public PrivateStatus StatusMeGet(string authorization = default(string))
		{
			Org.OpenAPITools.Client.ApiResponse<PrivateStatus> localVarResponse = StatusMeGetWithHttpInfo(authorization);
			return localVarResponse.Data;
		}

		/// <summary>
		///  
		/// </summary>
		/// <exception cref="Org.OpenAPITools.Client.ApiException">Thrown when fails to make API call</exception>
		/// <param name="authorization">For Authenticating the user (optional)</param>
		/// <returns>ApiResponse of PrivateStatus</returns>
		public Org.OpenAPITools.Client.ApiResponse<PrivateStatus> StatusMeGetWithHttpInfo(string authorization = default(string))
		{
			Org.OpenAPITools.Client.RequestOptions localVarRequestOptions = new Org.OpenAPITools.Client.RequestOptions();

			String[] _contentTypes = new String[] {
			};

			// to determine the Accept header
			String[] _accepts = new String[] {
				"text/plain",
				"application/json",
				"text/json"
			};

			var localVarContentType = Org.OpenAPITools.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
			if (localVarContentType != null)
				localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

			var localVarAccept = Org.OpenAPITools.Client.ClientUtils.SelectHeaderAccept(_accepts);
			if (localVarAccept != null)
				localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

			if (authorization != null)
			{
				localVarRequestOptions.HeaderParameters.Add("Authorization", Org.OpenAPITools.Client.ClientUtils.ParameterToString(authorization)); // header parameter
			}


			// make the HTTP request
			var localVarResponse = this.Client.Get<PrivateStatus>("/Status/@me", localVarRequestOptions, this.Configuration);

			if (this.ExceptionFactory != null)
			{
				Exception _exception = this.ExceptionFactory("StatusMeGet", localVarResponse);
				if (_exception != null)
					throw _exception;
			}

			return localVarResponse;
		}

		/// <summary>
		///  
		/// </summary>
		/// <exception cref="Org.OpenAPITools.Client.ApiException">Thrown when fails to make API call</exception>
		/// <param name="authorization">For Authenticating the user (optional)</param>
		/// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
		/// <returns>Task of PrivateStatus</returns>
		public async System.Threading.Tasks.Task<PrivateStatus> StatusMeGetAsync(string authorization = default(string), System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
		{
			Org.OpenAPITools.Client.ApiResponse<PrivateStatus> localVarResponse = await StatusMeGetWithHttpInfoAsync(authorization, cancellationToken).ConfigureAwait(false);
			return localVarResponse.Data;
		}

		/// <summary>
		///  
		/// </summary>
		/// <exception cref="Org.OpenAPITools.Client.ApiException">Thrown when fails to make API call</exception>
		/// <param name="authorization">For Authenticating the user (optional)</param>
		/// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
		/// <returns>Task of ApiResponse (PrivateStatus)</returns>
		public async System.Threading.Tasks.Task<Org.OpenAPITools.Client.ApiResponse<PrivateStatus>> StatusMeGetWithHttpInfoAsync(string authorization = default(string), System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
		{

			Org.OpenAPITools.Client.RequestOptions localVarRequestOptions = new Org.OpenAPITools.Client.RequestOptions();

			String[] _contentTypes = new String[] {
			};

			// to determine the Accept header
			String[] _accepts = new String[] {
				"text/plain",
				"application/json",
				"text/json"
			};


			var localVarContentType = Org.OpenAPITools.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
			if (localVarContentType != null)
				localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

			var localVarAccept = Org.OpenAPITools.Client.ClientUtils.SelectHeaderAccept(_accepts);
			if (localVarAccept != null)
				localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

			if (authorization != null)
			{
				localVarRequestOptions.HeaderParameters.Add("Authorization", Org.OpenAPITools.Client.ClientUtils.ParameterToString(authorization)); // header parameter
			}


			// make the HTTP request

			var localVarResponse = await this.AsynchronousClient.GetAsync<PrivateStatus>("/Status/@me", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

			if (this.ExceptionFactory != null)
			{
				Exception _exception = this.ExceptionFactory("StatusMeGet", localVarResponse);
				if (_exception != null)
					throw _exception;
			}

			return localVarResponse;
		}

		/// <summary>
		///  
		/// </summary>
		/// <exception cref="Org.OpenAPITools.Client.ApiException">Thrown when fails to make API call</exception>
		/// <param name="statusUpdate"> (optional)</param>
		/// <param name="authorization">For Authenticating the user (optional)</param>
		/// <returns>string</returns>
		public string StatusStatusupdatePost(StatusUpdate statusUpdate = default(StatusUpdate), string authorization = default(string))
		{
			Org.OpenAPITools.Client.ApiResponse<string> localVarResponse = StatusStatusupdatePostWithHttpInfo(statusUpdate, authorization);
			return localVarResponse.Data;
		}

		/// <summary>
		///  
		/// </summary>
		/// <exception cref="Org.OpenAPITools.Client.ApiException">Thrown when fails to make API call</exception>
		/// <param name="statusUpdate"> (optional)</param>
		/// <param name="authorization">For Authenticating the user (optional)</param>
		/// <returns>ApiResponse of string</returns>
		public Org.OpenAPITools.Client.ApiResponse<string> StatusStatusupdatePostWithHttpInfo(StatusUpdate statusUpdate = default(StatusUpdate), string authorization = default(string))
		{
			Org.OpenAPITools.Client.RequestOptions localVarRequestOptions = new Org.OpenAPITools.Client.RequestOptions();

			String[] _contentTypes = new String[] {
				"application/json",
				"text/json",
				"application/_*+json"
			};

			// to determine the Accept header
			String[] _accepts = new String[] {
				"text/plain",
				"application/json",
				"text/json"
			};

			var localVarContentType = Org.OpenAPITools.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
			if (localVarContentType != null)
				localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

			var localVarAccept = Org.OpenAPITools.Client.ClientUtils.SelectHeaderAccept(_accepts);
			if (localVarAccept != null)
				localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

			if (authorization != null)
			{
				localVarRequestOptions.HeaderParameters.Add("Authorization", Org.OpenAPITools.Client.ClientUtils.ParameterToString(authorization)); // header parameter
			}
			localVarRequestOptions.Data = statusUpdate;


			// make the HTTP request
			var localVarResponse = this.Client.Post<string>("/Status/statusupdate", localVarRequestOptions, this.Configuration);

			if (this.ExceptionFactory != null)
			{
				Exception _exception = this.ExceptionFactory("StatusStatusupdatePost", localVarResponse);
				if (_exception != null)
					throw _exception;
			}

			return localVarResponse;
		}

		/// <summary>
		///  
		/// </summary>
		/// <exception cref="Org.OpenAPITools.Client.ApiException">Thrown when fails to make API call</exception>
		/// <param name="statusUpdate"> (optional)</param>
		/// <param name="authorization">For Authenticating the user (optional)</param>
		/// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
		/// <returns>Task of string</returns>
		public async System.Threading.Tasks.Task<string> StatusStatusupdatePostAsync(StatusUpdate statusUpdate = default(StatusUpdate), string authorization = default(string), System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
		{
			Org.OpenAPITools.Client.ApiResponse<string> localVarResponse = await StatusStatusupdatePostWithHttpInfoAsync(statusUpdate, authorization, cancellationToken).ConfigureAwait(false);
			return localVarResponse.Data;
		}

		/// <summary>
		///  
		/// </summary>
		/// <exception cref="Org.OpenAPITools.Client.ApiException">Thrown when fails to make API call</exception>
		/// <param name="statusUpdate"> (optional)</param>
		/// <param name="authorization">For Authenticating the user (optional)</param>
		/// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
		/// <returns>Task of ApiResponse (string)</returns>
		public async System.Threading.Tasks.Task<Org.OpenAPITools.Client.ApiResponse<string>> StatusStatusupdatePostWithHttpInfoAsync(StatusUpdate statusUpdate = default(StatusUpdate), string authorization = default(string), System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
		{

			Org.OpenAPITools.Client.RequestOptions localVarRequestOptions = new Org.OpenAPITools.Client.RequestOptions();

			String[] _contentTypes = new String[] {
				"application/json",
				"text/json",
				"application/_*+json"
			};

			// to determine the Accept header
			String[] _accepts = new String[] {
				"text/plain",
				"application/json",
				"text/json"
			};


			var localVarContentType = Org.OpenAPITools.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
			if (localVarContentType != null)
				localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

			var localVarAccept = Org.OpenAPITools.Client.ClientUtils.SelectHeaderAccept(_accepts);
			if (localVarAccept != null)
				localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

			if (authorization != null)
			{
				localVarRequestOptions.HeaderParameters.Add("Authorization", Org.OpenAPITools.Client.ClientUtils.ParameterToString(authorization)); // header parameter
			}
			localVarRequestOptions.Data = statusUpdate;


			// make the HTTP request

			var localVarResponse = await this.AsynchronousClient.PostAsync<string>("/Status/statusupdate", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

			if (this.ExceptionFactory != null)
			{
				Exception _exception = this.ExceptionFactory("StatusStatusupdatePost", localVarResponse);
				if (_exception != null)
					throw _exception;
			}

			return localVarResponse;
		}

	}
}
