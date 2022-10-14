using Microsoft.AspNetCore.Components.Web;

namespace BlazorApp1.Pages
{
    public partial class Test
    {
        private string _message;

        private async Task OnTouchMove(TouchEventArgs args)
        {
            _message = "OnTouchMove " + args.Touches[0].ClientX;
            Console.WriteLine(_message);
        }
        private async Task OnTouchEnd(TouchEventArgs args)
        {
            _message = "OnTouchEnd ";
            Console.WriteLine(_message);
        }

        private async Task OnTouchStart(TouchEventArgs args)
        {
            _message = "OnTouchStart ";
            Console.WriteLine(_message);
        }
    }
}
