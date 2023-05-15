using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using CMS.Controllers;
using CMS.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;
using Newtonsoft.Json;
using Font = iTextSharp.text.Font;
using Image = iTextSharp.text.Image;
using Rectangle = iTextSharp.text.Rectangle;

namespace CMS.Views
{
    public class DashboardController : Controller
    {
        cmsEntities4 db = new cmsEntities4();
        // GET: Dashboard
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Users()
        {
            return View();
        }
        public ActionResult Venue()
        {
            return View();
        }
        public ActionResult EventType()
        {
            return View();
        }
        public ActionResult RD()
        {
            return View();
        }
        public ActionResult Reports()
        {
            return View();
        }
        public ActionResult EventList()
        {
            return View();
        }
        public string checkVenue(string venue)
        {
            var result = (from u in db.venues
                          where u.name == venue & u.isActive == true
                          select u).ToList();
            if (result.Count > 0)
                return JsonConvert.SerializeObject("False");

            return JsonConvert.SerializeObject("True"); 
        }
        public string checkEventType(string eventType)
        {
            var result = (from u in db.eventTypes
                          where u.name == eventType & u.isActive == true
                          select u).ToList();
            if (result.Count > 0)
                return JsonConvert.SerializeObject("False");

            return JsonConvert.SerializeObject("True");
        }
        public string FetchNameByUserID(int id) {
            
            return JsonConvert.SerializeObject(db.users.Where(x=>x.id == id).Select(y=>y.username));
                            
        }
        [System.Web.Mvc.HttpGet]
        public FileStreamResult pdf(string startdate, string enddate)
        {
            DateTime fromdate = Convert.ToDateTime(startdate);
            DateTime todate = Convert.ToDateTime(enddate);
            try
            {
                MemoryStream workStream = new MemoryStream();
                Document document = new Document(new Rectangle(288f, 144f), 10, 10, 10, 10);
                document.SetPageSize(iTextSharp.text.PageSize.A4.Rotate());



                PdfWriter.GetInstance(document, workStream).CloseStream = false;
                
                Font font = FontFactory.GetFont("Tahuma", 8.0f, BaseColor.BLACK);
                Font fontHeader = FontFactory.GetFont("Tahuma", 12.0f, BaseColor.BLACK);

                document.Open();
                string imageUrl = Server.MapPath("~/Content/") + "reportlogo.jpg";
                iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(imageUrl);
                img.ScaleToFit(80f, 70f);

                PdfPTable header = new PdfPTable(2);
                header.DefaultCell.Border = Rectangle.NO_BORDER;
                header.WidthPercentage = 100;
                header.AddCell(new Paragraph("Events Report", fontHeader));

                img.Alignment = Element.ALIGN_RIGHT;
                PdfPCell i = new PdfPCell();
                img.Alignment = Image.ALIGN_RIGHT;
                i.AddElement(img);


                i.Border = Rectangle.NO_BORDER;
                i.Rowspan = 3;

                header.AddCell(i);


                header.AddCell(new Paragraph("Created On: " + DateTime.Now.ToString(), fontHeader));


                document.Add(header);

                Paragraph p = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLACK, Element.ALIGN_LEFT, 1)));
                document.Add(p);
                document.Add(new Chunk("\n"));
                PdfPTable table = new PdfPTable(10);
                table.WidthPercentage = 100f;
                table.AddCell(new Paragraph("Booking ID",font));
                table.AddCell(new Paragraph("Booking Date", font));
                table.AddCell(new Paragraph("Event Name", font));
                table.AddCell(new Paragraph("Venue", font));
                table.AddCell(new Paragraph("No of Guests", font));
                table.AddCell(new Paragraph("Customer Name", font));
                table.AddCell(new Paragraph("Mobile Number", font));
                table.AddCell(new Paragraph("Total Plan Amount", font));
                table.AddCell(new Paragraph("Amount Recieved", font));
                table.AddCell(new Paragraph("Remaining Amount", font));

                var result = (from u in db.bookings
                              join v in db.venues
                              on u.venue_id equals v.id
                              where u.isActive == true & u.isBooked == true & u.bookingDate >= fromdate & u.bookingDate <= todate
                              
                              select new { u, v.name, V = EntityFunctions.TruncateTime(u.bookingDate) }).ToList().OrderBy(x=>x.u.bookingDate);

