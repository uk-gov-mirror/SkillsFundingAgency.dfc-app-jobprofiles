﻿using AutoMapper;
using DFC.App.JobProfile.Controllers;
using DFC.App.JobProfile.Data.Providers;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace DFC.App.JobProfile.UnitTests.ControllerTests.HealthControllerTests
{
    public class HealthControllerTests
    {
        private ILogger<HealthController> _mockLogger;
        private IProvideJobProfiles _mockService;
        private IMapper _mockMapper;

        [Fact]
        public async Task HealthControllerHealthReturnsServiceUnavailableWhenUnhealthy()
        {
            // arrange
            const bool expectedResult = false;
            var controller = BuildHealthController();
            A.CallTo(() => _mockService.Ping()).Returns(expectedResult);

            // act
            var result = await controller.Health();

            // assert
            A.CallTo(() => _mockService.Ping()).MustHaveHappenedOnceExactly();
            var statusResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal((int)HttpStatusCode.ServiceUnavailable, statusResult.StatusCode);
        }

        [Fact]
        public async Task HealthControllerHealthReturnsServiceUnavailableWhenException()
        {
            // arrange
            var controller = BuildHealthController();
            A.CallTo(() => _mockService.Ping()).Throws<Exception>();

            // act
            var result = await controller.Health();

            // assert
            A.CallTo(() => _mockService.Ping()).MustHaveHappenedOnceExactly();
            var statusResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal((int)HttpStatusCode.ServiceUnavailable, statusResult.StatusCode);
        }

        [Fact]
        public void HealthControllerPingReturnsSuccess()
        {
            // arrange
            var controller = BuildHealthController();

            // act
            var result = controller.Ping();

            // assert
            var statusResult = Assert.IsType<OkResult>(result);

            A.Equals((int)HttpStatusCode.OK, statusResult.StatusCode);
        }

        private HealthController BuildHealthController()
        {
            _mockLogger = A.Fake<ILogger<HealthController>>();
            _mockService = A.Fake<IProvideJobProfiles>();
            _mockMapper = A.Fake<IMapper>();

            return new HealthController(_mockLogger, _mockService, _mockMapper);
        }
    }
}