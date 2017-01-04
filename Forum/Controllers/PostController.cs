﻿using Forum.Classes;
using Forum.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Forum.Controllers
{
    public class PostController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Post
        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            var postList = db.Posts.ToList();

            return View(postList);
        }
        
        [HttpGet]
        public ActionResult Index(string id)
        {
            List<Post> viewModel = new List<Post>();
            if (id == null)
            {
                viewModel = db.Posts.ToList();
            }
            else
            {
                viewModel = db.Posts.ToList().FindAll(x => x.UserID == id);
            }

            return View(viewModel);
        }

        [HttpGet]
        public ActionResult Details(int id)
        {
            var post = db.Posts.Find(id);

            return View(post);
        }

        [HttpGet]
        public ActionResult Create(int id)
        {
            var newPost = new Post();
            newPost.Topic = db.Topics.Find(id);
            newPost.TopicID = id;

            return View(newPost);
        }

        [HttpPost]
        public ActionResult Create(Post post)
        {
            post.Date = DateTime.Now;
            if (User.Identity.IsAuthenticated)
            {
                post.UserID = User.Identity.GetUserId();
            }
            else
            {
                post.UserID = null;
            }
            post.Content = Html.EditMarkers(post.Content);

            db.Posts.Add(post);
            db.Topics.Find(post.TopicID).PostCount++;
            db.Fora.Find(db.Topics.Find(post.TopicID).ForumID).PostCount++;
            db.Topics.Find(post.TopicID).LastPostDate = post.Date;

            db.SaveChanges();


            return RedirectToAction("Details", "Topic", new { id = post.TopicID });
        }

        [OwnerAuthorize]
        public ActionResult Edit(int id)
        {
            Post post = db.Posts.Find(id);
            return View(post);
        }

        // POST: Topic/Edit/5
        [HttpPost]
        [OwnerAuthorize]
        public ActionResult Edit(int id, Post post)
        {
            post.Content = Html.EditMarkers(post.Content);
            db.Entry(post).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("Details", "Topic", new { id = post.TopicID });
        }

    }
}