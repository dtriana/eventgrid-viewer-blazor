using Azure.Messaging;
using Azure.Messaging.EventGrid;
using Blazor.EventGridViewer.Core.Models;
using Blazor.EventGridViewer.Services.Adapters;
using Blazor.EventGridViewer.Services.Interfaces;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace Blazor.EventGridViewer.Unit.Tests
{
    /// <summary>
    /// Test class used to test the EventGridSchemaAdapter
    /// </summary>
    public class EventGridSchemaAdapterTests
    {
        /// <summary>
        /// Testing that the Convert method throws an exception if the EventGridEvent is null
        /// </summary>
        [Fact]
        public void EventGridSchemaAdapterConvertNullThrowsExceptionTest()
        {
            // Arrange
            Mock<IEventGridIdentifySchemaService> mockEventGridIdentifySchemaService = new Mock<IEventGridIdentifySchemaService>();
            mockEventGridIdentifySchemaService.Setup(s => s.Identify(It.IsAny<string>())).Returns(Core.EventGridSchemaType.CloudEvent);
            IAdapter<string, List<EventGridEventModel>> adapter = new EventGridSchemaAdapter(mockEventGridIdentifySchemaService.Object);

            // Act
            var exception = Record.Exception(() => adapter.Convert(null));

            // Assert
            Assert.NotNull(exception);
        }

        /// <summary>
        /// Testing that the Convert method can convert a EventGridEvent to a EventGridEventModel
        /// </summary>
        [Fact]
        public void EventGridSchemaAdapterConvertEventGridTest()
        {
            // Arrange
            string json = Data.GetMockEventGridEventJson();
            Mock<IEventGridIdentifySchemaService> mockEventGridIdentifySchemaService = new Mock<IEventGridIdentifySchemaService>();
            mockEventGridIdentifySchemaService.Setup(s => s.Identify(json)).Returns(Core.EventGridSchemaType.EventGrid);
            IAdapter<string, List<EventGridEventModel>> adapter = new EventGridSchemaAdapter(mockEventGridIdentifySchemaService.Object);
            var mockModel = JsonSerializer.Deserialize<List<EventGridEvent>>(json).FirstOrDefault();

            // Act
            var model = adapter.Convert(Data.GetMockEventGridEventJson()).FirstOrDefault();

            // Assert
            Assert.True(model.Id == mockModel.Id && model.Subject == mockModel.Subject &&
                model.EventType == mockModel.EventType && model.EventTime == mockModel.EventTime.ToString("o"));

            var data = JsonSerializer.Serialize(mockModel, new JsonSerializerOptions { WriteIndented = true });
            Assert.Equal(data, model.Data);
        }

        /// <summary>
        /// Testing that the Convert method can convert a CloudEvent to a EventGridEventModel
        /// </summary>
        [Fact]
        public void EventGridSchemaAdapterConvertCloudEventTest()
        {
            // Arrange
            string json = Data.GetMockCloudEventJson();
            Mock<IEventGridIdentifySchemaService> mockEventGridIdentifySchemaService = new Mock<IEventGridIdentifySchemaService>();
            mockEventGridIdentifySchemaService.Setup(s => s.Identify(json)).Returns(Core.EventGridSchemaType.CloudEvent);
            IAdapter<string, List<EventGridEventModel>> adapter = new EventGridSchemaAdapter(mockEventGridIdentifySchemaService.Object);
            var mockModel = CloudEvent.Parse(new BinaryData(json));

            // Act
            var model = adapter.Convert(json).FirstOrDefault();

            // Assert
            Assert.True(model.Id == mockModel.Id && model.Subject == mockModel.Subject &&
                model.EventType == mockModel.Type && model.EventTime == mockModel.Time.ToString());

            var data = JsonSerializer.Serialize(mockModel, new JsonSerializerOptions { WriteIndented = true });
            Assert.Equal(data, model.Data);
        }
    }
}
