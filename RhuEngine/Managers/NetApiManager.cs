using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using SharedModels;

using StereoKit;

namespace RhuEngine.Managers
{
	public class NetApiManager : IManager
	{
		private class HttpDataResponse<T>
		{
			public T Data { get; set; }

			public HttpResponseMessage HttpResponseMessage { get; set; }

			public async static Task<HttpDataResponse<T>> Build(HttpResponseMessage httpResponseMessage) {
				var httpDataResponse = new HttpDataResponse<T> {
					HttpResponseMessage = httpResponseMessage
				};
				try {
					httpDataResponse.Data = JsonConvert.DeserializeObject<T>(await httpResponseMessage.Content.ReadAsStringAsync());
				}
				catch (Exception ex) {
					StereoKit.Log.Err(ex.ToString());
				}
				return httpDataResponse;
			}
			public bool IsDataGood => HttpResponseMessage?.IsSuccessStatusCode ?? false & Data is not null;
		}

		private async Task<HttpDataResponse<R>> SendPost<R, T>(string path, T value) {
			var httpContent = new StringContent(JsonConvert.SerializeObject(value), Encoding.UTF8, "application/json");
			var request = await _httpClient.PostAsync(path, httpContent);
			StereoKit.Log.Info($"Path {path}  data: {await request.Content.ReadAsStringAsync()}");
			return await HttpDataResponse<R>.Build(request);
		}

		private async Task<HttpDataResponse<R>> SendGet<R>(string path) {
			var request = await _httpClient.GetAsync(path);
			StereoKit.Log.Info($"Path {path}  data: {await request.Content.ReadAsStringAsync()}");
			return await HttpDataResponse<R>.Build(request);
		}

