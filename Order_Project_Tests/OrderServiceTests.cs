using Moq;
using Order_Project.Models;
using Order_Project.Services;
using Order_Project.Services.Intefraces;

namespace Order_Project_Tests
{
    public class OrderServiceTests
    {
        private readonly Mock<IInventoryService> _inventoryMock;
        private readonly Mock<IPaymentService> _paymentMock;
        private readonly Mock<INotificationService> _notificationMock;
        private readonly OrderService _service;

        public OrderServiceTests()
        {
            _inventoryMock = new Mock<IInventoryService>();
            _paymentMock = new Mock<IPaymentService>();
            _notificationMock = new Mock<INotificationService>();

            _service = new OrderService(_inventoryMock.Object, _paymentMock.Object, _notificationMock.Object);
        }

        /// <summary>                                                                                                                                                                          
        /// [Theory] / [InlineData] — Параметризований тест для перевірки викидання ArgumentException при невалідних назвах товару.                                                            
        /// </summary>                                                                                                                                                                         
        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void CreateOrder_InvalidProduct_ThrowsArgumentException(string invalidProduct)
        {
            // Act & Assert                                                                                                                                                                    
            Assert.Throws<ArgumentException>(() => _service.CreateOrder(invalidProduct, 5));
        }

        /// <summary>                                                                                                                                                                          
        /// Assert.Equal — Перевірка точного збігу атрибутів (Product та Quantity) у створеному замовленні.                                                                                    
        /// </summary>                                                                                                                                                                         
        [Fact]
        public void CreateOrder_ValidInput_ReturnsOrderWithEqualProductAndQuantity()
        {
            // Arrange                                                                                                                                                                         
            _inventoryMock.Setup(i => i.CheckStock("Laptop", 2)).Returns(true);
            _paymentMock.Setup(p => p.ProcessPayment(It.IsAny<Order>())).Returns(true);

            // Act                                                                                                                                                                             
            var order = _service.CreateOrder("Laptop", 2);

            // Assert                                                                                                                                                                          
            Assert.Equal("Laptop", order.Product);
            Assert.Equal(2, order.Quantity);
        }

        /// <summary>                                                                                                                                                                          
        /// Assert.NotEqual — Перевірка того, що згенерований ідентифікатор замовлення не дорівнює 0.                                                                                          
        /// </summary>                                                                                                                                                                         
        [Fact]
        public void CreateOrder_ValidOrder_IdIsNotEqualToZero()
        {
            // Arrange                                                                                                                                                                         
            _inventoryMock.Setup(i => i.CheckStock("Phone", 1)).Returns(true);
            _paymentMock.Setup(p => p.ProcessPayment(It.IsAny<Order>())).Returns(true);

            // Act                                                                                                                                                                             
            var order = _service.CreateOrder("Phone", 1);

            // Assert                                                                                                                                                                          
            Assert.NotEqual(0, order.Id);
        }

        /// <summary>                                                                                                                                                                          
        /// Assert.True — Перевірка булевого значення true при успішному оновленні кількості товару що існує в замовленні.                                                                     
        /// </summary>                                                                                                                                                                         
        [Fact]
        public void UpdateOrder_ExistingOrderAndValidQuantity_ReturnsTrue()
        {
            // Arrange                                                                                                                                                                         
            _inventoryMock.Setup(i => i.CheckStock("Tablet", 1)).Returns(true);
            _paymentMock.Setup(p => p.ProcessPayment(It.IsAny<Order>())).Returns(true);
            var createdOrder = _service.CreateOrder("Tablet", 1);

            // Act                                                                                                                                                                             
            var result = _service.UpdateOrder(createdOrder.Id, 5);

            // Assert                                                                                                                                                                          
            Assert.True(result);
        }

        /// <summary>                                                                                                                                                                          
        /// Assert.False — Перевірка булевого значення false при спробі оновити замовлення, яке не існує.                                                                                      
        /// </summary>                                                                                                                                                                         
        [Fact]
        public void UpdateOrder_NonExistingOrder_ReturnsFalse()
        {
            // Act                                                                                                                                                                             
            var result = _service.UpdateOrder(999, 10);

            // Assert                                                                                                                                                                          
            Assert.False(result);
        }

