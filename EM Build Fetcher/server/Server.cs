using EM_Build_Fetcher.commands;
using EM_Build_Fetcher.logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace EM_Build_Fetcher.server
{
    public class Server
    {
        private static Logger Logger = LoggerFactory.GetSimpleLogger(typeof(Server).Name);
        private CommandRepo _commands;
        private readonly int _port;
        private readonly Thread _thread;
        private HttpListener _listener;

        public Server(int port, CommandRepo repo)
        {
            this._port = port;
            this._commands = repo;
            _thread = new Thread(this.Listen);
            _thread.Start();
        }

        public void Stop()
        {
            _thread.Abort();
            _listener.Stop();
        }

        private void Listen()
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add("http://*:" + _port + "/");
            _listener.Start();

            while(_listener.IsListening)
            {
                try
                {
                    Process(_listener.GetContext());
                }
                catch (Exception)
                {
                    /* Ignoring exceptions */
                }
            }
        }

        private void Process(HttpListenerContext ctx)
        {

            if(!ctx.Request.HttpMethod.Equals("POST"))
            {
                var portalHtml = "";

                using (var reader = new StreamReader($"{Configuration.PortalLocation}"))
                {
                    var line = "";
                    while((line = reader.ReadLine()) != null) {
                        portalHtml += line;
                    }
                    reader.Close();
                }

                var output = ctx.Response.OutputStream;
                var buf = Encoding.UTF8.GetBytes(portalHtml);
                ctx.Response.ContentLength64 = buf.Length;
                output.Write(buf, 0, buf.Length);
                output.Close();

                return;
            }

            Logger.Info($"Received a web command from '{ctx.Request.UserHostAddress}'");

            var request = ctx.Request;
            string text;
            using (var reader = new StreamReader(request.InputStream,
                                                 request.ContentEncoding))
            {
                text = reader.ReadToEnd();
            }

            var args = text.Split('&');
            var list = new List<string>(args);

            var kvp = new Dictionary<string, string>();

            list.ForEach(s =>
            {
                var parts = s.Split('=');
                var key = parts[0];
                var value = parts[1];
                kvp.Add(key, value);
            });

            var complete = "";

            foreach (KeyValuePair<string, string> test in kvp)
            {
                complete += test.Value + " ";
            }

            //TODO: Handle encoded html properly
            complete = complete.Replace("%5C", @"\");

            var executed = ParseInputAndRunCommand(complete);
            Logger.Info(executed ? $"Successfully executed WEB action {complete}" : $"Failed to execute WEB action: {complete}");

            var htmlResponse =
                "<html>" +
                "<body>" +
                (executed ? $"Successfully executed WEB action {complete}" : $"Failed to execute WEB action: {complete}") +
                "</body>" +
                "</html>";


            var outputStream = ctx.Response.OutputStream;
            var buffer = Encoding.UTF8.GetBytes(htmlResponse);
            ctx.Response.ContentLength64 = buffer.Length;
            outputStream.Write(buffer, 0, buffer.Length);
            outputStream.Close();
        }

        private bool ParseInputAndRunCommand(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }

            var arguments = input.Split(' ');
            var cmd = arguments[0];

            if (string.IsNullOrWhiteSpace(cmd))
            {
                return false;
            }

            if (!_commands.Contains(cmd))
            {
                Logger.Info($"No internal command: {cmd}");
                return false;
            }

            var complete = _commands.Get(cmd).Execute(arguments);

            return complete; 
        }
    }
}