		private HttpClient _httpClient;
		//This crap is because quest go burr
		public static byte[][] LetsEncrypt = new byte[][]
		{
            //e2
            Convert.FromBase64String(@"MIICxjCCAkygAwIBAgIQTtI99q9+x/mwxHJv+VEqdzAKBggqhkjOPQQDAzBPMQswCQYDVQQGEwJVUzEpMCcGA1UEChMgSW50ZXJuZXQgU2VjdXJpdHkgUmVzZWFyY2ggR3JvdXAxFTATBgNVBAMTDElTUkcgUm9vdCBYMjAeFw0yMDA5MDQwMDAwMDBaFw0yNTA5MTUxNjAwMDBaMDIxCzAJBgNVBAYTAlVTMRYwFAYDVQQKEw1MZXQncyBFbmNyeXB0MQswCQYDVQQDEwJFMjB2MBAGByqGSM49AgEGBSuBBAAiA2IABCOaLO3lixmNYVWex+ZVYOiTLgi0SgNWtU4hufk50VU4Zp/LbBVDxCsnsI7vuf4xp4Cu+ETNggGEyBqJ3j8iUwe5Yt/qfSrRf1/D5R58duaJ+IvLRXeASRqEL+VkDXrW3qOCAQgwggEEMA4GA1UdDwEB/wQEAwIBhjAdBgNVHSUEFjAUBggrBgEFBQcDAgYIKwYBBQUHAwEwEgYDVR0TAQH/BAgwBgEB/wIBADAdBgNVHQ4EFgQUbZkq9U0C6+MRwWC6km+NPS7x6kQwHwYDVR0jBBgwFoAUfEKWrt5LSDv6kviejM9ti6lyN5UwMgYIKwYBBQUHAQEEJjAkMCIGCCsGAQUFBzAChhZodHRwOi8veDIuaS5sZW5jci5vcmcvMCcGA1UdHwQgMB4wHKAaoBiGFmh0dHA6Ly94Mi5jLmxlbmNyLm9yZy8wIgYDVR0gBBswGTAIBgZngQwBAgEwDQYLKwYBBAGC3xMBAQEwCgYIKoZIzj0EAwMDaAAwZQIxAPJCN9qpyDmZtX8K3m8UYQvK51BrXclM6WfrdeZlUBKyhTXUmFAtJw4X6A0x9mQFPAIwJa/No+KQUAM1u34E36neL/Zba7ombkIOchSgx1iVxzqtFWGddgoG+tppRPWhuhhn"),
            //r4
            Convert.FromBase64String(@"MIIFFjCCAv6gAwIBAgIRAIp5IlCr5SxSbO7Pf8lC3WIwDQYJKoZIhvcNAQELBQAwTzELMAkGA1UEBhMCVVMxKTAnBgNVBAoTIEludGVybmV0IFNlY3VyaXR5IFJlc2VhcmNoIEdyb3VwMRUwEwYDVQQDEwxJU1JHIFJvb3QgWDEwHhcNMjAwOTA0MDAwMDAwWhcNMjUwOTE1MTYwMDAwWjAyMQswCQYDVQQGEwJVUzEWMBQGA1UEChMNTGV0J3MgRW5jcnlwdDELMAkGA1UEAxMCUjQwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQCzKNx3KdPnkb7ztwoAx/vyVQslImNTNq/pCCDfDa8oPs3Gq1e2naQlGaXSMm1Jpgi5xy+hm5PFIEBrhDEgoo4wYCVg79kaiT8faXGy2uo/c0HEkG9m/X2eWNh3z81ZdUTJoQp7nz8bDjpmb7Z1z4vLr53AcMX/0oIKr13N4uichZSk5gA16H5OOYHHIYlgd+odlvKLg3tHxG0ywFJ+Ix5FtXHuo+8XwgOpk4nd9Z/buvHa4H6Xh3GBHhqCVuQ+fBiiCOUWX6j6qOBIUU0YFKAMo+W2yrO1VRJrcsdafzuM+efZ0Y4STTMzAyrxE+FCPMIuWWAubeAHRzNl39Jnyk2FAgMBAAGjggEIMIIBBDAOBgNVHQ8BAf8EBAMCAYYwHQYDVR0lBBYwFAYIKwYBBQUHAwIGCCsGAQUFBwMBMBIGA1UdEwEB/wQIMAYBAf8CAQAwHQYDVR0OBBYEFDadPuCxQPYnLHy/jZ0xivZUpkYmMB8GA1UdIwQYMBaAFHm0WeZ7tuXkAXOACIjIGlj26ZtuMDIGCCsGAQUFBwEBBCYwJDAiBggrBgEFBQcwAoYWaHR0cDovL3gxLmkubGVuY3Iub3JnLzAnBgNVHR8EIDAeMBygGqAYhhZodHRwOi8veDEuYy5sZW5jci5vcmcvMCIGA1UdIAQbMBkwCAYGZ4EMAQIBMA0GCysGAQQBgt8TAQEBMA0GCSqGSIb3DQEBCwUAA4ICAQCJbu5CalWO+H+Az0lmIG14DXmlYHQEk26umjuCyioWs2icOlZznPTcZvbfq02YPHGTCu3ctggVDULJ+fwOxKekzIqeyLNkp8dyFwSAr23DYBIVeXDpxHhShvv0MLJzqqDFBTHYe1X5X2Y7oogy+UDJxV2N24/gZ8lxG4Vr2/VEfUOrw4Tosl5Z+1uzOdvTyBcxD/E5rGgTLczmulctHy3IMTmdTFr0FnU0/HMQoquWQuODhFqzMqNcsdbjANUBwOEQrKI8Sy6+b84kHP7PtO+S4Ik8R2k7ZeMlE1JmxBi/PZU860YlwT8/qOYToCHVyDjhv8qutbf2QnUl3SV86th2I1QQE14s0y7CdAHcHkw3sAEeYGkwCA74MO+VFtnYbf9B2JBOhyyWb5087rGzitu5MTAW41X9DwTeXEg+a24tAeht+Y1MionHUwa4j7FB/trN3Fnb/r90+4P66ZETVIEcjseUSMHOw6yqv10/H/dw/8r2EDUincBBX3o9DL3SadqragkKy96HtMiLcqMMGAPm0gti1b6fbnvOdr0mrIVIKX5nzOeGZORaYLoSD4C8qvFT7U+Um6DMo36cVDNsPmkF575/s3C2CxGiCPQqVxPgfNSh+2CPd2Xv04lNeuw6gG89DlOhHuoFKRlmPnom+gwqhz3ZXMfzTfmvjrBokzCICA=="),
            //e1
            Convert.FromBase64String(@"MIICxjCCAk2gAwIBAgIRALO93/inhFu86QOgQTWzSkUwCgYIKoZIzj0EAwMwTzELMAkGA1UEBhMCVVMxKTAnBgNVBAoTIEludGVybmV0IFNlY3VyaXR5IFJlc2VhcmNoIEdyb3VwMRUwEwYDVQQDEwxJU1JHIFJvb3QgWDIwHhcNMjAwOTA0MDAwMDAwWhcNMjUwOTE1MTYwMDAwWjAyMQswCQYDVQQGEwJVUzEWMBQGA1UEChMNTGV0J3MgRW5jcnlwdDELMAkGA1UEAxMCRTEwdjAQBgcqhkjOPQIBBgUrgQQAIgNiAAQkXC2iKv0cS6Zdl3MnMayyoGli72XoprDwrEuf/xwLcA/TmC9N/A8AmzfwdAVXMpcuBe8qQyWj+240JxP2T35p0wKZXuskR5LBJJvmsSGPwSSB/GjMH2m6WPUZIvd0xhajggEIMIIBBDAOBgNVHQ8BAf8EBAMCAYYwHQYDVR0lBBYwFAYIKwYBBQUHAwIGCCsGAQUFBwMBMBIGA1UdEwEB/wQIMAYBAf8CAQAwHQYDVR0OBBYEFFrz7Sv8NsI3eblSMOpUb89Vyy6sMB8GA1UdIwQYMBaAFHxClq7eS0g7+pL4nozPbYupcjeVMDIGCCsGAQUFBwEBBCYwJDAiBggrBgEFBQcwAoYWaHR0cDovL3gyLmkubGVuY3Iub3JnLzAnBgNVHR8EIDAeMBygGqAYhhZodHRwOi8veDIuYy5sZW5jci5vcmcvMCIGA1UdIAQbMBkwCAYGZ4EMAQIBMA0GCysGAQQBgt8TAQEBMAoGCCqGSM49BAMDA2cAMGQCMHt01VITjWH+Dbo/AwCd89eYhNlXLr3pD5xcSAQh8suzYHKOl9YST8pE9kLJ03uGqQIwWrGxtO3qYJkgsTgDyj2gJrjubi1K9sZmHzOa25JK1fUpE8ZwYii6I4zPPS/Lgul/"),
            //r3
            Convert.FromBase64String(@"MIIFFjCCAv6gAwIBAgIRAJErCErPDBinU/bWLiWnX1owDQYJKoZIhvcNAQELBQAwTzELMAkGA1UEBhMCVVMxKTAnBgNVBAoTIEludGVybmV0IFNlY3VyaXR5IFJlc2VhcmNoIEdyb3VwMRUwEwYDVQQDEwxJU1JHIFJvb3QgWDEwHhcNMjAwOTA0MDAwMDAwWhcNMjUwOTE1MTYwMDAwWjAyMQswCQYDVQQGEwJVUzEWMBQGA1UEChMNTGV0J3MgRW5jcnlwdDELMAkGA1UEAxMCUjMwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQC7AhUozPaglNMPEuyNVZLD+ILxmaZ6QoinXSaqtSu5xUyxr45r+XXIo9cPR5QUVTVXjJ6oojkZ9YI8QqlObvU7wy7bjcCwXPNZOOftz2nwWgsbvsCUJCWH+jdxsxPnHKzhm+/b5DtFUkWWqcFTzjTIUu61ru2P3mBw4qVUq7ZtDpelQDRrK9O8ZutmNHz6a4uPVymZ+DAXXbpyb/uBxa3Shlg9F8fnCbvxK/eG3MHacV3URuPMrSXBiLxgZ3Vms/EY96Jc5lP/Ooi2R6X/ExjqmAl3P51T+c8B5fWmcBcUr2Ok/5mzk53cU6cG/kiFHaFpriV1uxPMUgP17VGhi9sVAgMBAAGjggEIMIIBBDAOBgNVHQ8BAf8EBAMCAYYwHQYDVR0lBBYwFAYIKwYBBQUHAwIGCCsGAQUFBwMBMBIGA1UdEwEB/wQIMAYBAf8CAQAwHQYDVR0OBBYEFBQusxe3WFbLrlAJQOYfr52LFMLGMB8GA1UdIwQYMBaAFHm0WeZ7tuXkAXOACIjIGlj26ZtuMDIGCCsGAQUFBwEBBCYwJDAiBggrBgEFBQcwAoYWaHR0cDovL3gxLmkubGVuY3Iub3JnLzAnBgNVHR8EIDAeMBygGqAYhhZodHRwOi8veDEuYy5sZW5jci5vcmcvMCIGA1UdIAQbMBkwCAYGZ4EMAQIBMA0GCysGAQQBgt8TAQEBMA0GCSqGSIb3DQEBCwUAA4ICAQCFyk5HPqP3hUSFvNVneLKYY611TR6WPTNlclQtgaDqw+34IL9fzLdwALduO/ZelN7kIJ+m74uyA+eitRY8kc607TkC53wlikfmZW4/RvTZ8M6UK+5UzhK8jCdLuMGYL6KvzXGRSgi3yLgjewQtCPkIVz6D2QQzCkcheAmCJ8MqyJu5zlzyZMjAvnnAT45tRAxekrsu94sQ4egdRCnbWSDtY7kh+BImlJNXoB1lBMEKIq4QDUOXoRgffuDghje1WrG9ML+Hbisq/yFOGwXD9RiX8F6sw6W4avAuvDszue5L3sz85K+EC4Y/wFVDNvZo4TYXao6Z0f+lQKc0t8DQYzk1OXVu8rp2yJMC6alLbBfODALZvYH7n7do1AZls4I9d1P4jnkDrQoxB3UqQ9hVl3LEKQ73xF1OyK5GhDDX8oVfGKF5u+decIsH4YaTw7mP3GFxJSqv3+0lUFJoi5Lc5da149p90IdshCExroL1+7mryIkXPeFM5TgO9r0rvZaBFOvV2z0gp35Z0+L4WPlbuEjN/lxPFin+HlUjr8gRsI3qfJOQFy/9rKIJR0Y/8Omwt/8oTWgy1mdeHmmjk7j1nYsvC9JSQ6ZvMldlTTKB3zhThV1+XWYp6rjd5JW1zbVWEkLNxE7GJThEUG3szgBVGP7pSWTUTsqXnLRbwHOoq7hHwg=="),
            //x2 cross
            Convert.FromBase64String(@"MIIEYDCCAkigAwIBAgIQB55JKIY3b9QISMI/xjHkYzANBgkqhkiG9w0BAQsFADBPMQswCQYDVQQGEwJVUzEpMCcGA1UEChMgSW50ZXJuZXQgU2VjdXJpdHkgUmVzZWFyY2ggR3JvdXAxFTATBgNVBAMTDElTUkcgUm9vdCBYMTAeFw0yMDA5MDQwMDAwMDBaFw0yNTA5MTUxNjAwMDBaME8xCzAJBgNVBAYTAlVTMSkwJwYDVQQKEyBJbnRlcm5ldCBTZWN1cml0eSBSZXNlYXJjaCBHcm91cDEVMBMGA1UEAxMMSVNSRyBSb290IFgyMHYwEAYHKoZIzj0CAQYFK4EEACIDYgAEzZvVn4CDCuwJSvMWSj5cz3es3mcFDR0HttwW+1qLFNvicWDEukWVEYmO6gbf9yoWHKS5xcUy4APgHoIYOIvXRdgKam7mAHf7AlF9ItgKbppbd9/w+kHsOdx1ymgHDB/qo4HlMIHiMA4GA1UdDwEB/wQEAwIBBjAPBgNVHRMBAf8EBTADAQH/MB0GA1UdDgQWBBR8Qpau3ktIO/qS+J6Mz22LqXI3lTAfBgNVHSMEGDAWgBR5tFnme7bl5AFzgAiIyBpY9umbbjAyBggrBgEFBQcBAQQmMCQwIgYIKwYBBQUHMAKGFmh0dHA6Ly94MS5pLmxlbmNyLm9yZy8wJwYDVR0fBCAwHjAcoBqgGIYWaHR0cDovL3gxLmMubGVuY3Iub3JnLzAiBgNVHSAEGzAZMAgGBmeBDAECATANBgsrBgEEAYLfEwEBATANBgkqhkiG9w0BAQsFAAOCAgEAG38lK5B6CHYAdxjhwy6KNkxBfr8XS+Mw11sMfpyWmG97sGjAJETM4vL80erb0p8B+RdNDJ1V/aWtbdIvP0tywC6uc8clFlfCPhWt4DHRCoSEbGJ4QjEiRhrtekC/lxaBRHfKbHtdIVwH8hGRIb/hL8Lvbv0FIOS093nzLbs3KvDGsaysUfUfs1oeZs5YBxg4f3GpPIO617yCnpp2D56wKf3L84kHSBv+q5MuFCENX6+Ot1SrXQ7UW0xx0JLqPaM2m3wf4DtVudhTU8yDZrtK3IEGABiL9LPXSLETQbnEtp7PLHeOQiALgH6fxatI27xvBI1sRikCDXCKHfESc7ZGJEKeKhcY46zHmMJyzG0tdm3dLCsmlqXPIQgb5dovy++fc5Ou+DZfR4+XKM6r4pgmmIv97igyIintTJUJxCD6B+GGLET2gUfA5GIy7R3YPEiIlsNekbave1mk7uOGnMeIWMooKmZVm4WAuR3YQCvJHBM8qevemcIWQPb1pK4qJWxSuscETLQyu/w4XKAMYXtX7HdOUM+vBqIPN4zhDtLTLxq9nHE+zOH40aijvQT2GcD5hq/1DhqqlWvvykdxS2McTZbbVSMKnQ+BdaDmQPVkRgNuzvpqfQbspDQGdNpT2Lm4xiN9qfgqLaSCpi4tEcrmzTFYeYXmchynn9NM0GbQp7s="),
            //x2
            Convert.FromBase64String(@"MIICGzCCAaGgAwIBAgIQQdKd0XLq7qeAwSxs6S+HUjAKBggqhkjOPQQDAzBPMQswCQYDVQQGEwJVUzEpMCcGA1UEChMgSW50ZXJuZXQgU2VjdXJpdHkgUmVzZWFyY2ggR3JvdXAxFTATBgNVBAMTDElTUkcgUm9vdCBYMjAeFw0yMDA5MDQwMDAwMDBaFw00MDA5MTcxNjAwMDBaME8xCzAJBgNVBAYTAlVTMSkwJwYDVQQKEyBJbnRlcm5ldCBTZWN1cml0eSBSZXNlYXJjaCBHcm91cDEVMBMGA1UEAxMMSVNSRyBSb290IFgyMHYwEAYHKoZIzj0CAQYFK4EEACIDYgAEzZvVn4CDCuwJSvMWSj5cz3es3mcFDR0HttwW+1qLFNvicWDEukWVEYmO6gbf9yoWHKS5xcUy4APgHoIYOIvXRdgKam7mAHf7AlF9ItgKbppbd9/w+kHsOdx1ymgHDB/qo0IwQDAOBgNVHQ8BAf8EBAMCAQYwDwYDVR0TAQH/BAUwAwEB/zAdBgNVHQ4EFgQUfEKWrt5LSDv6kviejM9ti6lyN5UwCgYIKoZIzj0EAwMDaAAwZQIwe3lORlCEwkSHRhtFcP9Ymd70/aTSVaYgLXTWNLxBo1BfASdWtL4ndQavEi51mI38AjEAi/V3bNTIZargCyzuFJ0nN6T5U6VR5CmD1/iQMVtCnwr1/q4AaOeMSQ+2b1tbFfLn"),
            //x1
            Convert.FromBase64String(@"MIIFazCCA1OgAwIBAgIRAIIQz7DSQONZRGPgu2OCiwAwDQYJKoZIhvcNAQELBQAwTzELMAkGA1UEBhMCVVMxKTAnBgNVBAoTIEludGVybmV0IFNlY3VyaXR5IFJlc2VhcmNoIEdyb3VwMRUwEwYDVQQDEwxJU1JHIFJvb3QgWDEwHhcNMTUwNjA0MTEwNDM4WhcNMzUwNjA0MTEwNDM4WjBPMQswCQYDVQQGEwJVUzEpMCcGA1UEChMgSW50ZXJuZXQgU2VjdXJpdHkgUmVzZWFyY2ggR3JvdXAxFTATBgNVBAMTDElTUkcgUm9vdCBYMTCCAiIwDQYJKoZIhvcNAQEBBQADggIPADCCAgoCggIBAK3oJHP0FDfzm54rVygch77ct984kIxuPOZXoHj3dcKi/vVqbvYATyjb3miGbESTtrFj/RQSa78f0uoxmyF+0TM8ukj13Xnfs7j/EvEhmkvBioZxaUpmZmyPfjxwv60pIgbz5MDmgK7iS4+3mX6UA5/TR5d8mUgjU+g4rk8Kb4Mu0UlXjIB0ttov0DiNewNwIRt18jA8+o+u3dpjq+sWT8KOEUt+zwvo/7V3LvSye0rgTBIlDHCNAymg4VMk7BPZ7hm/ELNKjD+Jo2FR3qyHB5T0Y3HsLuJvW5iB4YlcNHlsdu87kGJ55tukmi8mxdAQ4Q7e2RCOFvu396j3x+UCB5iPNgiV5+I3lg02dZ77DnKxHZu8A/lJBdiB3QW0KtZB6awBdpUKD9jf1b0SHzUvKBds0pjBqAlkd25HN7rOrFleaJ1/ctaJxQZBKT5ZPt0m9STJEadao0xAH0ahmbWnOlFuhjuefXKnEgV4We0+UXgVCwOPjdAvBbI+e0ocS3MFEvzG6uBQE3xDk3SzynTnjh8BCNAw1FtxNrQHusEwMFxIt4I7mKZ9YIqioymCzLq9gwQbooMDQaHWBfEbwrbwqHyGO0aoSCqI3Haadr8faqU9GY/rOPNk3sgrDQoo//fb4hVC1CLQJ13hef4Y53CIrU7m2Ys6xt0nUW7/vGT1M0NPAgMBAAGjQjBAMA4GA1UdDwEB/wQEAwIBBjAPBgNVHRMBAf8EBTADAQH/MB0GA1UdDgQWBBR5tFnme7bl5AFzgAiIyBpY9umbbjANBgkqhkiG9w0BAQsFAAOCAgEAVR9YqbyyqFDQDLHYGmkgJykIrGF1XIpu+ILlaS/V9lZLubhzEFnTIZd+50xx+7LSYK05qAvqFyFWhfFQDlnrzuBZ6brJFe+GnY+EgPbk6ZGQ3BebYhtF8GaV0nxvwuo77x/Py9auJ/GpsMiu/X1+mvoiBOv/2X/qkSsisRcOj/KKNFtY2PwByVS5uCbMiogziUwthDyC3+6WVwW6LLv3xLfHTjuCvjHIInNzktHCgKQ5ORAzI4JMPJ+GslWYHb4phowim57iaztXOoJwTdwJx4nLCgdNbOhdjsnvzqvHu7UrTkXWStAmzOVyyghqpZXjFaH3pO3JLF+l+/+sKAIuvtd7u+Nxe5AW0wdeRlN8NwdCjNPElpzVmbUq4JUagEiuTDkHzsxHpFKVK7q4+63SM1N95R1NbdWhscdCb+ZAJzVcoyi3B43njTOQ5yOf+1CceWxG1bQVs5ZufpsMljq4Ui0/1lvh+wjChP4kqKOJ2qxq4RgqsahDYVvTH9w7jXbyLeiNdd8XM2w9U/t7y0Ff/9yi0GE44Za4rF2LN9d11TPAmRGunUHBcnWEvgJBQl9nJEiU0Zsnvgc/ubhPgXRR4Xq37Z0j4r7g1SgEEzwxA57demyPxgcYxn/eR44/KJ4EBs+lVDR3veyJm+kXQ99b21/+jh5Xos1AnX5iItreGCc="),
            //x1 cross
            Convert.FromBase64String(@"MIIFYDCCBEigAwIBAgIQQAF3ITfU6UK47naqPGQKtzANBgkqhkiG9w0BAQsFADA/MSQwIgYDVQQKExtEaWdpdGFsIFNpZ25hdHVyZSBUcnVzdCBDby4xFzAVBgNVBAMTDkRTVCBSb290IENBIFgzMB4XDTIxMDEyMDE5MTQwM1oXDTI0MDkzMDE4MTQwM1owTzELMAkGA1UEBhMCVVMxKTAnBgNVBAoTIEludGVybmV0IFNlY3VyaXR5IFJlc2VhcmNoIEdyb3VwMRUwEwYDVQQDEwxJU1JHIFJvb3QgWDEwggIiMA0GCSqGSIb3DQEBAQUAA4ICDwAwggIKAoICAQCt6CRz9BQ385ueK1coHIe+3LffOJCMbjzmV6B493XCov71am72AE8o295ohmxEk7axY/0UEmu/H9LqMZshftEzPLpI9d1537O4/xLxIZpLwYqGcWlKZmZsj348cL+tKSIG8+TA5oCu4kuPt5l+lAOf00eXfJlII1PoOK5PCm+DLtFJV4yAdLbaL9A4jXsDcCEbdfIwPPqPrt3aY6vrFk/CjhFLfs8L6P+1dy70sntK4EwSJQxwjQMpoOFTJOwT2e4ZvxCzSow/iaNhUd6shweU9GNx7C7ib1uYgeGJXDR5bHbvO5BieebbpJovJsXQEOEO3tkQjhb7t/eo98flAgeYjzYIlefiN5YNNnWe+w5ysR2bvAP5SQXYgd0FtCrWQemsAXaVCg/Y39W9Eh81LygXbNKYwagJZHduRze6zqxZXmidf3LWicUGQSk+WT7dJvUkyRGnWqNMQB9GoZm1pzpRboY7nn1ypxIFeFntPlF4FQsDj43QLwWyPntKHEtzBRL8xurgUBN8Q5N0s8p0544fAQjQMNRbcTa0B7rBMDBcSLeCO5imfWCKoqMpgsy6vYMEG6KDA0Gh1gXxG8K28Kh8hjtGqEgqiNx2mna/H2qlPRmP6zjzZN7IKw0KKP/32+IVQtQi0Cdd4Xn+GOdwiK1O5tmLOsbdJ1Fu/7xk9TNDTwIDAQABo4IBRjCCAUIwDwYDVR0TAQH/BAUwAwEB/zAOBgNVHQ8BAf8EBAMCAQYwSwYIKwYBBQUHAQEEPzA9MDsGCCsGAQUFBzAChi9odHRwOi8vYXBwcy5pZGVudHJ1c3QuY29tL3Jvb3RzL2RzdHJvb3RjYXgzLnA3YzAfBgNVHSMEGDAWgBTEp7Gkeyxx+tvhS5B1/8QVYIWJEDBUBgNVHSAETTBLMAgGBmeBDAECATA/BgsrBgEEAYLfEwEBATAwMC4GCCsGAQUFBwIBFiJodHRwOi8vY3BzLnJvb3QteDEubGV0c2VuY3J5cHQub3JnMDwGA1UdHwQ1MDMwMaAvoC2GK2h0dHA6Ly9jcmwuaWRlbnRydXN0LmNvbS9EU1RST09UQ0FYM0NSTC5jcmwwHQYDVR0OBBYEFHm0WeZ7tuXkAXOACIjIGlj26ZtuMA0GCSqGSIb3DQEBCwUAA4IBAQAKcwBslm7/DlLQrt2M51oGrS+o44+/yQoDFVDC5WxCu2+b9LRPwkSICHXM6webFGJueN7sJ7o5XPWioW5WlHAQU7G75K/QosMrAdSW9MUgNTP52GE24HGNtLi1qoJFlcDyqSMo59ahy2cI2qBDLKobkx/J3vWraV0T9VuGWCLKTVXkcGdtwlfFRjlBz4pYg1htmf5X6DYO8A4jqv2Il9DjXA6USbW1FzXSLr9Ohe8Y4IWS6wY7bCkjCWDcRQJMEhg76fsO3txE+FiYruq9RUWhiF1myv4Q6W+CyBFCDfvp7OOGAN6dEOM4+qR9sdjoSYKEBpsr6GtPAQw4dy753ec5")
		};

