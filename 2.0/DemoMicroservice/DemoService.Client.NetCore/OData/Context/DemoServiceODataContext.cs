using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OData;
using Microsoft.OData.Client;
using Newtonsoft.Json.Linq;

namespace DemoService.Client.OData.Context
{
	public partial class DemoServiceODataContext
	{
		#region Properties
		/// <summary>Authentication ticket</summary>
		/// <remarks>For backward compatibility only</remarks>
		public string AuthenticationTicket { get; set; }

		/// <summary>JSon Web Token</summary>
		public string Token { get; set; }
		#endregion


		#region Constructors
		public DemoServiceODataContext(Uri serviceRoot, string jwtSecret, string userName, string tenantId)
			: this(serviceRoot)
		{
			ClaimsIdentity claimsIdentity = new ClaimsIdentity(
				new[]
				{
					new Claim(ClaimTypes.Name, userName),
					new Claim("TenantId", tenantId)
				});

			var symmetricKey = Convert.FromBase64String(jwtSecret);
			var tokenDescriptor = new SecurityTokenDescriptor
			{
				Subject = claimsIdentity,
				Expires = DateTime.UtcNow.AddMinutes(10),
				SigningCredentials = new SigningCredentials(
					new SymmetricSecurityKey(symmetricKey),
					SecurityAlgorithms.HmacSha256Signature)
			};
			var tokenHandler = new JwtSecurityTokenHandler();
			var stoken = tokenHandler.CreateToken(tokenDescriptor);
			Token = tokenHandler.WriteToken(stoken);
		}
		#endregion


		#region partial OnContextCreated
		partial void OnContextCreated()
		{
			UsePostTunneling = false;
			//MaxProtocolVersion = 
			//IgnoreResourceNotFoundException = true;
			AddAndUpdateResponsePreference = DataServiceResponsePreference.IncludeContent;
			MergeOption = MergeOption.OverwriteChanges;
			Credentials = CredentialCache.DefaultNetworkCredentials;
			SaveChangesDefaultOptions = SaveChangesOptions.None;
			SendingRequest2 += OnSendingRequest2;
		}
		#endregion


		#region private OnSendingRequest2
		private void OnSendingRequest2(object sender, SendingRequest2EventArgs e)
		{
			HttpWebRequestMessage message = (HttpWebRequestMessage)e.RequestMessage;

			message.HttpWebRequest.UseDefaultCredentials = true;
			message.HttpWebRequest.PreAuthenticate = true;

			AddAuthenticationHeaders(message);
		}
		#endregion


		#region public BuildMethodUri
		/// <summary>Builds method URI.</summary>
		/// <param name="methodName">Method name.</param>
		/// <returns>Method URI.</returns>
		public Uri BuildMethodUri(string methodName)
		{
			string baseUri = BaseUri.AbsoluteUri;

			return new Uri(
				(baseUri.EndsWith("/", StringComparison.Ordinal)
					? baseUri
					: baseUri + '/')
				+ methodName);
		}
		#endregion

		#region public AddAuthenticationHeaders
		public WebRequest AddAuthenticationHeaders(WebRequest request)
		{
			if (request == null)
				throw new ArgumentNullException(nameof(request));

			if (!string.IsNullOrEmpty(AuthenticationTicket))
				request.Headers["AuthenticationTicket"] = AuthenticationTicket;

			if (!string.IsNullOrEmpty(Token))
				request.Headers["Authorization"] = "Bearer " + Token;

			return request;
		}

		public HttpRequestMessage AddAuthenticationHeaders(HttpRequestMessage message)
		{
			if (message == null)
				throw new ArgumentNullException(nameof(message));

			if (!string.IsNullOrEmpty(AuthenticationTicket))
				message.Headers.Add("AuthenticationTicket", AuthenticationTicket);

			if (!string.IsNullOrEmpty(Token))
				message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Token);

			return message;
		}

		public HttpWebRequestMessage AddAuthenticationHeaders(HttpWebRequestMessage message)
		{
			if (message == null)
				throw new ArgumentNullException(nameof(message));

			if (!string.IsNullOrEmpty(AuthenticationTicket))
				message.SetHeader("AuthenticationTicket", AuthenticationTicket);

			if (!string.IsNullOrEmpty(Token))
				message.SetHeader("Authorization", "Bearer " + Token);

			return message;
		}
		#endregion