        /// <summary>                                                                                                                                                                          
        /// Assert.Null / Assert.NotNull — Перевірка наявності об'єкта в списку замовлень (NotNull для знайденого, Null для відсутнього).                                                      
        /// </summary>                                                                                                                                                                         
        [Fact]
        public void GetOrders_CheckNullabilityOfReturnedObjects()
        {
            // Arrange                                                                                                                                                                         
            _inventoryMock.Setup(i => i.CheckStock("Monitor", 1)).Returns(true);
            _paymentMock.Setup(p => p.ProcessPayment(It.IsAny<Order>())).Returns(true);
            var createdOrder = _service.CreateOrder("Monitor", 1);

            // Act                                                                                                                                                                             
            var foundOrder = _service.GetOrders().Find(o => o.Id == createdOrder.Id);
            var notFoundOrder = _service.GetOrders().Find(o => o.Id == 999);

            // Assert                                                                                                                                                                          
            Assert.NotNull(foundOrder);
            Assert.Null(notFoundOrder);
        }

        /// <summary>                                                                                                                                                                          
        /// Assert.Contains — Перевірка наявності конкретного елемента у списку замовлень.                                                                                                     
        /// </summary>                                                                                                                                                                         
        [Fact]
        public void GetOrders_ContainsCreatedOrder()
        {
            // Arrange                                                                                                                                                                         
            _inventoryMock.Setup(i => i.CheckStock("Mouse", 1)).Returns(true);
            _paymentMock.Setup(p => p.ProcessPayment(It.IsAny<Order>())).Returns(true);
            var order = _service.CreateOrder("Mouse", 1);

            // Act                                                                                                                                                                             
            var ordersList = _service.GetOrders();

            // Assert                                                                                                                                                                          
            Assert.Contains(order, ordersList);
        }

        /// <summary>                                                                                                                                                                          
        /// Assert.Empty / Assert.NotEmpty — Перевірка порожності колекції замовлень до створення та непорожності після створення.                                                             
        /// </summary>                                                                                                                                                                         
        [Fact]
        public void GetOrders_InitialStateIsEmpty_AfterCreateIsNotEmpty()
        {
            // Assert 1                                                                                                                                                                        
            Assert.Empty(_service.GetOrders());

            // Arrange                                                                                                                                                                         
            _inventoryMock.Setup(i => i.CheckStock("Keyboard", 1)).Returns(true);
            _paymentMock.Setup(p => p.ProcessPayment(It.IsAny<Order>())).Returns(true);

            // Act                                                                                                                                                                             
            _service.CreateOrder("Keyboard", 1);

            // Assert 2                                                                                                                                                                        
            Assert.NotEmpty(_service.GetOrders());
        }

        /// <summary>                                                                                                                                                                          
        /// Assert.Throws — Перевірка виклику винятку InvalidOperationException при недостатній кількості товару на складі.                                                                    
        /// </summary>                                                                                                                                                                         
        [Fact]
        public void CreateOrder_NotEnoughStock_ThrowsInvalidOperationException()
        {
            // Arrange                                                                                                                                                                         
            _inventoryMock.Setup(i => i.CheckStock("GPU", 10)).Returns(false);

            // Act & Assert                                                                                                                                                                    
            var exception = Assert.Throws<InvalidOperationException>(() => _service.CreateOrder("GPU", 10));
            Assert.Equal("Not enough stock.", exception.Message);
        }

        /// <summary>                                                                                                                                                                          
        /// Verify() — Перевірка виклику методу зменшення складського запасу ReduceStock мок-об'єкта.                                                                                          
        /// </summary>                                                                                                                                                                         
        [Fact]
        public void CreateOrder_ValidOrder_CallsReduceStock()
        {
            // Arrange                                                                                                                                                                         
            _inventoryMock.Setup(i => i.CheckStock("RAM", 2)).Returns(true);
            _paymentMock.Setup(p => p.ProcessPayment(It.IsAny<Order>())).Returns(true);

            // Act                                                                                                                                                                             
            _service.CreateOrder("RAM", 2);

            // Assert                                                                                                                                                                          
            _inventoryMock.Verify(i => i.ReduceStock("RAM", 2));
        }