		public bool IsLoggedIn { get; private set; }
		public PrivateUser User { get; private set; }
		private HttpClientHandler HttpClientHandler { get; set; }
		public CookieContainer Cookies => HttpClientHandler?.CookieContainer;
		public async Task<AccountCreationResponse> SignUp(string username, string email, string password, DateTime dateOfBirth) {
			try {
				var data = await SendPost<AccountCreationResponse, UserRegistration>("/api/authentication/Register", new UserRegistration { Username = username, Email = email, Password = password, DateOfBirth = dateOfBirth });
				return data.Data;
			}
			catch (HttpRequestException requestException) {
				throw ProssesHttpRequestException(requestException);
			}
		}

		private LoginResponse ProcessLoginResponse(HttpDataResponse<LoginResponse> data) {
			var login = false;
			if (data.IsDataGood) {
				if (data.Data.Login) {
					User = data.Data.User;
					IsLoggedIn = true;
					login = true;
				}
			}
			if (!login) {
				IsLoggedIn = false;
				User = null;
			}
			return data.Data;
		}

		public async Task<LoginResponse> Login(string email, string password) {
			try { 
				var data = await SendPost<LoginResponse, UserLogin>("/api/authentication/Login", new UserLogin { Email = email, Password = password });
				return ProcessLoginResponse(data);
			}
			catch (HttpRequestException requestException) {
				throw ProssesHttpRequestException(requestException);
			}
		}

