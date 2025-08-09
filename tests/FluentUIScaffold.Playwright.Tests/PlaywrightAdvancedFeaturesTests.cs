// Copyright (c) FluentUIScaffold. All rights reserved.
using System;
using System.Threading.Tasks;

using Microsoft.Playwright;

using Moq;

using NUnit.Framework;

namespace FluentUIScaffold.Playwright.Tests;

/// <summary>
/// Unit tests for the PlaywrightAdvancedFeatures class.
/// </summary>
[TestFixture]
public class PlaywrightAdvancedFeaturesTests
{
    [Test]
    public void Constructor_WithValidPage_ShouldCreateFeatures()
    {
        // Arrange
        var page = CreateMockPage();

        // Act
        var features = new PlaywrightAdvancedFeatures(page);

        // Assert
        Assert.That(features, Is.Not.Null);
    }

    [Test]
    public void Constructor_WithNullPage_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new PlaywrightAdvancedFeatures(null!));
        Assert.That(exception.ParamName, Is.EqualTo("page"));
    }

    [Test]
    public void InterceptNetworkRequests_WithNullUrlPattern_ShouldThrowArgumentException()
    {
        // Arrange
        var page = CreateMockPage();
        var features = new PlaywrightAdvancedFeatures(page);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => features.InterceptNetworkRequests(null!, response => { }));
        Assert.That(exception.ParamName, Is.EqualTo("urlPattern"));
    }

    [Test]
    public void InterceptNetworkRequests_WithEmptyUrlPattern_ShouldThrowArgumentException()
    {
        // Arrange
        var page = CreateMockPage();
        var features = new PlaywrightAdvancedFeatures(page);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => features.InterceptNetworkRequests(string.Empty, response => { }));
        Assert.That(exception.ParamName, Is.EqualTo("urlPattern"));
    }

    [Test]
    public void InterceptNetworkRequests_WithNullHandler_ShouldThrowArgumentNullException()
    {
        // Arrange
        var page = CreateMockPage();
        var features = new PlaywrightAdvancedFeatures(page);

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => features.InterceptNetworkRequests("pattern", null!));
        Assert.That(exception.ParamName, Is.EqualTo("handler"));
    }

    [Test]
    public void TakeScreenshotAsync_WithValidParameters_ShouldReturnByteArray()
    {
        // Arrange
        var page = CreateMockPage();
        var features = new PlaywrightAdvancedFeatures(page);

        // Act
        var screenshot = features.TakeScreenshotAsync().Result;

        // Assert
        Assert.That(screenshot, Is.Not.Null);
        Assert.That(screenshot, Is.InstanceOf<byte[]>());
    }

    [Test]
    public void TakeElementScreenshotAsync_WithNullSelector_ShouldThrowArgumentException()
    {
        // Arrange
        var page = CreateMockPage();
        var features = new PlaywrightAdvancedFeatures(page);

        // Act & Assert
        var exception = Assert.ThrowsAsync<ArgumentException>(async () => await features.TakeElementScreenshotAsync(null!));
        Assert.That(exception.ParamName, Is.EqualTo("selector"));
    }

    [Test]
    public void TakeElementScreenshotAsync_WithEmptySelector_ShouldThrowArgumentException()
    {
        // Arrange
        var page = CreateMockPage();
        var features = new PlaywrightAdvancedFeatures(page);

        // Act & Assert
        var exception = Assert.ThrowsAsync<ArgumentException>(async () => await features.TakeElementScreenshotAsync(string.Empty));
        Assert.That(exception.ParamName, Is.EqualTo("selector"));
    }

    [Test]
    public void TakeElementScreenshotAsync_WithValidSelector_ShouldReturnByteArray()
    {
        // Arrange
        var page = CreateMockPage();
        var features = new PlaywrightAdvancedFeatures(page);

        // Act
        var screenshot = features.TakeElementScreenshotAsync("selector").Result;

        // Assert
        Assert.That(screenshot, Is.Not.Null);
        Assert.That(screenshot, Is.InstanceOf<byte[]>());
    }

    [Test]
    public void GeneratePdfAsync_WithValidParameters_ShouldReturnByteArray()
    {
        // Arrange
        var page = CreateMockPage();
        var features = new PlaywrightAdvancedFeatures(page);

        // Act
        var pdf = features.GeneratePdfAsync().Result;

        // Assert
        Assert.That(pdf, Is.Not.Null);
        Assert.That(pdf, Is.InstanceOf<byte[]>());
    }

    [Test]
    public void StartVideoRecordingAsync_WithNullPath_ShouldThrowArgumentException()
    {
        // Arrange
        var page = CreateMockPage();
        var features = new PlaywrightAdvancedFeatures(page);

        // Act & Assert
        var exception = Assert.ThrowsAsync<ArgumentException>(async () => await features.StartVideoRecordingAsync(null!));
        Assert.That(exception.ParamName, Is.EqualTo("path"));
    }

    [Test]
    public void StartVideoRecordingAsync_WithEmptyPath_ShouldThrowArgumentException()
    {
        // Arrange
        var page = CreateMockPage();
        var features = new PlaywrightAdvancedFeatures(page);

        // Act & Assert
        var exception = Assert.ThrowsAsync<ArgumentException>(async () => await features.StartVideoRecordingAsync(string.Empty));
        Assert.That(exception.ParamName, Is.EqualTo("path"));
    }

    [Test]
    public void StartVideoRecordingAsync_WithValidPath_ShouldNotThrowException()
    {
        // Arrange
        var page = CreateMockPage();
        var features = new PlaywrightAdvancedFeatures(page);

        // Act & Assert
        Assert.DoesNotThrowAsync(async () => await features.StartVideoRecordingAsync("test.mp4"));
    }

    [Test]
    public void StopVideoRecordingAsync_ShouldNotThrowException()
    {
        // Arrange
        var page = CreateMockPage();
        var features = new PlaywrightAdvancedFeatures(page);

        // Act & Assert
        Assert.DoesNotThrowAsync(async () => await features.StopVideoRecordingAsync());
    }

    [Test]
    public void EmulateMobileDeviceAsync_WithNullDeviceName_ShouldThrowArgumentException()
    {
        // Arrange
        var page = CreateMockPage();
        var features = new PlaywrightAdvancedFeatures(page);

        // Act & Assert
        var exception = Assert.ThrowsAsync<ArgumentException>(async () => await features.EmulateMobileDeviceAsync(null!));
        Assert.That(exception.ParamName, Is.EqualTo("deviceName"));
    }

    [Test]
    public void EmulateMobileDeviceAsync_WithEmptyDeviceName_ShouldThrowArgumentException()
    {
        // Arrange
        var page = CreateMockPage();
        var features = new PlaywrightAdvancedFeatures(page);

        // Act & Assert
        var exception = Assert.ThrowsAsync<ArgumentException>(async () => await features.EmulateMobileDeviceAsync(string.Empty));
        Assert.That(exception.ParamName, Is.EqualTo("deviceName"));
    }

    [Test]
    public void EmulateMobileDeviceAsync_WithValidDeviceName_ShouldNotThrowException()
    {
        // Arrange
        var page = CreateMockPage();
        var features = new PlaywrightAdvancedFeatures(page);

        // Act & Assert
        Assert.DoesNotThrowAsync(async () => await features.EmulateMobileDeviceAsync("iPhone 12"));
    }

    [Test]
    public void SetGeolocationAsync_WithValidCoordinates_ShouldNotThrowException()
    {
        // Arrange
        var page = CreateMockPage();
        var features = new PlaywrightAdvancedFeatures(page);

        // Act & Assert
        Assert.DoesNotThrowAsync(async () => await features.SetGeolocationAsync(40.7128, -74.0060));
    }

    [Test]
    public void SetPermissionsAsync_WithNullPermissions_ShouldThrowArgumentException()
    {
        // Arrange
        var page = CreateMockPage();
        var features = new PlaywrightAdvancedFeatures(page);

        // Act & Assert
        var exception = Assert.ThrowsAsync<ArgumentException>(async () => await features.SetPermissionsAsync(null!));
        Assert.That(exception.ParamName, Is.EqualTo("permissions"));
    }

    [Test]
    public void SetPermissionsAsync_WithEmptyPermissions_ShouldThrowArgumentException()
    {
        // Arrange
        var page = CreateMockPage();
        var features = new PlaywrightAdvancedFeatures(page);

        // Act & Assert
        var exception = Assert.ThrowsAsync<ArgumentException>(async () => await features.SetPermissionsAsync());
        Assert.That(exception.ParamName, Is.EqualTo("permissions"));
    }

    [Test]
    public void SetPermissionsAsync_WithValidPermissions_ShouldNotThrowException()
    {
        // Arrange
        var page = CreateMockPage();
        var features = new PlaywrightAdvancedFeatures(page);

        // Act & Assert
        Assert.DoesNotThrowAsync(async () => await features.SetPermissionsAsync("geolocation", "notifications"));
    }

    [Test]
    public void AddInitScriptAsync_WithNullScript_ShouldThrowArgumentException()
    {
        // Arrange
        var page = CreateMockPage();
        var features = new PlaywrightAdvancedFeatures(page);

        // Act & Assert
        var exception = Assert.ThrowsAsync<ArgumentException>(async () => await features.AddInitScriptAsync(null!));
        Assert.That(exception.ParamName, Is.EqualTo("script"));
    }

    [Test]
    public void AddInitScriptAsync_WithEmptyScript_ShouldThrowArgumentException()
    {
        // Arrange
        var page = CreateMockPage();
        var features = new PlaywrightAdvancedFeatures(page);

        // Act & Assert
        var exception = Assert.ThrowsAsync<ArgumentException>(async () => await features.AddInitScriptAsync(string.Empty));
        Assert.That(exception.ParamName, Is.EqualTo("script"));
    }

    [Test]
    public void AddInitScriptAsync_WithValidScript_ShouldNotThrowException()
    {
        // Arrange
        var page = CreateMockPage();
        var features = new PlaywrightAdvancedFeatures(page);

        // Act & Assert
        Assert.DoesNotThrowAsync(async () => await features.AddInitScriptAsync("console.log('test');"));
    }

    [Test]
    public void EvaluateJavaScriptAsync_WithNullScript_ShouldThrowArgumentException()
    {
        // Arrange
        var page = CreateMockPage();
        var features = new PlaywrightAdvancedFeatures(page);

        // Act & Assert
        var exception = Assert.ThrowsAsync<ArgumentException>(async () => await features.EvaluateJavaScriptAsync(null!));
        Assert.That(exception.ParamName, Is.EqualTo("script"));
    }

    [Test]
    public void EvaluateJavaScriptAsync_WithEmptyScript_ShouldThrowArgumentException()
    {
        // Arrange
        var page = CreateMockPage();
        var features = new PlaywrightAdvancedFeatures(page);

        // Act & Assert
        var exception = Assert.ThrowsAsync<ArgumentException>(async () => await features.EvaluateJavaScriptAsync(string.Empty));
        Assert.That(exception.ParamName, Is.EqualTo("script"));
    }

    [Test]
    public void EvaluateJavaScriptAsync_WithValidScript_ShouldReturnResult()
    {
        // Arrange
        var page = CreateMockPage();
        var features = new PlaywrightAdvancedFeatures(page);

        // Act
        var result = features.EvaluateJavaScriptAsync("1 + 1").Result;

        // Assert
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void WaitForNetworkRequestAsync_WithNullUrlPattern_ShouldThrowArgumentException()
    {
        // Arrange
        var page = CreateMockPage();
        var features = new PlaywrightAdvancedFeatures(page);

        // Act & Assert
        var exception = Assert.ThrowsAsync<ArgumentException>(async () => await features.WaitForNetworkRequestAsync(null!, TimeSpan.FromSeconds(1)));
        Assert.That(exception.ParamName, Is.EqualTo("urlPattern"));
    }

    [Test]
    public void WaitForNetworkRequestAsync_WithEmptyUrlPattern_ShouldThrowArgumentException()
    {
        // Arrange
        var page = CreateMockPage();
        var features = new PlaywrightAdvancedFeatures(page);

        // Act & Assert
        var exception = Assert.ThrowsAsync<ArgumentException>(async () => await features.WaitForNetworkRequestAsync(string.Empty, TimeSpan.FromSeconds(1)));
        Assert.That(exception.ParamName, Is.EqualTo("urlPattern"));
    }

    [Test]
    public void WaitForNetworkRequestInitiatedAsync_WithNullUrlPattern_ShouldThrowArgumentException()
    {
        // Arrange
        var page = CreateMockPage();
        var features = new PlaywrightAdvancedFeatures(page);

        // Act & Assert
        var exception = Assert.ThrowsAsync<ArgumentException>(async () => await features.WaitForNetworkRequestInitiatedAsync(null!, TimeSpan.FromSeconds(1)));
        Assert.That(exception.ParamName, Is.EqualTo("urlPattern"));
    }

    [Test]
    public void WaitForNetworkRequestInitiatedAsync_WithEmptyUrlPattern_ShouldThrowArgumentException()
    {
        // Arrange
        var page = CreateMockPage();
        var features = new PlaywrightAdvancedFeatures(page);

        // Act & Assert
        var exception = Assert.ThrowsAsync<ArgumentException>(async () => await features.WaitForNetworkRequestInitiatedAsync(string.Empty, TimeSpan.FromSeconds(1)));
        Assert.That(exception.ParamName, Is.EqualTo("urlPattern"));
    }

    private static IPage CreateMockPage()
    {
        var mock = new Mock<IPage>();
        var locatorMock = new Mock<ILocator>();
        var contextMock = new Mock<IBrowserContext>();

        // Setup the Locator method
        mock.Setup(p => p.Locator(It.IsAny<string>(), It.IsAny<PageLocatorOptions>())).Returns(locatorMock.Object);
        // Setup ScreenshotAsync for page and locator
        mock.Setup(p => p.ScreenshotAsync(It.IsAny<PageScreenshotOptions>())).ReturnsAsync(new byte[1]);
        locatorMock.Setup(l => l.ScreenshotAsync(It.IsAny<LocatorScreenshotOptions>())).ReturnsAsync(new byte[1]);
        // Setup PdfAsync
        mock.Setup(p => p.PdfAsync(It.IsAny<PagePdfOptions>())).ReturnsAsync(new byte[1]);
        // Setup EvaluateAsync for object and JsonElement return types
        mock.Setup(p => p.EvaluateAsync<object?>(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync((object?)1);
        mock.Setup(p => p.EvaluateAsync(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(System.Text.Json.JsonDocument.Parse("null").RootElement);
        // Setup AddInitScriptAsync
        mock.Setup(p => p.AddInitScriptAsync(It.IsAny<string>(), null)).Returns(Task.CompletedTask);
        // Setup WaitForResponseAsync
        mock.Setup(p => p.WaitForResponseAsync(It.IsAny<string>(), It.IsAny<PageWaitForResponseOptions>())).ReturnsAsync(new Mock<IResponse>().Object);
        // Setup WaitForRequestAsync
        mock.Setup(p => p.WaitForRequestAsync(It.IsAny<string>(), It.IsAny<PageWaitForRequestOptions>())).ReturnsAsync(new Mock<IRequest>().Object);
        // Setup Context property and its methods
        mock.Setup(p => p.Context).Returns(contextMock.Object);
        contextMock.Setup(c => c.SetGeolocationAsync(It.IsAny<Geolocation>())).Returns(Task.CompletedTask);
        contextMock.Setup(c => c.GrantPermissionsAsync(It.IsAny<string[]>(), null)).Returns(Task.CompletedTask);
        return mock.Object;
    }
}
