﻿

namespace AdvancedForms.ViewModels
{
    public class AdvancedFormViewModel
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Instructions { get; set; }
        public string Container { get; set; }
        public string Submission { get; set; }
        public string SubmissionId { get; set; }
        public Enums.EntryType EntryType { get; set; }
    }
}