		public async Task<PublicUser> GetUserInfo(string id) {
			try {
				var data = await SendGet<PublicUser>($"/api/userinfo/FromUserID?id={id}");
				return data.Data;
			}
			catch (HttpRequestException requestException) {
				throw ProssesHttpRequestException(requestException);
			}
		}

		public async Task<LoginResponse> GetMe() {
			try {
				var data = await SendGet<LoginResponse>("/api/authentication/GetMe");
				return ProcessLoginResponse(data);
			}
			catch (HttpRequestException requestException) {
				throw ProssesHttpRequestException(requestException);
			}
		}

		public async Task<IEnumerable<SessionInfo>> GetSessions() {
			try {
				var data = await SendGet<IEnumerable<SessionInfo>>("/api/SessionInfo/GetAllSessions");
				return data.Data;
			}
			catch (HttpRequestException requestException) {
				throw ProssesHttpRequestException(requestException);
			}
		}

		public Exception ProssesHttpRequestException(HttpRequestException requestException) {
			if (requestException.InnerException is WebException webException && webException.Status == WebExceptionStatus.NameResolutionFailure) {
				return new ConnectToServerError(this);
			}
			else if (requestException.InnerException is WebException webExceptione && webExceptione.Status == WebExceptionStatus.Timeout) {
				return new ConnectToServerError(this);
			}
			else if (requestException.InnerException is WebException webExceptiond && webExceptiond.Status == WebExceptionStatus.ConnectFailure) {
				return new ConnectToServerError(this);
			}
			else if (requestException.InnerException is SocketException socketException && socketException.SocketErrorCode == SocketError.HostDown) {
				return new ConnectToServerError(this);
			}
			else if (requestException.InnerException is SocketException socketExceptione && socketExceptione.SocketErrorCode == SocketError.HostNotFound) {
				return new ConnectToServerError(this);
			}
			else if (requestException.InnerException is SocketException socketExceptionee && socketExceptionee.SocketErrorCode == SocketError.HostUnreachable) {
				return new ConnectToServerError(this);
			}
			else if (requestException.InnerException is SocketException socketExceptioneee && socketExceptioneee.SocketErrorCode == SocketError.TimedOut) {
				return new ConnectToServerError(this);
			}
			else if (requestException.InnerException is IOException ioException) {
				if (ioException.InnerException is SocketException socketException2 && socketException2.SocketErrorCode == SocketError.HostDown) {
					return new ConnectToServerError(this);
				}
				else if (ioException.InnerException is SocketException socketExceptione2 && socketExceptione2.SocketErrorCode == SocketError.HostNotFound) {
					return new ConnectToServerError(this);
				}
				else if (ioException.InnerException is SocketException socketExceptionee2 && socketExceptionee2.SocketErrorCode == SocketError.HostUnreachable) {
					return new ConnectToServerError(this);
				}
				else if (ioException.InnerException is SocketException socketExceptioneee2 && socketExceptioneee2.SocketErrorCode == SocketError.TimedOut) {
					return new ConnectToServerError(this);
				}
			}
			return requestException;
		}