        /// <summary>                                                                                                                                                                          
        /// Verify(..., Times.Never) — Переконання, що метод проведення оплати НІКОЛИ не викликається при відсутності товару на складі.                                                        
        /// </summary>                                                                                                                                                                         
        [Fact]
        public void CreateOrder_OutOfStock_NeverCallsProcessPayment()
        {
            // Arrange                                                                                                                                                                         
            _inventoryMock.Setup(i => i.CheckStock("CPU", 1)).Returns(false);

            // Act & Assert                                                                                                                                                                    
            Assert.Throws<InvalidOperationException>(() => _service.CreateOrder("CPU", 1));
            _paymentMock.Verify(p => p.ProcessPayment(It.IsAny<Order>()), Times.Never);
        }

        /// <summary>                                                                                                                                                                          
        /// Verify(..., Times.Exactly(n)) — Перевірка того, що при видаленні замовлення повернення товару на склад викликається рівно 1 раз.                                                   
        /// </summary>                                                                                                                                                                         
        [Fact]
        public void RemoveOrder_ExistingOrder_CallsIncreaseStockExactlyOnce()
        {
            // Arrange                                                                                                                                                                         
            _inventoryMock.Setup(i => i.CheckStock("SSD", 1)).Returns(true);
            _paymentMock.Setup(p => p.ProcessPayment(It.IsAny<Order>())).Returns(true);
            var order = _service.CreateOrder("SSD", 1);

            // Act                                                                                                                                                                             
            _service.RemoveOrder(order.Id);

            // Assert                                                                                                                                                                          
            _inventoryMock.Verify(i => i.IncreaseStock("SSD", 1), Times.Exactly(1));
        }

        /// <summary>                                                                                                                                                                          
        /// Verify(..., Times.AtLeastOnce) — Перевірка того, що надсилання підтвердження викликалося щонайменше один раз при успішному замовленні.                                             
        /// </summary>                                                                                                                                                                         
        [Fact]
        public void CreateOrder_SuccessfulOrder_CallsSendConfirmationAtLeastOnce()
        {
            // Arrange                                                                                                                                                                         
            _inventoryMock.Setup(i => i.CheckStock("Webcam", 1)).Returns(true);
            _paymentMock.Setup(p => p.ProcessPayment(It.IsAny<Order>())).Returns(true);

            // Act                                                                                                                                                                             
            _service.CreateOrder("Webcam", 1);

            // Assert                                                                                                                                                                          
            _notificationMock.Verify(n => n.SendConfirmation(It.IsAny<Order>()), Times.AtLeastOnce);
        }

        /// <summary>                                                                                                                                                                          
        /// It.Is(predicate) — Перевірка аргументу з предикатом (перевіряє, що в ProcessPayment передається замовлення з точними параметрами).                                                 
        /// </summary>                                                                                                                                                                         
        [Fact]
        public void CreateOrder_ValidOrder_PassesCorrectOrderToPaymentService()
        {
            // Arrange                                                                                                                                                                         
            _inventoryMock.Setup(i => i.CheckStock("Headphones", 3)).Returns(true);
            _paymentMock.Setup(p => p.ProcessPayment(It.IsAny<Order>())).Returns(true);

            // Act                                                                                                                                                                             
            _service.CreateOrder("Headphones", 3);

            // Assert                                                                                                                                                                          
            _paymentMock.Verify(p => p.ProcessPayment(It.Is<Order>(o => o.Product == "Headphones" && o.Quantity == 3)),
                Times.Once);
        }

        /// <summary>                                                                                                                                                                          
        /// It.IsAny() — Перевірка виклику методу повернення товару IncreaseStock з будь-якими параметрами аргументів у разі невдалої оплати.                                                  
        /// </summary>                                                                                                                                                                         
        [Fact]
        public void CreateOrder_PaymentFailed_CallsIncreaseStockWithAnyOrderParams()
        {
            // Arrange                                                                                                                                                                         
            _inventoryMock.Setup(i => i.CheckStock("Mic", 1)).Returns(true);
            _paymentMock.Setup(p => p.ProcessPayment(It.IsAny<Order>())).Returns(false);

            // Act & Assert                                                                                                                                                                    
            Assert.Throws<InvalidOperationException>(() => _service.CreateOrder("Mic", 1));
            _inventoryMock.Verify(i => i.IncreaseStock(It.IsAny<string>(), It.IsAny<int>()), Times.Once);
        }
    }
}