		#region public async ImportAssets
		/// <summary>Imports Assets.</summary>
		/// <param name="kind">Asset kind.</param>
		/// <param name="overwrite">Indicates whether to overwrite existing.</param>
		/// <param name="data">Assets data.</param>
		public async Task ImportAssets(
			string kind,
			bool overwrite,
			byte[] data)
		{
			if (kind == null)
				throw new ArgumentNullException(nameof(kind));
			if (data == null)
				throw new ArgumentNullException(nameof(data));

			var client = new HttpClient();

			try
			{
				using (var request = new HttpRequestMessage())
				{
					var boundary = Guid.NewGuid().ToString();

					var content = new MultipartFormDataContent(boundary);
					content.Headers.Remove("Content-Type");
					content.Headers.TryAddWithoutValidation("Content-Type",
						"multipart/form-data; boundary=" + boundary);
					content.Add(new StreamContent(new MemoryStream(data)), "File", "File");

					request.Content = content;
					request.Method = new HttpMethod("POST");

					request.RequestUri = BuildMethodUri("Import?kind='" + kind + "'&overwriteExisting=" +
						overwrite.ToString().ToLowerInvariant());

					AddAuthenticationHeaders(request);

					var response = await client.SendAsync(
						request,
						HttpCompletionOption.ResponseHeadersRead,
						default(CancellationToken)).ConfigureAwait(false);

					try
					{
						var headers = response.Headers.ToDictionary(h => h.Key, h => h.Value);

						if (response.Content?.Headers != null)
						{
							foreach (var item in response.Content.Headers)
								headers[item.Key] = item.Value;
						}

						if (response.StatusCode != HttpStatusCode.OK)
						{
							var responseData = response.Content == null
								? null
								: await response.Content.ReadAsStringAsync().ConfigureAwait(false);
							throw new DataServiceClientException(responseData, (int)response.StatusCode);
						}
					}
					finally
					{
						response?.Dispose();
					}
				}
			}
			finally
			{
				client.Dispose();
			}


			//HttpWebRequest webRequest =
			//	// ReSharper disable once AccessToStaticMemberViaDerivedType
			//	(HttpWebRequest)WebRequest.Create(BuildMethodUri("Import?kind='" + kind + "'&overwriteExisting=" +
			//		overwrite.ToString().ToLowerInvariant()));
			//webRequest.Method = "POST";
			//webRequest.ContentType = "application/octet-stream";

			//AddAuthenticationHeaders(webRequest);

			//#region generate request parameters
			//using (Stream postStream = webRequest.GetRequestStream())
			//	postStream.Write(data, 0, data.Length);
			//#endregion

			//#region import data
			//using (WebResponse webResponse = await webRequest.GetResponseAsync())
			//{
			//	if (webResponse.ContentLength == 0)
			//		return;

			//	Stream stream = webResponse.GetResponseStream();

			//	if (stream == null)
			//		return;

			//	using (StreamReader httpWebStreamReader = new StreamReader(stream))
			//	{
			//		string result = await httpWebStreamReader.ReadToEndAsync();

			//		throw new ODataException(result);
			//	}
			//}
			//#endregion
		}
		#endregion

		#region public async ExportAssets
		/// <summary>Exports Assets.</summary>
		/// <param name="kind">Asset kind.</param>
		/// <param name="productId">Product Id</param>
		/// <param name="additionalId">Additional Id</param>
		/// <param name="ids">Collections of Id</param>
		public async Task<byte[]> ExportAssets(
			string kind,
			string productId,
			string additionalId,
			ICollection<Guid> ids)
		{
			if (kind == null)
				throw new ArgumentNullException(nameof(kind));

			HttpWebRequest webRequest =
				// ReSharper disable once AccessToStaticMemberViaDerivedType
				(HttpWebRequest)WebRequest.Create(BuildMethodUri("Export"));
			webRequest.Method = "POST";
			webRequest.ContentType = "application/json; odata.metadata=minimal";

			AddAuthenticationHeaders(webRequest);

			#region generate request parameters
			JObject jsonRoot = new JObject { new JProperty(nameof(kind), kind) };


			if (!string.IsNullOrEmpty(productId))
				jsonRoot.Add(new JProperty(nameof(productId), productId));

			if (!string.IsNullOrEmpty(additionalId))
				jsonRoot.Add(new JProperty(nameof(additionalId), additionalId));

			if (ids != null && ids.Any())
			{
				JArray idsValues = new JArray();

				foreach (Guid id in ids)
					idsValues.Add(id.ToString());

				jsonRoot.Add(new JProperty(nameof(ids), idsValues));
			}


			byte[] data = Encoding.UTF8.GetBytes(jsonRoot.ToString());

			using (Stream postStream = webRequest.GetRequestStream())
				postStream.Write(data, 0, data.Length);
			#endregion

			using (HttpWebResponse webResponse = (HttpWebResponse)await webRequest.GetResponseAsync())
			{
				if (webResponse.ContentLength == 0)
					return null;

				#region get data
				Stream stream = webResponse.GetResponseStream();

				if (stream == null)
					return null;

				if (webResponse.StatusCode != HttpStatusCode.OK)
				{
					// ReSharper disable once AssignNullToNotNullAttribute
					using (StreamReader httpWebStreamReader = new StreamReader(stream))
					{
						string result = httpWebStreamReader.ReadToEnd();

						if (!string.IsNullOrEmpty(result))
							throw new ODataException(result);

						return null;
					}
				}
				#endregion

				using (MemoryStream rez = new MemoryStream())
				{
					await stream.CopyToAsync(rez);

					return rez.ToArray();
				}
			}
		}
		#endregion
	}
}