                int? mastertotalplanamount = 0;
                int? masteramountrecieved = 0;
                int? masterremainingamount = 0;
                foreach (var item in result)
                {

                    var resultAmount = (from t in db.eventTransactions
                                        where t.booking_id == item.u.id
                                        select t).ToList();
                    
                    table.AddCell(new Paragraph(item.u.id + "", font));
                    table.AddCell(new Paragraph(item.V.Value.ToString("dd/MM/yyy") + "", font));
                    table.AddCell(new Paragraph(item.u.eventName + "", font));
                    table.AddCell(new Paragraph(item.name, font));
                    table.AddCell(new Paragraph(item.u.numberOfPersons+"", font));
                    table.AddCell(new Paragraph(item.u.customerName + "", font));
                    table.AddCell(new Paragraph(item.u.phonenumber + "", font));
                    HomeController hm = new HomeController();
                    int? totalplanamount = hm.TotalPlanAmountByBookingID(item.u.id);
                    int? amountRecieved = resultAmount.Select(x => x.amount).Sum();
                    int? remainingamount = totalplanamount - amountRecieved;

                    mastertotalplanamount += totalplanamount;
                    masteramountrecieved += amountRecieved;
                    masterremainingamount += remainingamount;


                    table.AddCell(new Paragraph("Rs:" + totalplanamount + "/=", font));
                    table.AddCell(new Paragraph("Rs:" +amountRecieved + "/=", font));
                    table.AddCell(new Paragraph("Rs:" + remainingamount + "/=", font));
                }







                var cell = new PdfPCell(new Phrase(" "));
                cell.Colspan = 6;
                table.AddCell(cell);

                PdfPCell cell2 = new PdfPCell(new Phrase("Total:", font));
                cell2.BackgroundColor = BaseColor.LIGHT_GRAY;
                table.AddCell(cell2);

                PdfPCell cell3 = new PdfPCell(new Phrase("Rs:" + mastertotalplanamount + "/=", font));
                cell3.BackgroundColor = BaseColor.LIGHT_GRAY;
                table.AddCell(cell3);

                PdfPCell cell4 = new PdfPCell(new Phrase("Rs:" + masteramountrecieved + "/=", font));
                cell4.BackgroundColor = BaseColor.LIGHT_GRAY;
                table.AddCell(cell4);

                PdfPCell cell5 = new PdfPCell(new Phrase("Rs:" + masterremainingamount + "/=", font));
                cell5.BackgroundColor = BaseColor.LIGHT_GRAY;
                table.AddCell(cell5);


                document.Add(table);



                document.Close();

                byte[] byteInfo = workStream.ToArray();
                workStream.Write(byteInfo, 0, byteInfo.Length);
                workStream.Position = 0;

