using Azure.Messaging;
using Azure.Messaging.EventGrid;
using Blazor.EventGridViewer.Core;
using Blazor.EventGridViewer.Core.Models;
using Blazor.EventGridViewer.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Blazor.EventGridViewer.Services.Adapters
{
    /// <summary>
    /// Class used to convert EventGrid messages from multiple schemas to a custom EventGridEventModel
    /// </summary>
    public class EventGridSchemaAdapter : IAdapter<string, List<EventGridEventModel>>
    {
        private readonly IEventGridIdentifySchemaService _eventGridIdentifySchemaService;
        
        public EventGridSchemaAdapter(IEventGridIdentifySchemaService eventGridIdentifySchemaService)
        {
            _eventGridIdentifySchemaService = eventGridIdentifySchemaService;
        }

        /// <summary>
        /// Method used to convert a schema to a EventGridEventModel
        /// </summary>
        /// <param name="t">json</param>
        /// <returns>EventGridEvent list</returns>
        public List<EventGridEventModel> Convert(string t)
        {
            if (string.IsNullOrEmpty(t))
                throw new ArgumentNullException("json is null");

            List<EventGridEventModel> models = new List<EventGridEventModel>();
            if (_eventGridIdentifySchemaService.Identify(t) == EventGridSchemaType.EventGrid)
                models = AdaptEventGridEvent(t);
            else
                models = AdaptCloudEvent(t);

            return models;
        }

        /// <summary>
        /// Method used to convert a CloudEvent to a EventGridEventModel
        /// </summary>
        /// <param name="t">json</param>
        /// <returns>EventGrid list</returns>
        private List<EventGridEventModel> AdaptCloudEvent(string t)
        {
            List<EventGridEventModel> models = new List<EventGridEventModel>();
            var cloudEvent = JsonSerializer.Deserialize<CloudEvent>(t);

            var json = JsonSerializer.Serialize(cloudEvent, new JsonSerializerOptions { WriteIndented = true });
            EventGridEventModel model = new EventGridEventModel()
            {
                Id = cloudEvent.Id,
                EventType = cloudEvent.Type,
                Subject = string.IsNullOrEmpty(cloudEvent.Subject) ? cloudEvent.Type : cloudEvent.Subject,
                Data = json,
                EventData = cloudEvent.Data,
                EventTime = cloudEvent.Time.ToString()
            };
            models.Add(model);

            return models;
        }

        /// <summary>
        /// Method used to convert a EventGridEvent to a EventGridEventModel
        /// </summary>
        /// <param name="t">json</param>
        /// <returns>EventGrid list</returns>
        private List<EventGridEventModel> AdaptEventGridEvent(string t)
        {
            List<EventGridEventModel> models = new List<EventGridEventModel>();
            var eventGridEvents = JsonSerializer.Deserialize<List<EventGridEvent>>(t);

            foreach (var eventGridEvent in eventGridEvents)
            {
                var json = JsonSerializer.Serialize(eventGridEvent, new JsonSerializerOptions { WriteIndented = true });
                EventGridEventModel model = new EventGridEventModel()
                {
                    Id = eventGridEvent.Id,
                    EventType = eventGridEvent.EventType,
                    Subject = string.IsNullOrEmpty(eventGridEvent.Subject) ? eventGridEvent.EventType : eventGridEvent.Subject,
                    Data = json,
                    EventData = eventGridEvent.Data,
                    EventTime = eventGridEvent.EventTime.ToString("o") // https://docs.microsoft.com/en-us/dotnet/api/system.datetime.tostring?view=netcore-3.1
                };
                models.Add(model);
            }
            return models;
        }
    }
}
