using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Azure.Messaging.EventGrid;
using Azure.Messaging.EventGrid.SystemEvents;
using Blazor.EventGridViewer.Core.Models;
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
        private readonly IAdapter<string, List<EventGridEventModel>> _eventGridSchemaAdapter;

        public EventGridController(IEventGridService eventGridService, IAdapter<string, List<EventGridEventModel>> eventGridSchemaAdapter)
        {
            _eventGridService = eventGridService;
            _eventGridSchemaAdapter = eventGridSchemaAdapter;
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
                // using StreamReader due to changes in .Net Core 3 serializer ie ValueKind
                using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
                {
                    var json = await reader.ReadToEndAsync();
                    var eventGridEventModels = _eventGridSchemaAdapter.Convert(json);

                    foreach (EventGridEventModel model in eventGridEventModels)
                    {
                        // EventGrid validation message
                        if (model.EventType == SystemEventNames.EventGridSubscriptionValidation)
                        {
                            var eventData = ((JsonObject)model.EventData).Deserialize<SubscriptionValidationEventData>();
                            var responseData = new SubscriptionValidationResponse()
                            {
                                ValidationResponse = eventData.ValidationCode
                            };
                            return Ok(responseData);
                        }
                        // handle all other events
                        this.HandleEvent(model);
                        return result;
                    }
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
        private void HandleEvent(EventGridEventModel model)
        {
            _eventGridService.RaiseEventReceivedEvent(model);
        }
    }
}