                //return new FileStreamResult(workStream, "application/pdf");
                Response.AppendHeader("content-disposition", "inline; filename=file.pdf");
                return new FileStreamResult(workStream, "application/pdf");
            }
            catch (Exception)
            {
                
                throw;
            }
        }
        public FileStreamResult pdfEvent(int id)
        {
            try
            {

                booking Booking = db.bookings.Find(id);
                venue Venue = db.venues.Find(Booking.venue_id);

                MemoryStream workStream = new MemoryStream();
                Document document = new Document(PageSize.A4);

              
                PdfWriter.GetInstance(document, workStream).CloseStream = false;
                Font font = FontFactory.GetFont("Tahuma", 8.0f, BaseColor.BLACK);
                Font fontMain  = FontFactory.GetFont("Tahuma", 10.0f, BaseColor.BLACK);
                Font fontHeader = FontFactory.GetFont("Tahuma", 14.0f, BaseColor.BLACK);

                document.Open();


                string imageUrl = Server.MapPath("~/Content/") + "reportlogo.jpg";
                iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(imageUrl);
                img.ScaleToFit(80f, 70f);

                PdfPTable header = new PdfPTable(2);
                header.DefaultCell.Border = Rectangle.NO_BORDER;
                header.WidthPercentage = 100;
                
                header.AddCell(new Paragraph("Booking ID: " + Booking.id.ToString(), fontHeader));
                img.Alignment = Element.ALIGN_RIGHT;
                PdfPCell i = new PdfPCell();
                img.Alignment = Image.ALIGN_RIGHT;
                i.AddElement(img);
                
                
                i.Border = Rectangle.NO_BORDER;
                i.Rowspan = 3;
                
                header.AddCell(i);

                header.AddCell(new Paragraph("PEEKABEAR", fontHeader));

                header.AddCell(new Paragraph("Created On: " + DateTime.Now.ToString(), fontHeader));


                document.Add(header);

                
                
                Paragraph p = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLACK, Element.ALIGN_LEFT, 1)));
                document.Add(p);
                document.Add(new Chunk("\n"));


                PdfPTable table = new PdfPTable(6);
                table.DefaultCell.Border = Rectangle.NO_BORDER;
                table.WidthPercentage = 100;

                table.AddCell(new Paragraph("Advance Recipt #:", fontMain));

                PdfPCell cell = new PdfPCell(new Paragraph(Booking.reciptNumber.ToString(), fontMain));
                cell.Border = Rectangle.BOTTOM_BORDER;
                table.AddCell(cell);

                table.AddCell(new Paragraph("Event Date:", fontMain));

                PdfPCell cell1 = new PdfPCell(new Paragraph(Booking.bookingDate.Value.ToString("dd/MM/yyy"), fontMain));
                cell1.Border = Rectangle.BOTTOM_BORDER;
                table.AddCell(cell1);

                table.AddCell(new Paragraph("Venue:", fontMain));

                PdfPCell cell2 = new PdfPCell(new Paragraph(Venue.name.ToString(), fontMain));
                cell2.Border = Rectangle.BOTTOM_BORDER;
                table.AddCell(cell2);

                var cellSeperator = new PdfPCell(new Phrase(" "));
                cellSeperator.Colspan = 6;
                cellSeperator.Border = Rectangle.NO_BORDER;
                table.AddCell(cellSeperator);

                //Second Raw 
                table.AddCell(new Paragraph("Event Name:", fontMain));

                PdfPCell cell3 = new PdfPCell(new Paragraph(Booking.eventName, fontMain));
                cell3.Border = Rectangle.BOTTOM_BORDER;
                table.AddCell(cell3);

                table.AddCell(new Paragraph("No of Guests:", fontMain));

                PdfPCell cell4 = new PdfPCell(new Paragraph(Booking.numberOfPersons.ToString(), fontMain));
                cell4.Border = Rectangle.BOTTOM_BORDER;
                table.AddCell(cell4);

                table.AddCell(new Paragraph("Customer Name:", fontMain));

                PdfPCell cell5 = new PdfPCell(new Paragraph(Booking.customerName, fontMain));
                cell5.Border = Rectangle.BOTTOM_BORDER;
                table.AddCell(cell5);

                //Third Raw
                var cellSeperator2 = new PdfPCell(new Phrase(" "));
                cellSeperator2.Colspan = 6;
                cellSeperator2.Border = Rectangle.NO_BORDER;
                table.AddCell(cellSeperator2);




                table.AddCell(new Paragraph("Customer Phone:", fontMain));

                PdfPCell cell6 = new PdfPCell(new Paragraph(Booking.phonenumber, fontMain));
           
                cell6.Border = Rectangle.BOTTOM_BORDER;
                table.AddCell(cell6);

                table.AddCell(new Paragraph("Customer Email:", fontMain));

                PdfPCell cell7 = new PdfPCell(new Paragraph(Booking.email, fontMain));
                cell7.Colspan = 2;
                cell7.Border = Rectangle.BOTTOM_BORDER;
                table.AddCell(cell7);


                PdfPCell cell8 = new PdfPCell(new Phrase(" "));
                cell8.Border = Rectangle.NO_BORDER;
                table.AddCell(cell8);

                document.Add(table);

                Paragraph p2 = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLACK, Element.ALIGN_LEFT, 1)));
                document.Add(p2);

                Paragraph p3 = new Paragraph("Event Details:");
                p3.SpacingAfter = 10;
                document.Add(p3);
                //Event Setups
                PdfPTable setups = new PdfPTable(5);
                setups.WidthPercentage = 100;

                

                PdfPCell headerName = new PdfPCell(new Paragraph("Setup", font));
                headerName.HorizontalAlignment = Element.ALIGN_CENTER;
                headerName.BackgroundColor = new BaseColor(Color.LightGray);

                PdfPCell headerPrice = new PdfPCell(new Paragraph("Price", font));
                headerPrice.HorizontalAlignment = Element.ALIGN_CENTER;
                headerPrice.BackgroundColor = new BaseColor(Color.LightGray);

                PdfPCell headerQuantity = new PdfPCell(new Paragraph("Quantity", font));
                headerQuantity.HorizontalAlignment = Element.ALIGN_CENTER;
                headerQuantity.BackgroundColor = new BaseColor(Color.LightGray);

                PdfPCell headerTotal = new PdfPCell(new Paragraph("Total", font));
                headerTotal.HorizontalAlignment = Element.ALIGN_CENTER;
                headerTotal.BackgroundColor = new BaseColor(Color.LightGray);

                PdfPCell headerRemarks = new PdfPCell(new Paragraph("Remarks", font));
                headerRemarks.HorizontalAlignment = Element.ALIGN_CENTER;
                headerRemarks.BackgroundColor = new BaseColor(Color.LightGray);

                setups.AddCell(headerName);
                setups.AddCell(headerPrice);
                setups.AddCell(headerQuantity);
                setups.AddCell(headerTotal);
                setups.AddCell(headerRemarks);

                int? totalPlanAmount = 0;

                List<eventSetupsMap> map = db.eventSetupsMaps.Where(x=>x.booking_id == Booking.id).ToList();
                foreach (var row in map)
                {
                    eventSetup setup = db.eventSetups.Find(row.setup_id);

                    PdfPCell name = new PdfPCell(new Paragraph(setup.name.ToString(),font));
                    PdfPCell price = new PdfPCell(new Paragraph("Rs:"+ row.price.ToString() + "/=", font));
                    PdfPCell remarks = new PdfPCell(new Paragraph(row.remarks.ToString(), font));
                    PdfPCell quan = new PdfPCell(new Paragraph(row.quantity.ToString(), font));
                    PdfPCell total = new PdfPCell(new Paragraph("Rs:"+(row.price * row.quantity).ToString() + "/=", font));
                    totalPlanAmount += (row.price * row.quantity);
                    setups.AddCell(name);
                    setups.AddCell(price);
                    setups.AddCell(quan);
                    setups.AddCell(total);
                    setups.AddCell(remarks);



                }

              



                document.Add(setups);

                Paragraph p5totalPlanAmount = new Paragraph("Total: Rs:" + totalPlanAmount.ToString() + "/=");
                p5totalPlanAmount.Alignment = Element.ALIGN_RIGHT;
                document.Add(p5totalPlanAmount);

                Paragraph p32 = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLACK, Element.ALIGN_LEFT, 1)));
                document.Add(p32);

                Paragraph p4 = new Paragraph("Payment Transactions:");
                p4.SpacingAfter = 10;
                document.Add(p4);

                PdfPTable transcations = new PdfPTable(7);

                PdfPCell headerIndex = new PdfPCell(new Paragraph("Index", font));
                headerIndex.HorizontalAlignment = Element.ALIGN_CENTER;
                headerIndex.BackgroundColor = new BaseColor(Color.LightGray);

                PdfPCell headerAmount = new PdfPCell(new Paragraph("Amount", font));
                headerAmount.HorizontalAlignment = Element.ALIGN_CENTER;
                headerAmount.BackgroundColor = new BaseColor(Color.LightGray);

                PdfPCell headerPaymentType = new PdfPCell(new Paragraph("Payment Type", font));
                headerPaymentType.HorizontalAlignment = Element.ALIGN_CENTER;
                headerPaymentType.BackgroundColor = new BaseColor(Color.LightGray);

                PdfPCell headerBankName = new PdfPCell(new Paragraph("Bank Name", font));
                headerBankName.HorizontalAlignment = Element.ALIGN_CENTER;
                headerBankName.BackgroundColor = new BaseColor(Color.LightGray);

                PdfPCell headerCreditCard = new PdfPCell(new Paragraph("Credit Card", font));
                headerCreditCard.HorizontalAlignment = Element.ALIGN_CENTER;
                headerCreditCard.BackgroundColor = new BaseColor(Color.LightGray);

                PdfPCell headerChequeNumber = new PdfPCell(new Paragraph("Cheque Number", font));
                headerChequeNumber.HorizontalAlignment = Element.ALIGN_CENTER;
                headerChequeNumber.BackgroundColor = new BaseColor(Color.LightGray);

                PdfPCell headerDateTime = new PdfPCell(new Paragraph("Date Time", font));
                headerDateTime.HorizontalAlignment = Element.ALIGN_CENTER;
                headerDateTime.BackgroundColor = new BaseColor(Color.LightGray);

                transcations.AddCell(headerIndex);
                transcations.AddCell(headerAmount);
                transcations.AddCell(headerPaymentType);
                transcations.AddCell(headerBankName);
                transcations.AddCell(headerCreditCard);
                transcations.AddCell(headerChequeNumber);
                transcations.AddCell(headerDateTime);

                List<eventTransaction> transaction = db.eventTransactions.Where(x => x.booking_id == Booking.id).ToList();
                transcations.WidthPercentage = 100;
                int index = 0;
                int totalPayment = 0;
                foreach (var item in transaction)
                {
                    
                    string type = "";

                    if (item.paymentType == 1)
                    {
                        type = "Cash";
                    }
                    else if (item.paymentType == 2)
                    {
                        type = "Cheque";
                    }
                    else
                    {
                        type = "Credit Card";
                    }
                    string indexType = "";
                    if (Convert.ToInt32(item.amount) < 0)
                    {
                        indexType = "Refund";
                    }
                    else
                    {
                        if (index == 0)
                        {
                            indexType = "Advance Payment";
                        }
                        else if (index == 1)
                        {
                            indexType = "Second Payment";
                        }
                        else if (index == 2)
                        {
                            indexType = "Third Payment";
                        }
                        else if (index == 3)
                        {
                            indexType = "Fourth Payment";
                        }
                        else if (index == 4)
                        {
                            indexType = "Fifth Payment";
                        }
                        else if (index == 5)
                        {
                            indexType = "Sixth Payment";
                        }
                        else if (index == 6)
                        {
                            indexType = "Seventh Payment";
                        }
                        else if (index == 7)
                        {
                            indexType = "Eighth Payment";
                        }
                        else if (index == 8)
                        {
                            indexType = "Ninth Payment";
                        }
                        else if (index == 9)
                        {
                            indexType = "Tenth Payment";
                        }
                        else if (index == 10)
                        {
                            indexType = "Eleventh Payment";
                        }
                        else if (index == 11)
                        {
                            indexType = "Twelfth Payment";
                        }
                        else if (index == 12)
                        {
                            indexType = "Thirteenth Payment";
                        }
                        else if (index == 13)
                        {
                            indexType = "Fourteenth Payment";
                        }
                        else if (index == 14)
                        {
                            indexType = "Fifteenth Payment";
                        }
                    }

                    if(indexType == "Refund")
                    {
                        Font fontRefund = FontFactory.GetFont("Tahuma", 8.0f, BaseColor.RED);

                        Paragraph PaymentIndex = new Paragraph(indexType, fontRefund);
                        
                        Paragraph Amount = new Paragraph("Rs:" + item.amount.ToString() + "/=", fontRefund);
                        Paragraph PaymentType = new Paragraph(type, fontRefund);
                        Paragraph BankName = new Paragraph((item.bankName == "0" ? "-" : item.bankName), fontRefund);
                        Paragraph CreditCard = new Paragraph((item.creditCardNumber == "0" ? "-" : item.creditCardNumber), fontRefund);
                        Paragraph ChequeNumber = new Paragraph((item.chequeNumber == "0" ? "-" : item.chequeNumber), fontRefund);
                        Paragraph DateTime = new Paragraph(item.insertedDateTime.Value.ToString("dd/MM/yyy"), fontRefund);

                        transcations.AddCell(PaymentIndex);
                        transcations.AddCell(Amount);
                        transcations.AddCell(PaymentType);
                        transcations.AddCell(BankName);
                        transcations.AddCell(CreditCard);
                        transcations.AddCell(ChequeNumber);
                        transcations.AddCell(DateTime);

                        PdfPCell re = new PdfPCell();
                        re.Colspan = 7;
                        
                        Paragraph pRemarks = new Paragraph("Remarks:"+item.remarks,fontRefund);
                        re.AddElement(pRemarks);

                        transcations.AddCell(re);

                        totalPayment += Convert.ToInt32(item.amount);

                    }
                    else
                    {
                        Paragraph PaymentIndex = new Paragraph(indexType,font);
                        Paragraph Amount = new Paragraph("Rs:"+item.amount.ToString()+"/=", font);
                        Paragraph PaymentType = new Paragraph(type, font);
                        Paragraph BankName = new Paragraph((item.bankName == "0" ? "-" : item.bankName) , font);
                        Paragraph CreditCard = new Paragraph((item.creditCardNumber == "0" ? "-" : item.creditCardNumber), font);
                        Paragraph ChequeNumber = new Paragraph((item.chequeNumber == "0" ? "-" : item.chequeNumber), font);
                        Paragraph DateTime = new Paragraph(item.insertedDateTime.Value.ToString("dd/MM/yyy"),font);

                        transcations.AddCell(PaymentIndex);
                        transcations.AddCell(Amount);
                        transcations.AddCell(PaymentType);
                        transcations.AddCell(BankName);
                        transcations.AddCell(CreditCard);
                        transcations.AddCell(ChequeNumber);
                        transcations.AddCell(DateTime);

                        totalPayment += Convert.ToInt32(item.amount);





                    }
                    index++;
                }

                document.Add(transcations);

                Paragraph p5 = new Paragraph("Total: Rs:"+totalPayment.ToString()+"/=");
                p5.Alignment = Element.ALIGN_RIGHT;
                document.Add(p5);

                Paragraph p33 = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLACK, Element.ALIGN_LEFT, 1)));
                document.Add(p33);


                Paragraph p14 = new Paragraph("Terms & Conditions:");
                p14.SpacingAfter = 10;
                document.Add(p14);

                document.Add(new Paragraph("1- The event should be booked 03 days prior.",font));
                document.Add(new Paragraph("2 - Minimum 50 % of the total amount would be taken in advance at the time of booking.In case of payment by cross cheque it should be submitted before 5 working days prior to the event date.In case of a dishonored cheque, the event will be treated as cancelled.", font));
                
                document.Add(new Paragraph(" 3 - Party/ client must collect the payment receipt from the PEEKABEAR / accounts department.", font));
                
                document.Add(new Paragraph("4 - Music system from outside is not allowed.", font));
                
                document.Add(new Paragraph("5 - Outside beverages are not allowed.", font));
                
                document.Add(new Paragraph("6 - If the event date is transferred by the party due to any reason, new date will be confirmed after subject to venue availability only.", font));
                

                document.Add(new Paragraph("7 - In case of Cancellation of Event 100% payment will be refundable before 15 Days, 50 % payment refundable before 6 Days and 10% of the total amount will be refundable before 3 Days.", font));
                
                document.Add(new Paragraph("8 - PEEKABEAR shall assume no responsibility for loss of any personal belongings and valuable things.", font));
               
                document.Add(new Paragraph("9 - No political meeting or political functions are allowed under any circumstances in our Venue.", font));
                
                document.Add(new Paragraph("5 - 10 - Client / Party will be charged 100% and PEEKABEAR will not be responsible for any sort of law & order situation during the event day.", font));
         















               Chunk glue = new Chunk(new VerticalPositionMark());
                Paragraph signatures = new Paragraph(new Chunk("Manager Sales Signature").SetUnderline(1, 13));
                signatures.Add(new Chunk(glue));
                signatures.Add(new Chunk("Customer Signature").SetUnderline(1, 13));
                document.Add(new Chunk("\n"));
                document.Add(new Chunk("\n"));
                document.Add(new Chunk("\n"));
                document.Add(new Chunk("\n"));
                document.Add(new Chunk("\n"));
                document.Add(new Chunk("\n"));

                document.Add(signatures);






                document.Close();

                byte[] byteInfo = workStream.ToArray();
                workStream.Write(byteInfo, 0, byteInfo.Length);
                workStream.Position = 0;

                //return new FileStreamResult(workStream, "application/pdf");
                Response.AppendHeader("content-disposition", "inline; filename=file.pdf");
                return new FileStreamResult(workStream, "application/pdf");
            }
            catch (Exception)
            {

                throw;
            }
        }


        public string UserEditRoles(int id, bool aC, bool eC, bool dC, bool aET, bool eET, bool dET, bool aES, bool eES, bool dES, bool r)
        {
            //https://localhost:44393/Dashboard/UserEditRoles?id=7
            var list = db.userScreenPermissions.Where(x => x.user_id == id).ToList();
            foreach (var item in list)
            {
                var ChildList = db.userScreenChildPermissions.Where(y => y.screen_id == item.id);
                foreach (var itemChild in ChildList)
                {
                    db.userScreenChildPermissions.Remove(itemChild);
                }
                db.userScreenPermissions.Remove(item);
            }
            db.SaveChanges();

            userScreenPermission screenPermissionCalendar = new userScreenPermission();
            screenPermissionCalendar.user_id = id;
            screenPermissionCalendar.calendar = true;

            db.userScreenPermissions.Add(screenPermissionCalendar);
            db.SaveChanges();

            userScreenChildPermission screenChildPermissionCalendar = new userScreenChildPermission();
            screenChildPermissionCalendar.add = aC;
            screenChildPermissionCalendar.edit = eC;
            screenChildPermissionCalendar.delete = dC;
            screenChildPermissionCalendar.screen_id = screenPermissionCalendar.id;

            db.userScreenChildPermissions.Add(screenChildPermissionCalendar);
            db.SaveChanges();

            userScreenPermission screenPermissionES = new userScreenPermission();
            screenPermissionES.user_id = id;
            screenPermissionES.eventSetup = true;

            db.userScreenPermissions.Add(screenPermissionES);
            db.SaveChanges();

            userScreenChildPermission screenChildPermissionES = new userScreenChildPermission();
            screenChildPermissionES.add = aES;
            screenChildPermissionES.edit = eES;
            screenChildPermissionES.delete = dES;
            screenChildPermissionES.screen_id = screenPermissionES.id;

            db.userScreenChildPermissions.Add(screenChildPermissionES);
            db.SaveChanges();


            userScreenPermission screenPermissionET = new userScreenPermission();
            screenPermissionET.user_id = id;
            screenPermissionET.eventTransaction = true;

            db.userScreenPermissions.Add(screenPermissionET);
            db.SaveChanges();

            userScreenChildPermission screenChildPermissionET = new userScreenChildPermission();
            screenChildPermissionET.add = aET;
            screenChildPermissionET.edit = eET;
            screenChildPermissionET.delete = dET;
            screenChildPermissionET.screen_id = screenPermissionET.id;

            db.userScreenChildPermissions.Add(screenChildPermissionET);
            db.SaveChanges();



            userScreenPermission screenPermissionR = new userScreenPermission();
            screenPermissionR.user_id = id;
            screenPermissionR.reports = true;

            db.userScreenPermissions.Add(screenPermissionR);
            db.SaveChanges();


            userScreenChildPermission screenChildPermissionR = new userScreenChildPermission();
            screenChildPermissionR.add = r;
            screenChildPermissionR.edit = r;
            screenChildPermissionR.delete = r;
            screenChildPermissionR.screen_id = screenPermissionR.id;

            db.userScreenChildPermissions.Add(screenChildPermissionR);
            db.SaveChanges();


            return "Done";
                       
        }

        public string AddUser(string username, string password,bool aC,bool eC, bool dC,bool aET, bool eET,bool dET,bool aES,bool eES,bool dES,bool r) {
            user user = new user();
            
            user.username = username;
            user.password = password;
            user.isActive = true;
            user.insertedDateTime = DateTime.Now;
            
            db.users.Add(user);
            db.SaveChanges();
            
            userScreenPermission screenPermissionCalendar = new userScreenPermission();
            screenPermissionCalendar.user_id = user.id;
            screenPermissionCalendar.calendar = true;     
            
            db.userScreenPermissions.Add(screenPermissionCalendar);
            db.SaveChanges();

            userScreenChildPermission screenChildPermissionCalendar = new userScreenChildPermission();
            screenChildPermissionCalendar.add = aC;
            screenChildPermissionCalendar.edit = eC;
            screenChildPermissionCalendar.delete = dC;
            screenChildPermissionCalendar.screen_id = screenPermissionCalendar.id;

            db.userScreenChildPermissions.Add(screenChildPermissionCalendar);
            db.SaveChanges();

            userScreenPermission screenPermissionES = new userScreenPermission();
            screenPermissionES.user_id = user.id;
            screenPermissionES.eventSetup = true;

            db.userScreenPermissions.Add(screenPermissionES);
            db.SaveChanges();

            userScreenChildPermission screenChildPermissionES = new userScreenChildPermission();
            screenChildPermissionES.add = aES;
            screenChildPermissionES.edit = eES;
            screenChildPermissionES.delete = dES;
            screenChildPermissionES.screen_id = screenPermissionES.id;

            db.userScreenChildPermissions.Add(screenChildPermissionES);
            db.SaveChanges();


            userScreenPermission screenPermissionET = new userScreenPermission();
            screenPermissionET.user_id = user.id;
            screenPermissionET.eventTransaction = true;

            db.userScreenPermissions.Add(screenPermissionET);
            db.SaveChanges();

            userScreenChildPermission screenChildPermissionET = new userScreenChildPermission();
            screenChildPermissionET.add = aET;
            screenChildPermissionET.edit = eET;
            screenChildPermissionET.delete = dET;
            screenChildPermissionET.screen_id = screenPermissionET.id;

            db.userScreenChildPermissions.Add(screenChildPermissionET);
            db.SaveChanges();

            

            userScreenPermission screenPermissionR = new userScreenPermission();
            screenPermissionR.user_id = user.id;
            screenPermissionR.reports = true;

            db.userScreenPermissions.Add(screenPermissionR);
            db.SaveChanges();


            userScreenChildPermission screenChildPermissionR = new userScreenChildPermission();
            screenChildPermissionR.add = r;
            screenChildPermissionR.edit = r;
            screenChildPermissionR.delete = r;
            screenChildPermissionR.screen_id = screenPermissionR.id;

            db.userScreenChildPermissions.Add(screenChildPermissionR);
            db.SaveChanges();


            return "Done";




        }
       
        public string FetchVenues()
        {
            var results = from u in db.venues
                          where u.isActive == true
                          select new { u };

            return JsonConvert.SerializeObject(results.ToList());


        }
        public ActionResult Events()
        {
            return View();
        }
        public string AddVenue(string name)
        {
            venue v = new venue();
            v.name = name;
            v.isActive = true;
            db.venues.Add(v);
            db.SaveChanges();
            return "OK";
        }
        public string AddEventType(string name)
        {
            eventType et = new eventType();
            et.name = name;
            et.isActive = true;
            db.eventTypes.Add(et);
            db.SaveChanges();
            return "OK";
        }

        public string FetchUsers()
        {
            var results = from u in db.users                        
                          
                          
                          where u.isActive == true 
                          select new {
                              u,

                              childScreen = (
                             from sc in db.userScreenChildPermissions
                             join s in db.userScreenPermissions on sc.screen_id equals s.id
                             where s.user_id == u.id
                             

                                
                            

                             select new {sc,s}
                            )
                          };

            return JsonConvert.SerializeObject(results.ToList());


        }
        public string DeleteUser(int id)
        {
            try
            {
                user user = db.users.Find(id);
                user.isActive = false;
                db.SaveChanges();
                return "done";
            }
            catch (Exception)
            {
               
                throw;
            }
        }
        public string FetchEventTypes()
        {
            var results = from u in db.eventTypes
                          where u.isActive == true
                          select new { u };

            return JsonConvert.SerializeObject(results.ToList());
        }
        public string FetchEventType(int id)
        {
            var results = from u in db.eventTypes
                          where u.isActive == true & u.id == id
                          select new { u };

            return JsonConvert.SerializeObject(results.ToList());
        }
        public string DeleteVenue(int id)
        {
            try
            {
                venue user = db.venues.Find(id);
                user.isActive = false;
                db.SaveChanges();
                return "done";
            }
            catch (Exception)
            {

                throw;
            }
        }
        public string FetchVenue(int id)
        {
            return JsonConvert.SerializeObject(db.venues.Where(r => r.id == id).Select(r => r).ToList());
        }
        public string FetchUser(int id)
        {
            return JsonConvert.SerializeObject(db.users.Where(r => r.id == id).Select(r => r).ToList());
        }
        public string FetchUserPermission(int id)
        {
            //var result = from user in db.Users
            //             //join up in db.User_Permission on user.id equals up.userId
            //             where user.isActive == true & user.id == id
            //             select new {up};
            // return JsonConvert.SerializeObject(result.ToList());
            return "123";
        }
        public string EditUser(string username, string password,int id)
        {
            user u = db.users.Find(id);
            u.username = username;
            u.password = password;
            db.SaveChanges();
            return "Done";
            
        }
        public string EditEventType(int id, string name)
        {
            eventType u = db.eventTypes.Find(id);
            u.name = name;
            db.SaveChanges();
            return "Done";

        }      
        public string EditVenue(int id,string name)
        {
            venue u = db.venues.Find(id);
            u.name = name;
            db.SaveChanges();
            return "Done";

        }
        public string FetchUserRoles(int id)
        {
            var results = from u in db.users


                          where u.isActive == true && u.id == id
                          select new
                          {
                              u,

                              childScreen = (
                             from sc in db.userScreenChildPermissions
                             join s in db.userScreenPermissions on sc.screen_id equals s.id
                             where s.user_id == u.id





                             select new { sc, s }
                            )
                          };

            return JsonConvert.SerializeObject(results.ToList());
        }
        public string DeleteEventType(int id)
        {
            try
            {
                eventType user = db.eventTypes.Find(id);
                user.isActive = false;
                db.SaveChanges();
                return "done";
            }
            catch (Exception)
            {

                throw;
            }
        }

    }
}