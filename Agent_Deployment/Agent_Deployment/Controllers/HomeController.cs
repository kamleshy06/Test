using System.Diagnostics;
using System.Text.Json;
using Agent_Deployment.Models;
using Microsoft.AspNetCore.Mvc;

namespace Agent_Deployment.Controllers
{


    public class HomeController : Controller
    {
        // Temporary in-memory storage
        private static ChatState State = new ChatState();

        public IActionResult Index()
        {
            // Reset state when page loads
            State = new ChatState();
            return View();
        }

        [HttpPost]
        public IActionResult ChatMessage([FromBody] MessageModel req)
        {
            string msg = req.Message?.Trim();
            string bot = ProcessConversation(msg);

            return Json(new { reply = bot });
        }

        private string ProcessConversation(string msg)
        {
            // STEP 0: FIRST QUESTION
            if (State.Step == "Start")
            {
                State.Step = "AskName";
                return "Welcome <br/>May I know your name?";
            }

            // STEP 1: NAME
            if (State.Step == "AskName")
            {
                State.Name = msg;
                State.Step = "AskDeploy";
                return $"Hi {State.Name}!<br/>Do you want to Deploy Your Project On IIS? YES / NO";
            }

            if (State.Step == "AskDeploy")
            {
                msg = msg.ToUpper();

                if (msg != "YES" && msg != "NO")
                    return "Please answer only YES or NO.";

                // ❌ User selected NO → Restart conversation
                if (msg == "NO")
                {
                    ResetState();  // clear old values
                    State.Step = "AskName";  // restart from beginning
                    return "Okay! No problem.<br/><br/>Let's start again.<br/>May I know your name?";
                }

                // ✔ YES → Continue
                State.Deploy = msg;
                State.Step = "AskGitlink";
                return "Great! Please share the link of your git repository.";
            }

            // STEP 3: GIT LINK
            if (State.Step == "AskGitlink")
            {
                State.GitLink = msg;
                State.Step = "Done";
                return "Thank you! All details collected.<br/>Click <b>Submit Final</b> below.";
            }

            return "Conversation already completed.";
        }


        [HttpPost]
        public IActionResult SubmitFinal()
        {
            // Return all collected data
            return Json(State);
        }
        // ⭐ Reset State Function
        private void ResetState()
        {
            State.Name = null;
            State.Deploy = null;
            State.GitLink = null;
            State.Step = "Start";
        }
    }


    public class MessageModel
    {
        public string Message { get; set; }
    }

    public class ChatState
    {
        public string Step { get; set; } = "Start";  // conversation pointer
        public string Name { get; set; }
        public string Deploy { get; set; }
        public string GitLink { get; set; }
    }
}