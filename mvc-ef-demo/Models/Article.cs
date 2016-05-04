using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MvcEfSample.Models
{
    public class Article
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        [StringLengthAttribute(100)]
        public string Title { get; set; }        
        public DateTime CreatedTime { get; set; }        
        public DateTime UpdatedTime { get; set; } 
        
        [Column("Content", TypeName = "text")]
        public string Content { get; set; }
    }
}
