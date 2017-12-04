using System;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using PublicStatusIndicator.IndicatorEngine;
using PublicStatusIndicator.Webserver;

namespace PublicStatusIndicator.ApiController
{
    [Authentication]
    internal class StatusController : ApiController
    {
        App ParentApp;

        public StatusController(App parent)
        {
            ParentApp = parent;
        }

        public event SetNewState SetNewStateByHost;

        [Route("/StatusController/Blank")]
        public async Task<HttpResponseMessage> Blank()
        {
            SetNewStateByHost?.Invoke(EngineState.Blank);

            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () => { ParentApp.RefreshTimer.Start(); }
                );
            return await Ok("Off :)");
        }

        [Route("/StatusController/Idle")]
        public async Task<HttpResponseMessage> Idle()
        {
            SetNewStateByHost?.Invoke(EngineState.Idle);
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
        () => { ParentApp.RefreshTimer.Start(); }
        );

            return await Ok(EngineDefines.StateOutputs[EngineState.Idle]);
        }

        [Authentication]
        [Route("/StatusController/InProgress")]
        public async Task<HttpResponseMessage> InProgress()
        {
            SetNewStateByHost?.Invoke(EngineState.Progress);
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
        () => { ParentApp.RefreshTimer.Start(); }
        );

            return await Ok(EngineDefines.StateOutputs[EngineState.Progress]);
        }


        [Route("/StatusController/Bad")]
        public async Task<HttpResponseMessage> Bad()
        {
            SetNewStateByHost?.Invoke(EngineState.Bad);
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
        () => { ParentApp.RefreshTimer.Start(); }
        );

            return await Ok(EngineDefines.StateOutputs[EngineState.Bad]);
        }


        [Route("/StatusController/Unstable")]
        public async Task<HttpResponseMessage> Unstable()
        {
            SetNewStateByHost?.Invoke(EngineState.Unstable);
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
        () => { ParentApp.RefreshTimer.Start(); }
        );

            return await Ok(EngineDefines.StateOutputs[EngineState.Unstable]);
        }


        [Route("/StatusController/Stable")]
        public async Task<HttpResponseMessage> Stable()
        {
            SetNewStateByHost?.Invoke(EngineState.Stable);
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
        () => { ParentApp.RefreshTimer.Start(); }
        );
            return await Ok(EngineDefines.StateOutputs[EngineState.Stable]);
        }
    }
}