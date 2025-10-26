using System.Net;

namespace qbook_publisher
{
    internal class WebDavSession
    {
        private string v;
        private NetworkCredential credentials;

        public WebDavSession(string v, NetworkCredential credentials)
        {
            this.v = v;
            this.credentials = credentials;
        }
    }
}