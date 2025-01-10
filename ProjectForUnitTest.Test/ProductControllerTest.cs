using Microsoft.AspNetCore.Mvc;
using Moq;
using ProjectForUnitTest.Web.Controllers;
using ProjectForUnitTest.Web.Helpers;
using ProjectForUnitTest.Web.Models;
using ProjectForUnitTest.Web.Repositories;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ProjectForUnitTest.Test
{
    
    public class ProductControllerTest
    {
        private readonly Mock<IRepository<Product>> _mockRepo;
        private readonly ProductController _controller;

        private List<Product> products;

        public ProductControllerTest()
        {
            _mockRepo = new Mock<IRepository<Product>>();
            _controller = new ProductController(_mockRepo.Object);
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
        public async void Index_ActionExecutes_ReturnView()
        {
            var result = await _controller.Index();

            Assert.IsType<ViewResult>(result);
        }
        [Fact]
        public async void Index_ActionExecute_ReturnProductList()
        {
            _mockRepo.Setup(repo => repo.GetAll()).ReturnsAsync(products);
            var result = await _controller.Index();
            var viewResult = Assert.IsType<ViewResult>(result);
            var productList = Assert.IsAssignableFrom<IEnumerable<Product>>(viewResult.Model);
            Assert.Equal<int>(3, productList.Count());
        }
        [Fact]
        public async void Detail_IdInValid_ReturnNotFount()
        {
            Product product = null;
            _mockRepo.Setup(x=>x.GetById(0)).ReturnsAsync(product);
            var result = await _controller.Details(0);
            var redirect = Assert.IsType<NotFoundResult>(result);
            Assert.Equal<int>(404, redirect.StatusCode);
        }
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async void Detail_ValidId_ReturnProduct(int productId)
        {
            Product product = products.First(x => x.Id == productId);
            _mockRepo.Setup(repo => repo.GetById(productId)).ReturnsAsync(product);
            var result = await _controller.Details(productId);
            var viewResult = Assert.IsType<ViewResult>(result);
            var resultProduct = Assert.IsAssignableFrom<Product>(viewResult.Model);
            Assert.Equal(product.Id, resultProduct.Id);
            Assert.Equal(product.Name, resultProduct.Name);
        }

        [Fact]
        public void Create_ActionExecutes_ReturnView()
        {
            var result = _controller.Create();

            Assert.IsType<ViewResult>(result);
        }
        [Fact]
        public async void CreatePOST_InValidModelState_ReturnView()
        {
            _controller.ModelState.AddModelError("Name", "Name alanı gereklidir.");
            var result = await _controller.Create(products.First());
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<Product>(viewResult.Model);
        }
        [Fact]
        public async void CreatePOST_ValidModelState_ReturnRedirectToIndexAction()
        {
            var result = await _controller.Create(products.First());

            var redirect = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("Index", redirect.ActionName);
        }

        [Fact]
        public async void CreatePOST_ValidModelState_CreateMethodExecute()
        {
            Product newProduct = null;
            _mockRepo.Setup(repo => repo.Create(It.IsAny<Product>())).Callback<Product>(x => newProduct = x);

            var result = await _controller.Create(products.First());
            _mockRepo.Verify(repo => repo.Create(It.IsAny<Product>()), Times.Once);
            Assert.Equal(products.First().Id, newProduct.Id);

        }

        [Fact]
        public async void CreatePOST_InValidModelState_NeverCreateExecute()
        {
            _controller.ModelState.AddModelError("Name", "...");

            var result = await _controller.Create(products.First());
            _mockRepo.Verify(repo => repo.Create(It.IsAny<Product>()), Times.Never);
        }

        [Theory]
        [InlineData(0)]
        public async void Edit_InValidId_ReturnNotFound(int productId)
        {
            Product product = null;
            _mockRepo.Setup(x => x.GetById(productId)).ReturnsAsync(product);
            var result = await _controller.Edit(productId);

            var redirect = Assert.IsType<NotFoundResult>(result);
            Assert.Equal<int>(404, redirect.StatusCode);
        }

        [Theory]
        [InlineData(2)]
        public async void Edit_ActionExecute_ReturnProduct(int productId)
        {
            var product = products.First(x => x.Id == productId);
            _mockRepo.Setup(repot => repot.GetById(productId)).ReturnsAsync(product);
            var result = await _controller.Edit(productId);
            var viewResult = Assert.IsType<ViewResult>(result);
            var resultProduct = Assert.IsAssignableFrom<Product>(viewResult.Model);
            Assert.Equal(product.Id, resultProduct.Id);
            Assert.Equal(product.Name, resultProduct.Name);
        }

        [Theory]
        [InlineData(2)]
        public void EditPOST_IdIsNotEqualProduct_ReturnNotFound(int productId)
        {
            var result = _controller.Edit(1,products.First(x=>x.Id == productId));

            var redirect = Assert.IsType<NotFoundResult>(result);
            
        }

        [Theory]
        [InlineData(2)]
        public void EditPOST_InValidModelState_ReturnView(int productId)
        {
            _controller.ModelState.AddModelError("Name", "...");
            var result = _controller.Edit(productId, products.First(x => x.Id == productId));

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<Product>(viewResult.Model);
        }

        [Theory]
        [InlineData(2)]
        public void EditPOST_ValidModelState_ReturnRedirectToIndexAction(int productId)
        {
            var result = _controller.Edit(productId, products.First(x => x.Id == productId));
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }

        [Theory]
        [InlineData(2)]
        public void EditPOST_ValidModelState_UpdateMethodExecute(int productId)
        {
            Product product = products.First(x => x.Id==productId);
            _mockRepo.Setup(repo => repo.Update(product));
            _controller.Edit(productId, product);
            _mockRepo.Verify(repo => repo.Update(It.IsAny<Product>()), Times.Once);
        }

        [Theory]
        [InlineData(0)]
        public async void Delete_IdIsNotEqualProduct_ReturnNotFound(int productId)
        {
            Product product = null;
            _mockRepo.Setup(x => x.GetById(productId)).ReturnsAsync(product);
            var result = await _controller.Delete(productId);
            Assert.IsType<NotFoundResult>(result);
            
        }

        [Theory]
        [InlineData(1)]
        public async void Delete_ActionExecutes_ReturnProduct(int productId)
        {
            var product = products.First(x => x.Id == productId);
            _mockRepo.Setup(repo => repo.GetById(productId)).ReturnsAsync(product);
            var result = await _controller.Delete(productId);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsAssignableFrom<Product>(viewResult.Model);
        }

        [Theory]
        [InlineData(1)]
        public async void DeleteConfirmed_ActionExecutes_ReturnRedirectToAction(int productId)
        {
            var result = await _controller.DeleteConfirmed(productId);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }
        [Theory]
        [InlineData(1)]
        public async void DeleteConfirmed_ActionExecutes_DeleteMethodExecute(int productId)
        {
            var product = products.First(x=>x.Id==productId);
            _mockRepo.Setup(repo => repo.Delete(product));
            await _controller.DeleteConfirmed(productId);
            _mockRepo.Verify(repo => repo.Delete(It.IsAny<Product>()), Times.Once);
        }
    }
}
