using System;
using System.Reflection;
using System.Threading.Tasks;
using Azure.Messaging.EventGrid;
using Azure.Messaging.EventGrid.SystemEvents;
using Blazor.EventGridViewer.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Blazor.EventGridViewer.ServerApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class EventGridController : ControllerBase
    {
        private readonly IEventGridService _eventGridService;

        public EventGridController(IEventGridService eventGridService)
        {
            _eventGridService = eventGridService;
        }

        /// <summary>
        /// Test Endpoint
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public string Get()
        {
            return "EventGridController is running...";
        }

        /// <summary>
        /// Webhook for the Azure EventGrid
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Post()
        {
            IActionResult result = Ok();

            try
            {
                var events = await BinaryData.FromStreamAsync(Request.Body);
                var eventGridEvents = EventGridEvent.ParseMany(events);
                foreach (var eventGridEvent in eventGridEvents)
                {
                    // Handle system events
                    if (eventGridEvent.TryGetSystemEventData(out object eventData))
                    {
                        // Handle the subscription validation event
                        if (eventData is SubscriptionValidationEventData subscriptionValidationEventData)
                        {
                            var responseData = new
                            {
                                ValidationResponse = subscriptionValidationEventData.ValidationCode
                            };

                            return new OkObjectResult(responseData);
                        }
                    }
                    // handle all other events
                    this.HandleEvent(eventGridEvent);
                    return result;
                }
            }
            catch (Exception ex)
            {
                result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            return result;
        }

        /// <summary>
        /// Handle EventGrid Event
        /// </summary>
        /// <param name="EventGridEventModel"></param>
        private void HandleEvent(EventGridEvent model)
        {
            _eventGridService.RaiseEventReceivedEvent(model);
        }
    }
}
