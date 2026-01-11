using Bank.Context;
using Bank.Models;
using Microsoft.AspNetCore.Mvc;

namespace Bank.Controller
{
    [Route("api/[controller]")]
    [ApiExplorerSettings(GroupName = "v2")]
    public class TransactionController : Microsoft.AspNetCore.Mvc.Controller
    {
        [Route("List")]
        [HttpGet]
        [ProducesResponseType(typeof(List<Transaction>), 200)]
        [ProducesResponseType(500)]
        public ActionResult List()
        {
            try
            {
                IEnumerable<Transaction> Transaction = new TransactionContext().Transactions;
                return Json(Transaction);
            }
            catch (Exception exp)
            {
                return StatusCode(500);
            }
        }
        [Route("Item")]
        [HttpGet]
        [ProducesResponseType(typeof(List<Transaction>), 200)]
        [ProducesResponseType(500)]
        public ActionResult ItemTask(int id)
        {
            try
            {
                Transaction Transaction = new TransactionContext().Transactions.Where(x => x.Id == id).First();
                return Json(Transaction);
            }
            catch (Exception exp)
            {
                return StatusCode(500);
            }
        }
        [Route("Add")]
        [HttpPost]
        [ProducesResponseType(500)]
        [ProducesResponseType(500)]
        public ActionResult Add([FromForm] Transaction task)
        {
            try
            {
                Console.WriteLine($"Получен UserId: {task.UserId}");
                Console.WriteLine($"Получен Amount: '{task.Amount}'");
                Console.WriteLine($"Тип Amount: {task.Amount.GetType()}");
                TransactionContext TransactionContext = new TransactionContext();
                TransactionContext.Transactions.Add(task);
                TransactionContext.SaveChanges();
                return StatusCode(200);
            }
            catch (Exception exp)
            {
                return StatusCode(500);
            }
        }
        [Route("Update")]
        [HttpPut]
        [ProducesResponseType(500)]
        [ProducesResponseType(500)]
        public ActionResult Update([FromForm] Transaction task)
        {
            try
            {
                using (TransactionContext TransactionContext = new TransactionContext())
                {
                    var existingTask = TransactionContext.Transactions.FirstOrDefault(x => x.Id == task.Id);

                    if (existingTask == null)
                    {
                        return NotFound($"ID {task.Id} не найдено");
                    }

                    existingTask.UserId = task.UserId;
                    existingTask.Amount = task.Amount;
                    existingTask.Description = task.Description;
                    existingTask.Date = task.Date;
                    existingTask.Type = task.Type;

                    TransactionContext.SaveChanges();
                    return StatusCode(200);

                }
            }
            catch (Exception exp)
            {
                return StatusCode(500);
            }
        }
        [Route("Delete")]
        [HttpDelete]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public ActionResult DeleteUser(int id)
        {
            try
            {
                using (TransactionContext TransactionContext = new TransactionContext())
                {
                    var existingTask = TransactionContext.Transactions.FirstOrDefault(x => x.Id == id);

                    if (existingTask == null)
                    {
                        return NotFound($"ID {id} не найдено");
                    }

                    TransactionContext.Remove(existingTask);
                    TransactionContext.SaveChanges();
                    return StatusCode(200);
                }
            }
            catch (Exception exp)
            {

                return StatusCode(500);
            }
        }
    }
}
