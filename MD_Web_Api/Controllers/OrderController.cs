using MD_Web_Api.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Globalization;
using System.Collections.Generic;

namespace MD_Web_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly MyDbContext _dbContext;
        private readonly IWebHostEnvironment _hostEnvironment;

        public OrderController(MyDbContext dbContext, IWebHostEnvironment hostEnvironment)
        {
            _dbContext = dbContext;
            _hostEnvironment = hostEnvironment;
        }

        /*  [HttpPost]
          public async Task<IActionResult> CreateOrder([FromForm] OrderMaster order)
          {
              try
              {
                  if (order.ImageFile != null && order.ImageFile.Length > 0)
                  {
                      string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads");
                      string uniqueFileName = Guid.NewGuid().ToString() + "_" + order.ImageFile.FileName;
                      string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                      using (var fileStream = new FileStream(filePath, FileMode.Create))
                      {
                          await order.ImageFile.CopyToAsync(fileStream);
                      }
                      order.ImagePath = uniqueFileName;
                  }

                  // Save the order to the database
                  _dbContext.OrderMasters.Add(order);
                  await _dbContext.SaveChangesAsync();

                  return Ok(order);
              }
              catch (Exception ex)
              {
                  return StatusCode(500, $"An error occurred while creating the order: {ex.Message}");
              }
          }*/

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromForm] OrderMaster order, [FromForm] List<OrderDetail> orderDetails)
        {

            if (order.ImageFile != null && order.ImageFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads");
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + order.ImageFile.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await order.ImageFile.CopyToAsync(fileStream);
                }
                order.ImagePath = uniqueFileName;
            }

            // Add the order details to the order master
            order.OrderDetail = orderDetails;

            // Save the order to the database
            _dbContext.OrderMasters.Add(order);
            await _dbContext.SaveChangesAsync();

            // Return the newly created order with its ID
            return Ok();
        }
            
        




        [HttpGet]
        public IActionResult GetOrders()
        {
            var orders = _dbContext.OrderMasters.Include(od => od.OrderDetail).ToList();
            
            //var orders = _dbContext.OrderMasters.ToList();

            return Ok(orders);
        }

        [HttpGet("{orderId}")]
        public IActionResult GetOrder(int orderId)
        {
            var order = _dbContext.OrderMasters.Include(od => od.OrderDetail).FirstOrDefault(o => o.OrderId == orderId);
            if (order == null)
            {
                return NotFound($"Order with ID {orderId} not found.");
            }
            var set = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            };
            //var serial = JsonConvert.SerializeObject(order,Formatting.None, set);
            // var jsobj = JsonConvert.DeserializeObject(serial);

            //return Ok(jsobj);

            return Ok(order);

        }


        [HttpPut("{orderId}")]
        public async Task<IActionResult> UpdateOrder(int orderId, [FromForm] OrderMaster updatedOrder)
        {
            try
            {
                //var order = await _dbContext.OrderMasters.FirstOrDefaultAsync(o => o.OrderId == orderId);

                var order = await _dbContext.OrderMasters.Include(od => od.OrderDetail).FirstOrDefaultAsync(o => o.OrderId == orderId);

                if (order == null)
                {
                    return NotFound($"Order with ID {orderId} not found.");
                }

                if (updatedOrder.ImageFile != null && updatedOrder.ImageFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads");
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + updatedOrder.ImageFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await updatedOrder.ImageFile.CopyToAsync(fileStream);
                    }
                    order.ImagePath = uniqueFileName;
                }

                // Update the order properties
                order.CustomerName = updatedOrder.CustomerName;
                order.ImagePath = updatedOrder.ImagePath;
                order.OrderDate = updatedOrder.OrderDate;
                order.IsComplete = updatedOrder.IsComplete;
                //order.OrderDetail = updatedOrder.OrderDetail;

                order.OrderDetail.Clear();
                order.OrderDetail.AddRange(updatedOrder.OrderDetail);


                // Update the order in the database
                await _dbContext.SaveChangesAsync();

                return Ok(order);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while updating the order: {ex.Message}");
            }
        }



        [HttpDelete("{orderId}")]
        public async Task<IActionResult> DeleteOrder(int orderId)
        {
            var order = _dbContext.OrderMasters.FirstOrDefault(o => o.OrderId == orderId);
            if (order == null)
            {
                return NotFound($"Order with ID {orderId} not found.");
            }

            _dbContext.OrderMasters.Remove(order);
            await _dbContext.SaveChangesAsync();

            return Ok($"Order with ID {orderId} has been deleted.");
        }
    }

}
