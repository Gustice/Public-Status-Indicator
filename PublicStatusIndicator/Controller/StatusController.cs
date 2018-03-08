using System;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using PublicStatusIndicator.IndicatorEngine;
using PublicStatusIndicator.Webserver;

namespace PublicStatusIndicator.Controller
{
    [Authentication]
    internal class StatusController : ApiController
    {
        readonly App _parentApp;

        public StatusController(App parent)
        {
            _parentApp = parent;
        }

        public event SetNewState SetNewStateByHost;
        public event SetNewProfile SetNewProfileByGui;

        public event SetPropotionalValue SetBlamePosition;
        public event GetProportianalValue GetFixPointPosition;

        [Route("/StatusController/Blank")]
        public async Task<HttpResponseMessage> Blank()
        {
            SetNewStateByHost?.Invoke(EngineState.Blank);

            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () => { _parentApp.RefreshTimer.Start(); }
                );
            return await Ok("Off :)");
        }

        [Route("/StatusController/Idle")]
        public async Task<HttpResponseMessage> Idle()
        {
            SetNewStateByHost?.Invoke(EngineState.Idle);
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
        () => { _parentApp.RefreshTimer.Start(); }
        );

            return await Ok(EngineDefines.StateOutputs[EngineState.Idle]);
        }

        [Authentication]
        [Route("/StatusController/InProgress")]
        public async Task<HttpResponseMessage> InProgress()
        {
            SetNewStateByHost?.Invoke(EngineState.Progress);
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
        () => { _parentApp.RefreshTimer.Start(); }
        );

            return await Ok(EngineDefines.StateOutputs[EngineState.Progress]);
        }


        [Route("/StatusController/Bad")]
        public async Task<HttpResponseMessage> Bad()
        {
            SetNewStateByHost?.Invoke(EngineState.Bad);
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
        () => { _parentApp.RefreshTimer.Start(); }
        );

            return await Ok(EngineDefines.StateOutputs[EngineState.Bad]);
        }


        [Route("/StatusController/Unstable")]
        public async Task<HttpResponseMessage> Unstable()
        {
            SetNewStateByHost?.Invoke(EngineState.Unstable);
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
        () => { _parentApp.RefreshTimer.Start(); }
        );

            return await Ok(EngineDefines.StateOutputs[EngineState.Unstable]);
        }


        [Route("/StatusController/Stable")]
        public async Task<HttpResponseMessage> Stable()
        {
            SetNewStateByHost?.Invoke(EngineState.Stable);
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
        () => { _parentApp.RefreshTimer.Start(); }
        );
            return await Ok(EngineDefines.StateOutputs[EngineState.Stable]);
        }



        [Route("/StatusController/SummonSauron")]
        public async Task<HttpResponseMessage> SummonSauron()
        {
            SetNewStateByHost?.Invoke(EngineState.Unstable);
            SetNewProfileByGui?.Invoke(EngineDefines.SummonSauron);

            return await Ok(EngineDefines.StateOutputs[EngineState.Sauron]);
        }

        [Route("/StatusController/DismissSauron")]
        public async Task<HttpResponseMessage> DismissSauron()
        {
            throw new NotImplementedException();
            return await Ok(EngineDefines.StateOutputs[EngineState.Sauron]);
        }


        [Route("/StatusController/OrderSauronToBlame")] // @todo: Coords for Blame-Position have to be transfered too
        public async Task<HttpResponseMessage> SauronBlame()
        {
            throw new NotImplementedException();
            SetBlamePosition?.Invoke(0);

            return await Ok(EngineDefines.StateOutputs[EngineState.Sauron]);
        }

        [Route("/StatusController/MoveRightBy")]    // @todo 
        public async Task<HttpResponseMessage> MoveSauronRight()
        {
            throw new NotImplementedException();
            return await Ok(EngineDefines.StateOutputs[EngineState.Sauron]);
        }

        [Route("/StatusController/MoveLeftBy")]     // @todo 
        public async Task<HttpResponseMessage> MoveSauronLeft()
        {
            throw new NotImplementedException();
            return await Ok(EngineDefines.StateOutputs[EngineState.Sauron]);
        }

        [Route("/StatusController/AskSauronForCoords")] // @todo Coords for Blame-Position have to be returned
        public async Task<HttpResponseMessage> AskSauronForCoords()
        {
            throw new NotImplementedException();
            float? pos = GetFixPointPosition?.Invoke();

            return await Ok(EngineDefines.StateOutputs[EngineState.Sauron]);
        }
    }
}