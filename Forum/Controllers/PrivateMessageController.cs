﻿using Forum.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Forum.Controllers
{
    public class PrivateMessageController : Controller
    {
        ApplicationDbContext db = new ApplicationDbContext();
        public ActionResult Index(int? page)
        {
            PrivateThreadsListViewModel viewModel = new PrivateThreadsListViewModel();
            int startIndex, endIndex, postsPerPage;

            var user = db.Users.Find(User.Identity.GetUserId());
            postsPerPage = user.PostsPerPage.Quantity;


            var userId = User.Identity.GetUserId();

            viewModel.User = db.Users.ToList().Find(x => x.Id == userId);
            viewModel.Threads = db.PrivateThreads.ToList().FindAll(x => x.RecipientID == userId || x.SenderID == userId);

            viewModel.Threads = viewModel.Threads.OrderByDescending(x => x.LastPostDate()).ToList();

            viewModel.PostsCount = db.Posts.ToList().FindAll(x => x.UserID == viewModel.User.Id).Count();
            viewModel.TopicsCount = db.Topics.ToList().FindAll(x => x.UserID == viewModel.User.Id).Count();

            viewModel.Roles = new List<IdentityRole>();

            foreach (IdentityUserRole role in viewModel.User.Roles)
            {
                viewModel.Roles.Add(db.Roles.ToList().Find(x => x.Id == role.RoleId));
            }

            viewModel.Pages = viewModel.Threads.Count() / postsPerPage + 1;

            if (page == null)
            {
                startIndex = 0;
                viewModel.CurrentPage = 0;
            }
            else
            {
                startIndex = (int)page * postsPerPage;
                viewModel.CurrentPage = (int)page;
            }

            if (viewModel.Threads.Count() < startIndex + postsPerPage)
            {
                endIndex = viewModel.Threads.Count();
            }
            else
            {
                endIndex = startIndex + postsPerPage;
            }


            viewModel.Threads = viewModel.Threads.GetRange(startIndex, endIndex - startIndex);

            return View(viewModel);
        }

        public ActionResult ViewThread(int id, int? page)
        {
            PrivateThreadViewModel viewModel = new PrivateThreadViewModel();


            int startIndex, endIndex, postsPerPage;


            var userId = User.Identity.GetUserId();

            viewModel.User = db.Users.ToList().Find(x => x.Id == userId);

            postsPerPage = viewModel.User.PostsPerPage.Quantity;

            viewModel.PostsCount = db.Posts.ToList().FindAll(x => x.UserID == viewModel.User.Id).Count();
            viewModel.TopicsCount = db.Topics.ToList().FindAll(x => x.UserID == viewModel.User.Id).Count();

            viewModel.Roles = new List<IdentityRole>();

            viewModel.Messages = db.PrivateMessages.ToList().FindAll(x => x.PrivateThreadID == id);
            viewModel.PrivateThread = db.PrivateThreads.ToList().Find(x => x.ID == id);
            viewModel.PrivateThread.Seen = true;
            db.SaveChanges();

            viewModel.Pages = viewModel.Messages.Count() / postsPerPage + 1;

            if (page == null)
            {
                startIndex = 0;
                viewModel.CurrentPage = 0;
            }
            else
            {
                startIndex = (int)page * postsPerPage;
                viewModel.CurrentPage = (int)page;
            }

            if (viewModel.Messages.Count() < startIndex + postsPerPage)
            {
                endIndex = viewModel.Messages.Count();
            }
            else
            {
                endIndex = startIndex + postsPerPage;
            }


            viewModel.Messages = viewModel.Messages.GetRange(startIndex, endIndex - startIndex);
            foreach (IdentityUserRole role in viewModel.User.Roles)
            {
                viewModel.Roles.Add(db.Roles.ToList().Find(x => x.Id == role.RoleId));
            }


            return View(viewModel);
        }
        [HttpPost]
        public ActionResult CreateReply(PrivateThreadViewModel request, int id)
        {
            var _newMessage = new PrivateMessage();
            var userId = User.Identity.GetUserId();
            int startIndex, endIndex, postsPerPage;

            _newMessage.Content = request.Content;

            request = new PrivateThreadViewModel();

            _newMessage.Date = DateTime.Now;
            _newMessage.Author = db.Users.ToList().Find(x => x.Id == userId);

            request.User = _newMessage.Author;

            request.Content = "";

            request.PostsCount = db.Posts.ToList().FindAll(x => x.UserID == request.User.Id).Count();
            request.TopicsCount = db.Topics.ToList().FindAll(x => x.UserID == request.User.Id).Count();

            request.Roles = new List<IdentityRole>();

            request.PrivateThread = db.PrivateThreads.ToList().Find(x => x.ID == id);
            request.PrivateThread.Seen = false;

            _newMessage.PrivateThreadID = id;

            db.PrivateMessages.Add(_newMessage);
            db.SaveChanges();

            postsPerPage = request.User.PostsPerPage.Quantity;

            request.Messages = db.PrivateMessages.ToList().FindAll(x => x.PrivateThreadID == id);
            request.Pages = request.Messages.Count() / postsPerPage + 1;


            startIndex = (request.Pages - 1) * postsPerPage;
            request.CurrentPage = request.Pages;

            if (request.Messages.Count() < startIndex + postsPerPage)
            {
                endIndex = request.Messages.Count();
            }
            else
            {
                endIndex = startIndex + postsPerPage;
            }


            request.Messages = request.Messages.GetRange(startIndex, endIndex - startIndex);
            for (int i = 0; i < Request.Files.Count; i++)
            {
                HttpPostedFileBase file = Request.Files[i];

                if (file.FileName == "") continue;

                file.SaveAs(HttpContext.Server.MapPath("~/Content/Attachments/")
                         + file.FileName);
                MessageFile _messageFile = new MessageFile();
                _messageFile.Filename = "~/Content/Attachments/" + file.FileName;
                _messageFile.PrivateMessage = _newMessage;
                db.MessageFiles.Add(_messageFile);
                db.SaveChanges();
            }

            ModelState.Clear();

            return View("ViewThread", request);
        }

        // GET: PrivateMessage/Create
        public ActionResult CreateThread()
        {
            CreateThreadViewModel viewModel = new CreateThreadViewModel();
            var userId = User.Identity.GetUserId();
            viewModel.User = db.Users.ToList().Find(x => x.Id == userId);

            viewModel.PostsCount = db.Posts.ToList().FindAll(x => x.UserID == viewModel.User.Id).Count();
            viewModel.TopicsCount = db.Topics.ToList().FindAll(x => x.UserID == viewModel.User.Id).Count();

            viewModel.Roles = new List<IdentityRole>();

            foreach (IdentityUserRole role in viewModel.User.Roles)
            {
                viewModel.Roles.Add(db.Roles.ToList().Find(x => x.Id == role.RoleId));
            }

            return View(viewModel);
        }

        // POST: PrivateMessage/Create
        [HttpPost]
        public ActionResult CreateThread(CreateThreadViewModel request)
        {
            try
            {
                var newThread = new PrivateThread();

                var userId = User.Identity.GetUserId();

                newThread.Recipient = db.Users.ToList().Find(x => x.UserName == request.Recipient);
                newThread.SenderID = userId;
                newThread.Title = request.Title;

                db.PrivateThreads.Add(newThread);
                db.SaveChanges();

                var newMessage = new PrivateMessage();

                newMessage.AuthorID = userId;
                newMessage.Content = request.Content;
                newMessage.Date = DateTime.Now;
                newMessage.PrivateThread = newThread;

                db.PrivateMessages.Add(newMessage);
                db.SaveChanges();

                if (Request.Files != null)
                {
                    for (int i = 0; i < Request.Files.Count; i++)
                    {
                        HttpPostedFileBase file = Request.Files[i];
                        if (file == null || file.FileName == "") continue;
                        file.SaveAs(HttpContext.Server.MapPath("~/Content/Attachments/")
                                 + file.FileName);
                        MessageFile _messageFile = new MessageFile();
                        _messageFile.Filename = "~/Content/Attachments/" + file.FileName;
                        _messageFile.PrivateMessage = newMessage;
                        db.MessageFiles.Add(_messageFile);
                        db.SaveChanges();
                    }
                }


                return RedirectToAction("ViewThread", new { id = newThread.ID });
            }
            catch
            {
                var userId = User.Identity.GetUserId();

                request.User = db.Users.ToList().Find(x => x.Id == userId);
                request.PostsCount = db.Posts.ToList().FindAll(x => x.UserID == request.User.Id).Count();
                request.TopicsCount = db.Topics.ToList().FindAll(x => x.UserID == request.User.Id).Count();

                request.Roles = new List<IdentityRole>();

                foreach (IdentityUserRole role in request.User.Roles)
                {
                    request.Roles.Add(db.Roles.ToList().Find(x => x.Id == role.RoleId));
                }

                return View(request);
            }
        }

        // GET: PrivateMessage/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: PrivateMessage/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: PrivateMessage/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: PrivateMessage/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
        public FileResult DownloadAttachment(string Filename)
        {
            byte[] fileBytes = System.IO.File.ReadAllBytes(HttpContext.Server.MapPath(Filename));

            string fileName = Filename.Split('/').Last();

            return File(Filename, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }
        public JsonResult GetMatchingUsers(string term)
        {
            var results = db.Users.Where(user => term == null || user.UserName.ToLower().Contains(term.ToLower())).Select(x => new { id = x.Id, value = x.UserName }).Take(5).ToList();

            return Json(results, JsonRequestBehavior.AllowGet);
        }
    }

}