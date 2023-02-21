using System;

namespace MsmhTools.HTTPProxyServer
{
    public class Request : IDisposable
    {
        private bool Disposed = false;
        public string? Full;
        public bool Deception = false;
        public bool NotEnded = false;
        public string? Target;
        public string? Method;
        public string? Version;
        public string? HtmlBody;
        public Dictionary<string, string> Headers = new();

        public Request(string req, bool sslMode = false)
        {
            Full = req;
            Serialize(sslMode);
        }

        public void Serialize(bool sslMode = false)
        {
            if (string.IsNullOrEmpty(Full))
            {
                Deception = true;
                return;
            }

            // setting only when requests are marked to allow https packets even if they are not ending with \r\n\r\n
            if (!Full.EndsWith("\r\n\r\n") && sslMode)
                NotEnded = true;

            try
            {
                string infoLine = Full.Split('\n')[0].Replace("\r", string.Empty);
                string[] iParts = infoLine.Split(' ');
                Method = iParts[0];
                Target = iParts[1];
                Version = iParts[2];
                Headers = new Dictionary<string, string>();
                string[] data = Full.Split('\n');
                
                for (int i = 1; i < data.Length; i++)
                {
                    string line = data[i].Replace("\r", string.Empty);

                    bool isBody = false;
                    if (line == string.Empty)
                    {
                        isBody = true;
                    }

                    if (!isBody)
                    {
                        // Add headers
                        string hName = line.Substring(0, line.IndexOf(':'));
                        string hValue = line.Substring(line.IndexOf(':') + 2, line.Length - line.IndexOf(':') - 2);
                        Headers.Add(hName, hValue);
                    }
                    else
                    {
                        if ((i + 1) < data.Length) HtmlBody += line + Environment.NewLine;
                        else if ((i + 1) == data.Length) HtmlBody += line;
                    }
                }

                // Add SSL packet filter
                if (!Version.Contains("HTTP"))
                    Deception = true;
            }
            catch (Exception)
            {
                Deception = true;
            }
        }

        public string Deserialize()
        {
            string request = Method + " " + Target + " " + Version + Environment.NewLine;

            for (int i = 0; i < Headers.Count; i++)
            {
                string hName = Headers.Keys.ToArray()[i];
                string hValue = Headers.Values.ToArray()[i];
                string line = hName + ": " + hValue;
                request += line + Environment.NewLine;
            }
            request += Environment.NewLine;
            request += HtmlBody;
            return request;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (Disposed) return;
            if (disposing)
            {
                Full = null;
                Target = null;
                Method = null;
                Version = null;
                HtmlBody = null;
                Headers.Clear();
            }

            Disposed = true;
        }
    }
}
