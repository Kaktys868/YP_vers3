using Bank.Context;
using Bank.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace Bank.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "v3")]
    public class UserController : Microsoft.AspNetCore.Mvc.Controller
    {
        [Route("List")]
        [HttpGet]
        [ProducesResponseType(typeof(List<User>), 200)]
        [ProducesResponseType(500)]
        public ActionResult List()
        {
            try
            {
                IEnumerable<User> User = new UserContext().Users;
                return Json(User);
            }
            catch (Exception exp)
            {
                return StatusCode(500);
            }
        }
        [Route("Item")]
        [HttpGet]
        [ProducesResponseType(typeof(List<User>), 200)]
        [ProducesResponseType(500)]
        public ActionResult ItemTask(int id)
        {
            try
            {
                User User = new UserContext().Users.Where(x => x.Id == id).First();
                return Json(User);
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
        public ActionResult Add([FromForm] User task)
        { 
            try 
            { 
                task.Password = gleb(task.Password);
                UserContext UserContext = new UserContext();
                UserContext.Users.Add(task);
                UserContext.SaveChanges();
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
        public ActionResult Update([FromForm] User task)
        {
            try
            {
                using (UserContext UserContext = new UserContext())
                {
                    var existingTask = UserContext.Users.FirstOrDefault(x => x.Id == task.Id);

                    if (existingTask == null)
                    {
                        return NotFound($"ID {task.Id} не найдено");
                    }

                    existingTask.Email = task.Email;
                    existingTask.Password = task.Password;
                    existingTask.Name = task.Name;
                    existingTask.CreatedAt = task.CreatedAt;
                    existingTask.IsActive = task.IsActive;

                    UserContext.SaveChanges();
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
                using (UserContext UserContext = new UserContext())
                {
                    var existingTask = UserContext.Users.FirstOrDefault(x => x.Id == id);

                    if (existingTask == null)
                    {
                        return NotFound($"ID {id} не найдено");
                    }

                    UserContext.Remove(existingTask);
                    UserContext.SaveChanges();
                    return StatusCode(200);
                }
            }
            catch (Exception exp)
            {

                return StatusCode(500);
            }
        }
        private string gleb(string pwd)
        {
            var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(pwd);
            var hash = sha256.ComputeHash(bytes);
            var computedHash = Convert.ToBase64String(hash);
            return computedHash;
        }
    }
}