		private void WriteCookiesToDisk(string file, CookieContainer cookieJar) {
			if (!Directory.Exists(Path.GetDirectoryName(file))) {
				Directory.CreateDirectory(Path.GetDirectoryName(file));
			}
			using Stream stream = File.Create(file);
			try {
				StereoKit.Log.Info("Writing cookies to disk...");
				var formatter = new BinaryFormatter();
				formatter.Serialize(stream, cookieJar);
				StereoKit.Log.Info("Done.");
			}
			catch (Exception e) {
				StereoKit.Log.Err("Problem writing cookies to disk: " + e.GetType());
			}
		}


		private CookieContainer ReadCookiesFromDisk(string file) {
			try {
				using Stream stream = File.Open(file, FileMode.Open);
				StereoKit.Log.Info("Reading cookies from disk...");
				var formatter = new BinaryFormatter();
				return (CookieContainer)formatter.Deserialize(stream);
			}
			catch (Exception e) {
				StereoKit.Log.Err("Problem reading cookies from disk: " + e.GetType());
				return new CookieContainer();
			}
		}
		public void Logout() {
			if (IsLoggedIn) {
				IsLoggedIn = false;
				User = null;
				for (var i = Engine.worldManager.worlds.Count - 1; i >= 0; i--) {
					var item = Engine.worldManager.worlds[i];
					if (item.IsNetworked) {
						item.Dispose();
					}
				}
				//Reomve All Cookies
				try {
					var cookies = Cookies.GetCookies(BaseAddress);
					foreach (Cookie co in cookies) {
						co.Expires = DateTime.Now.Subtract(TimeSpan.FromDays(1));
					}
					var uri = new UriBuilder(BaseAddress) {
						Scheme = ((BaseAddress.Scheme == Uri.UriSchemeHttps) && !RuntimeInformation.FrameworkDescription.StartsWith("Mono ")) ? "wss" : "ws",
						Port = RuntimeInformation.FrameworkDescription.StartsWith("Mono ") ? 80 : BaseAddress.Port
					};
					if (RuntimeInformation.FrameworkDescription.StartsWith("Mono ")) {
						var ucookies = Cookies.GetCookies(BaseAddress);
						foreach (Cookie co in ucookies) {
							co.Expires = DateTime.Now.Subtract(TimeSpan.FromDays(1));
						}
					}
				}
				catch (Exception ex) {
					Log.Err($"Failed To Clear Cookies {ex}");
				}
				WriteCookiesToDisk(_cookiePath, Cookies);
			}
		}

