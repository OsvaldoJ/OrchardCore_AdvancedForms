using System;
using System.Collections.Generic;
using System.Text;

namespace AdvancedForms.Models
{
    public class AdvancedFormSubmissions
    {
        public EditorPart Submission;
        public string Title { get; set; }
        public EditorPart Metadata;
       
        public AdvancedFormSubmissions(string submission, string metadata, string title)
        {
            Submission = new EditorPart(submission);
            Title = title;
            Metadata = new EditorPart(metadata);                      
        }

    }
}
