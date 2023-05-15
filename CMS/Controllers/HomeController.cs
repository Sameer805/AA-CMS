using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using CMS.Models;
using Newtonsoft.Json;

namespace CMS.Controllers
{
    public class HomeController : Controller
    {
        cmsEntities4 db = new cmsEntities4();

        public ActionResult Index()
        {
            return View();
        }

        public string checkReciptNumber(int reciptnumber)
        {
            var result = (from b in db.bookings
                          where b.reciptNumber == reciptnumber
                          select new { b }).ToList();
            return ((result.Count == 0) ? "true"  : "false");
        }
      
        public String getEvents()
        {
            var result = (from b in db.bookings
                          join v in db.venues on b.venue_id equals v.id
                          join u in db.users on b.booked_by equals u.id
                          where b.isActive == true
                          select new { b, v.name,username = u.username,userid = u.id }).ToList();
            return JsonConvert.SerializeObject(result);
        }
        public int checkEventsOnDate(DateTime date)
        {
            var result = db.bookings.Where(d => d.bookingDate == date & d.isActive == true).Select(x => x).Count();
            return result;
        }
        public ActionResult Index2()
        {
            return View();
        }
        public ActionResult EventSetup()
        {
            return View();
        }
        public string FetchSetups()
        {
            var result = from u in db.eventSetups
                         where u.isActive == true
                         orderby u.id descending
                         select new { u };
            return JsonConvert.SerializeObject(result.ToList());
        }
        public ActionResult Reports()
        {
            return View();
        }
        public string FetchSetup(int id)
        {
            var result = from u in db.eventSetups
                         where u.isActive == true & u.id == id
                         select new { u };
            return JsonConvert.SerializeObject(result.ToList());
        }
        public string DeleteEventSetup(int id)
        {
            var result = db.eventSetups.Find(id);
            db.eventSetups.Remove(result);
            db.SaveChanges();
            return "Done";
        }
        public string AddSetup(string name)
        {
            eventSetup setup = new eventSetup();
            setup.name = name;
            setup.isActive = true;
            db.eventSetups.Add(setup);
            db.SaveChanges();
            return "Done";
        }
        public string UpdateSetup(int id, string name)
        {
            eventSetup setup = db.eventSetups.Find(id);
            setup.name = name;
            db.SaveChanges();
            return "Done";
        }
        public ActionResult Login()
        {
            return View();
        }
        public ActionResult Events()
        {
            return View();
        }
        public ActionResult EventList()
        {
            return View();
        }
        public ActionResult logout()
        {
            return View();
        }
         
        [HttpGet]
        public  ActionResult AddEvent(string eventname, string customername, DateTime bookingdate, string phonenumber, string email, int venue, string address, string extraarngement, int status, int number, string paymenttype, string creditcardnumber, string bankname, string chequenumber, int amount, int user, string birthdayperson, int numofkids, string meal, string theme, string addonshows, string tip, DateTime starttime, DateTime endtime)
        {
            try
            {               
                booking booking = new booking();
                booking.customerName = customername;
                booking.bookingDate = bookingdate;
                booking.phonenumber = phonenumber;
                booking.email = email;
                booking.venue_id = venue;
                booking.address = address;
                booking.time = "";
                booking.eventName = eventname;
                booking.extraArrangement = extraarngement;
                booking.isActive = true;
                booking.inserteddatetime = DateTime.Now;
                booking.numberOfPersons = number;
                booking.isBooked = (status == 0 ? false : true);                
                booking.booked_by = user;
                booking.birthdayGirlBoy = birthdayperson;
                booking.addOnShows = addonshows;
                booking.theme = theme;
                booking.tip = tip;
                booking.numberOfKids = numofkids;
                booking.meal = meal;
                booking.startTime = bookingdate.Date + starttime.TimeOfDay;
                booking.endTime = bookingdate.Date + endtime.TimeOfDay;
                db.bookings.Add(booking);
                db.SaveChanges();
                
                              
                if (status == 0)
                {
                    return Json(booking.id+"", JsonRequestBehavior.AllowGet);
                }
                eventTransaction et = new eventTransaction();
                et.amount = amount;
                et.booking_id = booking.id;
                et.paymentType = Convert.ToInt32(paymenttype);
                et.bankName = bankname;
                et.chequeNumber = chequenumber;
                et.creditCardNumber = creditcardnumber;
                et.insertedDateTime = DateTime.Now;
                db.eventTransactions.Add(et);
                db.SaveChanges();


                return Json(booking.id + "", JsonRequestBehavior.AllowGet);
            }
            catch (DbEntityValidationException ex)
            {
                var errorMessages = ex.EntityValidationErrors
                        .SelectMany(x => x.ValidationErrors)
                        .Select(x => x.ErrorMessage);

                // Join the list of error messages into a single string.
                var fullErrorMessage = string.Join("; ", errorMessages);

                // Throw a new exception with the combined error message.
                throw new Exception(fullErrorMessage, ex);
            }
        }
        public string IsBookingAvailable(DateTime starttime, DateTime endtime, DateTime bookingdate)
        {
            DateTime bookingStart = bookingdate.Date + starttime.TimeOfDay;
            DateTime bookingEnd = bookingdate.Date + endtime.TimeOfDay;
            var overlappingBookings = db.bookings
                .Where(b => b.startTime < bookingEnd && bookingStart < b.endTime)
                .ToList();
            if (overlappingBookings.Count == 0)
            {
                return "true";
            }
            return "false";
        }
        
