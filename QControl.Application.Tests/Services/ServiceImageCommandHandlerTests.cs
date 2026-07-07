using BuildingBlock.Application.Abstraction.Media;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Qcontrol.Application.Features.ServiceImages.Command.UploadServiceLogoImage;
using QControl.Application.Tests.TestSupport;
using QControl.Domain.Entities;
using QControl.Domain.Enums;
using ServiceEntity = QControl.Domain.Entities.Service;

namespace QControl.Application.Tests.Services;

public sealed class ServiceImageCommandHandlerTests
{
    private static readonly string ValidRowVersion =
        Convert.ToBase64String(new byte[8]);

    [Fact]
    public async Task Upload_logo_image_creates_service_image_and_touches_service()
    {
        var service = EntityTestFactory.Service(1);
        var services = new List<ServiceEntity> { service };
        var images = new List<ServiceImage>();
        var serviceWriteRepository =
            new InMemoryWriteRepository<ServiceEntity>(services);
        var imageWriteRepository =
            new InMemoryWriteRepository<ServiceImage>(images);
        var unitOfWork = new TestUnitOfWork();
        var mediaService = new TestMediaService();
        var handler = new UploadServiceLogoImageCommandHandler(
            new InMemoryWriteReadRepository<ServiceEntity>(services),
            serviceWriteRepository,
            new InMemoryWriteReadRepository<ServiceImage>(images),
            imageWriteRepository,
            new TestConcurrencyTokenManager(),
            new TestCurrentUser(),
            mediaService,
            unitOfWork,
            new TestLogger<UploadServiceLogoImageCommandHandler>());

        var result = await handler.Handle(
            new UploadServiceLogoImageCommand
            {
                ServiceId = service.Id,
                Image = CreatePngFormFile(),
                RowVersion = ValidRowVersion
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(images);
        Assert.Equal(service.Id, images[0].ServiceId);
        Assert.Equal(ServiceImageType.Logo, images[0].ImageType);
        Assert.Equal(mediaService.SavedPath, images[0].ImagePath);
        Assert.Equal(1, imageWriteRepository.AddCallCount);
        Assert.Equal(1, serviceWriteRepository.UpdateCallCount);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    private static IFormFile CreatePngFormFile()
    {
        var bytes = new byte[]
        {
            0x89,
            0x50,
            0x4E,
            0x47,
            0x0D,
            0x0A,
            0x1A,
            0x0A,
            0x00
        };

        var stream = new MemoryStream(bytes);

        return new FormFile(
            stream,
            baseStreamOffset: 0,
            length: bytes.Length,
            name: "image",
            fileName: "logo.png")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/png"
        };
    }

    private sealed class TestMediaService : IMediaService
    {
        public string SavedPath { get; } =
            "Services/1/Logo/logo.png";

        public List<string> RemovedPaths { get; } = new();

        public Task<string> SaveAsync(
            IFormFile mediaFile,
            string folderName)
            => Task.FromResult(SavedPath);

        public Task<List<string>> SaveAsync(
            List<IFormFile> formFiles,
            string folderName)
            => Task.FromResult(formFiles
                .Select((_, index) => $"{folderName}/image-{index}.png")
                .ToList());

        public Task<Stream> GetStream(IFormFile formFile)
            => Task.FromResult(formFile.OpenReadStream());

        public void Remove(string filePath)
        {
            RemovedPaths.Add(filePath);
        }

        public void RemoveRange(IEnumerable<string> filePaths)
        {
            RemovedPaths.AddRange(filePaths);
        }

        public Task<string> SaveVideoAsync(
            IFormFile videoFile,
            string folderName)
            => Task.FromResult($"{folderName}/video.mp4");
    }

    private sealed class TestLogger<T>
        : ILogger<T>
    {
        public IDisposable BeginScope<TState>(TState state)
            where TState : notnull
            => NoopScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => false;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
        }

        private sealed class NoopScope : IDisposable
        {
            public static NoopScope Instance { get; } = new();

            public void Dispose()
            {
            }
        }
    }
}
