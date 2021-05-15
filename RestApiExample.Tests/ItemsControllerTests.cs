using System;
using Moq;
using Xunit;
using RestApiExample.Repositories;
using RestApiExample.Entities;
using Microsoft.Extensions.Logging;
using RestApiExample.Controllers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FluentAssertions;
using RestApiExample.Extensions;
using RestApiExample.Dtos;

namespace RestApiExample.Tests
{
    public class UnitTest1
    {

        private readonly Mock<IItemsRepository> repositoryStub = new();
        private readonly Mock<ILogger<ItemsController>> loggerStub = new();
        private readonly Random random = new();

        [Fact]
        public async Task GetItemAsync_WithUnexistingItem_ReturnsNotFound()
        {
            //Arrange
            repositoryStub.Setup(repo => repo.GetItemAsync(It.IsAny<Guid>()))
                          .ReturnsAsync((Item)null);


            var controller = new ItemsController(repository: repositoryStub.Object, logger: loggerStub.Object);

            //Act
            var result = await controller.GetItemAsync(Guid.NewGuid());

            //Assert
            result.Result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task GetItemAsync_WithExistingItem_ReturnsExpectedItem()
        {
            //Arrange
            var expectedItem = CreateRandomItem();

            repositoryStub.Setup(repo => repo.GetItemAsync(It.IsAny<Guid>()))
                          .ReturnsAsync(expectedItem);

            var loggerStub = new Mock<ILogger<ItemsController>>();

            var controller = new ItemsController(repository: repositoryStub.Object, logger: loggerStub.Object);

            //Act
            var result = await controller.GetItemAsync(Guid.NewGuid());

            //Assert
            var itemResult = (result.Result as OkObjectResult).Value as ItemDto;
            itemResult.Should().NotBeNull();
            itemResult.Should().BeEquivalentTo(expectedItem, options => options.ComparingByMembers<Item>());

        }

        [Fact]
        public async Task GetItemsAsync_WithExistingItems_ReturnsAllItems()
        {
            //Arrange
            var expectedItems = new[] { CreateRandomItem(), CreateRandomItem(), CreateRandomItem() };

            repositoryStub.Setup(repo => repo.GetItemsAsync())
                          .ReturnsAsync(expectedItems);

            var loggerStub = new Mock<ILogger<ItemsController>>();

            var controller = new ItemsController(repository: repositoryStub.Object, logger: loggerStub.Object);

            //Act
            var result = await controller.GetItemsAsync();


            //Assert
            //var itemResult = (result.Result as OkObjectResult).Value as ItemDto[];
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedItems, options => options.ComparingByMembers<Item>());

        }

        [Fact]
        public async Task CreateItemAsync_WithItemToCreate_ReturnsCreatedItem()
        {
            //Arrange
            var itemToCreate = new CreateItemDto(
            
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                random.Next(100)
            );

            var loggerStub = new Mock<ILogger<ItemsController>>();

            var controller = new ItemsController(repository: repositoryStub.Object, logger: loggerStub.Object);

            //Act
            var result = await controller.CreateItemAsync(itemToCreate);

            //Assert
            var itemResult = (result.Result as CreatedAtActionResult).Value as ItemDto;
            itemResult.Should().NotBeNull();
            itemToCreate.Should().BeEquivalentTo(itemResult,
                options => options.ComparingByMembers<ItemDto>().ExcludingMissingMembers());
            itemResult.CreatedDate.Should().BeCloseTo(DateTimeOffset.UtcNow, 1000);
        }

        [Fact]
        public async Task UpdateItemAsync_WithExistingItem_ReturnsNoContent()
        {
            //Arrange
            var existingItem = CreateRandomItem();

            var updateItem = new UpdateItemDto(
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                random.Next(100)
            );


            repositoryStub.Setup(repo => repo.GetItemAsync(It.IsAny<Guid>()))
                          .ReturnsAsync(existingItem);

            var controller = new ItemsController(repository: repositoryStub.Object, logger: loggerStub.Object);

            //Act
            var result = await controller.UpdateItemAsync(existingItem.Id, updateItem);

            //Assert
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task DeleteItemAsync_WithExistingItem_ReturnsNoContent()
        {
            //Arrange
            var existingItem = CreateRandomItem();

            repositoryStub.Setup(repo => repo.GetItemAsync(It.IsAny<Guid>()))
                          .ReturnsAsync(existingItem);

            var controller = new ItemsController(repository: repositoryStub.Object, logger: loggerStub.Object);

            //Act
            var result = await controller.DeleteItemAsync(existingItem.Id);

            //Assert
            result.Should().BeOfType<NoContentResult>();
        }

        private Item CreateRandomItem()
        {
            return new()
            {
                Id = Guid.NewGuid(),
                Name = Guid.NewGuid().ToString(),
                Price = random.Next(100),
                CreatedDate = DateTimeOffset.UtcNow
            };
        }
    }
}
