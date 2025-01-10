using Microsoft.AspNetCore.Mvc;
using Moq;
using ProjectForUnitTest.Web.Controllers;
using ProjectForUnitTest.Web.Models;
using ProjectForUnitTest.Web.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectForUnitTest.Test
{
    public class ProductApiControllerTest
    {
        private readonly Mock<IRepository<Product>> _mockRepo;
        private readonly ProductsController _controller;

        private List<Product> products;

        public ProductApiControllerTest()
        {
            _mockRepo = new Mock<IRepository<Product>>();
            _controller = new ProductsController(_mockRepo.Object);
            products = new List<Product>() {
            new Product
            {
                Id = 1,
                Name = "Kalem",
                Color ="Kırmızı",
                Price = 10
            },
            new Product
            {
                Id = 2,
                Name = "Silgi",
                Color ="Beyaz",
                Price = 5
            },
            new Product
            {
                Id = 3,
                Name = "Defter",
                Color ="Mavi",
                Price = 20
            } };
        }

        [Fact]
        public async void GetProducts_ActionExecute_ReturnOkResultWithProducts()
        {
            _mockRepo.Setup(repo => repo.GetAll()).ReturnsAsync(products);

            var result = await _controller.GetProducts();
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnProduct = Assert.IsAssignableFrom<IEnumerable<Product>>(okResult.Value);

            Assert.Equal(3, returnProduct.ToList().Count());
        }

        [Theory]
        [InlineData(0)]
        public async void GetProduct_InValidId_ReturnNotFound(int productId)
        {
            Product product = null;
            _mockRepo.Setup(x => x.GetById(productId)).ReturnsAsync(product);
            var result = await _controller.GetProduct(productId);
            Assert.IsType<NotFoundResult>(result);
        }

        [Theory]
        [InlineData(1)]
        public async void GetProduct_ValidId_ReturnOkWithProduct(int productId)
        {
            _mockRepo.Setup(x => x.GetById(productId)).ReturnsAsync(products.First(x => x.Id == productId));
            var result = await _controller.GetProduct(productId);
            var okResult = Assert.IsType<OkObjectResult>(result);
            var resultProduct = Assert.IsType<Product>(okResult.Value);
            Assert.Equal(productId, resultProduct.Id);
        }

        [Theory]
        [InlineData(1)]
        public void PutProduct_IdIsNotEqualProductId_ReturnBadRequest(int productId)
        {
            var product = products.First(x => x.Id == productId);
            var result = _controller.PutProduct(2, product);
            Assert.IsType<BadRequestResult>(result);
        }

        [Theory]
        [InlineData(1)]
        public void PutProduct_ActionExecute_ReturnNoContent(int productId)
        {
            var product = products.First(x=>x.Id== productId);
            _mockRepo.Setup(x => x.Update(product));
            var result = _controller.PutProduct(productId, product);
            _mockRepo.Verify(x => x.Update(product), Times.Once);
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async void PostProduct_ActionExecute_ReturnCreatedAtAction()
        {
            Product product = products.First();
            _mockRepo.Setup(x => x.Create(product)).Returns(Task.CompletedTask);
            var result = await _controller.PostProduct(product);
            var createdAtAction = Assert.IsType<CreatedAtActionResult>(result);
            _mockRepo.Verify(x => x.Create(product), Times.Once);
            Assert.Equal("GetProduct", createdAtAction.ActionName);

        }

        [Theory]
        [InlineData(0)]
        public async void DeleteProduct_InValidId_ReturnNotFound(int productId)
        {
            Product product = null;
            _mockRepo.Setup(x => x.GetById(productId)).ReturnsAsync(product);
            var result = await _controller.DeleteProduct(productId);
            Assert.IsType<NotFoundResult>(result);
        }
        [Theory]
        [InlineData(1)]
        public async void DeleteProduct_ActionExecute_ReturnNoContent(int productId)
        {

            var product = products.First(x=>x.Id==productId);
            _mockRepo.Setup(x => x.GetById(productId)).ReturnsAsync(product);
            _mockRepo.Setup(x => x.Delete(product));
            var result = await _controller.DeleteProduct(productId);
            _mockRepo.Verify(x => x.Delete(product), Times.Once);
            Assert.IsType<NoContentResult>(result);
        }

    }
}
