using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using OpenGameListWebApp.Data.Comments;
using OpenGameListWebApp.Data.Items;

namespace OpenGameListWebApp.Data.Users
{
    public class ApplicationUser : IdentityUser
    {
        #region Constructor
        public ApplicationUser()
        {
        }
        #endregion Constructor
        #region Properties
        [Key]
        //[Required]
        //public string Id { get; set; }
        //[Required]
        //[MaxLength(128)]
        //public string UserName { get; set; }
        //[Required]
        //public string Email { get; set; }
        public string DisplayName { get; set; }
        public string Notes { get; set; }
        [Required]
        public int Type { get; set; }
        [Required]
        public int Flags { get; set; }
        [Required]
        public DateTime CreatedDate { get; set; }
        [Required]
        public DateTime LastModifiedDate { get; set; }
        #endregion Properties

        #region Related Properties
        /// <summary>
        /// A list of items wrote by this user: this property will be loaded on first use using EF's Lazy-Loading feature.
        /// </summary>
        public virtual List<Item> Items { get; set; }
        /// <summary>
        /// A list of comments wrote by this user: this property will be loaded on first use using EF's Lazy-Loading feature.
        /// </summary>
        public virtual List<Comment> Comments { get; set; }
        #endregion Related Properties

    }
}