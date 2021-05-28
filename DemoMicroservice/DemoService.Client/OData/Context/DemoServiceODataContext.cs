using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Foundry.Core.Shared.Security;
using Foundry.Shared.Extensions;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OData;
using Microsoft.OData.Client;
using Newtonsoft.Json.Linq;

namespace DemoService.Client.OData.Context
{
	public partial class DemoServiceODataContext
	{
		#region Properties
		/// <summary>JSon Web Token</summary>
		public string Token { get; set; }

		/// <summary>Certificate Key</summary>
		private string CertificateKey { get; set; }

		/// <summary>Additional HTTP headers</summary>
		// ReSharper disable once CollectionNeverUpdated.Global
		public Dictionary<string, string> AdditionalHeaders { get; } = new Dictionary<string, string>();
		#endregion


		#region private static ToServiceUri
		private static Uri ToServiceUri(Uri serviceRoot)
		{
			return new Uri(serviceRoot.AbsoluteUri + "api/v1");
		}
		#endregion

		#region private static ParseCertificateKey
		private static (Guid CertificateId, string PrivateKey) ParseCertificateKey(string certificateKey)
		{
			if (certificateKey == null)
				throw new ArgumentNullException(nameof(certificateKey));

			string[] parts = certificateKey.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

			if (parts.Length != 2)
				throw new ArgumentException($"{certificateKey} - invalid format", nameof(certificateKey));

			if (!Guid.TryParse(parts[0], out var id))
				throw new ArgumentException($"{certificateKey} - invalid format, cannot detect certificate id", nameof(certificateKey));

			return (id, parts[1]);
		}
		#endregion


		#region public static Create
		public static DemoServiceODataContext Create(Uri serviceRoot)
		{
			return new DemoServiceODataContext(ToServiceUri(serviceRoot));
		}
		#endregion

		#region public static CreateWithCertificate
		public static DemoServiceODataContext CreateWithCertificate(Uri serviceRoot, string certificateKey)
		{
			return
				new DemoServiceODataContext(ToServiceUri(serviceRoot))
				{
					CertificateKey = certificateKey
				};
		}
		#endregion

		#region public static CreateWithJwt
		public static DemoServiceODataContext CreateWithJwt(Uri serviceRoot, string jwt)
		{
			return
				new DemoServiceODataContext(ToServiceUri(serviceRoot))
				{
					Token = jwt
				};
		}
		#endregion

		#region public static CreateWithJwtSecret
		public static DemoServiceODataContext CreateWithJwtSecret(
			Uri serviceRoot,
			string jwtSecret,
			string userName,
			string tenantId,
			string impersonatedTenantId = null)
		{
			DemoServiceODataContext context = new DemoServiceODataContext(ToServiceUri(serviceRoot));

			ClaimsIdentity claimsIdentity = new ClaimsIdentity(
				new[]
				{
					new Claim(ClaimTypes.Name, userName),
					new Claim(SystemDefaults.JwtClaimTypeTenantId, tenantId)
				});

			if (impersonatedTenantId != null)
				claimsIdentity.AddClaim(new Claim(SystemDefaults.JwtClaimTypeImpersonatedTenantId, impersonatedTenantId));

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
			context.Token = tokenHandler.WriteToken(stoken);

			return context;
		}
		#endregion


		#region public static GenerateTokenByCertificateKey
		public static string GenerateTokenByCertificateKey(string certificateKey)
		{
			var (certificateId, privateKey) = ParseCertificateKey(certificateKey);

			var cryptoServiceProvider = RSA.Create();
			cryptoServiceProvider.FromXmlStringTemp(privateKey);

			var tokenDescriptor = new SecurityTokenDescriptor
			{
				Subject = new ClaimsIdentity(),
				Expires = DateTime.UtcNow.Add(TimeSpan.FromMinutes(5)),
				SigningCredentials = new SigningCredentials(
					new RsaSecurityKey(cryptoServiceProvider)
					{
						KeyId = SystemDefaults.JwtCertificateKidPrefix + certificateId
					},
					SecurityAlgorithms.RsaSha256Signature,
					SecurityAlgorithms.Sha256Digest)
			};
			var tokenHandler = new JwtSecurityTokenHandler();
			var stoken = tokenHandler.CreateToken(tokenDescriptor);

			return tokenHandler.WriteToken(stoken);
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

			foreach (KeyValuePair<string, string> header in AdditionalHeaders)
				message.SetHeader(header.Key, header.Value);
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

			if (!string.IsNullOrEmpty(Token))
				request.Headers["Authorization"] = "Bearer " + Token;
			else if (CertificateKey != null)
				request.Headers["Authorization"] = "Bearer " + GenerateTokenByCertificateKey(CertificateKey);

			return request;
		}

		public HttpRequestMessage AddAuthenticationHeaders(HttpRequestMessage message)
		{
			if (message == null)
				throw new ArgumentNullException(nameof(message));

			if (!string.IsNullOrEmpty(Token))
				message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Token);
			else if (CertificateKey != null)
				message.Headers.Authorization = new AuthenticationHeaderValue(
					"Bearer ",
					GenerateTokenByCertificateKey(CertificateKey));

			return message;
		}

		public HttpWebRequestMessage AddAuthenticationHeaders(HttpWebRequestMessage message)
		{
			if (message == null)
				throw new ArgumentNullException(nameof(message));

			if (!string.IsNullOrEmpty(Token))
				message.SetHeader("Authorization", "Bearer " + Token);
			else if (CertificateKey != null)
				message.SetHeader("Authorization", "Bearer " + GenerateTokenByCertificateKey(CertificateKey));

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