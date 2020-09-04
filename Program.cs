using System;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using DocuSignApiSoap.DocuSignService;

namespace DocuSignApiSoap
{
    public class Program
    {
        // USER ACCOUNT
        private const string _userName = "";
        private const string _password = "";
        private const string _apiKey = "";

        // ENVELOPE ID
        private const string _envelopeId = "";

        public static void Main()
        {
            var authString = $"<DocuSignCredentials><Username>{_userName}</Username><Password>{_password}</Password><IntegratorKey>{_apiKey}</IntegratorKey></DocuSignCredentials>";

            var client = new DSAPIServiceSoapClient();

            using (OperationContextScope scope = new OperationContextScope(client.InnerChannel))
            {
                HttpRequestMessageProperty httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers.Add("X-DocuSign-Authentication", authString);
                OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;

                EnvelopeStatus status = client.RequestStatusEx(_envelopeId);
                Console.WriteLine($"Envelope ID: {_envelopeId}");
                Console.WriteLine("Subject: " + status.Subject);

                // RequestEnvelope Method
                //var envelope = client.RequestEnvelope(_envelopeId, false);

                // RequestEnvelopeV2 Method
                var requestOptions = new RequestEnvelopeV2Options()
                {
                    IncludeAC = false,
                    IncludeAnchorTabLocations = true,
                    IncludeDocumentBytes = false
                };
                var envelopeV2 = client.RequestEnvelopeV2(_envelopeId, requestOptions);

                if (envelopeV2.Recipients != null && envelopeV2.Recipients.Any())
                {
                    var index = 1;
                    Console.WriteLine($"{Environment.NewLine}Recipients:{Environment.NewLine}");

                    foreach(var rec in envelopeV2.Recipients)
                    {
                        Console.WriteLine($"{index++}. {rec.UserName} <{rec.Email}>");
                    }
                }

                if (envelopeV2.Tabs != null && envelopeV2.Tabs.Any())
                {
                    var index = 1;
                    Console.WriteLine($"{Environment.NewLine}Tab data:{Environment.NewLine}");

                    foreach (var tab in envelopeV2.Tabs)
                    {
                        var recipient = envelopeV2.Recipients.FirstOrDefault(r => r.ID == tab.RecipientID);
                        Console.WriteLine($"{index++}: ({recipient?.RoleName ?? "[no role]"}/{recipient?.UserName}) {tab.TabLabel}: {tab.Value}");
                    }
                } else
                {
                    Console.WriteLine($"{Environment.NewLine}No tab data found.");
                }

                Console.WriteLine($"{Environment.NewLine}Done.");
                Console.ReadKey();
            }

        }
    }
}