		public void Dispose() {
			WriteCookiesToDisk(_cookiePath, Cookies);
		}

		public Uri BaseAddress =>
				_httpClient?.BaseAddress ?? new Uri("http://localhost:5000/"); 
#if DEBUG
				//_httpClient?.BaseAddress ?? new Uri("https://unstable.family/");
#else
				_httpClient?.BaseAddress ?? new Uri("https://RhubarbVR.net/");
#endif
		public NetApiManager(string path) {
			_cookiePath = path is null ? Engine.BaseDir + "\\RhuCookies" : path + "\\RhuCookies";
		}
		private readonly string _cookiePath;

		private Engine Engine { get; set; }

		public bool _isOnline = false;

		public event Action HasGoneOfline;

		public bool IsOnline { get => _isOnline; 
			set {
				if (!value) {
					IsGoneOfline();
					HasGoneOfline?.Invoke();
				}
				_isOnline = value;
			}
		}

		public void Init(Engine engine) {
			Engine = engine;
			HttpClientHandler = new HttpClientHandler {
				AllowAutoRedirect = true,
				UseCookies = true,
				CookieContainer = ReadCookiesFromDisk(_cookiePath)
			};
			HttpClientHandler.ServerCertificateCustomValidationCallback = ValidateRemoteCertificate;
			_httpClient = new HttpClient(HttpClientHandler) {
				BaseAddress = BaseAddress
			};
			UpdateCheckForInternetConnection();
		}

