using Bank.Context;
using Bank.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace Bank.Controllers
{
    [Route("api/[controller]")]
    [ApiExplorerSettings(GroupName = "v1")]
    public class GoalController : Microsoft.AspNetCore.Mvc.Controller
    {
        [Route("List")]
        [HttpGet]
        [ProducesResponseType(typeof(List<Goal>), 200)]
        [ProducesResponseType(500)]
        public ActionResult List()
        {
            try
            {
                IEnumerable<Goal> Goal = new GoalContext().Goals;
                return Json(Goal);
            }
            catch (Exception exp)
            {
                return StatusCode(500);
            }
        }
        [Route("Item")]
        [HttpGet]
        [ProducesResponseType(typeof(List<Goal>), 200)]
        [ProducesResponseType(500)]
        public ActionResult ItemTask(int id)
        {
            try
            {
                Goal Goal = new GoalContext().Goals.Where(x => x.Id == id).First();
                return Json(Goal);
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
        public ActionResult Add([FromForm] Goal task)
        {
            try
            {
                GoalContext GoalContext = new GoalContext();
                GoalContext.Goals.Add(task);
                GoalContext.SaveChanges();
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
        public ActionResult Update([FromForm] Goal task)
        {
            try
            {
                using (GoalContext GoalContext = new GoalContext())
                {
                    var existingTask = GoalContext.Goals.FirstOrDefault(x => x.Id == task.Id);

                    if (existingTask == null)
                    {
                        return NotFound($"ID {task.Id} не найдено");
                    }

                    existingTask.UserId = task.UserId;
                    existingTask.Name = task.Name;
                    existingTask.TargetAmount = task.TargetAmount;
                    existingTask.CurrentAmount = task.CurrentAmount;
                    existingTask.Deadline = task.Deadline;

                    GoalContext.SaveChanges();
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
                using (GoalContext GoalContext = new GoalContext())
                {
                    var existingTask = GoalContext.Goals.FirstOrDefault(x => x.Id == id);

                    if (existingTask == null)
                    {
                        return NotFound($"ID {id} не найдено");
                    }

                    GoalContext.Remove(existingTask);
                    GoalContext.SaveChanges();
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
