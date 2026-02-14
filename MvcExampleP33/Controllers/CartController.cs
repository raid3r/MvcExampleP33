using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcExampleP33.Migrations;
using MvcExampleP33.Models;
using MvcExampleP33.Models.Dto;
using MvcExampleP33.Models.Forms;
using MvcExampleP33.Services;
using MvcExampleP34.Models.LiqPay;
using System.Security.Claims;

namespace MvcExampleP33.Controllers;

[Authorize]
public class CartController(
    StoreContext context
    ) : Controller
{
    public async Task<User> GetCurrentUser()
    {
        var identityId = int.Parse(User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);
        var user = await context
            .Users
            .Include(u => u.Avatar)
            .FirstAsync(u => u.Id == identityId);
        return user;
    }

    public OrderDto MapToOrderDto(Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            CreatedAt = order.CreatedAt,
            Status = order.Status,
            Items = order.Items.Select(oi => new OrderItemDto
            {
                Id = oi.Id,
                ProductId = oi.Product.Id,
                ProductTitle = oi.Product.Title,
                UnitPrice = oi.UnitPrice,
                Quantity = oi.Quantity
            }).ToList()
        };
    }

    public async Task<Order> GetOrCreateCurrentOrderAsync()
    {
        var user = await GetCurrentUser();
        var order = await context
                .Orders
                .Include(o => o.Items)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.User.Id == user.Id && o.Status == OrderStatus.New);

        if (order == null)
        {
            order = new Order
            {
                User = user,
                CreatedAt = DateTime.UtcNow,
                Status = OrderStatus.New
            };
            context.Orders.Add(order);
            await context.SaveChangesAsync();
        }
        return order;
    }


    public async Task<IActionResult> Index()
    {
        var order = await GetOrCreateCurrentOrderAsync();
        return View(MapToOrderDto(order ));
    }


    [HttpPost]
    public async Task<CommandResultDto> AddToCart([FromBody] OrderItemForm itemForm)
    {
        // Поточне замовлення або створюємо нове, якщо його ще немає
        var order = await GetOrCreateCurrentOrderAsync();
        // Знаходимо продукт, який хочемо додати до корзини
        var product = await context.Products.FirstOrDefaultAsync(x => x.Id == itemForm.ProductId);
        if (product == null)
        {
            // Якщо продукт не знайдено, повертаємо помилку
            Response.StatusCode = 404;
            return new CommandResultDto
            {
                Success = false,
                Message = "Product not found"
            };
        }

        // Перевіряємо, чи вже є цей продукт у замовленні
        var orderItem = order.Items.FirstOrDefault(oi => oi.Product.Id == product.Id);
        if (orderItem == null)
        {
            // Якщо продукту ще немає в замовленні, створюємо новий OrderItem
            orderItem = new OrderItem
            {
                Product = product,
                Quantity = itemForm.Quantity,
                UnitPrice = product.Price
            };
            order.Items.Add(orderItem);
        }
        else
        {
            // Якщо продукт вже є в замовленні, просто збільшуємо кількість
            orderItem.Quantity += itemForm.Quantity;
        }

        await context.SaveChangesAsync();

        return new CommandResultDto
        {
            Success = true,
            Message = "Product added to cart"
        };
    }


    [HttpPost]
    public async Task<CommandResultDto> RemoveFromCart([FromBody] OrderItemForm itemForm)
    {
        // Поточне замовлення або створюємо нове, якщо його ще немає
        var order = await GetOrCreateCurrentOrderAsync();
        // Знаходимо позицію замовлення, яку хочемо видалити
        var orderItem = order.Items.FirstOrDefault(oi => oi.Product.Id == itemForm.ProductId);

        if (orderItem == null)
        {
            // Якщо позиція замовлення не знайдена, повертаємо помилку
            Response.StatusCode = 404;
            return new CommandResultDto
            {
                Success = false,
                Message = "Order item not found"
            };
        }
        // Видаляємо позицію замовлення з корзини
        order.Items.Remove(orderItem);
        // Зберігаємо зміни в базі даних
        await context.SaveChangesAsync();

        return new CommandResultDto
        {
            Success = true,
            Message = "Product removed from cart"
        };
    }

    [HttpPost]
    public async Task<CommandResultDto> UpdateCartItem([FromBody] OrderItemForm form)
    {
        // Поточне замовлення або створюємо нове, якщо його ще немає
        var order = await GetOrCreateCurrentOrderAsync();
        // Знаходимо позицію замовлення, яку хочемо оновити
        var orderItem = order.Items.FirstOrDefault(oi => oi.Product.Id == form.ProductId);
        if (orderItem == null)
        {
            // Якщо позиція замовлення не знайдена, повертаємо помилку
            Response.StatusCode = 404;
            return new CommandResultDto
            {
                Success = false,
                Message = "Order item not found"
            };
        }
        // Оновлюємо кількість позиції замовлення
        orderItem.Quantity = form.Quantity;
        // Зберігаємо зміни в базі даних
        await context.SaveChangesAsync();

        return new CommandResultDto
        {
            Success = true,
            Message = "Cart item updated"
        };
    }



    public async Task<IActionResult> Checkout(int id)
    {
        var order = await context.Orders
            .Include(x => x.Items)
            .ThenInclude(x => x.Product)
            .FirstAsync(x => x.Id == id);

        if (order.Items.Count == 0)
        {
            // Якщо в корзині немає товарів, перенаправляємо користувача назад до сторінки корзини
            return RedirectToAction("Index");
        }
        // Тут можна додати логіку обробки замовлення, наприклад, створення запису в базі даних для підтвердження замовлення, відправку email тощо.
        // Після успішного оформлення замовлення оновлюємо статус замовлення
        order.Status = OrderStatus.Processing;
        await context.SaveChangesAsync();
        // Перенаправляємо користувача на сторінку підтвердження замовлення або на головну сторінку

        var checkout = LiqPayHelper.GetLiqPayModel(order.Id.ToString(), 
            order.Items.Sum(x => x.UnitPrice * x.Quantity));
        ViewData["LiqPayCheckout"] = checkout;


        return View(MapToOrderDto(order));
    }

    [HttpPost]
    public async Task<IActionResult> LiqPayCallback()
    {
        // TODO handle LiqPay callback (succes payment notification)

        /*
         * {
    "transaction_id": 2763752316,
    "amount_bonus": 0,
    "status": "sandbox",
    "type": "buy",
    "user": {
        "country_code": null,
        "id": null,
        "nick": null,
        "phone": null
    },
    "sender_card_mask2": "424242*42",
    "ip": "145.224.94.149",
    "is_3ds": false,
    "currency": "UAH",
    "show_moment_part": false,
    "amount_credit": 210,
    "action": "pay",
    "create_date": 1765561484791,
    "need_cardholder_name": true,
    "payment_id": 2763752316,
    "language": "uk",
    "version": 3,
    "public_key": "sandbox_i75414272515",
    "currency_credit": "UAH",
    "commission_debit": 0,
    "sender_bonus": 0,
    "notify": {
        "data": "eyJwYXltZW50X2lkIjoyNzYzNzUyMzE2LCJhY3Rpb24iOiJwYXkiLCJzdGF0dXMiOiJzYW5kYm94IiwidmVyc2lvbiI6MywidHlwZSI6ImJ1eSIsInBheXR5cGUiOiJjYXJkIiwicHVibGljX2tleSI6InNhbmRib3hfaTc1NDE0MjcyNTE1IiwiYWNxX2lkIjo0MTQ5NjMsIm9yZGVyX2lkIjoiYjhmYjgwOTMtY2U4Zi00ZWZkLWIzYjUtZmQxMGFlNmU5ZDA3IiwibGlxcGF5X29yZGVyX2lkIjoiRVpDREpWQksxNzY1NTYxNDg0Nzg3NzA3IiwiZGVzY3JpcHRpb24iOiLQntC/0LvQsNGC0LAg0LfQsNC80L7QstC70LXQvdC90Y8gIyBiOGZiODA5My1jZThmLTRlZmQtYjNiNS1mZDEwYWU2ZTlkMDciLCJzZW5kZXJfZmlyc3RfbmFtZSI6InRlc3QiLCJzZW5kZXJfbGFzdF9uYW1lIjoidGVzdCIsInNlbmRlcl9jYXJkX21hc2syIjoiNDI0MjQyKjQyIiwic2VuZGVyX2NhcmRfYmFuayI6IlRlc3QiLCJzZW5kZXJfY2FyZF90eXBlIjoidmlzYSIsInNlbmRlcl9jYXJkX2NvdW50cnkiOjgwNCwiaXAiOiIxNDUuMjI0Ljk0LjE0OSIsImFtb3VudCI6MjEwLjAsImN1cnJlbmN5IjoiVUFIIiwic2VuZGVyX2NvbW1pc3Npb24iOjAuMCwicmVjZWl2ZXJfY29tbWlzc2lvbiI6My4xNSwiYWdlbnRfY29tbWlzc2lvbiI6MC4wLCJhbW91bnRfZGViaXQiOjIxMC4wLCJhbW91bnRfY3JlZGl0IjoyMTAuMCwiY29tbWlzc2lvbl9kZWJpdCI6MC4wLCJjb21taXNzaW9uX2NyZWRpdCI6My4xNSwiY3VycmVuY3lfZGViaXQiOiJVQUgiLCJjdXJyZW5jeV9jcmVkaXQiOiJVQUgiLCJzZW5kZXJfYm9udXMiOjAuMCwiYW1vdW50X2JvbnVzIjowLjAsIm1waV9lY2kiOiI3IiwiaXNfM2RzIjpmYWxzZSwibGFuZ3VhZ2UiOiJ1ayIsImNyZWF0ZV9kYXRlIjoxNzY1NTYxNDg0NzkxLCJlbmRfZGF0ZSI6MTc2NTU2MTQ4NDkwNiwidHJhbnNhY3Rpb25faWQiOjI3NjM3NTIzMTZ9",
        "signature": "BZZqCEtGcEdB58vn/F8+LeaWgZI="
    },
    "sender_card_country": 804,
    "amount_debit": 210,
    "result": "ok",
    "amount": 210,
    "commission_credit": 3.15,
    "currency_debit": "UAH",
    "sender_card_bank": "Test",
    "end_date": 1765561484906,
    "receiver_commission": 3.15,
    "acq_id": 414963,
    "order_id": "b8fb8093-ce8f-4efd-b3b5-fd10ae6e9d07",
    "description": "нОКЮРЮ ГЮЛНБКЕММЪ # b8fb8093-ce8f-4efd-b3b5-fd10ae6e9d07",
    "agent_commission": 0,
    "sender_first_name": "test",
    "pay_way": "card,privat24,gpay,apay,qr",
    "sender_last_name": "test",
    "mpi_eci": "7",
    "liqpay_order_id": "EZCDJVBK1765561484787707",
    "sender_card_type": "visa",
    "card_mask": "424242*42",
    "paytype": "card",
    "sender_commission": 0,
    "cmd": "liqpay.callback",
    "data": "eyJwYXltZW50X2lkIjoyNzYzNzUyMzE2LCJhY3Rpb24iOiJwYXkiLCJzdGF0dXMiOiJzYW5kYm94IiwidmVyc2lvbiI6MywidHlwZSI6ImJ1eSIsInBheXR5cGUiOiJjYXJkIiwicHVibGljX2tleSI6InNhbmRib3hfaTc1NDE0MjcyNTE1IiwiYWNxX2lkIjo0MTQ5NjMsIm9yZGVyX2lkIjoiYjhmYjgwOTMtY2U4Zi00ZWZkLWIzYjUtZmQxMGFlNmU5ZDA3IiwibGlxcGF5X29yZGVyX2lkIjoiRVpDREpWQksxNzY1NTYxNDg0Nzg3NzA3IiwiZGVzY3JpcHRpb24iOiLQntC/0LvQsNGC0LAg0LfQsNC80L7QstC70LXQvdC90Y8gIyBiOGZiODA5My1jZThmLTRlZmQtYjNiNS1mZDEwYWU2ZTlkMDciLCJzZW5kZXJfZmlyc3RfbmFtZSI6InRlc3QiLCJzZW5kZXJfbGFzdF9uYW1lIjoidGVzdCIsInNlbmRlcl9jYXJkX21hc2syIjoiNDI0MjQyKjQyIiwic2VuZGVyX2NhcmRfYmFuayI6IlRlc3QiLCJzZW5kZXJfY2FyZF90eXBlIjoidmlzYSIsInNlbmRlcl9jYXJkX2NvdW50cnkiOjgwNCwiaXAiOiIxNDUuMjI0Ljk0LjE0OSIsImFtb3VudCI6MjEwLjAsImN1cnJlbmN5IjoiVUFIIiwic2VuZGVyX2NvbW1pc3Npb24iOjAuMCwicmVjZWl2ZXJfY29tbWlzc2lvbiI6My4xNSwiYWdlbnRfY29tbWlzc2lvbiI6MC4wLCJhbW91bnRfZGViaXQiOjIxMC4wLCJhbW91bnRfY3JlZGl0IjoyMTAuMCwiY29tbWlzc2lvbl9kZWJpdCI6MC4wLCJjb21taXNzaW9uX2NyZWRpdCI6My4xNSwiY3VycmVuY3lfZGViaXQiOiJVQUgiLCJjdXJyZW5jeV9jcmVkaXQiOiJVQUgiLCJzZW5kZXJfYm9udXMiOjAuMCwiYW1vdW50X2JvbnVzIjowLjAsIm1waV9lY2kiOiI3IiwiaXNfM2RzIjpmYWxzZSwibGFuZ3VhZ2UiOiJ1ayIsImNyZWF0ZV9kYXRlIjoxNzY1NTYxNDg0NzkxLCJlbmRfZGF0ZSI6MTc2NTU2MTQ4NDkwNiwidHJhbnNhY3Rpb25faWQiOjI3NjM3NTIzMTZ9",
    "signature": "BZZqCEtGcEdB58vn/F8+LeaWgZI="
}
         * 
         */



        // { Data: "", Signature  }

        return new JsonResult(new { Ok = true });
    }

}