		public void UpdateCheckForInternetConnection() {
			Task.Run(() =>{
				IsOnline = CheckForInternetConnection();
				if (IsOnline) {
					if (Cookies.Count > 0) {
						GetMe().ConfigureAwait(false);
					}
				}
			});
		}
		
		public void IsGoneOfline() {
			User = null;
			IsLoggedIn = false;
		}

		public static bool CheckForInternetConnection(int timeoutMs = 10000, string url = null) {
			try {
				url ??= CultureInfo.InstalledUICulture switch { { Name: var n } when n.StartsWith("fa") => // Iran
																	"http://www.aparat.com", { Name: var n } when n.StartsWith("zh") => // China
																								 "http://www.baidu.com",
					_ =>
						"http://www.gstatic.com/generate_204",
				};

				var request = (HttpWebRequest)WebRequest.Create(url);
				request.KeepAlive = false;
				request.Timeout = timeoutMs;
				using var response = (HttpWebResponse)request.GetResponse();
				return true;
			}
			catch {
				return false;
			}
		}

		public static bool ValidateRemoteCertificate(
	HttpRequestMessage sender,
	X509Certificate cert,
	X509Chain chain,
	SslPolicyErrors policyErrors) {
			if(policyErrors == SslPolicyErrors.None) {
				return true;
			}
			var foundCert = chain.ChainElements[1].Certificate.RawData;
			return LetsEncrypt.Any((val) => val.SequenceEqual(foundCert));
		}

		public void Step() {
		}
	}
}
