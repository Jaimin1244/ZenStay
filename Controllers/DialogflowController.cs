using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Learn_Auth.Controllers
{
    public class DialogflowController : Controller
    {
        [HttpPost]
        public JsonResult Webhook()
        {
            // Fetch states from the database (Replace this with actual DB call)
            List<string> states = new List<string>
            {
                "Delhi", "Goa", "Gujarat", "Haryana", "Himachal Pradesh",
                "Karnataka", "Kathmandu", "Kerala", "Madhya Pradesh",
                "Maharashtra", "Punjab", "Rajasthan", "Tamil Nadu",
                "Uttar Pradesh", "Uttarakhand", "West Bengal"
            };

            string responseText = "We serve hotels in the following states:\n" + string.Join(", ", states);

            var response = new
            {
                fulfillmentText = responseText
            };

            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public class DialogflowRequest
        {
            public QueryResult QueryResult { get; set; }
        }

        public class QueryResult
        {
            public Intent Intent { get; set; }
        }

        public class Intent
        {
            public string DisplayName { get; set; }
        }

        public class DialogflowResponse
        {
            public DialogflowResponse(string text)
            {
                FulfillmentText = text;
            }

            public string FulfillmentText { get; set; }
        }
    }
}