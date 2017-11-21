using System;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using PublicStatusIndicator.IndicatorEngine;
using PublicStatusIndicator.Webserver;

namespace PublicStatusIndicator.ApiController
{
    internal class StatusController : ApiController
    {
        [Route("/StatusController/Idle")]
        public async Task<HttpResponseMessage> Idle()
        {
            var engine = InidicatorEngine.Instance;
            engine.State = EngineState.Idle;
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () => { engine.RefreshTimer.Start(); }
            );

            return Ok(engine.State);
        }


        [Route("/StatusController/InProgress")]
        public async Task<HttpResponseMessage> InProgress()
        {
            var engine = InidicatorEngine.Instance;
            engine.State = EngineState.Progress;
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () => { engine.RefreshTimer.Start(); }
            );
            return Ok(engine.State);
        }


        [Route("/StatusController/Good")]
        public async Task<HttpResponseMessage> Good()
        {
            var engine = InidicatorEngine.Instance;
            engine.State = EngineState.Good;
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () => { engine.RefreshTimer.Start(); }
            );
            return Ok(engine.State);
        }


        [Route("/StatusController/Bad")]
        public async Task<HttpResponseMessage> Bad()
        {
            var engine = InidicatorEngine.Instance;
            engine.State = EngineState.Bad;
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () => { engine.RefreshTimer.Start(); }
            );
            return Ok(engine.State);
        }
    }
}