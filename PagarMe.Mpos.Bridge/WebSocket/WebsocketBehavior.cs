using System.Threading.Tasks;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace PagarMe.Mpos.Bridge.WebSocket
{
    internal class WebsocketBehavior : WebSocketBehavior
    {
        private Mpos _mpos;

        public WebsocketBehavior(Mpos mpos)
        {
            _mpos = mpos;
        }

        protected override async Task OnMessage(MessageEventArgs e)
        {
        }
    }
}