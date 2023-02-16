using Azure.Messaging.EventGrid;
using Blazor.EventGridViewer.Core.Models;
using Blazor.EventGridViewer.Services.Interfaces;
using System;

namespace Blazor.EventGridViewer.Services.Adapters
{
    /// <summary>
    /// Class used to convert an EventGridEvent to a EventGridViewerModel
    /// </summary>
    public class EventGridEventModelAdapter : IAdapter<EventGridEvent, EventGridViewerEventModel>
    {
        /// <summary>
        /// Method used to convert a EventGridEvent to a EventGridViewerEventModel
        /// </summary>
        /// <param name="t">EventGridEvent</param>
        /// <returns>EventGridViewerEventModel</returns>
        public EventGridViewerEventModel Convert(EventGridEvent t)
        {
            if (t == null)
                throw new ArgumentNullException("EventGridEvent is null.");

            EventGridViewerEventModel model = new()
            {
                Data = t.Data.ToString(),
                EventType = t.EventType,
                Subject = t.Subject,
                Id = t.Id,
                EventTime = t.EventTime.ToString("o")
            };
            return model;
        }
    }
}