        public string FetchEventTypes()
        {
            var results = from u in db.eventTypes
                          where u.isActive == true
                          select new { u };

            return JsonConvert.SerializeObject(results.ToList());
        }
        public string FetchVenues()
        {
            var results = from u in db.venues
                          where u.isActive == true
                          select new { u };

            return JsonConvert.SerializeObject(results.ToList());
        }
        public string FetchSlots()
        {
            var results = from u in db.slots
                          where u.isActive == true
                          select new { u };

            return JsonConvert.SerializeObject(results.ToList());
        }
        public ActionResult CheckSlot(int id, DateTime date, int venueid)
        {
            var result = (from u in db.bookings
                          where u.slot_id == id & u.bookingDate == date & u.venue_id == venueid & u.isActive == true
                          select u).ToList();
            if(result.Count() > 0)
            {
                return Json("false", JsonRequestBehavior.AllowGet);
            }
            return Json("ok", JsonRequestBehavior.AllowGet);
        }
        public string loginProceed(string username, string password)
        {
            var u = db.users.ToList().Where(x => x.username == username && x.password == password);
            if (u.Count() > 0)
            {
                foreach (var item in u)
                {
                    if (item.isAdmin == true)
                    {
                        return JsonConvert.SerializeObject( new {id= item.id, admin = true });
                    }
                    return JsonConvert.SerializeObject(new { id = item.id, admin = false });
                }
                return JsonConvert.SerializeObject(new { id = 0, admin = false });
            }
            else
            {
                return JsonConvert.SerializeObject(new { id = 0, admin = false });
            }
        }
        public ActionResult deleteEvent(int id)
        {
            try
            {
                booking eve = db.bookings.Find(id);

                eve.isActive = false;

                db.SaveChanges();

                delete_log dl = new delete_log();
                dl.event_id = id;
                dl.deleted_timedate = DateTime.Now;
                db.delete_log.Add(dl);

                db.SaveChanges();
                return Json("ok", JsonRequestBehavior.AllowGet);
            }

            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {


                    foreach (var ve in eve.ValidationErrors)
                    {
                        return Json("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, JsonRequestBehavior.AllowGet);
                    }
                }
                throw;
            }


        }
        public ActionResult editEvent(int id, string eventname, string customername, DateTime bookingdate, string phonenumber , string email, int venue, string address, string extraarngement,int guests, int status, string paymenttype, string creditcardnumber, string bankname, string chequenumber, int amount, string birthdayperson, int numofkids, string meal, string theme, string addonshows, string tip, DateTime starttime, DateTime endtime)
        {
            try
            {
                booking booking = db.bookings.Find(id);
                booking.customerName = customername;
                booking.bookingDate = bookingdate;
                booking.phonenumber = phonenumber;
                booking.email = email;
                booking.venue_id = venue;
                booking.address = address;
                booking.eventName = eventname;
                booking.extraArrangement = extraarngement;
                booking.isActive = true;
                booking.numberOfPersons = guests;
                booking.inserteddatetime = DateTime.Now;
                booking.birthdayGirlBoy = birthdayperson;
                booking.addOnShows = addonshows;
                booking.tip = tip;
                booking.numberOfKids = numofkids;
                booking.theme = theme;
                booking.startTime = bookingdate.Date + starttime.TimeOfDay; ;
                booking.endTime = bookingdate.Date + endtime.TimeOfDay; ;
                booking.meal = meal;
                booking.isBooked = (status == 0 ? false : true);

                db.SaveChanges();
                if(status == 0)
                {
                    db.SaveChanges();

                    return Json("ok", JsonRequestBehavior.AllowGet);
                }
                eventTransaction et = db.eventTransactions.Where(i => i.booking_id == id).FirstOrDefault();
                
                if (et == null)
                {
                    eventTransaction et2 = new eventTransaction();
                    et2.amount = amount;
                    et2.booking_id = id;
                    et2.bankName = bankname;
                    et2.chequeNumber = chequenumber;                    
                    et2.creditCardNumber = creditcardnumber;
                    et2.paymentType = Convert.ToInt32(paymenttype);
                    et2.insertedDateTime = DateTime.Now;
                    db.eventTransactions.Add(et2);
                    db.SaveChanges();

                    return Json("ok", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    et.amount = amount;
                    et.bankName = bankname;
                    et.chequeNumber = chequenumber;
                    et.creditCardNumber = creditcardnumber;
                    et.paymentType = Convert.ToInt32(paymenttype);
                }
               

                db.SaveChanges();
                
                return Json("ok", JsonRequestBehavior.AllowGet);

            }
            catch (Exception e)
            {
                return Json("not ok"+e, JsonRequestBehavior.AllowGet);
                throw;
            }
        }
        public string FetchSetupsByBookingId(int id)
        {
            var result = (from s in db.eventSetupsMaps
                           join set in db.eventSetups on s.setup_id equals set.id
                           where s.booking_id == id

                           select new {id =s.id, price = s.price,quantity = s.quantity,amount = s.price * s.quantity, remarks = s.remarks, name = set.name }).ToList();
            return JsonConvert.SerializeObject(result);
        }
        
        public string EditEventSetup(int id,string quan,string price,string remarks)
        {
            eventSetupsMap s = db.eventSetupsMaps.Find(id);
            s.quantity = Int32.Parse(quan);
            s.remarks = remarks;
            s.price = Int32.Parse(price);
            db.SaveChanges();
            return "Done";


        }
        public int? FetchTotalPlanAmountByBookingID(int id)
        {
            var result = (from b in db.eventSetupsMaps
                          where b.booking_id == id
                          select new { amount = b.price * b.quantity }).ToList();
            var final = result.Select(x => x.amount).Sum();
            return final;

        }
        public int? FetchAdvanceAmountByBookingID(int id)
        {
            var result = (from et in db.eventTransactions
                          where et.booking_id == id
                          select et.amount).FirstOrDefault();
            return result;
        }

        public string FetchTransactionsByBookingID(int id)
        {
            var result = (from u in db.eventTransactions
                          where u.booking_id == id
                          select u).ToList();
            return JsonConvert.SerializeObject(result);
                        
        }
        public string FetchVenueByID(int id)
        {
            var result = (from u in db.venues
                          where u.id == id
                          select u.name).ToList();
            return JsonConvert.SerializeObject(result);
        }
        public String getEventFromId(int id)
        {
            var result = (from b in db.bookings
                          join v in db.venues on b.venue_id equals v.id
                          where b.id == id
                          select new { b, v.name }).ToList();
            //Advance
            var result2 = (from b in db.bookings
                           join et in db.eventTransactions on b.id equals et.booking_id
                           where b.id == id
                           select new { et.amount,et.bankName,et.chequeNumber,et.creditCardNumber,et.paymentType}
                           ).FirstOrDefault();
            var slotname = (from u in db.bookings
                            join s in db.slots on u.slot_id equals s.id
                            where u.id == id
                            select new { slot = s.slottime }).ToList();

            return JsonConvert.SerializeObject(new { result,result2,slotname});
        }
        public int? TotalPlanAmountByBookingID(int id)
        {
            var result = (from s in db.eventSetupsMaps
                          where s.booking_id == id
                          select new { amount = s.price * s.quantity }).ToList();
            var sum = result.Select(x => x.amount).Sum();
            return sum;
        }
        
        public string AddBookingSetup(int id,int setupId,int price, int quantity,string remarks)
        {
            eventSetupsMap map = new eventSetupsMap();
            map.booking_id = id;
            map.price = price;
            map.quantity = quantity;
            map.remarks = remarks;
            map.setup_id = setupId;
            db.eventSetupsMaps.Add(map);
            db.SaveChanges();
            return "Done";


        }
        public string getRoles(int id)
        {
            var results = from u in db.users
                          where u.isActive == true && u.id == id
                          select new
                          {childScreen = (
                             from sc in db.userScreenChildPermissions
                             join s in db.userScreenPermissions on sc.screen_id equals s.id
                             where s.user_id == u.id
                             select new { sc, s }
                            )
                          };

            return JsonConvert.SerializeObject(results.ToList());
        }
        public string ReturnIDByPhonenumber(string phonenumber)
        {
            var result = (from u in db.bookings
                          where u.phonenumber == phonenumber & u.isActive == true
                          select u.id);
            return JsonConvert.SerializeObject(result);
        }
        //This is not only new event (email sending) function all email routing actions are routed here.
        [HttpPost, ValidateInput(false)]
        public string SendEmailNewEvent(string from, string to, string body, string subject, string[] BCC)
        {
            //string to = "m.talhasharieff@gmail.com"; //To address    
            //string from = "New Event ibnelaiq@gmail.com"; //From address    
            string[] BCCFinal = { "aajasmat@gmail.com", "naeemrajput80@gmail.com", "wasimrao620@gmail.com", "talhaayaseenn@gmail.com"};
            MailMessage message = new MailMessage(from, to);

            message.Subject = subject;
            message.Body = body;
            message.BodyEncoding = Encoding.UTF8;
            message.IsBodyHtml = true;
            foreach (var item in BCCFinal)
            {
                message.Bcc.Add(item);
            }
            SmtpClient client = new SmtpClient("smtp.gmail.com", 587); //Gmail smtp    
            System.Net.NetworkCredential basicCredential1 = new
            System.Net.NetworkCredential("decreative.eventsemail@gmail.com", "beegees11111");
            
            client.EnableSsl = true;
            client.UseDefaultCredentials = false;
            client.Credentials = basicCredential1;
            try
            {
                client.Send(message);
                return "Done";
            }
            catch (Exception e) {
                return e.ToString();
            }
            
        }
        public ActionResult testmail()
        {
            return View();
        }
        public string AddTransaction(int bid,int amount,string bankname,string creditcardnumber,string chequenumber,int type,string remarks)
        {
            eventTransaction et = new eventTransaction();
            et.booking_id = bid;
            et.bankName = (bankname == null ? "0" : bankname);
            et.amount = amount;
            et.chequeNumber = (chequenumber == null ? "0" : chequenumber);
            et.creditCardNumber = (creditcardnumber == null ? "0" : creditcardnumber);
            et.paymentType = type;
            et.insertedDateTime = DateTime.Now;
            et.remarks = remarks;
                db.eventTransactions.Add(et);
            db.SaveChanges();
            return "Done";
        }
        #region EventsTransaction
        public string FetchEventDetail(int bookingid,string phonenumber)
        {
            //Phone number
            if(bookingid == 0)
            {
                var result = (from u in db.bookings
                              where u.phonenumber == phonenumber & u.isActive == true
                              select u).ToList();
                return JsonConvert.SerializeObject(result);

                
            }
            //Booking Id
            if(phonenumber == "0")
            {
                var result = (from u in db.bookings
                              where u.id == bookingid & u.isActive == true
                              select u).ToList();
                return JsonConvert.SerializeObject(result);
            }
            return JsonConvert.SerializeObject("Some Error Occured");
        }
        #endregion



    }
}