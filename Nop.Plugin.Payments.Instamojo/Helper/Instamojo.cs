using Newtonsoft.Json;
using Nop.Plugin.Payments.Instamojo.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Payments.Instamojo.Helper
{
    public class InstamojoHelper
    {
        private WebHeaderCollection headers;
        private string _instamojoBaseUrl;

        // Instamojo
        public const string InstamojoLiveBaseURL = "https://www.instamojo.com/api/1.1/";
        public const string InstamojoTestBaseURL = "https://test.instamojo.com/api/1.1/";
        public const string EndPoint_PaymentRequest = "payment-requests/";
        public const string PaymentSuccessUrl = "order/history";

        // API
        public const string WebLocalBaseUrl = "https://localhost:44369/";

        public InstamojoHelper()
        {
        }

        public InstamojoHelper(string key, string token)
        {
            _instamojoBaseUrl = InstamojoLiveBaseURL;

            headers = new WebHeaderCollection();
            headers.Add("X-API-KEY: " + key);
            headers.Add("X-AUTH-TOKEN: " + token);
        }

        public PaymentRequestResponse CreatePaymentRequest(PaymentRequest paymentRequest)
        {
            HttpWebRequest httpReq = (HttpWebRequest)WebRequest.Create(_instamojoBaseUrl + EndPoint_PaymentRequest);
            httpReq.Headers = headers;
            httpReq.ContentType = "application/json";
            httpReq.Method = WebRequestMethods.Http.Post;
            string json = JsonConvert.SerializeObject(paymentRequest);
            try
            {
                using (var writer = new StreamWriter(httpReq.GetRequestStream()))
                {
                    writer.Write(json);
                    writer.Flush();
                    writer.Close();
                }

                using (HttpWebResponse response = (HttpWebResponse)httpReq.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        PaymentRequestResponse prr = JsonConvert.DeserializeObject<PaymentRequestResponse>(reader.ReadToEnd());
                        reader.Close();
                        response.Close();
                        return prr;
                    }
                }
            }
            catch (Exception ex)
            {
                return new PaymentRequestResponse() { success = false, payment_request = null };
            }
        }

        public PaymentRequestResponse GetPaymentDetails(string paymentRequestId)
        {
            try
            {
                HttpWebRequest httpReq = (HttpWebRequest)WebRequest.Create(_instamojoBaseUrl + EndPoint_PaymentRequest
                    + paymentRequestId + "/");
                httpReq.Headers = headers;
                httpReq.Method = WebRequestMethods.Http.Get;
                httpReq.ContentType = "application/json";
                using (HttpWebResponse response = (HttpWebResponse)httpReq.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        PaymentRequestResponse jobj = JsonConvert.DeserializeObject<PaymentRequestResponse>(reader.ReadToEnd());
                        reader.Close();
                        response.Close();
                        return jobj;
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public PaymentRequestResponse CreatePaymentRequest(string price, string userName, string mobileNumber, string email, bool? sendEmail, bool? sendSms, string purpose)
        {
            PaymentRequest pr = new PaymentRequest();
            pr.allow_repeated_payments = false;
            pr.amount = price;
            pr.buyer_name = userName;
            pr.email = email;
            pr.phone = mobileNumber;
            pr.partner_fee_type = "fixed";
            if (sendEmail != null) pr.send_email = (bool)sendEmail;
            //if (sendSms != null) pr.send_sms = (bool)sendSms;
            pr.redirect_url = WebLocalBaseUrl + PaymentSuccessUrl;
            pr.purpose = purpose;

            PaymentRequestResponse response = CreatePaymentRequest(pr);
            return response;
        }
    }
}
